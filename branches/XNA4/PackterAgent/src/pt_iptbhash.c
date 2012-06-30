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
#include <netinet/in.h>
#include <arpa/inet.h>

#include "pt_std.h"
#include "pt_iptbhash.h"

#include "proto_ip.h"
#include "proto_ip6.h"

#include <openssl/md5.h>

void
generate_hash(unsigned char *packet, int caplen, char *mesgbuf)
{
	struct ip *ip;
	int hdrlen;
	ip = (struct ip *)packet;
	MD5_CTX context;
	unsigned char digest[16];

	hdrlen = ip->ip_hl * 4;
	if (hdrlen > IP_HDRLEN){
		memset((void *)(packet + IP_HDRLEN), 0, (hdrlen - IP_HDRLEN));
	}
	ip->ip_tos = 0x0;
	ip->ip_len = 0x0;
	ip->ip_off = 0x0;
	ip->ip_ttl = 0x0;
	ip->ip_sum = 0x0;

	MD5_Init(&context);
	MD5_Update(&context, (unsigned char *)packet, (unsigned long)(hdrlen + 8));
	MD5_Final(digest, &context);
	sprintf(mesgbuf, "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x",
		digest[0], digest[1], digest[2], digest[3],
		digest[4], digest[5], digest[6], digest[7],
		digest[8], digest[9], digest[10], digest[11],
		digest[12], digest[13], digest[14], digest[15]);

}

void generate_hash6(unsigned char *packet, int caplen, char *mesgbuf)
{
	struct ip6_hdr *ip6;
	ip6 = (struct ip6_hdr *)packet;
	ip6->ip6_vfc = ip6->ip6_vfc & 0xf0;
	ip6->ip6_hlim = 0x0;
	MD5_CTX context;
	unsigned char digest[16];

	MD5_Init(&context);
	MD5_Update(&context, (unsigned char *)ip6, (unsigned int)(IP6_HDRLEN + 8));
	MD5_Final(digest, &context);

	sprintf(mesgbuf, "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x",
		digest[0], digest[1], digest[2], digest[3],
		digest[4], digest[5], digest[6], digest[7],
		digest[8], digest[9], digest[10], digest[11],
		digest[12], digest[13], digest[14], digest[15]);
}
