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

#include "pt_sflow.h"

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

int enable_sound = PACKTER_FALSE;

struct pcap_pkthdr *pt;

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
	while ((op = getopt(argc, argv, "v:b:l:p:R:T:f:u:g:Us6dh?")) != -1)
#else
	while ((op = getopt(argc, argv, "v:b:l:p:R:T:f:u:g:Usdh?")) != -1)
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
					packter_sflow_usage();
				}
				strncpy(trace_server, optarg, PACKTER_BUFSIZ);
				break;

			case 'h': /* usage */
      case '?':
      default:
        packter_sflow_usage();
        exit(EXIT_SUCCESS);
		}
	}

	pt = (struct pcap_pkthdr *)malloc(sizeof(struct pcap_pkthdr));
	if (pt == NULL){
		perror("malloc");
		exit(EXIT_FAILURE);
	}
	else {
		memset((void *)pt, '\0', sizeof(struct pcap_pkthdr));
	}

  if (viewer != PACKTER_TRUE){
    packter_sflow_usage();
  }

  if (packter_flagbase < 0){
    packter_flagbase = 0;
  }

  if (rate_limit < 1){
    rate_limit = 1;
  }

  rate = packter_rate();

	if ((sock = pt_sock(ip, port, use6)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	if ((bind_sock = socket(PF_INET, SOCK_DGRAM, 0)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	memset((void *)&sflow_server, 0, sizeof(struct sockaddr_in));
  sflow_server.sin_family = AF_INET;
  sflow_server.sin_port = htons(sflow_bind_port);

	if (bind_set == PACKTER_TRUE){
		if (inet_pton(AF_INET, sflow_bind_addr, (void *)&(sflow_server.sin_addr.s_addr)) < 0){
      perror("inet_pton(4)");
    }
	}
	else {
    sflow_server.sin_addr.s_addr = htonl(INADDR_ANY);
  }

  if (bind(bind_sock, (struct sockaddr *)&sflow_server, sizeof(sflow_server)) < 0){
    perror("bind");
    return(PACKTER_TRUE);
  }

  for (;;) {
    memset((void *)&sflow_client, 0, sizeof(struct sockaddr_in));
    memset((void *)&buf, 0, sizeof(buf));
    recvlen = sizeof(struct sockaddr_in);

		if ((len = (int)recvfrom(bind_sock, (void *)buf, PACKTER_BUFSIZ, 0,
          (struct sockaddr *)&sflow_client, (socklen_t *)&recvlen)) < 1)
    {
      perror("recvfrom");
    }
		else {
			packter_sflow(buf, len);
		}
	}
}

void packter_sflow_usage()
{
	printf("usage: %s \n", progname);
	printf("      -v [ Viewer IP address ]\n");
	printf("      -p [ Viewer Port number ] (optional: default %d)\n", PACKTER_VIEWER_PORT);
	printf("      -b [ sFlow Bind IP address ] (optional: default 0.0.0.0)\n");
	printf("      -l [ sFlow Listen port number ] (optional: default %d)\n", PACKTER_SFLOW_PORT);
	printf("      -r [ Pcap dump file ] (optional)\n");
	printf("      -f [ Flab base ] (optional: default 0)\n");
	printf("      -u [ Run as another username ] (optional)\n");
	printf("      -R [ Random droprate ] (optional)\n");
	printf("      -T [ Traceback Client ] (optional)\n");
	printf("      -s ( enable PACKERSE: optional)\n");
  printf("\n");
  printf(" ex) %s -v 192.168.1.1 -b 192.168.1.1 -l 6343\n", progname);
  printf("\n");

	exit(EXIT_SUCCESS);
}

int packter_sflow(char *buf, int len)
{
	int num;
	int exnum;
	int mesglen = 0;
	int samplelen = 0;
	int padding = 0;
	int i,j ;
	struct sflow_v4_header *header;
	struct sflow_sample *sample;
	struct sflow_ex_num *ex_num;
	struct sflow_ex_type *ex_type;
	struct sflow_ex_gateway *ex_gateway;

	header = (struct sflow_v4_header *)buf;

	if (debug == PACKTER_TRUE){
		printf("version:%d:%d, seq:%d, uptime:%d, sample:%d\n",
						ntohl(header->version), ntohl(header->counter_version),
						ntohl(header->seq), ntohl(header->sysuptime),
						ntohl(header->numsamples));
	}
	num = ntohl(header->numsamples);
	mesglen += sizeof(struct sflow_v4_header);

	for (i = 0; i < num; i++){
		if (mesglen > len) break;

		sample = (struct sflow_sample *)(buf + mesglen);

		if (debug == PACKTER_TRUE){
			printf("sample:%d\n", (i + 1));
			printf("type:%d, seq:%d, id:%d, rate:%d, len:%d\n",
							ntohl(sample->type), ntohl(sample->seq), ntohl(sample->id_type),
							ntohl(sample->rate), ntohl(sample->len));
		}
		samplelen = ntohl(sample->len);	
		padding = 0;
		if (samplelen % 4 > 0){
			padding = 4 - (samplelen % 4); 
		}

		mesglen += sizeof(struct sflow_sample);

		gettimeofday(&(pt->ts), NULL); 
		pt->caplen = samplelen;
		pt->len = samplelen;

		packter_ether(NULL, pt, (buf + mesglen));

		mesglen += (samplelen + padding);
		
		if (mesglen > len) break;
		ex_num = (struct sflow_ex_num *)(buf + mesglen);
		if (debug == PACKTER_TRUE){
			printf("ex_num:%d\n", ntohl(ex_num->num));
		}

		exnum = ntohl(ex_num->num);
		mesglen += sizeof(struct sflow_ex_num);

		for (j = 0; j < exnum; j++){
			if (mesglen > len) break;
			ex_type = (struct sflow_ex_type *)(buf + mesglen);

			switch(ntohl(ex_type->type)){
				case PACKTER_SFLOW_SWITCH:
					mesglen += sizeof(struct sflow_ex_switch);
					break;

				case PACKTER_SFLOW_ROUTER:
					mesglen += sizeof(struct sflow_ex_router);
					break;

				case PACKTER_SFLOW_GATEWAY:
					ex_gateway = (struct sflow_ex_gateway *)(buf + mesglen);
					if (debug == PACKTER_TRUE){
						printf("Gateway\n");
						printf("Router:%d\n", ntohl(ex_gateway->as_router));	
						printf("Src:%d\n", ntohl(ex_gateway->as_src));	
						printf("Peer:%d\n", ntohl(ex_gateway->as_peer));	
						printf("Dst1:%d\n", ntohl(ex_gateway->as_dst1));	
					}
					mesglen += sizeof(struct sflow_ex_router);
					break;
			}	
		}
	}
	return;
}
