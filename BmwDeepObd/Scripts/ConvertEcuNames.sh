#!/bin/sh
sort NamesAll.txt | sed 's/[[:blank:]]*$//g' | uniq > NamesAllSort.txt
sed -e 's/^\([[:alnum:]]\{2,2\}\)[[:blank:]]-[[:blank:]]\(.*\)/    <Ecu AddrHex="\1" Name="\2" \/>/g' -e '1s;^;<?xml version="1.0" encoding="utf-8"?>\n<GatewayECUList>\n  <EcuList>\n;' -e '$a\ \ </EcuList>\n</GatewayECUList>' NamesAllSort.txt > ECU_Names.xml