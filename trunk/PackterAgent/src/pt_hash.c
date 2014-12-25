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
#include <signal.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>
#include <arpa/inet.h>
#include <sys/time.h>
#include <netdb.h>

#include <glib.h>

#include "pt_std.h"
#include "pt_hash.h"

extern GHashTable *config; 

void packter_hash_new()
{
	config = g_hash_table_new((GHashFunc)g_str_hash, (GCompareFunc)g_str_equal);
	return;
}

void *packter_destroy_func(void *data) {
  GHashTable *cf_hash;
  if (data == NULL){
    return;
  }
  else {
    cf_hash = (GHashTable *)data;
    g_hash_table_foreach(cf_hash, packter_free_hash, NULL);
    g_hash_table_destroy(cf_hash);
  }
  return;
}

void packter_free_hash(gpointer key, gpointer value, gpointer user_data)
{
  if (value != NULL){
    g_free(value);
  }
  g_free(key);
  return;
}

void packter_addstring_hash(char *buf, char *key)
{
	char *val;
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);

	if (key != NULL){
		val = (char *)g_hash_table_lookup(config, key);
		if (val != NULL && strlen(val) > 0){
				snprintf(tmp, PACKTER_BUFSIZ, "%s%s",
										buf, val);
				strncpy(buf, tmp, PACKTER_BUFSIZ);
		}
	}
}

int packter_is_exist_key_str(char *key)
{
	int ret = PACKTER_FALSE;
	char *val;

	do {
		if (key == NULL){
			break;
		}
		val = (char *)g_hash_table_lookup(config, key);
		if (val == NULL){
			break;
		}
		if (strlen(val) < 1){
			break;
		}
		ret = PACKTER_TRUE;
	} while(0);
	return ret;
}

int packter_is_exist_key(char *key)
{
	int ret = PACKTER_FALSE;
	char *val;

	do {
		if (key == NULL){
			break;
		}
		val = (char *)g_hash_table_lookup(config, key);
		if (val == NULL){
			break;
		}
		ret = PACKTER_TRUE;
	} while(0);
	return ret;
}
