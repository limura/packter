/*
 * Copyright (c) 2008-15 Project PACKTER. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 *
 */

#ifndef __PACKTER_H__
#define __PACKTER_H__

#define PACKTER_SNAPLEN	128
#define PACKTER_BUFSIZ	1024
#define PACKTER_BUFSIZ_LONG	16384
#define PACKTER_VIEWER_PORT 11300
#define PACKTER_TC_PORT 11301
#define PACKTER_HEADER	"PACKTER\n"
#define PACKTER_MSG	"PACKTERMSG\n"
#define PACKTER_SOUND	"PACKTERSOUND\n"
#define PACKTER_SE	"PACKTERSE\n"
#define PACKTER_VOICE	"PACKTERVOICE\n"
#define PACKTER_SKYDOME	"PACKTERSKYDOMETEXTURE\n"
#define PACKTER_EARTH	"PACKTEARTH\n"
#define PACKTER_TCP_ACK	0
#define PACKTER_TCP_SYN	1
#define PACKTER_TCP_FIN	2
#define PACKTER_UDP			3
#define PACKTER_ICMP		4
#define PACKTER_TCP_ACK6	5
#define PACKTER_TCP_SYN6	6
#define PACKTER_TCP_FIN6	7
#define PACKTER_UDP6			8
#define PACKTER_ICMP6			9

#define PACKTER_TRUE		1
#define PACKTER_FALSE		-1

#ifndef NULL_HDRLEN
#define NULL_HDRLEN	4
#endif

#ifndef ETHER_HDRLEN
#define ETHER_HDRLEN    14
#endif

#ifndef IP_HDRLEN
#define IP_HDRLEN				20
#endif

#ifndef IP6_HDRLEN
#define IP6_HDRLEN			40
#endif

#ifndef TCP_HDRLEN
#define TCP_HDRLEN			20
#endif

#ifndef UDP_HDRLEN			
#define UDP_HDRLEN			8
#endif

#ifndef ICMP_MIN_HDRLEN			/* 8 bit for ICMP type, 8 bit for ICMP code */
#define ICMP_MIN_HDRLEN	2
#endif

#endif
