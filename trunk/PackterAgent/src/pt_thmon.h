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

#include <sys/time.h>
#include <pcap.h>

#ifndef __PACKTER_THMON_H__
#define __PACKTER_THMON_H__

#define PACKTER_THCOUNT 500
#define PACKTER_INTVAL 30
#define PACKTER_THCONFIG "/usr/local/etc/packter.conf"

struct pt_threshold {
	/* Threshold Rate */
	float rate_syn;
	float rate_fin;
	float rate_rst;
	float rate_icmp;
	float rate_udp;
	float rate_pps;
	struct timeval start;
	struct timeval stop;

	/* Save variable */
	int count_max;
	int count_all;
	int count_syn;
	int count_fin;
	int count_rst;
	int count_icmp;
	int count_udp;

};

void packter_thmon_pcap(char *, char *, char *);
void packter_thmon_lback(u_char *, const struct pcap_pkthdr *, const u_char *);
void packter_thmon_ether(u_char *, const struct pcap_pkthdr *, const u_char *);
void packter_thmon_usage();
void packter_thmon_init();
void packter_thmon_count_init();
void packter_thmon_analy();
void packter_thmon_ip(u_char *, u_int);
void packter_thmon_ip6(u_char *, u_int);
void packter_thmon_tcp(u_char *, u_int, char *, char *, int, char *);
void packter_thmon_udp(u_char *, u_int, char *, char *, int, char *);
void packter_thmon_send(char *);

int packter_thmon_config_parse(char *);
int packter_thmon_config_trim(char *);
int packter_thmon_generate_alert(int, char *, char *, char *, char *, char *, char *, char *, float, float);
void packter_thmon_sig_handler(int);

#endif

