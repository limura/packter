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

#include "pt_std.h"

extern int debug;

int packter_rate(int rate_limit)
{
	if (rate_limit < 1){
		return 1;
	}
	else {
		return( rand() % rate_limit + 1 );
	}
}

void packter_addstring(char *buf, char *val)
{
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);
	if (val != NULL){
		snprintf(tmp, PACKTER_BUFSIZ, "%s%s", buf, val);
	 	strncpy(buf, tmp, PACKTER_BUFSIZ);
	}
  return;
}

void packter_addfloat(char *buf, float val)
{
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);
	snprintf(tmp, PACKTER_BUFSIZ, "%s %.f", buf, val);
  strncpy(buf, tmp, PACKTER_BUFSIZ);
  return;
}

u_long
packter_diff_sec(struct timeval *a, struct timeval *b)
{
	u_long sec = 0;
	if (a->tv_sec < b->tv_sec){
		sec = 0;
	}
	else if (a->tv_sec == b->tv_sec){
		if (a->tv_usec <= b->tv_usec){
			sec = 0;
		}
    else {
      sec = a->tv_sec - b->tv_sec;
    }
  }
  else {
    if (a->tv_usec <= b->tv_usec){
      sec = a->tv_sec - b->tv_sec - 1;
    }
    else {
      sec = a->tv_sec - b->tv_sec;
    }
  }
  return sec;
}

u_long
packter_diff_usec(struct timeval *a, struct timeval *b)
{
	u_long usec = 0;
  if (a->tv_sec < b->tv_sec){
		usec = 0;
	}
	else if (a->tv_sec == b->tv_sec){
		if (a->tv_usec <= b->tv_usec){
			usec = 0;
		}
		else {
			usec = a->tv_usec - b->tv_usec;
		}
	}
  else {
		if (a->tv_usec <= b->tv_usec){
			usec = 1000 * 1000 + a->tv_usec - b->tv_usec;
		}
		else {
			usec = a->tv_usec - b->tv_usec;
		}
	}
  return usec;
}