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
#include <time.h>
#include <sys/types.h>
#include <sys/socket.h>

#include "pt_std.h"
#include "pt_mesg.h"
#include "pt_geoip.h"

extern int trace;
extern char *trace_server[];
extern int packter_flagbase;
extern int geoip;

void packter_mesg(char *mesg, char *srcip, char *dstip, int data1, int data2, int flag, char *mesgbuf)
{

#ifdef USE_GEOIP
	if (geoip == PACKTER_TRUE){
		char srcgeo[PACKTER_BUFSIZ];
		char dstgeo[PACKTER_BUFSIZ];

		if (packter_geoip(srcip, srcgeo) == PACKTER_FALSE ||
				packter_geoip(dstip, dstgeo) == PACKTER_FALSE)
		{
			return;
		}
		if (trace == PACKTER_TRUE){
			snprintf(mesg, PACKTER_BUFSIZ, "%s%s,%s,%d,%s-%s\n",
						PACKTER_EARTH,
						srcgeo,
						dstgeo,
						(flag + packter_flagbase),
						mesgbuf,
						trace_server					
			);
		}
		else {
			snprintf(mesg, PACKTER_BUFSIZ, "%s%s,%s,%d,%s\n",
						PACKTER_EARTH,
						srcgeo,
						dstgeo,
						(flag + packter_flagbase),
						mesgbuf
			);
		}
		return;	
	}
#endif

	if (trace == PACKTER_TRUE){
		snprintf(mesg, PACKTER_BUFSIZ, "%s%s,%s,%d,%d,%d,%s-%s\n",
						PACKTER_HEADER,
						srcip,
						dstip,
						data1,
						data2,
						(flag + packter_flagbase),
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
						(flag + packter_flagbase),
						mesgbuf
		);
	}
	return;
}
