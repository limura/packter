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

#include <openssl/md5.h>

#include "pt_std.h"
#include "pt_sflow.h"
#include "pt_sflowd.h"

extern int debug;

void pt_sflowd(char *sflow_bind_addr, int sflow_bind_port, int bind_set)
{
	int bind_sock;
	int recvlen;
	struct sockaddr_in sflow_server;
	struct sockaddr_in sflow_client;
	int len;
	char buf[PACKTER_BUFSIZ];

	if ((bind_sock = socket(PF_INET, SOCK_DGRAM, 0)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	memset((void *)&sflow_server, 0, sizeof(struct sockaddr_in));
  sflow_server.sin_family = AF_INET;
  sflow_server.sin_port = htons(sflow_bind_port);

	if (bind_set == PACKTER_TRUE){
		if (inet_pton(AF_INET, sflow_bind_addr, (void *)&(sflow_server.sin_addr.s_addr)) < 0){
      perror("inet_pton(4)");
    }
	}
	else {
    sflow_server.sin_addr.s_addr = htonl(INADDR_ANY);
  }

  if (bind(bind_sock, (struct sockaddr *)&sflow_server, sizeof(sflow_server)) < 0){
    perror("bind");
    return(PACKTER_TRUE);
  }

  for (;;) {
    memset((void *)&sflow_client, 0, sizeof(struct sockaddr_in));
    memset((void *)&buf, 0, sizeof(buf));
    recvlen = sizeof(struct sockaddr_in);

		if ((len = (int)recvfrom(bind_sock, (void *)buf, PACKTER_BUFSIZ, 0,
          (struct sockaddr *)&sflow_client, (socklen_t *)&recvlen)) < 1)
    {
      perror("recvfrom");
    }
		else {
			packter_sflow_read(buf, len);
		}
	}
	return;
}
