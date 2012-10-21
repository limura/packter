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
#include "pt_icmp.h"

#include "proto_ip.h"
#include "proto_icmp.h"

extern int enable_sound;
extern int trace;
extern int snort_report;

/* process icmp header */
void
packter_icmp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct icmphdr *ih;
	char mesg[PACKTER_BUFSIZ];
	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (len < ICMP_MIN_HDRLEN){
		return;
	}
	else {
		ih = (struct icmphdr *)p;
	}

	if (trace == PACKTER_FALSE && snort_report == PACKTER_FALSE){
		sprintf(mesgbuf, "ICMPv4 src:%s dst:%s (type:%d code:%d)",
						srcip, dstip, (ih->icmp_type), (ih->icmp_code));
	}

	packter_mesg(mesg, srcip, dstip, (ih->icmp_type) * 256, (ih->icmp_code) * 256, flag, mesgbuf);
	packter_send(mesg);

	if (enable_sound == PACKTER_TRUE){
		char se[PACKTER_BUFSIZ];
		memset((void *)&se, '\0', PACKTER_BUFSIZ);
		snprintf(se, PACKTER_BUFSIZ, "%sse%d.wav", PACKTER_SE, flag);
		packter_send(se);
	}

	return;
}
