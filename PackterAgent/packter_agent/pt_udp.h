#ifndef __PACKTER_UDP_H__
#define __PACKTER_UDP_H__

struct udphdr {
    u_int16_t   uh_sport;       /* source port */
    u_int16_t   uh_dport;       /* destination port */
    u_int16_t   uh_ulen;        /* udp length */
    u_int16_t   uh_sum;         /* udp checksum */
};

#endif
