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

#include <glib.h>

#include "pt_std.h"
#include "pt_netflow.h"

#include "proto_tcp.h"
#include "proto_ipproto.h"

extern int debug;
extern GHashTable *config;

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

		if (ntohs(nt->id) == PACKTER_NETFLOW_TEMPLATE_SET){
			packter_netflow_template_set(buf, len, srcid);		
		}
		else if (ntohs(nt->id) >= 256){
			packter_netflow_template(buf, len, srcid);
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
	struct netflow_v9_pointer *np;
	char key[PACKTER_BUFSIZ];

	u_short templateid;
	int fieldcount;
	int fieldlen;
	int i;

	int bufp = 0;

	/* SET : (template id) (length) (template id) (length) ... */

	bufp += sizeof(struct netflow_v9_template);

	if ((len - bufp) < sizeof(struct netflow_v9_template)){
		return PACKTER_FALSE;
	}

	nt = (struct netflow_v9_template *)(buf + bufp);
	bufp += sizeof(struct netflow_v9_template);

	templateid = ntohs(nt->id);
	fieldcount = ntohs(nt->len);

	snprintf(key, PACKTER_BUFSIZ, "%d-%d", srcid, templateid);
	if (debug == PACKTER_TRUE){
		printf("SET templateid:%d, srcid:%d\n", templateid, srcid);
	}

	if (packter_is_exist_key(key) == PACKTER_TRUE){
		np = (struct netflow_v9_pointer *)g_hash_table_lookup(config, key);
	}
	else {
		np = (struct netflow_v9_pointer *)malloc(sizeof(struct netflow_v9_pointer));
		if (np == NULL){
			perror("malloc");
			exit(EXIT_FAILURE);
		}
		/* memset((void *)np, '\0', sizeof(struct netflow_v9_pointer)); */
		packter_netflow_pointer_init(np);
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
				np->l3_src = fieldlen;
				break;

			case PACKTER_NETFLOW_IPV4_DST_ADDR:
				np->l3_dst = fieldlen;
				break;

			case PACKTER_NETFLOW_IPV6_SRC_ADDR:
				np->l3_src = fieldlen;
				np->isv6 = PACKTER_TRUE;
				break;

			case PACKTER_NETFLOW_IPV6_DST_ADDR:
				np->l3_dst = fieldlen;
				np->isv6 = PACKTER_TRUE;
				break;

			case PACKTER_NETFLOW_L4_SRC_PORT:
				np->l4_src = fieldlen;
				break;
	
			case PACKTER_NETFLOW_L4_DST_PORT:
				np->l4_dst = fieldlen;
				break;

			case PACKTER_NETFLOW_ICMP_TYPE:
				np->icmp = fieldlen;
				break;

			case PACKTER_NETFLOW_IN_PROTOCOL:
				np->proto = fieldlen;
				break;

			case PACKTER_NETFLOW_TCP_FLAGS:
				np->tcp_flags = fieldlen;
				break;

			default:
				break;
		}
		fieldlen += ntohs(nf->len);
	
		if (g_hash_table_lookup(config, (gconstpointer)key) == NULL){
			g_hash_table_insert(config, g_strdup(key), (gpointer)np);
		}
	
	}
}

int packter_netflow_pointer_init(struct netflow_v9_pointer *np)
{
	np->l3_src = PACKTER_FALSE;
  np->l3_dst = PACKTER_FALSE;
  np->l4_src = PACKTER_FALSE;
  np->l4_dst = PACKTER_FALSE;
  np->isv6 = PACKTER_FALSE;
  np->proto = PACKTER_FALSE;
  np->icmp = PACKTER_FALSE;
  np->tcp_flags = PACKTER_FALSE;
}
			
int packter_netflow_template(const char *buf, const int len, int srcid)
{
	struct netflow_v9_template *nt;
	struct netflow_v9_pointer *np;
	char key[PACKTER_BUFSIZ];

	char srcip[PACKTER_BUFSIZ];
	char dstip[PACKTER_BUFSIZ];
	int srcport;
	int dstport;
	int flag;
	short proto;
	short tcp_flags;
	int isv6;
	u_short templateid;

	char mesg[PACKTER_BUFSIZ];
	char mesgbuf[PACKTER_BUFSIZ];
	int bufp = 0;

	/* GET : (template id) (length)  ... */

	if (len < sizeof(struct netflow_v9_template)){
		return PACKTER_FALSE;
	}

	nt = (struct netflow_v9_template *)buf;
	bufp += sizeof(struct netflow_v9_template);

	templateid = ntohs(nt->id);

	snprintf(key, PACKTER_BUFSIZ, "%d-%d", srcid, templateid);
	if (debug == PACKTER_TRUE){
		printf("GET templateid:%d, srcid:%d\n", templateid, srcid);
	}

	if (packter_is_exist_key(key) == PACKTER_FALSE){
		/* this is uknown till format received */
		return PACKTER_FALSE;
	}

	np = (struct netflow_v9_pointer *)g_hash_table_lookup(config, key);

	if (np->l3_src == PACKTER_FALSE || np->l3_dst == PACKTER_FALSE 
			|| np->proto == PACKTER_FALSE){
		printf("unknown netflow\n");
		return;
	}

	/* get ipaddrss */
	isv6 = np->isv6;
	if (isv6 == PACKTER_TRUE){
		if (inet_ntop(AF_INET6,
									(void *)(buf + bufp + np->l3_src),
									srcip, PACKTER_BUFSIZ) == NULL)
		{
			perror("inet_ntop");
			return;
		}

		if (inet_ntop(AF_INET6,
									(void *)(buf + bufp + np->l3_dst),
									dstip, PACKTER_BUFSIZ) == NULL)
		{
			perror("inet_ntop");
			return;
		}
	}
	else {
		if (inet_ntop(AF_INET,
									(void *)(buf + bufp + np->l3_src),
									srcip, PACKTER_BUFSIZ) == NULL)
		{
			perror("inet_ntop");
			return;
		}
		if (inet_ntop(AF_INET,
									(void *)(buf + bufp + np->l3_dst),
									dstip, PACKTER_BUFSIZ) == NULL)
		{
			perror("inet_ntop");
			return;
		}
	}

	srcport = 0; dstport = 0; flag = 0;
	tcp_flags = 0; proto = 0;

	/* get protocol type */
	memcpy((void *)&proto,
				(void *)(buf + bufp + np->proto),
				PACKTER_NETFLOW_IN_PROTOCOL_LEN);

	switch(proto){
			case IPPROTO_TCP:
				/* tcp port */
				if (np->l4_src == PACKTER_FALSE || np->l4_dst == PACKTER_FALSE){
					return;
				}
				memcpy((void *)&srcport,
							(void *)(buf + bufp + np->l4_src),
							PACKTER_NETFLOW_L4_SRC_PORT_LEN);

				memcpy((void *)&dstport,
							(void *)(buf + bufp + np->l4_dst),
							PACKTER_NETFLOW_L4_SRC_PORT_LEN);

				/* tcp flags */
				if (isv6 == PACKTER_TRUE){
					flag = PACKTER_TCP_ACK6;
				}
				else {
					flag = PACKTER_TCP_ACK;
				}

				if (np->tcp_flags == PACKTER_TRUE){
					memcpy((void *)&tcp_flags,
							(void *)(buf + bufp + np->tcp_flags),
							PACKTER_NETFLOW_TCP_FLAGS_LEN); 

					if (tcp_flags != 0){
						if (tcp_flags & TH_SYN){
							flag += (PACKTER_TCP_SYN - PACKTER_TCP_ACK);
						}
						else if (tcp_flags & (TH_FIN|TH_RST)){
    					flag += (PACKTER_TCP_FIN - PACKTER_TCP_ACK);
						}
					}
				}
				sprintf(mesgbuf, "TCP src:%s(%d) dst:%s(%d)",
								srcip, ntohs(srcport), dstip, ntohs(dstport));
				break;

			/* UDP */
			case IPPROTO_UDP:
				if (np->l4_src == PACKTER_FALSE || np->l4_dst == PACKTER_FALSE){
					return;
				}
				memcpy((void *)&srcport,
							(void *)(buf + bufp + np->l4_src),
							PACKTER_NETFLOW_L4_SRC_PORT_LEN);

				memcpy((void *)&dstport,
							(void *)(buf + bufp + np->l4_dst),
							PACKTER_NETFLOW_L4_SRC_PORT_LEN);

				if (isv6 == PACKTER_TRUE){
					flag = PACKTER_UDP6;
				}
				else {
					flag = PACKTER_UDP;
				}
				sprintf(mesgbuf, "UDP src:%s(%d) dst:%s(%d)",
								srcip, ntohs(srcport), dstip, ntohs(dstport));
				break;

			case IPPROTO_ICMP:	
				if (np->icmp == PACKTER_FALSE){
					return;
				}
				memcpy((void *)&srcport,
							(void *)(buf + bufp + np->icmp),
							PACKTER_NETFLOW_ICMP_TYPE_LEN);
				dstport = srcport;
				flag = PACKTER_ICMP;
				sprintf(mesgbuf, "ICMPv4 src:%s dst:%s (type:%d code:%d)",
								srcip, dstip, (ntohs(srcport) / 256), (ntohs(srcport) % 256));
				break;

			case IPPROTO_ICMPV6:	
				if (np->icmp == PACKTER_FALSE){
					return;
				}
				memcpy((void *)&srcport,
							(void *)(buf + bufp + np->icmp),
							PACKTER_NETFLOW_ICMP_TYPE_LEN);
				dstport = srcport;
				flag = PACKTER_ICMP6;
				sprintf(mesgbuf, "ICMPv6 src:%s dst:%s (type:%d code:%d)",
								srcip, dstip, (ntohs(srcport) / 256), (ntohs(srcport) % 256));
				break;

			default:
				if (debug == PACKTER_TRUE){
					printf("unknown protocol: %d\n", proto);
				}
				return;
	}

	packter_mesg(mesg, srcip, dstip, ntohs(srcport),ntohs(dstport), flag, mesgbuf);
	packter_send(mesg);
}
