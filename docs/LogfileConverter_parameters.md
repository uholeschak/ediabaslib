# LogfileConverter command line parameters
For conversion of trace files to CarSimulator response files or EdiabasLib simulation files there is a console application `LogfileConverter.exe` which accepts command line parameters. The following parameters are supported:
* `-i or --input=`: EdiabasLib input trace file (`.trc` extension). Multiple files are allowed
* `-o or --output=`: Output file in the specifed format.
* `--sim=`: Output simulation file name (`.sim` extension).
* `-c or --cformat`: Output in C style format (0x prefix), not recommended.
* `-e or --errors`: Ignore CRC errors in input files, not recommended.
* `-r or --response`: Output in CarSimulator response format, recommended.
* `-s or --sort`: Sort response file. Only useful with response output format, recommended.
* `--sformat`: Simulation file format. Possible values are: `bmw_fast`, `ds2`, `edic`). If ommited the format is auto detected (recommended).

Example convert trace files to CarSimulator response file and merge old files:  
`-i "trace1.trc" -i "trace2.trc" -m "response_old.txt" -o "response.txt"`

Example convert CarSimulator response files to simulation files:  
`-m "response1.txt" -m "response2.txt" -sim="obd.sim"`
