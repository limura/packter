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

#include "pt_std.h"
#include "pt_pcap.h"

#include "pt_sflow.h"
#include "pt_sflowd.h"
#include "pt_sflow_agent.h"

/* for debug */
int debug = PACKTER_FALSE;

char *progname;
int sock;
struct sockaddr_in addr;
int rate_limit;
int rate;
int notsend = PACKTER_FALSE;


/* for IPv6 */
#ifdef USE_INET6
struct sockaddr_in6 addr6;
#endif
int use6 = PACKTER_FALSE;

/* for InterTrack */
char trace_server[PACKTER_BUFSIZ];
int trace = PACKTER_FALSE;

/* for GeoIP */
int geoip = PACKTER_FALSE;
char geoip_datfile[PACKTER_BUFSIZ];

int packter_flagbase = 0;
int snort_report = PACKTER_FALSE;
int enable_sound = PACKTER_FALSE;

int main(int argc, char *argv[])
{
	/* sflow server */
	int bind_sock;
	int bind_set = PACKTER_FALSE;
	char *sflow_bind_addr;
	int sflow_bind_port = PACKTER_SFLOW_PORT;
	struct sockaddr_in sflow_server;

	/* sflow clinent */
	struct sockaddr_in sflow_client;
	socklen_t recvlen;

	/* misc */
	int op;
  int len;
	char buf[PACKTER_BUFSIZ];
	int i;

  /* viewer */
  char *ip = NULL;
  int port = 0;

  /* packter */
  int viewer = PACKTER_FALSE;

	/* uid */
	struct passwd *pw;


  setvbuf(stdout, 0, _IONBF, 0);
  len = 0;
	progname = argv[0];

	packter_init();

	/* getopt */
#ifdef USE_INET6
	while ((op = getopt(argc, argv, "v:b:l:p:R:T:G:f:u:ns6dh?")) != -1)
#else
	while ((op = getopt(argc, argv, "v:b:l:p:R:T:G:f:u:nsdh?")) != -1)
#endif
	{
		switch(op){
			case 'b':
				sflow_bind_addr = optarg;
				bind_set = PACKTER_TRUE;
				break;

			case 'l':
				sflow_bind_port = atoi(optarg);
				break;

			case 'f': /* packter flag base */
				packter_flagbase = atoi(optarg);
				break;
	
			case 'd':
				debug = PACKTER_TRUE;
				break;
	
#ifdef USE_INET6
			case '6': /* use inet6 */
				use6 = PACKTER_TRUE;
				break;
#endif

			case 'v': /* viewer ip */
				ip = optarg;
				viewer = PACKTER_TRUE;
				break;

			case 'p': /* viewer port */
				port = atoi(optarg);
				break;

			case 's': /* sound enable */
				enable_sound = PACKTER_TRUE;
				break;

			case 'n': /* not send */
				notsend = PACKTER_TRUE;
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

			case 'R': /* specifi rate_limit */
				rate_limit = atoi(optarg);
				break;

			case 'T': /* for traceback support */
				trace = PACKTER_TRUE;
				if (strlen(optarg) < 1){
					packter_sflow_agent_usage();
				}
				strncpy(trace_server, optarg, PACKTER_BUFSIZ);
				break;

			case 'G': /* enable GeoIP */
				geoip = PACKTER_TRUE;
				if (strlen(optarg) < 1){
					packter_sflow_agent_usage();
				}
				strncpy(geoip_datfile, optarg, PACKTER_BUFSIZ);
				break;

			case 'h': /* usage */
      case '?':
      default:
        packter_sflow_agent_usage();
        exit(EXIT_SUCCESS);
		}
	}

  if (viewer != PACKTER_TRUE){
    packter_sflow_agent_usage();
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

	/* slow daemon start : loop */
	pt_sflowd(sflow_bind_addr, sflow_bind_port, bind_set);

	exit(EXIT_FAILURE);
}

void packter_sflow_agent_usage()
{
	printf("usage: %s \n", progname);
	printf("      -v [ Viewer IP address ]\n");
	printf("      -p [ Viewer Port number ] (optional: default %d)\n", PACKTER_VIEWER_PORT);
	printf("      -b [ sFlow Bind IP address ] (optional: default 0.0.0.0)\n");
	printf("      -l [ sFlow Listen port number ] (optional: default %d)\n", PACKTER_SFLOW_PORT);
	printf("      -u [ Run as another username ] (optional)\n");
	printf("      -f [ Flag base ] (optional: default 0)\n");
	printf("      -R [ Random droprate ] (optional)\n");
	printf("      -T [ Traceback Client ] (optional)\n");
	printf("      -G [ GeoLiteCity datafile ] (optional)\n");
  printf("\n");
  printf(" ex) %s -v 192.168.1.1 -b 192.168.1.2 -l 6343\n", progname);
  printf("\n");

	exit(EXIT_SUCCESS);
}
