#                                               -*- Autoconf -*-
# Process this file with autoconf to produce a configure script.

AC_PREREQ(2.61)
#AC_INIT(pt-agent, 2.6.0, namaya2hashi@packter.jp)
#AM_INIT_AUTOMAKE($PACKAGE_NAME, $PACKAGE_VERSION)

AC_INIT([pt-agent], [2.6.0])
AC_CONFIG_SRCDIR([src/pt_agent.c])
AC_CONFIG_HEADER([config.h])
AM_INIT_AUTOMAKE


# Checks for programs.
#AC_PROG_CC
AM_PROG_CC_C_O
AC_PROG_INSTALL

# Checks for libraries.
AC_SUBST(PKG_CONFIG)
#PKG_CHECK_MODULES(GNET, gnet-2.0 > 2.0.0,
#  [LIBS="$LIBS $GNET_LIBS" CFLAGS="$CFLAGS $GNET_CFLAGS"],
#  AC_MSG_ERROR(Cannot find GNet: do you have the GNet development packages installed?))
PKG_CHECK_MODULES(GLIB, glib-2.0 > 2.0.0,
  [LIBS="$LIBS $GLIB_LIBS" CFLAGS="$CFLAGS $GLIB_CFLAGS"],
  AC_MSG_ERROR(Cannot find GLib: do you have the GLib development packages installed?))
AC_HAVE_LIBRARY(pcap,
  ,
  [AC_MSG_ERROR([Cannot find PCAP: do you have the PCAP development packages installed?])])
AC_HAVE_LIBRARY(ssl,
  ,
  [AC_MSG_ERROR([Cannot find SSL: do you have the SSL development packages installed?])])

AC_HAVE_LIBRARY(crypto,
  ,
  [AC_MSG_ERROR([Cannot find SSL: do you have the SSL development packages installed?])])

# Checks for header files.
AC_HEADER_STDC
AC_CHECK_HEADERS([arpa/inet.h fcntl.h netdb.h netinet/in.h stdlib.h string.h sys/socket.h sys/time.h sys/un.h unistd.h])

# Checks for typedefs, structures, and compiler characteristics.
AC_C_CONST
AC_TYPE_UINT32_T

# Checks for library functions.
AC_CHECK_FUNCS([memset socket])

AC_ARG_ENABLE(ipv6,
[  --enable-ipv6           enable ipv6 PACKTER viewer [default no]])
if test "$enable_ipv6" = "yes"; then
  CFLAGS="-DUSE_INET6 $CFLAGS"
fi

AC_ARG_ENABLE(geoip,
[  --enable-geoip          enable geoip support [default no]])
if test "$enable_geoip" = "yes"; then
	CFLAGS="-DUSE_GEOIP $CFLAGS"
	LIBS="$LIBS -lGeoIP"
fi

AC_CONFIG_FILES([Makefile
                 src/Makefile])
AC_OUTPUT
