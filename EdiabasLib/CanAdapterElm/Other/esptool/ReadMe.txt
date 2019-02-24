Command line for flashing ESP8266ex:
------------------------------------
"C:\Program Files\Python36\python.exe" D:\Projects\esptool\esptool.py --port COM8 --baud 460800 write_flash -fs 1MB -ff 40m 0x00000 boot_v1.7.bin 0x01000 user1.bin 0xfc000 esp_init_data_default.bin 0xfe000 blank.bin
