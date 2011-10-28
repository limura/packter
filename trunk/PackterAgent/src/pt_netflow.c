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
#include "pt_netflow.h"

extern int debug;

int packter_netflow_read(char *buf, int len)
{
	struct netflow_v9_header *nh;
	struct netflow_v9_template *nt;
	int srcid;
	int recvlen;

	/* obtain source id */
	if (len < sizeof(struct netflow_v9_header)){
		return;
	}
		
	nh = (struct netflow_v9_header *)buf;
	buf = buf + (sizeof(struct netflow_v9_header));
	len = len - (sizeof(struct netflow_v9_header));

	srcid = ntohl(nh->srcid);

	/* obtain flow set(s) */
	while (len > 0){
		if (len < sizeof(struct netflow_v9_template)){
			break;
		}
		nt = (struct netflow_v9_template *)buf;

		if (nt->id == PACKTER_NETFLOW_TEMPLATE_SET){
			packter_netflow_template_set(buf, len, srcid);		
		}

		buf += ntohs(nt->len);
		len -= ntohs(nt->len);
	}
	return;
}

int packter_netflow_template_set(const char *buf, const int len, int srcid) 
{
	struct netflow_v9_template *nt;
	struct netflow_v9_field *nf;

	int templateid;
	int fieldcount;
	int fieldlen;
	int i;

	int bufp = 0;

	bufp += sizeof(struct netflow_v9_template);

	if ((len - bufp) < sizeof(struct netflow_v9_template)){
		return PACKTER_FALSE;
	}

	nt = (struct netflow_v9_template *)(buf + bufp);
	bufp += sizeof(struct netflow_v9_template);

	templateid = ntohs(nt->id);
	fieldcount = ntohs(nt->len);

	if (debug == PACKTER_TRUE){
		printf("templadid: %d, fieldcount: %d\n", templateid, fieldcount);
	}

	fieldlen = 0;
	for (i = 0; i < fieldcount; i++){
		if ((len - bufp) < sizeof(struct netflow_v9_field)){
			break;
		}
		nf = (struct netflow_v9_field *)(buf + bufp);
		bufp += sizeof(struct netflow_v9_field);

		switch(ntohs(nf->type)){
			case PACKTER_NETFLOW_IPV4_SRC_ADDR:
				printf("src ip4: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_IPV4_DST_ADDR:
				printf("dst ip4: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_IPV6_SRC_ADDR:
				printf("dst ip6: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_IPV6_DST_ADDR:
				printf("dst ip6: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_L4_SRC_PORT:
				printf("src port: %d\n", fieldlen);
				break;
	
			case PACKTER_NETFLOW_L4_DST_PORT:
				printf("dst port: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_ICMP_TYPE:
				printf("icmp: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_IN_PROTOCOL:
				printf("proto: %d\n", fieldlen);
				break;

			case PACKTER_NETFLOW_IP_PROTOCOL_VERSION:
				printf("ver:%d\n", fieldlen);
				break;

		}
		fieldlen += ntohs(nf->len);
		if (debug == PACKTER_TRUE){
			printf("%d:%d,%d,%d\n", i, templateid, ntohs(nf->type), ntohs(nf->len));
		}
		
	}
}
