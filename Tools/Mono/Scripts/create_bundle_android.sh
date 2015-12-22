#!/bin/sh
# Mono paths
export MONO="/cygdrive/c/Programs/Mono4_0_1"
export PATH="$PATH:$MONO/bin"
export PKG_CONFIG_PATH="$MONO/lib/pkgconfig"
machineconfig="./info1.xml"
# Compiler
export CC="i686-pc-mingw32-gcc -U _WIN32"

#mkbundle --deps -z --machine-config $machineconfig -o temp.c -oo bundles.o -c ./mandroid-win.exe ./Xamarin.Android.Cecil.dll ./Xamarin.Android.Cecil.Mdb.dll ./Mono.Touch.Client.dll ./Mono.Touch.Common.dll ./Ionic.Zip.dll
mkbundle --deps -z --machine-config $machineconfig -o temp.c -oo bundles.o -c ./mandroid-win.exe ./Xamarin.Android.Cecil.dll ./Xamarin.Android.Cecil.Mdb.dll ./Mono.Touch.Common.dll ./Ionic.Zip.dll
i686-pc-mingw32-gcc -U _WIN32 -s -o mandroid.exe -Wall temp.c -Wl,-Bstatic `pkg-config --cflags --libs monosgen-2|dos2unix` -lmswsock -lz bundles.o #-mconsole
rm temp.c
rm bundles.o
