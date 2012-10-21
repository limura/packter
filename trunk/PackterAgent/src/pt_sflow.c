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
#include "pt_sflow.h"

#include "pt_datalink.h"

#include "pt_sflow.h"

extern int debug;

int packter_sflow_read(char *buf, int len)
{
	int num;
	int exnum;
	int mesglen = 0;
	int samplelen = 0;
	int padding = 0;
	int i,j ;
	struct pcap_pkthdr *pt;
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

		pt = (struct pcap_pkthdr *)malloc(sizeof(struct pcap_pkthdr));
		if (pt == NULL){
			perror("malloc");
			exit(EXIT_FAILURE);
		}
		else {
			memset((void *)pt, '\0', sizeof(struct pcap_pkthdr));
		}

		gettimeofday(&(pt->ts), NULL); 
		pt->caplen = samplelen;
		pt->len = samplelen;

		packter_ether_callback(NULL, pt, (buf + mesglen));

		free(pt);

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
