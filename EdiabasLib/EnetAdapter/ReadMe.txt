Build OpenWrt:
--------------
Pepare build system according to:
https://wiki.openwrt.org/doc/howto/build
copy diffconfig to build directory
copy patches/770-group_key_timeout.patch to package/network/services/hostapd/patches

cp diffconfig .config
make defconfig
make

If download of packages fails build with
make V=s
and copy the missing files to the dl directory.

copy imagebulder from bin new location
extract imagebilder tar -xvjf ...
copy files directory and build.sh to imagebilder build directory

sh build.sh

Debuging hostadp:
-----------------
make menuconfig
Network->wap-supplicant->Minimum debug message priority: 0
Debug according to
https://wiki.openwrt.org/doc/devel/debugging
kill `cat /var/run/wifi-phy0.pid` until hostadp finally dies.
iw dev wlan0 del
iw phy phy0 interface add wlan0 type managed
/usr/sbin/hostapd -ddddddd -P /var/run/wifi-phy0.pid /var/run/hostapd-phy0.conf > /tmp/log.txt &
