/*
 * Copyright (c) 2011 Daisuke Miyamoto. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *		notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *		notice, this list of conditions and the following disclaimer in the
 *		documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.	IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 *
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <time.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>
#include <arpa/inet.h>
#include <sys/time.h>
#include <netdb.h>
#include <pwd.h>

#include <pcap.h>

#include <openssl/md5.h>

#include "pt_std.h"
#include "pt_agent.h"
#include "pt_ether.h"
#include "pt_ip.h"
#include "pt_ip6.h"
#include "pt_ipproto.h"
#include "pt_icmp.h"
#include "pt_icmp6.h"
#include "pt_tcp.h"
#include "pt_udp.h"
#include "pt_snort.h"

/* for debug */
int debug = PACKTER_FALSE;

char *progname;
int sock;
struct sockaddr_in addr;
int rate_limit;
int rate;

/* for IPv6 */
#ifdef USE_INET6
struct sockaddr_in6 addr6;
#endif
int use6 = PACKTER_FALSE;

/* for InterTrack */
char trace_server[PACKTER_BUFSIZ];
int trace = PACKTER_FALSE;
int packter_flagbase = 0;

/* for Sound */
int enable_sound = PACKTER_FALSE;

int main(int argc, char *argv[])
{
	/* pcap */
	char *dumpfile = NULL ;										/* pcap dumpfile */
	char *device = NULL;											/* interface */
	char *filter = NULL;											/* pcap filter */

	/* viewer */
	char *ip = NULL;
	int port = 0;

	/* misc */
	int op;
	int viewer = PACKTER_FALSE;
	int snort = PACKTER_FALSE;

	/* setuid */
	struct passwd *pw;

	progname = argv[0];

	packter_init();

	/* getopt */
#ifdef USE_INET6
	while ((op = getopt(argc, argv, "v:i:r:p:R:T:f:u:g:Us6dh?")) != -1) 
#else
	while ((op = getopt(argc, argv, "v:i:r:p:R:T:f:u:g:Usdh?")) != -1) 
#endif
	{
		switch (op) {
		case 'f': /* packter flag base */
			packter_flagbase = atoi(optarg);
			break;

		case 'd': /* show debug */
			debug = PACKTER_TRUE;
			break;

#ifdef USE_INET6
		case '6':	/* use inet6 */
			use6 = PACKTER_TRUE;
			break;
#endif

		case 'v':	/* viewer ip */
			ip = optarg;
			viewer = PACKTER_TRUE;
			break;

		case 'p': /* viewer port */
			port = atoi(optarg);
			break;

		case 'i':	/* interface specified */
			device = optarg;
			break;

		case 'r':	/* read local files */
			dumpfile = optarg;
			break;

		case 's': /* sound enable */
			enable_sound = PACKTER_TRUE;
			break;

		case 'u': /* setuid */
			if ((pw = getpwnam(optarg)) == NULL){
				fprintf(stderr, "unknown username: %s\n", optarg);
				exit(EXIT_FAILURE);
			}
			if (setgid(pw->pw_gid) < 0){
				perror("setgid");
				exit(EXIT_FAILURE);
			}
			if (setuid(pw->pw_uid) < 0){
				perror("setuid");
				exit(EXIT_FAILURE);
			}
			break;

		case 'U': /* read from snort */
			snort = PACKTER_TRUE;
			break;

		case 'R': /* specifi rate_limit */
			rate_limit = atoi(optarg);
			break;

		case 'T': /* for traceback support */
			trace = PACKTER_TRUE;
			if (strlen(optarg) < 1){
				packter_usage();
			}
			strncpy(trace_server, optarg, PACKTER_BUFSIZ);
			break;	

		case 'h':
		case '?':	/* usage */
			packter_usage();
			break;
		}
	}

	if (argv[optind] != NULL){
		filter = argv[optind];
	}

	if (viewer != PACKTER_TRUE){
		packter_usage();
	}

	if (packter_flagbase < 0){
		packter_flagbase = 0;
	}

	if (rate_limit < 1){
		rate_limit = 1;
	}

	rate = packter_rate(rate_limit);

	if ((sock = pt_sock(ip, port, use6)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	if (snort == PACKTER_TRUE){
		packter_snort(dumpfile, device, filter);
	}
	else {
		packter_pcap(dumpfile, device, filter);
	}
	exit(EXIT_SUCCESS);
}

void
packter_usage(void)
{
	printf("usage: %s \n", progname);
	printf("      -v [ Viewer IP address ]\n");
	printf("      -p [ Viewer Port number ] (optional: default %d)\n", PACKTER_VIEWER_PORT);
	printf("      -i [ Monitor device ] (optional)\n");
	printf("      -r [ Pcap dump file ] (optional)\n");
	printf("      -f [ Flab base ] (optional: default 0)\n");
	printf("      -u [ Run as another username ] (optional)\n");
	printf("      -U ( Read from Snort's UNIX domain socket: optional)\n");
	printf("      -d ( Show debug information: optional)\n");
	printf("      -R [ Random droprate ] (optional)\n");
	printf("      -T [ Traceback Client ] (optional)\n");
	printf("      -s ( enable PACKERSE: optional)\n");
	printf("      [ pcap filter expression ] (optional)\n");
	printf("      (if -u specified, then [ UNIX domain socket path ]) \n");
	printf("\n");
	printf(" ex) %s -v 192.168.1.1 \"port not 11300 and port not 22\"\n", progname);
	printf("\n");

	exit(EXIT_SUCCESS);
}
