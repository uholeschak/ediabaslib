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

Flash firmware with uboot:
--------------------------
Set adapter ip address to 192.168.1.55
add arp table entry:
arp -s 192.168.1.109 00-AA-BB-CC-DD-10 192.168.1.55
Start uboot and select 2
local ip: 192.168.1.109
server ip: 192.168.1.55
file: openwrt-15.05-ramips-rt305x-a5-v11-squashfs-sysupgrade.bin
Start tftp server

Update QualComm factory firmware:
---------------------------------
Copy uboot.img and mini.bin to USB stick
Set network adapter to 192.168.100.10/255.255.255.0
insert USB stick into the router

telnet 192.168.100.1
mount /dev/sda1 /mnt
ls /mnt
uboot.img and mini.bin should be visible
mtd_write write /mnt/uboot.img Bootloader
mtd_write write /mnt/mini.bin Kernel
reboot
remove USB stick

start miniweb.exe
telnet 192.168.100.1
cd /tmp
wget http://192.168.100.10:8000/openwrt-15.05-ramips-rt305x-a5-v11-squashfs-sysupgrade.bin
sysupgrade -v -n /tmp/openwrt-15.05-ramips-rt305x-a5-v11-squashfs-sysupgrade.bin
