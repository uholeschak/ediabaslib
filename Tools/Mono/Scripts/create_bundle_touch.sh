# install mono with: msiexec -i xxx.msi INSTALLFOLDER="C:\Programs\Mono"
# Mono paths
mono_version="4.5"
export MONO="/c/Programs/Mono/"
export PATH="$PATH:$MONO/lib/mono/$mono_version:$MONO/bin"
export PKG_CONFIG_PATH="$MONO/lib/pkgconfig"
machineconfig="info1.xml"
# Compiler
export CC="gcc -U _WIN32"
dep_files="./I18N.CJK.dll \
./I18N.MidEast.dll \
./I18N.Other.dll \
./I18N.Rare.dll \
./I18N.West.dll \
./I18N.dll \
./Mono.Posix.dll \
./Mono.Security.dll \
./Mono.Touch.Activation.Common.dll \
$MONO/lib/mono/$mono_version/mscorlib.dll \
./System.Configuration.dll \
./System.Core.dll \
./System.Security.dll \
./System.Xml.dll \
./System.dll"

#echo "files: $dep_files"
mkbundle --deps -z --machine-config $machineconfig -o temp.c -oo bundles.o -c ./mtouch-win.exe $dep_files
gcc -U _WIN32 -g -o mtouch.exe -Wall temp.c -Wl,-Bstatic `pkg-config --cflags --libs monosgen-2` -lz bundles.o #-mconsole
rm temp.c
rm bundles.o
cp "$MONO/bin/monosgen-2.0.dll" .
