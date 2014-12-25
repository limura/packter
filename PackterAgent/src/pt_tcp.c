/*
 * Copyright (c) 2008-15 Project PACKTER. All rights reserved.
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
#include "pt_tcp.h"

#include "proto_tcp.h"

extern int enable_sound;
extern int trace;
extern int snort_report;

/* process tcp header */
void
packter_tcp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct tcphdr *th;
	char mesg[PACKTER_BUFSIZ];

	if (len < TCP_HDRLEN){
		return;
	}
	else {
		th = (struct tcphdr *)p;
	}

	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (th->th_flags & TH_SYN){
		flag += (PACKTER_TCP_SYN - PACKTER_TCP_ACK);	/* PACKTER_TCP_SYN(6) */
	}
	else if (th->th_flags & (TH_FIN|TH_RST)){
		flag += (PACKTER_TCP_FIN - PACKTER_TCP_ACK);	/* PACKTER_TCP_FIN(6) */
	}

	if (trace == PACKTER_FALSE && snort_report == PACKTER_FALSE){
		if (mesgbuf != NULL && strlen(mesgbuf) > 0){
			sprintf(mesgbuf, "\"%s\" TCP src:%s(%d) dst:%s(%d)",
							mesgbuf, srcip, ntohs(th->th_sport), dstip, ntohs(th->th_dport));
		}
		else {
			sprintf(mesgbuf, "TCP src:%s(%d) dst:%s(%d)",
							srcip, ntohs(th->th_sport), dstip, ntohs(th->th_dport));
		}
	}

	packter_mesg(mesg, srcip, dstip, ntohs(th->th_sport),ntohs(th->th_dport), flag, mesgbuf);
	packter_send(mesg);

	if (enable_sound == PACKTER_TRUE){
		char se[PACKTER_BUFSIZ];
		memset((void *)&se, '\0', PACKTER_BUFSIZ);
		snprintf(se, PACKTER_BUFSIZ, "%sse%d.wav", PACKTER_SE, flag);
		packter_send(se);
	}
	return;
}
