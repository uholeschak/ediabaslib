@echo off

"c:\Program Files\USBPcap\USBPcapCMD.exe" -d \\.\USBPcap2 -o - | "c:\Program Files\Wireshark\Wireshark.exe" -k -i -
