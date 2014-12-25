/*
 * Copyright (c) 2008-15 Project PACKTER. All rights reserved.
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
#include <sys/types.h>

#ifdef USE_GEOIP
#include <GeoIP.h>
#include <GeoIPCity.h>
#endif

#include "pt_std.h"
#include "pt_geoip.h"

extern int debug;
extern char *geoip_datfile[];

static const char * _mk_NA( const char * p ){
	return p ? p : "N/A";
}

void packter_geoip(char *host, char *buf)
{
#ifdef USE_GEOIP
	GeoIP       *gi;
	GeoIPRecord *gir;
	const char  *time_zone = NULL;
	char        **ret;

	gi = GeoIP_open((const char *)geoip_datfile, GEOIP_INDEX_CACHE);
	gir = GeoIP_record_by_name(gi, host);
	if (gir == NULL) {
		printf("GeoIP record is NULL!\n");
		exit(PACKTER_FALSE);
	}
	ret = GeoIP_range_by_ip(gi, host);
	time_zone = GeoIP_time_zone_by_country_and_region(gir->country_code, gir->region);
	snprintf(buf, PACKTER_BUFSIZ, "%f,%f", gir->latitude, gir->longitude);
	GeoIP_range_by_ip_delete(ret);
	GeoIPRecord_delete(gir);
	GeoIP_delete(gi);
#endif
	return;
}

