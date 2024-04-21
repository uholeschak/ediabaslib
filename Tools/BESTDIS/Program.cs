/*
The MIT License (MIT)

Copyright (c) 2014 tacotruck

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BESTDIS
{
	class Program
	{
		public class OpArg
		{
			public byte oa;
			public string name;

			public OpArg(byte oa, string pneumonic)
			{
				this.oa = oa;
				this.name = pneumonic;
			}
		}

		public class OpCode
		{
			public byte oc;
			public string pneumonic;
			public bool arg0IsNearAddress;

			public OpCode(byte oc, string pneumonic)
				: this(oc, pneumonic, false)
			{
			}

			public OpCode(byte oc, string pneumonic, bool arg0IsNearAddress)
			{
				this.oc = oc;
				this.pneumonic = pneumonic;
				this.arg0IsNearAddress = arg0IsNearAddress;
			}
		}

		public enum OpAddrMode : byte
		{
			None = 0,
			RegS = 1,
			RegAB = 2,
			RegI = 3,
			RegL = 4,
			Imm8 = 5,
			Imm16 = 6,
			Imm32 = 7,
			ImmStr = 8,
			IdxImm = 9,
			IdxReg = 10,
			IdxRegImm = 11,
			IdxImmLenImm = 12,
			IdxImmLenReg = 13,
			IdxRegLenImm = 14,
			IdxRegLenReg = 15,
		}

		static OpArg[] oaList = new OpArg[]
		{
			new OpArg(0x00, "B0"),
			new OpArg(0x01, "B1"),
			new OpArg(0x02, "B2"),
			new OpArg(0x03, "B3"),
			new OpArg(0x04, "B4"),
			new OpArg(0x05, "B5"),
			new OpArg(0x06, "B6"),
			new OpArg(0x07, "B7"),
			new OpArg(0x08, "B8"),
			new OpArg(0x09, "B9"),
			new OpArg(0x0A, "BA"),
			new OpArg(0x0B, "BB"),
			new OpArg(0x0C, "BC"),
			new OpArg(0x0D, "BD"),
			new OpArg(0x0E, "BE"),
			new OpArg(0x0F, "BF"),
			new OpArg(0x10, "I0"),
			new OpArg(0x11, "I1"),
			new OpArg(0x12, "I2"),
			new OpArg(0x13, "I3"),
			new OpArg(0x14, "I4"),
			new OpArg(0x15, "I5"),
			new OpArg(0x16, "I6"),
			new OpArg(0x17, "I7"),
			new OpArg(0x18, "L0"),
			new OpArg(0x19, "L1"),
			new OpArg(0x1A, "L2"),
			new OpArg(0x1B, "L3"),
			new OpArg(0x1C, "S0"),
			new OpArg(0x1D, "S1"),
			new OpArg(0x1E, "S2"),
			new OpArg(0x1F, "S3"),
			new OpArg(0x20, "S4"),
			new OpArg(0x21, "S5"),
			new OpArg(0x22, "S6"),
			new OpArg(0x23, "S7"),
			new OpArg(0x24, "F0"),
			new OpArg(0x25, "F1"),
			new OpArg(0x26, "F2"),
			new OpArg(0x27, "F3"),
			new OpArg(0x28, "F4"),
			new OpArg(0x29, "F5"),
			new OpArg(0x2A, "F6"),
			new OpArg(0x2B, "F7"),
			new OpArg(0x2C, "S8"),
			new OpArg(0x2D, "S9"),
			new OpArg(0x2E, "SA"),
			new OpArg(0x2F, "SB"),
			new OpArg(0x30, "SC"),
			new OpArg(0x31, "SD"),
			new OpArg(0x32, "SE"),
			new OpArg(0x33, "SF"),
			new OpArg(0x80, "A0"),
			new OpArg(0x81, "A1"),
			new OpArg(0x82, "A2"),
			new OpArg(0x83, "A3"),
			new OpArg(0x84, "A4"),
			new OpArg(0x85, "A5"),
			new OpArg(0x86, "A6"),
			new OpArg(0x87, "A7"),
			new OpArg(0x88, "A8"),
			new OpArg(0x89, "A9"),
			new OpArg(0x8A, "AA"),
			new OpArg(0x8B, "AB"),
			new OpArg(0x8C, "AC"),
			new OpArg(0x8D, "AD"),
			new OpArg(0x8E, "AE"),
			new OpArg(0x8F, "AF"),
			new OpArg(0x90, "I8"),
			new OpArg(0x91, "I9"),
			new OpArg(0x92, "IA"),
			new OpArg(0x93, "IB"),
			new OpArg(0x94, "IC"),
			new OpArg(0x95, "ID"),
			new OpArg(0x96, "IE"),
			new OpArg(0x97, "IF"),
			new OpArg(0x98, "L4"),
			new OpArg(0x99, "L5"),
			new OpArg(0x9A, "L6"),
			new OpArg(0x9B, "L7"),
		};

		private static OpCode[] ocList = new OpCode[]
		{
			new OpCode(0x00, "move"),
			new OpCode(0x01, "clear"),
			new OpCode(0x02, "comp"),
			new OpCode(0x03, "subb"),
			new OpCode(0x04, "adds"),
			new OpCode(0x05, "mult"),
			new OpCode(0x06, "divs"),
			new OpCode(0x07, "and"),
			new OpCode(0x08, "or"),
			new OpCode(0x09, "xor"),
			new OpCode(0x0A, "not"),
			new OpCode(0x0B, "jump", true),
			new OpCode(0x0C, "jtsr", true),
			new OpCode(0x0D, "ret"),
			new OpCode(0x0E, "jc", true),
			new OpCode(0x0F, "jae", true),
			new OpCode(0x10, "jz", true),
			new OpCode(0x11, "jnz", true),
			new OpCode(0x12, "jv", true),
			new OpCode(0x13, "jnv", true),
			new OpCode(0x14, "jmi", true),
			new OpCode(0x15, "jpl", true),
			new OpCode(0x16, "clrc"),
			new OpCode(0x17, "setc"),
			new OpCode(0x18, "asr"),
			new OpCode(0x19, "lsl"),
			new OpCode(0x1A, "lsr"),
			new OpCode(0x1B, "asl"),
			new OpCode(0x1C, "nop"),
			new OpCode(0x1D, "eoj"),
			new OpCode(0x1E, "push"),
			new OpCode(0x1F, "pop"),
			new OpCode(0x20, "scmp"),
			new OpCode(0x21, "scat"),
			new OpCode(0x22, "scut"),
			new OpCode(0x23, "slen"),
			new OpCode(0x24, "spaste"),
			new OpCode(0x25, "serase"),
			new OpCode(0x26, "xconnect"),
			new OpCode(0x27, "xhangup"),
			new OpCode(0x28, "xsetpar"),
			new OpCode(0x29, "xawlen"),
			new OpCode(0x2A, "xsend"),
			new OpCode(0x2B, "xsendf"),
			new OpCode(0x2C, "xrequf"),
			new OpCode(0x2D, "xstopf"),
			new OpCode(0x2E, "xkeyb"),
			new OpCode(0x2F, "xstate"),
			new OpCode(0x30, "xboot"),
			new OpCode(0x31, "xreset"),
			new OpCode(0x32, "xtype"),
			new OpCode(0x33, "xvers"),
			new OpCode(0x34, "ergb"),
			new OpCode(0x35, "ergw"),
			new OpCode(0x36, "ergd"),
			new OpCode(0x37, "ergi"),
			new OpCode(0x38, "ergr"),
			new OpCode(0x39, "ergs"),
			new OpCode(0x3A, "a2flt"),
			new OpCode(0x3B, "fadd"),
			new OpCode(0x3C, "fsub"),
			new OpCode(0x3D, "fmul"),
			new OpCode(0x3E, "fdiv"),
			new OpCode(0x3F, "ergy"),
			new OpCode(0x40, "enewset"),
			new OpCode(0x41, "etag", true),
			new OpCode(0x42, "xreps"),
			new OpCode(0x43, "gettmr"),
			new OpCode(0x44, "settmr"),
			new OpCode(0x45, "sett"),
			new OpCode(0x46, "clrt"),
			new OpCode(0x47, "jt", true),
			new OpCode(0x48, "jnt", true),
			new OpCode(0x49, "addc"),
			new OpCode(0x4A, "subc"),
			new OpCode(0x4B, "break"),
			new OpCode(0x4C, "clrv"),
			new OpCode(0x4D, "eerr"),
			new OpCode(0x4E, "popf"),
			new OpCode(0x4F, "pushf"),
			new OpCode(0x50, "atsp"),
			new OpCode(0x51, "swap"),
			new OpCode(0x52, "setspc"),
			new OpCode(0x53, "srevrs"),
			new OpCode(0x54, "stoken"),
			new OpCode(0x55, "parb"),
			new OpCode(0x56, "parw"),
			new OpCode(0x57, "parl"),
			new OpCode(0x58, "pars"),
			new OpCode(0x59, "fclose"),
			new OpCode(0x5A, "jg", true),
			new OpCode(0x5B, "jge", true),
			new OpCode(0x5C, "jl", true),
			new OpCode(0x5D, "jle", true),
			new OpCode(0x5E, "ja", true),
			new OpCode(0x5F, "jbe", true),
			new OpCode(0x60, "fopen"),
			new OpCode(0x61, "fread"),
			new OpCode(0x62, "freadln"),
			new OpCode(0x63, "fseek"),
			new OpCode(0x64, "fseekln"),
			new OpCode(0x65, "ftell"),
			new OpCode(0x66, "ftellln"),
			new OpCode(0x67, "a2fix"),
			new OpCode(0x68, "fix2flt"),
			new OpCode(0x69, "parr"),
			new OpCode(0x6A, "test"),
			new OpCode(0x6B, "wait"),
			new OpCode(0x6C, "date"),
			new OpCode(0x6D, "time"),
			new OpCode(0x6E, "xbatt"),
			new OpCode(0x6F, "tosp"),
			new OpCode(0x70, "xdownl"),
			new OpCode(0x71, "xgetport"),
			new OpCode(0x72, "xignit"),
			new OpCode(0x73, "xloopt"),
			new OpCode(0x74, "xprog"),
			new OpCode(0x75, "xraw"),
			new OpCode(0x76, "xsetport"),
			new OpCode(0x77, "xsireset"),
			new OpCode(0x78, "xstoptr"),
			new OpCode(0x79, "fix2hex"),
			new OpCode(0x7A, "fix2dez"),
			new OpCode(0x7B, "tabset"),
			new OpCode(0x7C, "tabseek"),
			new OpCode(0x7D, "tabget"),
			new OpCode(0x7E, "strcat"),
			new OpCode(0x7F, "pary"),
			new OpCode(0x80, "parn"),
			new OpCode(0x81, "ergc"),
			new OpCode(0x82, "ergl"),
			new OpCode(0x83, "tabline"),
			new OpCode(0x84, "xsendr"),
			new OpCode(0x85, "xrecv"),
			new OpCode(0x86, "xinfo"),
			new OpCode(0x87, "flt2a"),
			new OpCode(0x88, "setflt"),
			new OpCode(0x89, "cfgig"),
			new OpCode(0x8A, "cfgsg"),
			new OpCode(0x8B, "cfgis"),
			new OpCode(0x8C, "a2y"),
			new OpCode(0x8D, "xparraw"),
			new OpCode(0x8E, "hex2y"),
			new OpCode(0x8F, "strcmp"),
			new OpCode(0x90, "strlen"),
			new OpCode(0x91, "y2bcd"),
			new OpCode(0x92, "y2hex"),
			new OpCode(0x93, "shmset"),
			new OpCode(0x94, "shmget"),
			new OpCode(0x95, "ergsysi"),
			new OpCode(0x96, "flt2fix"),
			new OpCode(0x97, "iupdate"),
			new OpCode(0x98, "irange"),
			new OpCode(0x99, "iincpos"),
			new OpCode(0x9A, "tabseeku"),
			new OpCode(0x9B, "flt2y4"),
			new OpCode(0x9C, "flt2y8"),
			new OpCode(0x9D, "y42flt"),
			new OpCode(0x9E, "y82flt"),
			new OpCode(0x9F, "plink"),
			new OpCode(0xA0, "pcall"),
			new OpCode(0xA1, "fcomp"),
			new OpCode(0xA2, "plinkv"),
			new OpCode(0xA3, "ppush"),
			new OpCode(0xA4, "ppop"),
			new OpCode(0xA5, "ppushflt"),
			new OpCode(0xA6, "ppopflt"),
			new OpCode(0xA7, "ppushy"),
			new OpCode(0xA8, "ppopy"),
			new OpCode(0xA9, "pjtsr"),
			new OpCode(0xAA, "tabsetex"),
			new OpCode(0xAB, "ufix2dez"),
			new OpCode(0xAC, "generr"),
			new OpCode(0xAD, "ticks"),
			new OpCode(0xAE, "waitex"),
			new OpCode(0xAF, "xopen"),
			new OpCode(0xB0, "xclose"),
			new OpCode(0xB1, "xcloseex"),
			new OpCode(0xB2, "xswitch"),
			new OpCode(0xB3, "xsendex"),
			new OpCode(0xB4, "xrecvex"),
			new OpCode(0xB5, "ssize"),
			new OpCode(0xB6, "tabcols"),
			new OpCode(0xB7, "tabrows"),
			new OpCode(0x0E, "jb", true),
			new OpCode(0x0F, "jnc", true)
		};

		private static Dictionary<string, List<string>> comments = new Dictionary<string, List<string>>();
		private static Encoding encoding = Encoding.GetEncoding(1252);
		private static TextWriter output;

		static int Main(string[] args)
		{
			if ((args.Length != 1))
			{
				Console.Error.WriteLine("Usage BESTDIS bestobjfile");
				return -1;
			}

            string dirName = Path.GetDirectoryName(args[0]);
            if (dirName == null || dirName.Length == 0)
            {
                Console.Error.WriteLine("Specify file with path name");
                return -1;
            }
            string[] files = Directory.GetFiles(dirName, Path.GetFileName(args[0]));
            foreach (string file in files)
            {
                string inputFile = file;
                string inputExtension = Path.GetExtension(inputFile);
                string outputExtension = ".b1v";
                if (string.Compare(inputExtension, ".grp", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    outputExtension = ".b1g";
                }
                string outputFile = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + outputExtension;
                output = new StreamWriter(File.OpenWrite(outputFile), encoding);

                comments.Clear();
                FileStream fs = File.OpenRead(inputFile);
                byte[] buffer = new byte[0x9C];
                fs.Read(buffer, 0, buffer.Length);

                //0x00 magic
                //0x10 0: group file 1: prg file
                //0x14 -1
                //0x18 ssize
                //0x78 ??? ptr
                //0x7C uses ptr
                //0x80 job code ptr
                //0x84 table ptr
                //0x88 job list ptr
                //0x8C ??? ptr
                //0x90 description ptr
                //0x94 info ptr
                //0x98 ??? ptr (missing/reserved?)

                dumpInfo(fs, buffer);
                dumpDescription(fs, buffer);
                dumpUses(fs, buffer);
                dumpSsize(fs, buffer);
                dumpTables(fs, buffer);
                dumpJobs(fs, buffer);

                fs.Close();
                output.Close();
            }

			return 0;
		}

		public static void dumpTables(FileStream fs, byte[] buffer)
		{
			int tableOffset = BitConverter.ToInt32(buffer, 0x84);
			fs.Seek(tableOffset, SeekOrigin.Begin);

			byte[] tableCountBuffer = new byte[4];
			readAndDecryptBytes(fs, tableCountBuffer, 0, tableCountBuffer.Length);
			int tableCount = BitConverter.ToInt32(tableCountBuffer, 0);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < tableCount; ++i)
			{
				byte[] tableBuffer = new byte[0x50];
				readAndDecryptBytes(fs, tableBuffer, 0, tableBuffer.Length);
				string name = encoding.GetString(tableBuffer, 0, 0x40).TrimEnd('\0');

				int tableColumnOffset = BitConverter.ToInt32(tableBuffer, 0x40);
				int tableColumnCount = BitConverter.ToInt32(tableBuffer, 0x48);
				int tableRowCount = BitConverter.ToInt32(tableBuffer, 0x4C);

				output.WriteLine("TBEG \"{0}\"", name);

				long savedPos = fs.Position;
				fs.Seek(tableColumnOffset, SeekOrigin.Begin);

				sb.Length = 0;
				sb.Append("HEAD ");
				for (int j = 0; j < tableColumnCount; ++j)
				{
					byte[] tableItemBuffer = new byte[1024];

					int k;
					for (k = 0; k < tableItemBuffer.Length; ++k)
					{
						readAndDecryptBytes(fs, tableItemBuffer, k, 1);
						if (tableItemBuffer[k] == 0)
							break;
					}

					sb.AppendFormat("\"{0}\", ", encoding.GetString(tableItemBuffer, 0, k));
				}

				sb.Remove(sb.Length - 2, 2);
				output.WriteLine(sb.ToString());

				for (int j = 0; j < tableRowCount; ++j)
				{
					sb.Length = 0;
					sb.Append("LINE ");

					for (int k = 0; k < tableColumnCount; ++k)
					{
						byte[] tableItemBuffer = new byte[1024];

						int l;
						for (l = 0; l < tableItemBuffer.Length; ++l)
						{
							readAndDecryptBytes(fs, tableItemBuffer, l, 1);
							if (tableItemBuffer[l] == 0)
								break;
						}

						sb.AppendFormat("\"{0}\", ", encoding.GetString(tableItemBuffer, 0, l));
					}

					sb.Remove(sb.Length - 2, 2);
					output.WriteLine(sb.ToString());
				}

				output.WriteLine("TEND");
				output.WriteLine();
				fs.Seek(savedPos, SeekOrigin.Begin);
			}

			output.WriteLine();
		}

		public static void dumpUses(FileStream fs, byte[] buffer)
		{
			int usesOffset = BitConverter.ToInt32(buffer, 0x7C);
			fs.Seek(usesOffset, SeekOrigin.Begin);

			int usesCount = readInt32(fs);
			for (int i = 0; i < usesCount; ++i)
			{
				byte[] usesBuffer = new byte[0x100];
				readAndDecryptBytes(fs, usesBuffer, 0, usesBuffer.Length);
				output.WriteLine(";Uses: " + encoding.GetString(usesBuffer).TrimEnd('\0'));
			}

			output.WriteLine();
		}

        public static void dumpSsize(FileStream fs, byte[] buffer)
        {
            int ssize = BitConverter.ToInt32(buffer, 0x18);
            if (ssize != 0)
            {
                output.WriteLine("#SSIZE {0}", ssize.ToString());
                output.WriteLine();
            }
        }

        public static void dumpInfo(FileStream fs, byte[] buffer)
		{
			int infoOffset = BitConverter.ToInt32(buffer, 0x94);
			byte[] infoBuffer = new byte[0x6C];
			fs.Seek(infoOffset, SeekOrigin.Begin);
			readAndDecryptBytes(fs, infoBuffer, 0, infoBuffer.Length);

			output.WriteLine(";BIP Version: {0:X2}.{1:X2}.{2:X2}", infoBuffer[2], infoBuffer[1], infoBuffer[0]);
			output.WriteLine(";Revision Number: {0}.{1}", BitConverter.ToInt16(infoBuffer, 0x06), BitConverter.ToInt16(infoBuffer, 0x04));
			output.WriteLine(";Last Changed: {0}", encoding.GetString(infoBuffer, 0x48, 0x24).TrimEnd('\0'));
			output.WriteLine(";By: {0}", encoding.GetString(infoBuffer, 0x08, 0x40).TrimEnd('\0'));
			output.WriteLine(";Package Version: {0:X8}", BitConverter.ToInt32(infoBuffer, 0x68));
			output.WriteLine();
		}

		public static void dumpJobs(FileStream fs, byte[] buffer)
		{
			int jobListOffset = BitConverter.ToInt32(buffer, 0x88);
			fs.Seek(jobListOffset, SeekOrigin.Begin);
			int numJobs = readInt32(fs);
			for (int i = 0; i < numJobs; ++i)
			{
				byte[] jobBuffer = new byte[0x44];
				readAndDecryptBytes(fs, jobBuffer, 0, jobBuffer.Length);
				string jobNameString = encoding.GetString(jobBuffer, 0, 0x40).TrimEnd('\0');
				int jobAddress = BitConverter.ToInt32(jobBuffer, 0x40);

				output.WriteLine("{0}#", jobNameString);
				List<string> jobComments;
				if (comments.TryGetValue(jobNameString, out jobComments))
				{
					foreach (string c in jobComments)
						output.WriteLine(";{0,4}{1}", "", c);
					output.WriteLine(";");
				}

				long fsPos = fs.Position;
				disassembleJob(fs, jobAddress);
				fs.Seek(fsPos, SeekOrigin.Begin);
				output.WriteLine();
				output.WriteLine();
			}
		}

		public static void disassembleJob(FileStream fs, int jobAddress)
		{
			fs.Seek(jobAddress, SeekOrigin.Begin);
			byte[] buffer = new byte[2];
			
			bool foundFirstEoj = false;

			SortedDictionary<int, string> lines = new SortedDictionary<int, string>();
			HashSet<int> labels = new HashSet<int>();

			while (true)
			{
				int address = (int)fs.Position;
				readAndDecryptBytes(fs, buffer, 0, buffer.Length);
				byte opCodeVal = buffer[0];
				byte opAddrMode = buffer[1];
				OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
				OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

				OpCode oc = ocList.First(o => o.oc == opCodeVal);
				string arg0 = disassembleOpArg(fs, opAddrMode0);
				string arg1 = disassembleOpArg(fs, opAddrMode1);

				//special near address arg0 opcode handling mainly for jumps
				if (oc.arg0IsNearAddress && (opAddrMode0 == OpAddrMode.Imm32))
				{
					int labelAddress = (int)fs.Position + Convert.ToInt32(arg0.Substring(2, arg0.Length - 4), 16);
					labels.Add(labelAddress);
					arg0 = string.Format("__{0:X8}", labelAddress);
				}

				string format;

				if (!string.IsNullOrEmpty(arg0))
				{
					if (!string.IsNullOrEmpty(arg1))
						format = "{0,-10} {1},{2}";
					else
						format = "{0,-10} {1}";
				}
				else
					format = "{0}";

				lines[address] = string.Format(format, oc.pneumonic, arg0, arg1);

				if (opCodeVal == 0x1D)
				{
					if (foundFirstEoj)
						break;

					foundFirstEoj = true;
				}
				else
					foundFirstEoj = false;
			}

			foreach (KeyValuePair<int, string> kvp in lines)
				if (labels.Contains(kvp.Key))
					output.WriteLine("__{0:X8}: {1,8}{2}", kvp.Key, "", kvp.Value);
				else
					output.WriteLine("{0,20}{1}", "", kvp.Value);
		}

		public static bool isPrintable(byte b)
		{
			return ((b == 9) || (b == 10) || (b == 13) || ((b >= 32) && (b < 127)));
		}

		public static string disassembleOpArg(FileStream fs, OpAddrMode opAddrMode)
		{
			switch (opAddrMode)
			{
				case OpAddrMode.None:
					return null;
				case OpAddrMode.RegS:
				case OpAddrMode.RegAB:
				case OpAddrMode.RegI:
				case OpAddrMode.RegL:
					{
						byte[] buffer = new byte[1];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						return oaReg.name;
					}
				case OpAddrMode.Imm8:
					{
						byte[] buffer = new byte[1];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						if (isPrintable(buffer[0]))
						{
							string v;
							if (buffer[0] == '\t')
								v = "\\t";
							else if (buffer[0] == '\r')
								v = "\\r";
							else if (buffer[0] == '\n')
								v = "\\n";
							else
								v = encoding.GetString(buffer);

							return string.Format("#'{0}'", v);
						}
						else
							return string.Format("#${0:X}.B", buffer[0]);
					}
				case OpAddrMode.Imm16:
					{
						byte[] buffer = new byte[2];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						return string.Format("#${0:X}.I", BitConverter.ToInt16(buffer, 0));
					}
				case OpAddrMode.Imm32:
					{
						byte[] buffer = new byte[4];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						return string.Format("#${0:X}.L", BitConverter.ToInt32(buffer, 0));
					}
				case OpAddrMode.ImmStr:
					{
						byte[] buffer = new byte[2];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						short slen = BitConverter.ToInt16(buffer, 0);
						buffer = new byte[slen];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						
						bool printable = true;
						for (int i = 0; i < slen - 1; ++i)
							if (!isPrintable(buffer[i]))
							{
								printable = false;
								break;
							}
						
						if (printable && (buffer[slen - 1] == 0))
							return "\"" + encoding.GetString(buffer, 0, slen - 1) + "\"";

						StringBuilder sb = new StringBuilder();
						sb.Append("{");
						for (int i = 0; i < slen; ++i)
							sb.AppendFormat("${0:X2}.B,", buffer[i]);
						sb.Remove(sb.Length - 1, 1);
						sb.Append("}");
						return sb.ToString();
					}
				case OpAddrMode.IdxImm:
					{
						byte[] buffer = new byte[3];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						short idx = BitConverter.ToInt16(buffer, 1);
						return string.Format("{0}[#${1:X}]", oaReg.name, idx);
					}
				case OpAddrMode.IdxReg:
					{
						byte[] buffer = new byte[2];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg0 = oaList.First(o => o.oa == buffer[0]);
						OpArg oaReg1 = oaList.First(o => o.oa == buffer[1]);
						return string.Format("{0}[{1}]", oaReg0.name, oaReg1.name);
					}
				case OpAddrMode.IdxRegImm:
					{
						byte[] buffer = new byte[4];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg0 = oaList.First(o => o.oa == buffer[0]);
						OpArg oaReg1 = oaList.First(o => o.oa == buffer[1]);
						short inc = BitConverter.ToInt16(buffer, 2);
						return string.Format("{0}[{1},#${2:X}]", oaReg0.name, oaReg1.name, inc);
					}
				case OpAddrMode.IdxImmLenImm:
					{
						byte[] buffer = new byte[5];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						short idx = BitConverter.ToInt16(buffer, 1);
						short len = BitConverter.ToInt16(buffer, 3);
						return string.Format("{0}[#${1:X}]#${2:X}", oaReg.name, idx, len);
					}
				case OpAddrMode.IdxImmLenReg:
					{
						byte[] buffer = new byte[4];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						short idx = BitConverter.ToInt16(buffer, 1);
						OpArg oaLen = oaList.First(o => o.oa == buffer[3]);
						return string.Format("{0}[#${1:X}]{2}", oaReg.name, idx, oaLen.name);
					}
				case OpAddrMode.IdxRegLenImm:
					{
						byte[] buffer = new byte[4];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						OpArg oaIdx = oaList.First(o => o.oa == buffer[1]);
						short len = BitConverter.ToInt16(buffer, 2);
						return string.Format("{0}[{1}]#${2:X}", oaReg.name, oaIdx.name, len);
					}
				case OpAddrMode.IdxRegLenReg:
					{
						byte[] buffer = new byte[3];
						readAndDecryptBytes(fs, buffer, 0, buffer.Length);
						OpArg oaReg = oaList.First(o => o.oa == buffer[0]);
						OpArg oaIdx = oaList.First(o => o.oa == buffer[1]);
						OpArg oaLen = oaList.First(o => o.oa == buffer[2]);
						return string.Format("{0}[{1}]{2}", oaReg.name, oaIdx.name, oaLen.name);
					}
				default:
					throw new ArgumentOutOfRangeException("opAddrMode", opAddrMode, "Unsupported OpAddrMode");
			}
		}

		public static void dumpDescription(FileStream fs, byte[] buffer)
		{
			int startOffset = BitConverter.ToInt32(buffer, 0x90);
			if (startOffset == -1)
				return;

			fs.Seek(startOffset, SeekOrigin.Begin);

			List<string> commentList = new List<string>();
			string previousJobName = string.Empty;

			int numBytes = readInt32(fs);
			byte[] recordBuffer = new byte[1100];
			int recordOffset = 0;
			for (int i = 0; i < numBytes; ++i)
			{
				readAndDecryptBytes(fs, recordBuffer, recordOffset, 1);
				recordOffset += 1;

				if (recordOffset >= 1098)
					recordBuffer[recordOffset++] = 10; //\n

				if (recordBuffer[recordOffset - 1] == 10) //\n
				{
					recordBuffer[recordOffset] = 0;
					string comment = encoding.GetString(recordBuffer, 0, recordOffset - 1);
					if (comment.StartsWith("JOBNAME:"))
					{
						comments.Add(previousJobName, commentList);
						commentList = new List<string>();
						previousJobName = comment.Substring(8);
					}

					commentList.Add(comment);
					recordOffset = 0;
				}
			}

			comments.Add(previousJobName, commentList);

			if (comments.TryGetValue(string.Empty, out commentList))
			{
				foreach (string c in commentList)
					output.WriteLine(";{0}", c);

				output.WriteLine();
			}
		}

		public static void readAndDecryptBytes(FileStream fs, byte[] buffer, int offset, int count)
		{
			fs.Read(buffer, offset, count);
			for (int i = offset; i < (offset + count); ++i)
				buffer[i] ^= 0xF7;
		}

		public static int readInt32(FileStream fs)
		{
			byte[] buffer = new byte[4];
			fs.Read(buffer, 0, 4);
			return BitConverter.ToInt32(buffer, 0);
		}
	}
}
