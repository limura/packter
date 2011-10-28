/*
 * Copyright (c) 2010 Daisuke Miyamoto. All rights reserved.
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
#include <signal.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>
#include <arpa/inet.h>
#include <sys/time.h>
#include <netdb.h>

#include <pcap.h>
#include <glib.h>
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
#include "pt_thmon.h"
#include "pt_hash.h"

char *progname;
int debug = PACKTER_FALSE;

int sock;
struct sockaddr_in addr;

/* for IPv6 */
#ifdef USE_INET6
struct sockaddr_in6 addr6;
#endif
int use6 = PACKTER_FALSE;

/* for thresohold */
struct pt_threshold th;

/* interval */
int interval = PACKTER_INTVAL;

/* sound */
int enable_sound = PACKTER_FALSE;

/* config */
GHashTable *config = NULL;
char *configfile = NULL ;

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

	progname = argv[0];

	packter_init();

	/* getopt */
#ifdef USE_INET6
	while ((op = getopt(argc, argv, "v:i:r:p:S:F:R:I:U:P:C:w:c:s6dh?")) != -1) 
#else
	while ((op = getopt(argc, argv, "v:i:r:p:S:F:R:I:U:P:C:w:c:sdh?")) != -1) 
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

		/* Threshold */
		case 'S': /* SYN */
			th.rate_syn = atof(optarg);
			break;

		case 'F': /* FIN */
			th.rate_fin = atof(optarg);
			break;

		case 'R': /* RST */
			th.rate_rst = atof(optarg);
			break;

		case 'I': /* ICMP */
			th.rate_icmp = atof(optarg);
			break;

		case 'U': /* UDP */
			th.rate_udp = atof(optarg);
			break;

		case 'P': /* PPS */
			th.rate_pps = atof(optarg);
			break;

		case 'C': /* Count */
			th.count_max = atoi(optarg);
			break;
	
		case 'w': /* interval */
			interval = atoi(optarg);
			break;

		case 's': /* enable sound */
			enable_sound = PACKTER_TRUE;
			break;

		case 'c': /* config */
			configfile = optarg;
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

	if (packter_config_parse(configfile) < 0){
		packter_usage();
	}

	/* If no threshold specified */
	if (th.rate_syn < 0 && th.rate_fin < 0 && th.rate_rst < 0 &&
			th.rate_udp < 0 && th.rate_icmp < 0 && th.rate_pps < 0){
		printf("*** No threhold is givn : %s will not alert to viewer. ***\n", progname);
	}

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

	packter_pcap(dumpfile, device, filter);

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

	th.count_max = PACKTER_THCOUNT;
	th.rate_syn = -1;
	th.rate_fin = -1;
	th.rate_rst = -1;
	th.rate_icmp = -1;
	th.rate_udp = -1;
	th.rate_pps = -1;

	th.stop.tv_sec = 0;
	th.stop.tv_usec = 0;

	packter_count_init();

	config = g_hash_table_new((GHashFunc)g_str_hash, (GCompareFunc)g_str_equal);
	signal(SIGHUP, packter_sig_handler);

	return;
}

void packter_count_init()
{
	th.count_all = 0;
	th.count_syn = 0;
	th.count_fin = 0;
	th.count_rst = 0;
	th.count_icmp = 0;
	th.count_udp = 0;
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
	printf("      -p [ Viewer Port number ] (optional: default 11300)\n");
	printf("      -i [ Monitor device ] (optional)\n");
	printf("      -r [ Pcap dump file ] (optional)\n");
	printf("      -d ( Show debug information: optional)\n");
	printf("      -w [ Waint Interval ] (optional: default 30)\n");
	printf("      -c [ config file ] (optional: default %s)\n", PACKTER_THCONFIG);
	printf("      -s ( Enable Sound: optional: default no)\n", PACKTER_THCONFIG);
	printf("      -C [ Number of couting packet ] (optional: default 500)\n");
	printf("      -S [ TCP SYN Threshold ] (optional)\n");
	printf("      -F [ TCP FIN Threshold ] (optional)\n");
	printf("      -R [ TCP RST Threshold ] (optional)\n");
	printf("      -I [ ICMP Threshold ] (optional)\n");
	printf("      -U [ UDP Threshold ] (optional)\n");
	printf("      -P [ PPS Threshold ] (optional)\n");
	printf("      [ pcap filter expression ] (optional)\n");
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
			if (th.count_all == 0){
				gettimeofday(&th.start, NULL);
			}

			/* check interval */
			if (th.start.tv_sec - th.stop.tv_sec > interval){
				th.count_all = th.count_all + 1;
				packter_ip((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			}
			break;

		case ETHERTYPE_IPV6:
			if (th.count_all == 0){
				gettimeofday(&th.start, NULL);
			}
			/* check interval */
			if (th.start.tv_sec - th.stop.tv_sec > interval){
				th.count_all = th.count_all + 1;
				packter_ip6((u_char *)(p + skiplen), (u_int)(h->len - skiplen));
			}
			break;

		default:
			break;
	}
	/* after packter_ip() ends */

	if (th.count_all >= th.count_max){
		packter_analy();
		packter_count_init();
	}
	return;
}

void
packter_analy()
{
	int alert = PACKTER_FALSE;
	char mesg[PACKTER_BUFSIZ];
	char sound[PACKTER_BUFSIZ];
	char voice[PACKTER_BUFSIZ];
	char skydome[PACKTER_BUFSIZ];
	float diff = 0;
	float mon_syn, mon_fin, mon_rst, mon_icmp, mon_udp, mon_pps;

	/* Finalize Counter Variable */
	gettimeofday(&th.stop, NULL);
	mon_syn = (float)th.count_syn/(float)th.count_all;
	mon_fin = (float)th.count_fin/(float)th.count_all;
	mon_rst = (float)th.count_rst/(float)th.count_all;
	mon_icmp = (float)th.count_icmp/(float)th.count_all;
	mon_udp = (float)th.count_udp/(float)th.count_all;

	diff = (float)(th.stop.tv_sec - th.start.tv_sec) +
		(float)(th.stop.tv_usec - th.start.tv_usec)/(float)1000000;
	if (diff < 1){
		diff = 1;
	}
	mon_pps = (float)th.count_all/(float)diff;

	/* Initialize PACKTER Protocol Header */
	snprintf(mesg, PACKTER_BUFSIZ, "%s", PACKTER_MSG);
	snprintf(sound, PACKTER_BUFSIZ, "%s", PACKTER_SOUND);
	snprintf(voice, PACKTER_BUFSIZ, "%s", PACKTER_VOICE);
	snprintf(skydome, PACKTER_BUFSIZ, "%s", PACKTER_SKYDOME);

	printf ("-------------------------\n");
	printf ("Statistics of %d packet\n", th.count_all);
	printf ("Observed: %d.%d - %d.%d\n", th.start.tv_sec, th.start.tv_usec, th.stop.tv_sec, th.stop.tv_usec);
	printf ("SYN : %.4f   FIN : %.4f   RST: %.4f\n", mon_syn, mon_fin, mon_rst);
	printf ("ICMP: %.4f   UDP : %.4f   PPS: %.4f\n", mon_icmp, mon_udp, mon_pps);
	printf ("-------------------------\n");

	if (mon_syn > th.rate_syn && th.rate_syn > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_SYN_PIC", "MON_SYN_MSG", "MON_SYN_SOUND", "MON_SYN_VOICE",
											mon_syn * 100, th.rate_syn * 100);
	}

  if (mon_fin > th.rate_fin && th.rate_fin > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_FIN_PIC", "MON_FIN_MSG", "MON_FIN_SOUND", "MON_FIN_VOICE",
											mon_fin * 100, th.rate_fin * 100);
  }

  if (mon_rst > th.rate_rst && th.rate_rst > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_RST_PIC", "MON_RST_MSG", "MON_RST_SOUND", "MON_RST_VOICE",
											mon_rst * 100, th.rate_rst * 100);
  }

  if (mon_icmp > th.rate_icmp && th.rate_icmp > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_ICMP_PIC", "MON_ICMP_MSG", "MON_ICMP_SOUND", "MON_ICMP_VOICE",
											mon_icmp * 100, th.rate_icmp * 100);
	}

  if (mon_udp > th.rate_udp && th.rate_udp > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_UDP_PIC", "MON_UDP_MSG", "MON_UDP_SOUND", "MON_UDP_VOICE",
											mon_udp * 100, th.rate_udp * 100);
	}

  if (mon_pps > th.rate_pps && th.rate_pps > 0){
		alert = packter_generate_alert(alert, mesg, sound, voice,
											"MON_PPS_PIC", "MON_PPS_MSG", "MON_PPS_SOUND", "MON_PPS_VOICE",
											mon_pps, th.rate_pps);
	}

	if (alert == PACKTER_TRUE){
		if (packter_is_exist_key("MON_OPT_MSG_FOOT") == PACKTER_TRUE){
    	packter_addstring_hash(mesg, "MON_OPT_MSG_FOOT");
		}
		packter_send(mesg);

		if (enable_sound == PACKTER_TRUE){
			/* PACKTERSOUND uses no footer, but codes are prepared */
			if (packter_is_exist_key("MON_OPT_SOUND_FOOT") == PACKTER_TRUE){
				packter_addstring_hash(voice, "MON_OPT_SOUND");
			}
			packter_send(sound);

			/* PACKTERVOICE sometimes needs footer */
			if (packter_is_exist_key("MON_OPT_VOICE_FOOT") == PACKTER_TRUE){
				packter_addstring_hash(voice, "MON_OPT_VOICE_FOOT");
			}
			packter_send(voice);

			/* PACKTER_SKYDOME sends only skydome texture is specified */	
			if (packter_is_exist_key("MON_SKYDOME_START") == PACKTER_TRUE){
				packter_addstring_hash(skydome, "MON_SKYDOME_START");
				packter_send(skydome);
			}
		}
	}
	
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

	switch (ip->ip_p) {
		case IPPROTO_TCP:
			packter_tcp((u_char *)(p + ip->ip_hl * 4), (u_int)(len - ip->ip_hl*4),
										srcip, dstip, PACKTER_TCP_ACK, mesgbuf);
			break;

		case IPPROTO_UDP:
			th.count_udp = th.count_udp + 1;
			break;

		case IPPROTO_ICMP:
			th.count_icmp = th.count_icmp + 1;
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

	if (len < sizeof(struct ip6_hdr)){
		return;
	}
	else {
		ip6 = (struct ip6_hdr *)p;
	}

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
			th.count_udp = th.count_udp + 1;
			break;

		case IPPROTO_ICMPV6:
			th.count_icmp = th.count_icmp + 1;
			break;

		default:
			return;
	}
}


/* process tcp header */
void
packter_tcp(u_char *p, u_int len, char *srcip, char *dstip, int flag, char *mesgbuf)
{
	struct tcphdr *tcph;

	if (len < TCP_HDRLEN){
		return;
	}
	else {
		tcph = (struct tcphdr *)p;
	}


	if (tcph->th_flags & TH_SYN){
		th.count_syn = th.count_syn + 1;
	}

	if (tcph->th_flags & TH_FIN){
		th.count_fin = th.count_fin + 1;
	}

	if (tcph->th_flags & TH_RST){
		th.count_rst = th.count_rst + 1;
	}

	return;
}

void packter_send(char *mesg)
{
	/* check the limit */

	if (debug == PACKTER_TRUE){
		printf("%s\n", mesg);
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

int packter_config_trim(char buf[])
{
  char newbuf[PACKTER_BUFSIZ];
  int i, j, space, frontspace;

  /* init variables */
  frontspace = space = i = j = 0;

  for (i = 0; i < strlen(buf); i++){
    if (frontspace == 0 && (buf[i] == '\t' || buf[i] == ' ')){
        continue;
    }
    else {
      frontspace = 1;
    }

    switch(buf[i]){
      case '\r':
      case '\n':
        break;

      case '\t':
      case ' ':
        if (space == 0){
          newbuf[j++] = ' ';
          space = 1;
        }
        break;

      default:
        newbuf[j++] = buf[i];
        space = 0;
        break;
    }
  }
  newbuf[j] = '\0';

  /* copy */
  memcpy((void *)buf, (void *)newbuf, PACKTER_BUFSIZ);

  return j;
}

int packter_config_parse(char *configfile)
{
	FILE *fp;
	GHashTable *hash = NULL;
	char buf[PACKTER_BUFSIZ];
	char key[PACKTER_BUFSIZ];
	char val[PACKTER_BUFSIZ];
	char *tmp;

	if (configfile == NULL){
	 if ((fp = fopen(PACKTER_THCONFIG, "r")) == NULL){
		fprintf(stderr, "configuration file %s is not readable\n", PACKTER_THCONFIG);
			return PACKTER_FALSE;
		}
	}
	else {
	 if ((fp = fopen(configfile, "r")) == NULL){
		fprintf(stderr, "configuration file %s is not readable\n", configfile);
			return PACKTER_FALSE;
		}
	}

	while(fgets(buf, PACKTER_BUFSIZ, fp) != NULL){
    if (buf == NULL || strlen(buf) < 1 ){
      break;
    }
    if (buf[0] == '#'){
      continue;
    }
    if (packter_config_trim(buf) < 1){
      continue;
    }

    if ((tmp = strchr(buf, '=')) != NULL){
      *(tmp++) = '\0';
      strncpy(key, buf, PACKTER_BUFSIZ);
      strncpy(val, tmp, PACKTER_BUFSIZ);
			if (strlen(key) < 1 || strlen(val) < 1){
				continue;
			}
			if (g_hash_table_lookup(config, (gconstpointer)key) == NULL){
				g_hash_table_insert(config, g_strdup(key), g_strdup(val));
			}
    }
	}
	return PACKTER_TRUE;
}

int packter_generate_alert(int alert, char *mesg, char *sound, char *voice, char *mon_pic, char *mon_mesg, char *mon_sound, char *mon_voice, float mon_th, float given_th)
{
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);

  if (alert == PACKTER_FALSE){
		/*
		 * PACKTERMESG
		 * PIC,CONTENT(HEAD+BODY+FOOT)
		 */

		/* PIC for PACKTERMESG called at once */
		if (packter_is_exist_key(mon_pic) == PACKTER_TRUE){
			packter_addstring_hash(mesg, mon_pic);
		}
		packter_addstring(mesg, ",");
		/* Header for PACKTERMESG called at once */
		if (packter_is_exist_key("MON_OPT_MSG_HEAD") == PACKTER_TRUE){
    	packter_addstring_hash(mesg, "MON_OPT_MSG_HEAD");
		}

		/*
		 * PACKTERSOUND
		 * PLAYTIME,FILENAME(HEAD+BODY+FOOT)
		 */

		/* Header for PACKTERSOUND called at once */
		if (packter_is_exist_key("MON_OPT_SOUND_HEAD") == PACKTER_TRUE){
			packter_addstring_hash(voice, "MON_OPT_SOUND_HEAD");
		}
		/* PACKTERSOUND called at once */
		if (packter_is_exist_key(mon_sound) == PACKTER_TRUE){
			packter_addstring_hash(sound, mon_sound);
		}

		/*
		 * PACKTERVOICE
		 * CONTENT(HEAD+BODY+FOOT)
		 */		
		/* Header for PACKTERVOICE called at once */
		if (packter_is_exist_key("MON_OPT_VOICE_HEAD") == PACKTER_TRUE){
			packter_addstring_hash(voice, "MON_OPT_VOICE_HEAD");
		}

    alert = PACKTER_TRUE;
	}

	/* PACKTERMESG main content */
	if (packter_is_exist_key(mon_mesg) == PACKTER_TRUE){
		packter_addstring_hash(mesg, mon_mesg);
	}

	/* PACKTERMESG for Observed Variable */
	if (packter_is_exist_key("MONITOR") == PACKTER_TRUE){
		packter_addstring_hash(mesg, "MONITOR");
	}
	else {
		packter_addstring(mesg, " Observed:");
	}
	packter_addfloat(mesg, mon_th);

	/* PACKTERMESG for Thershold Variable */
	if (packter_is_exist_key("THRESHOLD") == PACKTER_TRUE){
		packter_addstring_hash(mesg, "THRESHOLD");
	}
	else {
		packter_addstring(mesg, " Threshold:");
	}
	packter_addfloat(mesg, given_th);

	/* PACKTERVOICE */
	if (packter_is_exist_key(mon_voice) == PACKTER_TRUE){
		packter_addstring_hash(voice, mon_voice);
	}
	return alert;
}

void packter_sig_handler(int sig){
	switch(sig){
		case SIGHUP:
			g_hash_table_destroy(config);			
			config = g_hash_table_new((GHashFunc)g_str_hash, (GCompareFunc)g_str_equal);
			if (packter_config_parse(configfile) < 0){
				printf("reload configfile failed\n");
			}
			else {
				printf("reload configfile succeeded\n");
			}
			break;
		default:
			break;
	}
	return;
}
