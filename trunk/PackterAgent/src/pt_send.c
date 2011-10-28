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
#include "pt_snort.h"

extern int sock;
extern struct sockaddr_in addr;
#ifdef USE_INET6
extern struct sockaddr_in6 addr6;
#endif
extern int use6;

extern int debug;
extern int rate_limit;
extern int rate;

void packter_send(char *mesg)
{
	/* check the limit */
	if (rate != 1){
		rate -= 1;
		return;
	}

	rate = packter_rate(rate_limit);

	if (debug == PACKTER_TRUE){
		printf("%s", mesg);
	}

#ifdef USE_INET6
	if (use6 == PACKTER_TRUE){
		if (sendto(sock, mesg, strlen(mesg), 0,
					(struct sockaddr *)&addr6, sizeof(struct sockaddr_in6)) < 0){
			perror("sendto");
		}
	}
	else {
#endif

		if (sendto(sock, mesg, strlen(mesg), 0,
					(struct sockaddr *)&addr, sizeof(struct sockaddr)) < 0){
			perror("sendto");
		}

#ifdef USE_INET6
	}
#endif
	return;
}
