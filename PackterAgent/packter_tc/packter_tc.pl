#!/usr/bin/perl 
use Socket;
use IO::Socket;
use IPTB;
use Time::HiRes (gettimeofday);

my $dp_host = '127.0.0.1';
my $dp_port = '9005';

my $my_host = '210.135.85.66';
my $my_port = 11301;

my $pt_host = '210.135.85.68';
my $pt_port = 11300;

$| = 0;

&main;

sub main {
	local ($src, $dst, $sport, $dport, $flag, $hash);

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
				next;
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
		local $msg = "";
		local $pt_addr;

		socket(PACKTER, PF_INET, SOCK_DGRAM, 0);
		$pt_addr = pack_sockaddr_in($pt_port, inet_aton($pt_host));

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
						$msg = "";

						$result = &show_TraceReply($tc_rep);
						if (length $result > 1){
							# $msg = sprintf("ult for %s", $result);

							$result =~ s/\n/\<br\>/g;
							if ($result =~ m/found/){
								$msg = sprintf("PACKTERMSG\npackter02.png, <div align=center><font color=red>追跡が成功しました</font></div><br>ハッシュ値：%s<br>", $result);

								$sound = sprintf("PACKTERSOUND\n30000, packter03.wav");

							}
							else {
								$msg = sprintf("PACKTERMSG\npackter03.png, <center><font color=red>追跡しましたが、発見できませんでした。</font></center>s<br>", $result);
							}

							send(PACKTER, $msg, 0, $pt_addr);
							send(PACKTER, $sound, 0, $pt_addr);
							print "send $msg";
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
