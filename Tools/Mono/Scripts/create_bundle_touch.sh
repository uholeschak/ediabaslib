#!/bin/sh
# Mono paths
export MONO="/cygdrive/c/Programs/Mono4_0_1"
export PATH="$PATH:$MONO/bin"
export PKG_CONFIG_PATH="$MONO/lib/pkgconfig"
machineconfig="./info1.xml"
# Compiler
export CC="i686-pc-mingw32-gcc -U _WIN32"

mkbundle --deps -z --machine-config $machineconfig -o temp.c -oo bundles.o -c ./mtouch-win.exe ./Mono.Touch.Activation.Common.dll
i686-pc-mingw32-gcc -U _WIN32 -s -o mtouch.exe -Wall temp.c -Wl,-Bstatic `pkg-config --cflags --libs monosgen-2|dos2unix` -lmswsock -lz bundles.o -mconsole
rm temp.c
rm bundles.o
