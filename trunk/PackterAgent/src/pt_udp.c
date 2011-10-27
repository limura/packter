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
#include "pt_agent.h"
#include "pt_ether.h"
#include "pt_ip.h"
#include "pt_ip6.h"
#include "pt_ipproto.h"
#include "pt_udp.h"

extern int enable_sound;
extern int trace;

/* process udp header */
void
packter_udp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct udphdr *uh;
	char mesg[PACKTER_BUFSIZ];

	if (len < UDP_HDRLEN){
		return;
	}
	else {
		uh = (struct udphdr *)p;
	}
	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (trace == PACKTER_FALSE){
		sprintf(mesgbuf, "UDP src:%s(%d) dst:%s(%d)",
						srcip, ntohs(uh->uh_sport), dstip, ntohs(uh->uh_dport));
	}

	packter_mesg(mesg, srcip, dstip, ntohs(uh->uh_sport), ntohs(uh->uh_dport), flag, mesgbuf);
	packter_send(mesg);

	if (enable_sound == PACKTER_TRUE){
		char se[PACKTER_BUFSIZ];
		memset((void *)&se, '\0', PACKTER_BUFSIZ);
		snprintf(se, PACKTER_BUFSIZ, "%sse%d.wav", PACKTER_SE, flag);
		packter_send(se);
	}
	return;
}
