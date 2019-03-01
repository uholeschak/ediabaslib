Command line for flashing ESP8266ex:
------------------------------------
Programming:
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 write_flash -fs 1MB -ff 40m 0x00000 boot_v1.6.bin 0x01000 user1.bin 0xfc000 esp_init_data_default.bin 0xfe000 blank.bin
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 write_flash -fs 1MB -ff 40m 0x00000 boot_v1.7.bin 0x01000 user1.bin 0xfc000 esp_init_data_default.bin 0xfe000 blank.bin

Read settings:
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 read_flash 0x7e000 0x2000 userdata.bin
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 read_flash 0xfd000 0x1000 sysdata.bin

Write settings:
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 write_flash 0x7e000 userdata.bin 0xfd000 sysdata.bin

Factory reset:
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 write_flash 0x7e000 blank.bin 0x7f000 blank.bin 0xfd000 blank.bin
