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

extern int debug;

/* packter pcap */
void
packter_pcap(char *dumpfile, char *device, char *filter)
{
	/* pcap */
	pcap_t *pd;														/* pcap descriptor */
	char errbuf	[PCAP_ERRBUF_SIZE];				/* errbuf for pcap */
	uint32_t localnet, netmask;						/* network for interface */
	pcap_handler	callback;								/* pcap callback function */
	int	datalink;													/* pcap datalink */

	/* pcap filter */
	struct bpf_program fcode;							/* compiled pcap filter */

	if (dumpfile != NULL) {
		/* pcap offline mode : read dump file */
		if ((pd = pcap_open_offline(dumpfile, errbuf)) == NULL) {
			fprintf(stderr, "pcap_open_offline: %s\n", errbuf);
			exit(EXIT_FAILURE);
		}
		localnet = 0;
		netmask = 0;
	}
	else {
		if (device == NULL) {
			if ((device = pcap_lookupdev(errbuf)) == NULL) {
				fprintf(stderr, "pcap_lookup: %s", errbuf);
				exit(EXIT_FAILURE);
			}
		}
		if (debug == PACKTER_TRUE){
			printf("device = %s\n", device);
		}

		/* open pcap with promisc mode */
		if ((pd = pcap_open_live(device, PACKTER_SNAPLEN, 1, 500, errbuf))
				== NULL) {
			fprintf(stderr, "pcap_open_live: %s\n", errbuf);
			exit(EXIT_FAILURE);
		}
		/* get netmask */
		if (pcap_lookupnet(device, &localnet, &netmask, errbuf) < 0) {
			fprintf(stderr, "pcap_lookupnet: %s\n", errbuf);
		}
	}

	if (pcap_compile(pd, &fcode, filter, 0, netmask) < 0) {
		fprintf(stderr, "pcap_compile: %s\n", pcap_geterr(pd));
		exit(EXIT_FAILURE);
	}

	/* set filter */
	if (pcap_setfilter(pd, &fcode) < 0) {
		fprintf(stderr, "pcap_setfilter: %s\n", pcap_geterr(pd));
		exit(EXIT_FAILURE);
	}
	/* get datalink type */
	if ((datalink = pcap_datalink(pd)) < 0) {
		fprintf(stderr, "pcap_datalink: %s\n", pcap_geterr(pd));
		exit(EXIT_FAILURE);
	}
	/* select callback function */
	switch (datalink) {
	case DLT_NULL:
		if (debug == PACKTER_TRUE){
			printf("linktype = LoopBack\n");
		}
		callback = packter_lback;
		break;

	case DLT_EN10MB:
		if (debug == PACKTER_TRUE){
			printf("linktype = Ethernet\n");
		}
		callback = packter_ether;
		break;

	default:
		fprintf(stderr, "linktype = change other link\n");
		exit(EXIT_FAILURE);
	}

	/* Loop -> pcap read packets and excute callback funcation */
	if (pcap_loop(pd, -1, callback, NULL) < 0) {
		fprintf(stderr, "pcap_loop: %s\n", pcap_geterr(pd));
		exit(EXIT_FAILURE);
	}
	pcap_close(pd);

	return; 
}
