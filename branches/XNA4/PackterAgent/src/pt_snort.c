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

#include "proto_ether.h"
#include "proto_snort.h"

extern int debug;

void packter_snort(char *dumpfile, char *device, char *filter)
{
	int	snort_sock;
	struct sockaddr_un un;
	Alertpkt alert;
	char sockname[SUN_BUFSIZ];

	/* if filter was set */
	if (filter != NULL){
		strncpy(sockname, filter, SUN_BUFSIZ);
	}
	else {
		strncpy(sockname, SOCK_NAME, SUN_BUFSIZ);
	}

	if ((snort_sock = socket(PF_UNIX, SOCK_DGRAM, 0)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	memset((void *)&un, 0, sizeof(struct sockaddr_un));
	un.sun_family = AF_UNIX;
	strncpy(un.sun_path , sockname, strlen(sockname));
	unlink(sockname);
	if (bind(snort_sock, (struct sockaddr *)&un, sizeof(un)) < 0){
		perror("bind");
		exit(EXIT_FAILURE);
	}

	/* Loop -> pcap read packets and excute packter_ether funcation */
	while ( 1 ) {
		if (recv(snort_sock, (void *)&alert, sizeof(alert), 0) < 0){
			perror("recv");
			continue;
		}
		else {
			if (debug == PACKTER_TRUE){
				printf("incident: %s\n", alert.alertmsg);
			}
			packter_ether(NULL, &(alert.pkth), (u_char *)alert.pkt);
		}
	}
}
