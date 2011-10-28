#ifndef __PACKTER_NETFLOW_H__
#define __PACKTER_NETFLOW_H__

#define PACKTER_NETFLOW_PORT	2055

#define PACKTER_NETFLOW_TEMPLATE_SET	0
#define PACKTER_NETFLOW_TEMPLATE_OPT	1

/* See RFC 3954 */

struct netflow_v9_header {
	u_int16_t version;
	u_int16_t counter;
	u_int32_t sysuptime;
	u_int32_t currenttime;
	u_int32_t sequence;
  u_int32_t srcid;
};

struct netflow_v9_template {
	u_int16_t id;
	u_int16_t len;
};

struct netflow_v9_field {
	u_int16_t type;
	u_int16_t len;
};

#define PACKTER_NETFLOW_IN_BYTES 1
#define PACKTER_NETFLOW_IN_PKTS	2
#define PACKTER_NETFLOW_IN_FLOWS 3
#define PACKTER_NETFLOW_IN_PROTOCOL 4
#define PACKTER_NETFLOW_IN_PROTOCOL_LEN 1
#define PACKTER_NETFLOW_IN_TOS 5
#define PACKTER_NETFLOW_IN_TOS_LEN 1
#define PACKTER_NETFLOW_TCP_FLAGS 6
#define PACKTER_NETFLOW_TCP_FLAGS_LEN 1
#define PACKTER_NETFLOW_L4_SRC_PORT 7
#define PACKTER_NETFLOW_L4_SRC_PORT_LEN 2
#define PACKTER_NETFLOW_IPV4_SRC_ADDR 8
#define PACKTER_NETFLOW_IPV4_SRC_ADDR_LEN 4
#define PACKTER_NETFLOW_L4_DST_PORT 11
#define PACKTER_NETFLOW_L4_DST_PORT_LEN 2
#define PACKTER_NETFLOW_IPV4_DST_ADDR 12
#define PACKTER_NETFLOW_IPV4_DST_ADDR_LEN 4
#define PACKTER_NETFLOW_IPV6_SRC_ADDR		27
#define PACKTER_NETFLOW_IPV6_SRC_ADDR_LEN		16
#define PACKTER_NETFLOW_IPV6_DST_ADDR		28
#define PACKTER_NETFLOW_IPV6_DST_ADDR_LEN		16
#define PACKTER_NETFLOW_ICMP_TYPE  32
#define PACKTER_NETFLOW_ICMP_TYPE_LEN  2
#define PACKTER_NETFLOW_IP_PROTOCOL_VERSION  60
#define PACKTER_NETFLOW_IP_PROTOCOL_VERSION_LEN  1

struct netflow_v9_pointer {
	int l3_src;
	int l3_dst;
	int l4_src;
	int l4_dst;
	int proto;
	int flag;
};

int packter_netflow(char *, int);
#endif
