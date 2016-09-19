@echo off

"C:\Program Files\Wireshark\extcap\USBPcapCMD.exe" -d \\.\USBPcap2 -o - | "c:\Program Files\Wireshark\Wireshark.exe" -k -i -
