# install mono with: msiexec -i xxx.msi INSTALLFOLDER="C:\Programs\Mono"
# Mono paths
mono_version="4.5"
export MONO="/c/Programs/Mono/"
export PATH="$PATH:$MONO/lib/mono/$mono_version:$MONO/bin"
export PKG_CONFIG_PATH="$MONO/lib/pkgconfig"
machineconfig="info1.xml"
# Compiler
export CC="gcc -U _WIN32"
dep_files="./I18N.dll \
./I18N.West.dll \
./Ionic.Zip.dll \
./Mono.Data.Sqlite.dll \
./Mono.Data.Tds.dll \
./Mono.Posix.dll \
./Mono.Security.dll \
./Mono.Touch.Client.dll \
./Mono.Touch.Common.dll \
./Mono.Web.dll \
$MONO/lib/mono/$mono_version/mscorlib.dll \
./System.Configuration.dll \
./System.Core.dll \
./System.Data.dll \
./System.dll \
./System.Drawing.dll \
./System.EnterpriseServices.dll \
./System.Security.dll \
./System.Transactions.dll \
./System.Web.ApplicationServices.dll \
./System.Web.dll \
./System.Web.Services.dll \
./System.Xml.dll \
./System.Xml.Linq.dll \
./Xamarin.Android.Cecil.dll \
./Xamarin.Android.Cecil.Mdb.dll "

#echo "files: $dep_files"
mkbundle --deps -z --machine-config $machineconfig -o temp.c -oo bundles.o -c ./mandroid-win.exe $dep_files
gcc -U _WIN32 -g -o mandroid.exe -Wall temp.c -Wl,-Bstatic `pkg-config --cflags --libs monosgen-2` -lz bundles.o
#gcc -U _WIN32 -g -o mandroid.exe -Wall temp.c -Lc:/Programs/Mono/lib -Ic:/Programs/Mono/include/mono-2.0 -lmonosgen-2.0 -lz bundles.o
rm temp.c
rm bundles.o
cp "$MONO/bin/monosgen-2.0.dll" .
