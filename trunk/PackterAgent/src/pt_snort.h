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

#ifndef __PACKTER_SNORT_H__
#define __PACKTER_SNORT_H__

#ifndef SNORT_ALERT_MSG_LENGTH
#define SNORT_ALERT_MSG_LENGTH          256
#endif

#ifndef SNORT_ALERT_PKT_LENGTH
#define SNORT_ALERT_PKT_LENGTH          1500
#endif

#ifndef SUN_BUFSIZ
#define SUN_BUFSIZ 108
#endif

#ifndef SNORT_SNAPLEN
#define SNORT_SNAPLEN										1500
#endif

#ifndef SOCK_NAME
#define SOCK_NAME "/var/log/snort/snort_alert"
#endif

struct timeval32
{
    u_int32_t tv_sec;      /* seconds */
    u_int32_t tv_usec;     /* microseconds */
};

struct pcap_pkthdr32
{
    struct timeval32 ts;   /* packet timestamp */
    u_int32_t caplen;      /* packet capture length */
    u_int32_t pktlen;      /* packet "real" length */
};

typedef struct _Event
{
    u_int32_t sig_generator;   /* which part of snort generated the alert? */
    u_int32_t sig_id;          /* sig id for this generator */
    u_int32_t sig_rev;         /* sig revision for this id */
    u_int32_t classification;  /* event classification */
    u_int32_t priority;        /* event priority */
    u_int32_t event_id;        /* event ID */
    u_int32_t event_reference; /* reference to other events that have gone off,
                                * such as in the case of tagged packets...
                                */
    struct timeval32 ref_time;   /* reference time for the event reference */

    /* Don't add to this structure because this is the serialized data
     * struct for unified logging.
     */
} Event;

typedef struct _Alertpkt
{
    u_int8_t alertmsg[SNORT_ALERT_MSG_LENGTH]; /* variable.. */
    struct pcap_pkthdr pkth;
    u_int32_t dlthdr;       /* datalink header offset. (ethernet, etc.. ) */
    u_int32_t nethdr;       /* network header offset. (ip etc...) */
    u_int32_t transhdr;     /* transport header offset (tcp/udp/icmp ..) */
    u_int32_t data;
    u_int32_t val;  /* which fields are valid. (NULL could be
                    * valids also) */
    /* Packet struct --> was null */
#define NOPACKET_STRUCT 0x1
    /* no transport headers in packet */
#define NO_TRANSHDR    0x2
    u_int8_t pkt[SNORT_SNAPLEN];
    Event event;
} Alertpkt;

void packter_snort(char *, char *, char *);
extern int snort_report;
#endif
