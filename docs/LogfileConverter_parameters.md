# LogfileConverter command line parameters
For conversion of trace files to CarSimulator response files or response files to [EdiabasLib simulation files](EdiabasLib.simulation_file.md) there is a console application `LogfileConverter.exe` with command line parameters.  
The following command line parameters are supported:
* `-i or --input=`: EdiabasLib input trace file (`.trc` extension). Multiple files are allowed
* `-o or --output=`: Output file in the specified format.
* `--sim=`: Output simulation file name (`.sim` extension).
* `-c or --cformat`: Output in C style format (0x prefix), not recommended.
* `-e or --errors`: Ignore CRC errors in input files, not recommended.
* `-r or --response`: Output in CarSimulator response format, recommended.
* `-s or --sort`: Sort response file. Only useful with response output format and for BMW-FAST or DS2 files, recommended if possible.
* `--sformat`: Simulation file format. Possible values are: `bmw_fast`, `ds2`, `edic`). If omitted the format is auto detected (recommended).

It's recommended to convert trace files to CarSimulator response files first and then convert them in the next step to simulation files.  

Example arguments: Convert trace files, merge old response files and output to sorted CarSimulator response file.  
`-i "trace1.trc" -i "trace2.trc" -m "response_old.txt" -o "response.txt" -r -s`

Example arguments: Convert CarSimulator response files to simulation file.  
`-m "response1.txt" -m "response2.txt" --sim="obd.sim"`
