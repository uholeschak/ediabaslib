#!/bin/sh
# build with: http://downloads.openwrt.org/chaos_calmer/15.05/ramips/rt305x/OpenWrt-ImageBuilder-15.05-ramips-rt305x.Linux-x86_64.tar.bz2
make image PROFILE="A5-V11" PACKAGES="luci" FILES=files/
