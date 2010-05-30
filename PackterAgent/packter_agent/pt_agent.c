/*
 * Copyright (c) 2008 Daisuke Miyamoto. All rights reserved.
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

#include <gnet.h>
#include <openssl/md5.h>

#include "pt_ether.h"
#include "pt_ip.h"
#include "pt_ip6.h"
#include "pt_ipproto.h"
#include "pt_icmp.h"
#include "pt_icmp6.h"
#include "pt_tcp.h"
#include "pt_udp.h"
#include "pt_snort.h"
#include "pt.h"

static char *progname;
static int debug = PACKTER_FALSE;

static int sock;
static struct sockaddr_in addr;
static int rate_limit;
static int rate;

/* for IPv6 */
#ifdef USE_INET6
static struct sockaddr_in6 addr6;
#endif
int use6 = PACKTER_FALSE;

/* for InterTrack */
char trace_server[PACKTER_BUFSIZ];
int trace = PACKTER_FALSE;


int main(int argc, char *argv[])
{
	/* pcap */
	char *dumpfile = NULL ;										/* pcap dumpfile */
	char *device = NULL;											/* interface */
	char *filter = NULL;											/* pcap filter */

	/* viewer */
	char *ip = NULL;
	int port = 0;

	/* misc */
	int op;
	int viewer = PACKTER_FALSE;
	int snort = PACKTER_FALSE;

	progname = argv[0];

	packter_init();

	/* getopt */
#ifdef USE_INET6
	while ((op = getopt(argc, argv, "v:i:r:p:R:T:s6dh?")) != -1) 
#else
	while ((op = getopt(argc, argv, "v:i:r:p:R:T:sdh?")) != -1) 
#endif
	{
		switch (op) {
		case 'd': /* show debug */
			debug = PACKTER_TRUE;
			break;

#ifdef USE_INET6
		case '6':	/* use inet6 */
			use6 = PACKTER_TRUE;
			break;
#endif

		case 'v':	/* viewer ip */
			ip = optarg;
			viewer = PACKTER_TRUE;
			break;

		case 'p': /* viewer port */
			port = atoi(optarg);
			break;

		case 'i':	/* interface specified */
			device = optarg;
			break;

		case 'r':	/* read local files */
			dumpfile = optarg;
			break;

		case 's': /* read from snort */
			snort = PACKTER_TRUE;
			break;

		case 'R': /* specifi rate_limit */
			rate_limit = atoi(optarg);
			break;

		case 'T': /* for traceback support */
			trace = PACKTER_TRUE;
			if (strlen(optarg) < 1){
				packter_usage();
			}
			strncpy(trace_server, optarg, PACKTER_BUFSIZ);
			break;	

		case 'h':
		case '?':	/* usage */
			packter_usage();
			break;
		}
	}

	if (argv[optind] != NULL){
		filter = argv[optind];
	}

	if (viewer != PACKTER_TRUE){
		packter_usage();
	}

	if (rate_limit < 1){
		rate_limit = 1;
	}
	rate = packter_rate();


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

	if (snort == PACKTER_TRUE){
		packter_snort(dumpfile, device, filter);
	}
	else {
		packter_pcap(dumpfile, device, filter);
	}

	exit(EXIT_SUCCESS);
}

void packter_init()
{

	memset((void *)&addr, 0, sizeof(struct sockaddr_in));
	addr.sin_family = AF_INET;

#ifdef USE_INET6 
	memset((void *)&addr6, 0, sizeof(struct sockaddr_in6));
	addr6.sin6_family = AF_INET6;
#endif

	srand((unsigned)time(NULL));
	rate_limit = 1;
	memset((void *)&trace_server, '\0', sizeof(trace_server)); 
	return;
}

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

void packter_pcap(char *dumpfile, char *device, char *filter)
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

void
packter_usage(void)
{
	printf("usage: %s \n", progname);
	printf("      -v [ Viewer IP address ]\n");
	printf("      -p [ Viewer Port number ] (optiona: default 11300)\n");
	printf("      -i [ Monitor device ] (optional)\n");
	printf("      -r [ Pcap dump file ] (optional)\n");
	printf("      -s ( Read from Snort's UNIX domain socket: optional)\n");
	printf("      -d ( Show debug information: optional)\n");
	printf("      -R [ Random droprate ] (optional)\n");
	printf("      -T [ Traceback Client ] (optional)\n");
	printf("      [ pcap filter expression ] (optional)\n");
	printf("      (if -s specified, then [ UNIX domain socket path ]) \n");
	printf("\n");
	printf(" ex) %s -v 192.168.1.1 \"port not 11300 and port not 22\"\n", progname);
	printf("\n");

	exit(EXIT_SUCCESS);
}

/* process Loop Back */
void
packter_lback(u_char * userdata, const struct pcap_pkthdr *h, const u_char * p)
{
	/* paranoia NULL check */
	if (userdata == NULL || h == NULL || p == NULL)
		return;
	/* if capture size is too short */
	if (h->caplen < NULL_HDRLEN)
		return;
	else
		packter_ip((u_char *)(p + NULL_HDRLEN), (u_int)(h->len - NULL_HDRLEN));
	return;
}

/* process IEEE 802.3 Ethernet */
void
packter_ether(u_char * userdata, const struct pcap_pkthdr *h, const u_char * p)
{
	struct ether_header *ep;
	u_int	ether_type;
	int skiplen = ETHER_HDRLEN;

	/* if capture size is too short */
	if (h->caplen < ETHER_HDRLEN)
		return;
	else
		ep = (struct ether_header *)p;

	ether_type = ntohs(ep->ether_type);

	if (ether_type == ETHERTYPE_8021Q){
		ep = (struct ether_header *)(p + 4);
		ether_type = ntohs(ep->ether_type);
		skiplen += 4;
	}

	switch(ether_type){
		case ETHERTYPE_IP:
			packter_ip((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			break;

		case ETHERTYPE_IPV6:
			packter_ip6((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			break;

		default:
			break;
	}
	/* after packter_ip() ends */
	return;
}

/* process ip header */
void
packter_ip(u_char * p, u_int len)
{
	struct ip			*ip;
	char srcip[PACKTER_BUFSIZ];
	char dstip[PACKTER_BUFSIZ];
	char mesgbuf[PACKTER_BUFSIZ];
	u_char *packet;

	/* if ip is too short */
	if (len < sizeof(struct ip))
		return;
	else
		ip = (struct ip *)p;

	/* if not ipv4 or not tcp or udp */
	if (ip->ip_v != IPVERSION){
		return;
	}

	memset((void *)&srcip, '\0', PACKTER_BUFSIZ);
	memset((void *)&dstip, '\0', PACKTER_BUFSIZ);
	inet_ntop(AF_INET, (void *)(&ip->ip_src), srcip, PACKTER_BUFSIZ);
	inet_ntop(AF_INET, (void *)(&ip->ip_dst), dstip, PACKTER_BUFSIZ);

	/* packter trace */
	if (trace == PACKTER_TRUE){
		/* calculate other hash */
		if ((packet = (u_char *)malloc(len)) == NULL){
			fprintf(stderr, "malloc failed\n");
			return;
		}	
		memcpy((void *)packet, (void *)p, len);
		generate_hash(packet, len, mesgbuf);
		free(packet);
	}

	switch (ip->ip_p) {
		case IPPROTO_TCP:
			packter_tcp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_TCP_ACK, mesgbuf);
			break;

		case IPPROTO_UDP:
			packter_udp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_UDP, mesgbuf);
			break;

		case IPPROTO_ICMP:
			packter_icmp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_ICMP, mesgbuf);
			break;

		default:
			return;
	}
	return;
}

/* process ipv6 header */
void
packter_ip6(u_char * p, u_int len)
{
	struct ip6_hdr *ip6;
	char srcip[PACKTER_BUFSIZ];
	char dstip[PACKTER_BUFSIZ];
	char mesgbuf[PACKTER_BUFSIZ];
	u_char *packet;

	if (len < sizeof(struct ip6_hdr)){
		return;
	}
	else {
		ip6 = (struct ip6_hdr *)p;
	}

	/* calculate other hash */
	if ((packet = (u_char *)malloc(len)) == NULL){
		fprintf(stderr, "malloc failed\n");
		return;
	}
	memcpy((void *)packet, (void *)p, len);
	generate_hash6(packet, len, mesgbuf);
	free(packet);

	memset((void *)&srcip, '\0', PACKTER_BUFSIZ);
	memset((void *)&dstip, '\0', PACKTER_BUFSIZ);
	inet_ntop(AF_INET6, (void *)(&ip6->ip6_src), srcip, PACKTER_BUFSIZ);
	inet_ntop(AF_INET6, (void *)(&ip6->ip6_dst), dstip, PACKTER_BUFSIZ);

	switch(ip6->ip6_nxt){
		case IPPROTO_TCP:
			packter_tcp((u_char *)(p + ntohs(ip6->ip6_plen)),
										(u_int)(len - ntohs(ip6->ip6_plen)),
										srcip, dstip, PACKTER_TCP_ACK6, mesgbuf);
			break;

		case IPPROTO_UDP:
			packter_udp((u_char *)(p + ntohs(ip6->ip6_plen)),
										(u_int)(len - ntohs(ip6->ip6_plen)),
										srcip, dstip, PACKTER_UDP6, mesgbuf);
			break;

		case IPPROTO_ICMPV6:
			packter_icmp6((u_char *)(p + ntohs(ip6->ip6_plen)),
										(u_int)(len - ntohs(ip6->ip6_plen)),
										srcip, dstip, PACKTER_ICMP6, mesgbuf);
			break;

		default:
			return;
	}
}


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

	if (trace == PACKTER_FALSE){
		sprintf(mesgbuf, "TCP src:%s(%d) dst:%s(%d)",
						srcip, ntohs(th->th_sport), dstip, ntohs(th->th_dport));
	}

	packter_mesg(mesg, srcip, dstip, ntohs(th->th_sport),ntohs(th->th_dport), flag, mesgbuf);
	packter_send(mesg);

	return;
}

/* process udp header */
void
packter_udp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct udphdr *uh;
	char mesg[PACKTER_BUFSIZ];

	if (len < UDP_HDRLEN){
		return;
	}
	else {
		uh = (struct udphdr *)p;
	}
	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (trace == PACKTER_FALSE){
		sprintf(mesgbuf, "UDP src:%s(%d) dst:%s(%d)",
						srcip, ntohs(uh->uh_sport), dstip, ntohs(uh->uh_dport));
	}

	packter_mesg(mesg, srcip, dstip, ntohs(uh->uh_sport), ntohs(uh->uh_dport), flag, mesgbuf);
	packter_send(mesg);
	return;
}

/* process icmp header */
void
packter_icmp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct icmphdr *ih;
	char mesg[PACKTER_BUFSIZ];
	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (len < ICMP_MIN_HDRLEN){
		return;
	}
	else {
		ih = (struct icmphdr *)p;
	}

	if (trace == PACKTER_FALSE){
		sprintf(mesgbuf, "ICMPv4 src:%s dst:%s (type:%d code:%d)",
						srcip, dstip, (ih->icmp_type), (ih->icmp_code));
	}

	packter_mesg(mesg, srcip, dstip, (ih->icmp_type) * 256, (ih->icmp_code) * 256, flag, mesgbuf);
	packter_send(mesg);
	return;
}

/* process icmp6 header */
void packter_icmp6(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct icmp6hdr *ih6;
	char mesg[PACKTER_BUFSIZ];
	memset((void *)&mesg, '\0', PACKTER_BUFSIZ);

	if (len < ICMP_MIN_HDRLEN){
		return;
	}
	else {
		ih6 = (struct icmp6hdr *)p;
	}

	if (trace == PACKTER_FALSE){
		sprintf(mesgbuf, "ICMPv6 src:%s dst:%s (type:%d code:%d)",
						srcip, dstip, (ih6->icmp6_type), (ih6->icmp6_code));
	}

	packter_mesg(mesg, srcip, dstip,
							(int)(ih6->icmp6_type) * 256, 
							(int)(ih6->icmp6_code) * 256,
							flag, mesgbuf);
	packter_send(mesg);
	return;
}

void packter_send(char *mesg)
{
	/* check the limit */
	if (rate != 1){
		rate -= 1;
		return;
	}

	rate = packter_rate();

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

int packter_rate()
{
	if (rate_limit < 1){
		return 1;
	}
	else {
		return( rand() % rate_limit + 1 );
	}
}

void packter_mesg(char *mesg, char *srcip, char *dstip, int data1, int data2, int flag, char *mesgbuf)
{
	if (trace == PACKTER_TRUE){
		snprintf(mesg, PACKTER_BUFSIZ, "%s%s,%s,%d,%d,%d,%s-%s\n",
						PACKTER_HEADER,
						srcip,
						dstip,
						data1,
						data2,
						flag,
						mesgbuf,
						trace_server					
		);
	}
	else {
		snprintf(mesg, PACKTER_BUFSIZ, "%s%s,%s,%d,%d,%d,%s\n",
						PACKTER_HEADER,
						srcip,
						dstip,
						data1,
						data2,
						flag,
						mesgbuf
		);
	}
}
