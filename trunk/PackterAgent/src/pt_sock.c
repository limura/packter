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
#include <pwd.h>

#include <pcap.h>

#include <openssl/md5.h>

#include "pt_std.h"
#include "pt_agent.h"

extern struct sockaddr_in addr;

/* for IPv6 */
#ifdef USE_INET6
extern struct sockaddr_in6 addr6;
#endif

extern int debug;

int pt_sock(char *ip, int port, int use6){
	int sock;
#ifdef USE_INET6
	if (use6 == PACKTER_TRUE){
		if ((sock = socket(PF_INET6, SOCK_DGRAM, 0)) < 0){
			perror("socket");
			exit(EXIT_FAILURE);
		}
		if (inet_pton(AF_INET6, ip, (void *)&(addr6.sin6_addr)) < 0){
				perror("inet_pton");
				exit(EXIT_FAILURE);
		}
		if (port > 0){
			addr6.sin6_port = htons(port);
		}
		else {
			addr6.sin6_port = htons(PACKTER_VIEWER_PORT);
		}
	}
	else {
#endif
		if ((sock = socket(PF_INET, SOCK_DGRAM, 0)) < 0){
			perror("socket");
			exit(EXIT_FAILURE);
		}
		if (inet_pton(AF_INET, ip, (void *)&(addr.sin_addr)) < 0){
				perror("inet_pton");
				exit(EXIT_FAILURE);
		}
		if (port > 0){
			addr.sin_port = htons(port);
		}
		else {
			addr.sin_port = htons(PACKTER_VIEWER_PORT);
		}
#ifdef USE_INET6
	}
#endif

	return sock;
}
