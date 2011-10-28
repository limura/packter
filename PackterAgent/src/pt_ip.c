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

#include <pcap.h>

#include "pt_std.h"
#include "pt_ip.h"

#include "proto_ip.h"
#include "proto_ipproto.h"

extern int trace;

/* process ip header */
void
packter_ip(u_char * p, u_int len)
{
	struct ip			*ip;
	char srcip[PACKTER_BUFSIZ];
	char dstip[PACKTER_BUFSIZ];
	char mesgbuf[PACKTER_BUFSIZ];
	u_char *packet;

	/* if ip is too short */
	if (len < sizeof(struct ip))
		return;
	else
		ip = (struct ip *)p;

	/* if not ipv4 or not tcp or udp */
	if (ip->ip_v != IPVERSION){
		return;
	}

	memset((void *)&srcip, '\0', PACKTER_BUFSIZ);
	memset((void *)&dstip, '\0', PACKTER_BUFSIZ);
	inet_ntop(AF_INET, (void *)(&ip->ip_src), srcip, PACKTER_BUFSIZ);
	inet_ntop(AF_INET, (void *)(&ip->ip_dst), dstip, PACKTER_BUFSIZ);

	/* packter trace */
	if (trace == PACKTER_TRUE){
		/* calculate other hash */
		if ((packet = (u_char *)malloc(len)) == NULL){
			fprintf(stderr, "malloc failed\n");
			return;
		}	
		memcpy((void *)packet, (void *)p, len);
		generate_hash(packet, len, mesgbuf);
		free(packet);
	}

	switch (ip->ip_p) {
		case IPPROTO_TCP:
			packter_tcp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_TCP_ACK, mesgbuf);
			break;

		case IPPROTO_UDP:
			packter_udp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_UDP, mesgbuf);
			break;

		case IPPROTO_ICMP:
			packter_icmp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_ICMP, mesgbuf);
			break;

		default:
			return;
	}
	return;
}
