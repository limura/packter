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
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>
#include <arpa/inet.h>
#include <sys/time.h>
#include <netdb.h>
#include <pwd.h>

#include <pcap.h>

#include <openssl/md5.h>

#include "pt_std.h"
#include "pt_replay.h"
#include "pt_init.h"
#include "pt_send.h"
#include "pt_util.h"

/* for debug */
int debug = PACKTER_FALSE;

char *progname;
int sock;
struct sockaddr_in addr;
int rate_limit;
int rate;
int notsend = PACKTER_FALSE;

/* for IPv6 */
#ifdef USE_INET6
struct sockaddr_in6 addr6;
#endif
int use6 = PACKTER_FALSE;


int main(int argc, char *argv[])
{
	/* pcap */
	char *textfile = NULL ;										/* pcap textfile */

	/* viewer */
	char *ip = NULL;
	int port = 0;

	/* misc */
	int op;
	int viewer = PACKTER_FALSE;
	int snort = PACKTER_FALSE;

	/* setuid */
	struct passwd *pw;

	progname = argv[0];

	setvbuf(stdout, NULL, _IONBF, PACKTER_BUFSIZ);
	packter_init();

	/* getopt */
#ifdef USE_INET6
	while ((op = getopt(argc, argv, "v:r:p:T:f:u:g:nUs6dh?")) != -1) 
#else
	while ((op = getopt(argc, argv, "v:r:p:T:f:u:g:nUsdh?")) != -1) 
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

		case 'r':	/* read local files */
			textfile = optarg;
			break;

		case 'u': /* setuid */
			if ((pw = getpwnam(optarg)) == NULL){
				fprintf(stderr, "unknown username: %s\n", optarg);
				exit(EXIT_FAILURE);
			}
			if (setgid(pw->pw_gid) < 0){
				perror("setgid");
				exit(EXIT_FAILURE);
			}
			if (setuid(pw->pw_uid) < 0){
				perror("setuid");
				exit(EXIT_FAILURE);
			}
			break;

		case 'h':
		case '?':	/* usage */
			packter_usage();
			break;
		}
	}

  rate_limit = 1;
  rate = packter_rate(rate_limit);

	if (viewer != PACKTER_TRUE){
		packter_usage();
	}

	if ((sock = pt_sock(ip, port, use6)) < 0){
		perror("socket");
		exit(EXIT_FAILURE);
	}

	packter_text(textfile);
	exit(EXIT_SUCCESS);
}

void
packter_usage(void)
{
	printf("usage: %s \n", progname);
	printf("      -v [ Viewer IP address ]\n");
	printf("      -p [ Viewer Port number ] (optional: default %d)\n", PACKTER_VIEWER_PORT);
	printf("      -r [ Text replay file ] (optional)\n");
	printf("      -u [ Run as another username ] (optional)\n");
	printf("      -d ( Show debug information: optional)\n");
	printf("      [ pcap filter expression ] (optional)\n");
	printf("      (if -u specified, then [ UNIX domain socket path ]) \n");
	printf("\n");
	printf(" ex) %s -v 192.168.1.1 -r replay_file\n", progname);
	printf("\n");

	exit(EXIT_SUCCESS);
}

void
packter_text(char *textfile)
{
	struct timeval now;
	struct timeval text;
	FILE *fp;
	char buf[PACKTER_BUFSIZ];
	char mesg[PACKTER_BUFSIZ];
	char *t1, *t2;
	long diff_sec, diff_usec;

	if ((fp = fopen(textfile, "r")) == NULL){
		fprintf(stderr, "Replay file not found\n");
		exit(EXIT_FAILURE);
	}

	if ((t1 = (char *)malloc(PACKTER_BUFSIZ)) == NULL){
		fprintf(stderr, "Malloc failed\n");
		exit(EXIT_FAILURE);
	}

	if ((t2 = (char *)malloc(PACKTER_BUFSIZ)) == NULL){
		fprintf(stderr, "Malloc failed\n");
		exit(EXIT_FAILURE);
	}

	now.tv_sec = 0;
	now.tv_usec = 0;
	memset((void *)mesg, '\0', PACKTER_BUFSIZ);				

	while (fgets(buf, PACKTER_BUFSIZ, fp) != NULL) {
		if (strcmp(buf, PACKTER_TIME) > 0){
			if (sscanf(buf, "TIME %s %s\n", t1, t2) == 2){
				if (now.tv_sec == 0 && now.tv_usec == 0){
					now.tv_sec = atol(t1);
					now.tv_usec = atol(t2);
				}
				else {
					text.tv_sec = atol(t1);
					text.tv_usec = atol(t2);

					diff_sec = packter_diff_sec(&text, &now);
					diff_usec = packter_diff_usec(&text, &now);
					packter_send(mesg);
					memset((void *)mesg, '\0', PACKTER_BUFSIZ);				

					now.tv_sec = text.tv_sec;
					now.tv_usec = text.tv_usec;
					usleep((int)(diff_sec * 1000 * 1000 + diff_usec));
				}
			}
		}
		else {
			strcat(mesg, buf);
		}
	}
	packter_send(mesg);
	fclose(fp);	

	return;

}
