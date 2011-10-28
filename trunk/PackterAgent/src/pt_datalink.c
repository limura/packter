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

#include <openssl/md5.h>

#include "pt_std.h"
#include "pt_datalink.h"

#include "proto_ether.h"
#include "proto_ip.h"
#include "proto_ip6.h"
#include "proto_ipproto.h"

/* process Loop Back */
void
packter_lback(u_char * userdata, const struct pcap_pkthdr *h, const u_char * p)
{
	/* paranoia NULL check */
	if (userdata == NULL || h == NULL || p == NULL)
		return;
	/* if capture size is too short */
	if (h->caplen < NULL_HDRLEN)
		return;
	else
		packter_ip((u_char *)(p + NULL_HDRLEN), (u_int)(h->len - NULL_HDRLEN));
	return;
}

/* process IEEE 802.3 Ethernet */
void
packter_ether(u_char * userdata, const struct pcap_pkthdr *h, const u_char * p)
{
	struct ether_header *ep;
	u_int	ether_type;
	int skiplen = ETHER_HDRLEN;

	/* if capture size is too short */
	if (h->caplen < ETHER_HDRLEN){
		return;
	}
	else {
		ep = (struct ether_header *)p;
	}
	ether_type = ntohs(ep->ether_type);

	if (ether_type == ETHERTYPE_8021Q){
		ep = (struct ether_header *)(p + 4);
		ether_type = ntohs(ep->ether_type);
		skiplen += 4;
	}

	switch(ether_type){
		case ETHERTYPE_IP:
			packter_ip((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			break;

		case ETHERTYPE_IPV6:
			packter_ip6((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			break;

		default:
			break;
	}
	/* after packter_ip() ends */
	return;
}
