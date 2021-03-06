AC_INIT(last-exit, 0.2)
AC_CONFIG_SRCDIR(src/Driver.cs)

AM_CONFIG_HEADER(config.h)

AM_INIT_AUTOMAKE([1.9 foreign dist-bzip2 no-dist-gzip])

AM_MAINTAINER_MODE

AC_PROG_INTLTOOL([0.21])

AC_PROG_CC
AC_ISC_POSIX
AC_HEADER_STDC
AM_PROG_LIBTOOL

GSTREAMER_REQUIRED=0.10.0
GTK_REQUIRED=2.6
MONO_REQUIRED=1.0
GTKSHARP_REQUIRED=1.9.2

PKG_CHECK_MODULES(LASTEXIT,
		  gconf-2.0 \
		  gdk-pixbuf-2.0 \
		  gtk+-2.0 >= $GTK_REQUIRED \
		  gstreamer-0.10 >= $GSTREAMER_REQUIRED \
		  gstreamer-base-0.10			\
		  gstreamer-plugins-base-0.10 >= $GSTREAMER_REQUIRED)
AC_SUBST(LASTEXIT_CFLAGS)
AC_SUBST(LASTEXIT_LIBS)

PKG_CHECK_MODULES(GTKSHARP,
		  gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
		  gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)

AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

AC_PATH_PROG(MCS, mcs)
if test x$MCS = x; then
	AC_MSG_ERROR(You need mcs)
fi

AC_PATH_PROG(MONO, mono)
if test x$MONO = x; then
	AC_MSG_ERROR(You need mono)
fi

MCS_FLAGS="-codepage:utf8"
MONO_FLAGS=
if test $USE_MAINTAINER_MODE = yes; then
	MCS_FLAGS="$MCS_FLAGS -debug"
	MONO_FLAGS="$MONO_FLAGS --debug"
fi
AC_SUBST(MCS_FLAGS)
AC_SUBST(MONO_FLAGS)

AC_PATH_PROG(GACUTIL, gacutil)
if test x$GACUTIL = x; then
	AC_MSG_ERROR(You need gacutil)
fi

GACUTIL_FLAGS='/package lastexit /gacdir $(libdir) /root $(DESTDIR)$(libdir)'
AC_SUBST(GACUTIL_FLAGS)

#ALL_LINGUAS=
#GETTEXT_PACKAGE=lastexit
#AC_SUBST(GETTEXT_PACKAGE)
#AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [Gettext package])
#AM_GLIB_GNU_GETTEXT

AC_OUTPUT([
Makefile
data/Makefile
data/glade/Makefile
data/icons/Makefile
data/ui/Makefile
liblast-exit/Makefile
src/Makefile
])
