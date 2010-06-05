#!/usr/bin/perl 
use Socket;
use IO::Socket;
use IPTB;
use Time::HiRes (gettimeofday);
use Getopt::Long;

my $progname = 'packter_tc.pl';

my ($dp_host, $my_host, $pt_host);
my $dp_port = '9005';
my $my_port = 11301;
my $pt_port = 11300;

my $config_file = "/usr/local/etc/packter.conf";
my $enable_sound = 0;

my $pt_msg_h = "PACKTERMSG";
my $pt_sound_h = "PACKTERSOUND";
my $pt_voice_h = "PACKTERVOICE";

my %config = ();

$| = 1;
$SIG{PIPE} = 'connect_DP';

GetOptions(
	'dp=s' => \$dp_host,
	'listen=s' => \$my_host,
	'viewer=s' => \$pt_host,
	'config=s' => \$config_file,
	'sound' => \$enable_sound 
);

&main;

sub main {
	my ($src, $dst, $sport, $dport, $flag, $hash);
	my ($msg, $sound, $voice);
	if ($my_host eq "" || $dp_host eq "" || $pt_host eq ""){
		&usage();
	}


	&read_config();

	&connect_DP;

	$pid = fork();
	if ($pid > 0){
		socket(SOCKET, PF_INET, SOCK_DGRAM, 0);
		bind(SOCKET, pack_sockaddr_in($my_port, INADDR_ANY));
		while(1){
			recv(SOCKET, $buf, 10000, 0);
			$buf =~ s/\r//g;
			$buf =~ s/\n//g;

			($src, $dst, $sport, $dport, $flag, $hash) = split(/,/, $buf);
			if ($hash eq ""){
				$hash = $buf;
			}

			#
			# Process Request
			#	
			{	
				$msg = "";
				$sound = "";
				$voice = "";

				socket(PACKTER, PF_INET, SOCK_DGRAM, 0);
				$pt_addr = pack_sockaddr_in($pt_port, inet_aton($pt_host));
				$msg = sprintf("%s\n%s,%s<br>%s%s",
									$pt_msg_h,
									$config{'IPTB_START_PIC'},
									$config{'IPTB_START_MSG'},
									$config{'IPTB_OPT_HASH'},
									$hash
							);
				send(PACKTER, $msg, 0, $pt_addr);
				printf("MSG:%s\n", $msg);
				if ($enable_sound){
					$voice = sprintf("%s\n%s%s%s",
									$pt_voice_h,
									$config{'IPTB_OPT_VOICE_HEAD'},
									$config{'IPTB_START_VOICE'},
									$config{'IPTB_OPT_VOICE_FOOT'}
									);
					send(PACKTER, $voice, 0, $pt_addr);

					$sound = sprintf("%s\n%s%s",
									$pt_sound_h,
									$config{'IPTB_OPT_SOUND'},
									$config{'IPTB_START_SOUND'}
									);
					send(PACKTER, $sound, 0, $pt_addr);
				}
				sleep 10;	
			}

			$req = &generate_req($hash);

			print $dp_sock "$req";
			print "Request $hash\n";
		}
	}
	elsif (defined $pid){
		local $buf = undef;
		local $mode = 0;
		local $tc_rep = undef;
		local $result = "";
		local $pt_addr;

		socket(PACKTER, PF_INET, SOCK_DGRAM, 0);
		$pt_addr = pack_sockaddr_in($pt_port, inet_aton($pt_host));

		while (1){
			while(<$dp_sock>){
				if ($mode == 0){
      	  if (/<InterTrackMessage/){
        	  $mode = 1;
       	 }
      	}

	      if ($mode == 1){
  	      $buf .= $_;
	
			  	if(/<\/InterTrackMessage>/){
						$mode = 0;
						$tc_rep = &gen_InterTrackMessage($buf);
						$buf = undef;
						if ($tc_rep->type eq "ClientMessageIDReply"){
							&show_IDReply($tc_rep);	
						}
						elsif ($tc_rep->type eq "ClientTraceReply"){
							$result = "";

							$result = &show_TraceReply($tc_rep);
							if (length $result > 1){
								$result =~ s/\n/\<br\>/;
								$result = $result . "Target";

								$msg = "";
								$sound = "";
								$voice = "";

							if ($result =~ m/notfound/i){
								$msg = sprintf("%s\n%s,%s<br>%s%s<br>",
												$pt_msg_h,
												$config{'IPTB_FAILED_PIC'},
												$config{'IPTB_FAILED_MSG'},
												$config{'IPTB_OPT_HASH'},
												$result
											);
								if (enable_sound){
									$sound = sprintf("%s\n%s%s",
													$pt_sound_h,
													$config{'OPT_SOUND'},
													$config{'IPTB_FAILED_SOUND'}
									);
									$voice = sprintf("%s\n%s%s",
													$pt_voice_h,
													$config{'OPT_VOICE'},
													$config{'IPTB_FAILED_VOICE'}
									);
								}
							}
							elsif ($result =~ m/found/i){
								$result =~ s/\n/\<font color=\"red\"\ size=+1> --\&gt;\ <\/font\>/g;
								$msg = sprintf("%s\n%s,%s<br>%s%s<br>",
												$pt_msg_h,
												$config{'IPTB_SUCCEED_PIC'},
												$config{'IPTB_SUCCEED_MSG'},
												$config{'IPTB_OPT_HASH'},
												$result
											);
								if (enable_sound){
									$sound = sprintf("%s\n%s%s",
													$pt_sound_h,
													$config{'OPT_SOUND'},
													$config{'IPTB_SUCCEED_SOUND'}
									);
									$voice = sprintf("%s\n%s%s",
													$pt_voice_h,
													$config{'OPT_VOICE'},
													$config{'IPTB_SUCCEED_VOICE'}
									);
								}
							}

							if ($msg ne ""){
								send(PACKTER, $msg, 0, $pt_addr);
								print "send $msg\n";
							}

							if ($enable_sound){
								if ($sound ne ""){
									send(PACKTER, $sound, 0, $pt_addr);
									print "send $sound\n";
								}
					
								if ($voice ne ""){
									send(PACKTER, $voice, 0, $pt_addr);
									print "send $voice\n";
								}
							}
						}
					}
					else {
						print "DP returns unknown message type : " 
												. $tc_rep->type . "\n";
					}
				}
			}
		}	
		}
	}
}

#
# Connect to DP
#
sub connect_DP {
	$dp_sock = new IO::Socket::INET(
		PeerAddr=>$dp_host,
		PeerPort=>$dp_port,
		Proto=>'tcp'
	) or die;

}

#
# Generate DP TraceRequest
#
sub generate_req {
	local ($hash) = $_[0];

	local $tc				 = IPTB::InterTrackMessage->new;

		local $tc_req		 = IPTB::ClientTraceRequest->new;
			local $tc_dst = IPTB::DestinationNode->new;
				local $tc_dst_node = IPTB::NodeID->new;
					local $tc_dst_node_addr = IPTB::IPAddress->new($dp_host);
					$tc_dst_node_addr->block('loopback');
					$tc_dst_node_addr->mask('32');
					$tc_dst_node_addr->version('4');
				$tc_dst_node->IPAddress($tc_dst_node_addr);
				$tc_dst_node->idtype('IP');
			$tc_dst->NodeID($tc_dst_node);

			local $tc_src = IPTB::SourceNode->new;
				local $tc_src_node = IPTB::NodeID->new;
					local $tc_src_node_addr = IPTB::IPAddress->new($my_host);
					$tc_src_node_addr->block('loopback');
					$tc_src_node_addr->mask('32');
					$tc_src_node_addr->version('4');
				$tc_src_node->IPAddress($tc_src_node_addr);
				$tc_src_node->idtype('IP');
			$tc_src->NodeID($tc_src_node);

			local $tc_seq = IPTB::TemporarySequenceNumber->new;
			local ($sec, $usec) = gettimeofday;
			$tc_seq->sec($sec);
			$tc_seq->usec($usec);

			local $tc_ttl = IPTB::TTL->new(16);

			local $tc_pd = IPTB::PacketDump->new($hash);
			$tc_pd->PayloadLength('32');
			$tc_pd->encodetype('md5');
			$tc_pd->header('ip');
			$tc_pd->iftype('1');

			local $tc_ops = IPTB::Options->new();

				local $tc_op = IPTB::Option->new("PACKTER");
				$tc_op->type("type");

			$tc_ops->Option($tc_op);

		$tc_req->DestinationNode($tc_dst);
		$tc_req->SourceNode($tc_src);
		$tc_req->TemporarySequenceNumber($tc_seq);
		$tc_req->TTL($tc_ttl);
		$tc_req->PacketDump($tc_pd);
		$tc_req->Options($tc_ops);

	$tc->ClientTraceRequest($tc_req);
	$tc->type('ClientTraceRequest');

	return $tc->to_xml_string(); 
}

sub gen_InterTrackMessage {
  local $buffer = $_[0];
  return IPTB::InterTrackMessage->from_xml_string($buffer);
}

sub show_IDReply {
	local $id_rep = $_[0];
	print "ClientMessageIDReply : (SN = "
				.  $id_rep->ClientMessageIDReply->MessageID->SequenceNumber->sec
				. "."
				.  $id_rep->ClientMessageIDReply->MessageID->SequenceNumber->usec
				. ");";
	print "\n";
	return ;
}

sub show_TraceReply {
	local $tr_rep = $_[0];
	local $i = 0;
	local $req_hash;
	local $as;
	local $as_result, @as_list;
	local $pt_mesg;

	print "ClientTraceReply : (SN = "
				.  $tr_rep->ClientTraceReply->MessageID->SequenceNumber->sec
				. "."
				.  $tr_rep->ClientTraceReply->MessageID->SequenceNumber->usec
				. ");";
	print "\n";

	#
	# Not support bulk type
	#
	$req_hash = $tr_rep->ClientTraceReply->AllResultExport->ITMTraceRequest->Footmark->PacketDump;
	$pt_mesg = $req_hash . "\n";

	$as_result = $tr_rep->ClientTraceReply->AllResultExport->ITMTraceReply->InterASTraceResult;
	@as_list = @{$as_result->AS};

	for ($i = 0; $i <= $#as_list; $i++){
		$as = $as_list[$i];

		if ($as_list[$i]->NodeID->idtype eq "ITMID"){
			$pt_mesg = sprintf("%sDepth %d:%s AS:%d\n", 
						$pt_mesg, $as->depth, $as->state, $as_list[$i]->NodeID->ITMID->ASNumber());
		}
	}
	print $pt_mesg;

	return $pt_mesg;
}

sub usage ()
{
  print $progname . " : Packter TC\n";
  print "\n";
  print $progname . "\n";
  print "   -v [ Viewer IP address ] (must)\n";
  print "   -d [ DP IP address ] (must)\n";
  print "   -l [ Listening IP address ] (must)\n";
  print "   -c [ Config file ] (optional: default \"./packter.conf\")\n";
  print "   -s ( Enable Sound )(optional: default no)\n";
  print "\n";
  print "example) ./$progname -v 127.0.0.1 -d 127.0.0.1 -l 127.0.0.1 -c /usr/local/etc/packter.conf -s\n";
  print "\n";

  exit;
}

sub read_config()
{
	local ($key, @list);
	open(IN, $config_file) or die;
	while(<IN>){
		s/\r//g;
		s/\n//g;
		if ($_ eq ""){
			next;
		}
		elsif (m/^#/){
			next;
		}
		else {
			($key, @list) = split(/=/, $_);	
			$config{$key} = join('=', @list);	
		}
	}
	close(IN);
	return;
}
