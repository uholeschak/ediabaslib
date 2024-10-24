# EdiabasTest command line parameters
For testing of the EdiabasLib.dll there is a console application `EdiabasTest.exe` with command line parameters.  
The following command line parameters are supported:
* `--cfg="<property1>=<value1>;<property2>=<value2>`: This parameter allows to override default values of the [EdiabasLib.config](EdiabasLib.config_file.md) file.
* `-s or --sgbd=`: Specify a SGBD (ECU) file to execute.
* `-p or --port=`: Specify a COM port for communication.
* `-o or --out=`: This parameter specifies a file name for the output. If the parameter is missing, the output is written to the console.
* `-a or --append=`: 0=Override output file, 1=Append output file. 
* `--ifh=`: Select interface handler. Possible values are `STD:OBD`, `ADS` or `ENET`.
* `-f or --format=<result name>=<format string>`: Allow to select a format string for a specific job result. This parameter can be specified multiple times.
* `-j or --job=<job name>#<job parameters semicolon separated>#<request results semicolon separated>#<standard job parameters semicolon separated>`: Execute a job with `<job name>`, `<job parameters>` and `<result requests>`. For binary job parameters prepend the hex string with `|` (e.g. `|A3C2`)
* `-h or --help=`: Displays the help page.

Example arguments:  
`-p "COM4" --cfg="IfhTrace=2" -s "Ecu\d_motor.grp" -l "ediabas.log" -j "FS_LESEN" -j "FS_LESEN_DETAIL#0x4232#F_ART_ANZ;F_UW_ANZ" -j "STATUS_RAILDRUCK_IST##STAT_RAILDRUCK_IST_WERT" -j "STATUS_MOTORTEMPERATUR##STAT_MOTORTEMPERATUR_WERT" -j "STATUS_LMM_MASSE##STAT_LMM_MASSE_WERT" -j "STATUS_MOTORDREHZAHL" -j "STATUS_SYSTEMCHECK_PM_INFO_1" -j "STATUS_SYSTEMCHECK_PM_INFO_2"`

Example arguments with binary data:  
`-p "COM4" -s "binary_test.prg" -l "ediabas.log" -j "BINARY_TEST#|23ABC3"`
