#ifndef __PACKTER_SFLOW_H__
#define __PACKTER_SFLOW_H__

#define PACKTER_SFLOW_PORT	6343
#define PACKTER_SFLOW_SNAPLEN 128
#define PACKTER_SFLOW_MAXLEN  128 
#define PACKTER_SFLOW_VERSION 4
#define PACKTER_SFLOW_DEFAULT 1
#define PACKTER_SFLOW_EXTEND  3

#define PACKTER_SFLOW_SWITCH 1
#define PACKTER_SFLOW_ROUTER 2
#define PACKTER_SFLOW_GATEWAY 3

struct sflow_v4_header {
	u_int32_t	version;
	u_int32_t counter_version;
	struct in_addr agent_addr;
	/*	u_int32_t subid; */
	u_int32_t seq;
	u_int32_t sysuptime;
	u_int32_t numsamples;
};

struct sflow_v6_header {
	u_int32_t	version;
	u_int32_t counter_version;
	u_int32_t	agent_addr[4];
	u_int32_t subid;
	u_int32_t seq;
	u_int32_t sysuptime;
	u_int32_t numsamples;
};

/* See RFC 3176 */
struct sflow_sample {
	u_int32_t	type;			/* sFlow sample type */
	u_int32_t	seq;			/* sequence number */
	u_int32_t id_type;	/* Source ID class */
	u_int32_t rate;			/* sampling rate */
	u_int32_t pool;			/* sample pool */
	u_int32_t drop;			/* dropped packet */
	u_int32_t iif;			/* input interface */
	u_int32_t oif;			/* output interface */
	u_int32_t num;			/* output interface */
	u_int32_t next;			/* output interface */
	u_int32_t len;			/* output interface */
	u_int32_t len2;			/* output interface */
};

struct sflow_ex_num {
	uint32_t num;
};

struct sflow_ex_type {
  u_int32_t type;
};

struct sflow_ex_switch {
	u_int32_t type;
	u_int32_t in_vlan;
	u_int32_t in_priority;
	u_int32_t out_vlan;
	u_int32_t out_priority;
};

struct sflow_ex_router {
	u_int32_t type;
	u_int32_t num;
	struct in_addr nexthop;
	u_int32_t src_mask;
	u_int32_t dst_mask;
};

struct sflow_ex_gateway {
	u_int32_t type;
	u_int32_t as_router;
	u_int32_t as_src;
	u_int32_t as_peer;
	u_int32_t as_num;
	u_int32_t as_seq1;
	u_int32_t as_seq2;
	u_int32_t as_dst1;
	u_int32_t as_dst2;
	u_int32_t gateway_community;
	u_int32_t local_pref;
};

int packter_sflow(char *, int);
#endif
