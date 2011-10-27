/*
 * Copyright (c) 2008 Daisuke Miyamoto. All rights reserved.
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

#ifndef __PACKTER_AGENT_H__
#define __PACKTER_AGENT_H__

void packter_init();
void packter_pcap(char *, char *, char *);
void packter_snort(char *, char *, char *);
void packter_usage(void);
void packter_lback(u_char *, const struct pcap_pkthdr *, const u_char *);
void packter_ether(u_char *, const struct pcap_pkthdr *, const u_char *);
void packter_ip(u_char *, u_int);
void packter_ip6(u_char *, u_int);
void packter_tcp(u_char *, u_int, char *, char *, int, char *);
void packter_udp(u_char *, u_int, char *, char *, int, char *);
void packter_icmp(u_char *, u_int, char *, char *, int, char *);
void packter_icmp6(u_char *, u_int, char *, char *, int, char *);
void packter_mesg(char *, char *, char *, int, int, int, char *);
int packter_rate();
void packter_send(char *);

void generate_hash(u_char *, int, char *);
void generate_hash6(u_char *, int, char *);

#endif

