/*
 * Copyright (c) 2011 Daisuke Miyamoto. All rights reserved.
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

extern GHashTable *config; 

void pt_hash_init()
{
	config = g_hash_table_new((GHashFunc)g_str_hash, (GCompareFunc)g_str_equal);
	return;
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

void
packter_addstring_hash(char *buf, char *key)
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

int
packter_is_exist_key(char *key)
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

void
packter_addstring(char *buf, char *val)
{
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);

	if (val != NULL){
			snprintf(tmp, PACKTER_BUFSIZ, "%s%s",
										buf, val);
			strncpy(buf, tmp, PACKTER_BUFSIZ);
	}
	return;
}

void
packter_addfloat(char *buf, float val)
{
	char tmp[PACKTER_BUFSIZ];
	memset((void *)tmp, '\0', PACKTER_BUFSIZ);

	snprintf(tmp, PACKTER_BUFSIZ, "%s %.f",
								buf,
								val);
	strncpy(buf, tmp, PACKTER_BUFSIZ);

	return;
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
