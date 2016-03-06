#!/bin/sh
# build with: http://downloads.openwrt.org/chaos_calmer/15.05/ramips/rt305x/OpenWrt-ImageBuilder-15.05-ramips-rt305x.Linux-x86_64.tar.bz2
make image PROFILE="A5-V11" PACKAGES="luci luci-theme-openwrt luci-i18n-base-de luci-i18n-base-en luci-i18n-base-es luci-i18n-base-fr luci-i18n-base-it luci-i18n-base-ja luci-i18n-base-ru" FILES=files/
