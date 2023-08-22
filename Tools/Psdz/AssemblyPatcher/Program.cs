using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Emit;
using dnpatch;

namespace AssemblyPatcher
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("No directory specified");
                    return 1;
                }

                string assemblyDir = args[0];
                if (string.IsNullOrEmpty(assemblyDir) || !Directory.Exists(assemblyDir))
                {
                    Console.WriteLine("Directory not existing: {0}", assemblyDir);
                    return 1;
                }

                string patchCtorNamespace = ConfigurationManager.AppSettings["PatchCtorNamespace"];
                if (string.IsNullOrEmpty(patchCtorNamespace))
                {
                    Console.WriteLine("PatchCtorNamespace not configured");
                    return 1;
                }

                string patchCtorClass = ConfigurationManager.AppSettings["PatchCtorClass"];
                if (string.IsNullOrEmpty(patchCtorClass))
                {
                    Console.WriteLine("PatchCtorClass not configured");
                    return 1;
                }

                string patchMethodNamespace = ConfigurationManager.AppSettings["PatchMethodNamespace"];
                if (string.IsNullOrEmpty(patchCtorNamespace))
                {
                    Console.WriteLine("PatchMethodNamespace not configured");
                    return 1;
                }

                string patchMethodClass = ConfigurationManager.AppSettings["PatchMethodClass"];
                if (string.IsNullOrEmpty(patchMethodClass))
                {
                    Console.WriteLine("PatchMethodName not configured");
                    return 1;
                }

                string patchMethodName = ConfigurationManager.AppSettings["PatchMethodName"];
                if (string.IsNullOrEmpty(patchMethodName))
                {
                    Console.WriteLine("PatchMethodName not configured");
                    return 1;
                }

                string licFileName = ConfigurationManager.AppSettings["LicFileName"];
                if (string.IsNullOrEmpty(licFileName))
                {
                    Console.WriteLine("LicFileName not configured");
                    return 1;
                }

                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                if (string.IsNullOrEmpty(codeBase))
                {
                    Console.WriteLine("Assembly location not found");
                    return 1;
                }

                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                string appDir = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(appDir))
                {
                    Console.WriteLine("Assembly location not found");
                    return 1;
                }

                string licFileSrc = Path.Combine(appDir, "Data", licFileName);
                string licFileDst = Path.Combine(assemblyDir, licFileName);
                if (File.Exists(licFileSrc) && !File.Exists(licFileDst))
                {
                    try
                    {
                        File.Copy(licFileSrc, licFileDst, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Copy license failed: {0}", e.Message);
                        return 1;
                    }
                }

                string exeConfigFile = Path.Combine(assemblyDir, "ISTAGUI.exe.config");
                if (!UpdateExeConfig(exeConfigFile))
                {
                    Console.WriteLine("Update config file failed: {0}", exeConfigFile);
                    return 1;
                }

                string[] files = Directory.GetFiles(assemblyDir, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    string baseName = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(baseName))
                    {
                        continue;
                    }

                    string ext = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(ext))
                    {
                        continue;
                    }

                    if ((string.Compare(ext, ".exe", StringComparison.OrdinalIgnoreCase) != 0) &&
                        (string.Compare(ext, ".dll", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        continue;
                    }

                    if (baseName.EndsWith("interop", StringComparison.OrdinalIgnoreCase) ||
                        baseName.EndsWith("IDESKernel", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!File.Exists(file))
                    {
                        Console.WriteLine("Assembly not existing: {0}", file);
                        return 1;
                    }

                    string assemblyPathBak = Path.Combine(assemblyDir, file + ".bak");
                    if (File.Exists(assemblyPathBak))
                    {
                        Console.WriteLine("Assembly already patched: {0}", file);
                        continue;
                    }

                    try
                    {
                        bool patched = false;
                        Patcher patcher = new Patcher(file, false);

                        try
                        {
                            Target target = new Target
                            {
                                Namespace = patchCtorNamespace,
                                Class = patchCtorClass,
                                Method = ".ctor",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                                patched = true;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            Target target = new Target
                            {
                                Namespace = patchMethodNamespace,
                                Class = patchMethodClass,
                                Method = patchMethodName,
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_0));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            Target target = new Target
                            {
                                Namespace = "BMW.Rheingold.VehicleCommunication",
                                Class = "ECUKom",
                                Method = "InitVCI",
                                Parameters = new []{ "ECUKom", "IVciDevice", "Boolean" },
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("ECUKom.InitVCI found");
                                int patchIndex = -1;
                                int index = 0;
                                foreach (Instruction instruction in instructions)
                                {
                                    if (instruction.OpCode == OpCodes.Ldstr &&
                                        string.Compare(instruction.Operand.ToString(), "ENET::remotehost=", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        Console.WriteLine("'ENET::remotehost=' found at index: {0}", index);
                                        patchIndex = index;
                                        break;
                                    }

                                    index++;
                                }

                                if (patchIndex >= 0)
                                {
                                    if ((instructions[patchIndex + 1].OpCode != OpCodes.Ldarg_1) ||     // ldarg.1
                                        (instructions[patchIndex + 2].OpCode != OpCodes.Callvirt) ||    // callvirt    instance string[RheingoldCoreContracts] BMW.Rheingold.CoreFramework.Contracts.Vehicle.IVciDevice::get_IPAddress()
                                        (instructions[patchIndex + 3].OpCode != OpCodes.Call) ||        // call	string [mscorlib]System.String::Concat(string, string)
                                        (instructions[patchIndex + 4].OpCode != OpCodes.Ldstr) ||       // ldstr	"_"
                                        (instructions[patchIndex + 5].OpCode != OpCodes.Ldstr) ||       // ldstr	"Rheingold"
                                        (instructions[patchIndex + 6].OpCode != OpCodes.Ldsfld) ||      // ldsfld	string [mscorlib]System.String::Empty
                                        (instructions[patchIndex + 7].OpCode != OpCodes.Ldarg_2) ||     // ldarg.2
                                        (instructions[patchIndex + 8].OpCode != OpCodes.Callvirt))      // callvirt	instance bool BMW.Rheingold.VehicleCommunication.Ediabas.API::apiInitExt(string, string, string, string, bool)
                                    {
                                        Console.WriteLine("InitVCI patch location invalid");
                                    }
                                    else
                                    {
                                        /*
                                        Change:
                                            flag = this.api.apiInitExt("ENET::remotehost=" + device.IPAddress, "_", "Rheingold", string.Empty, logging);
                                        to:
                                            flag = this.api.apiInitExt("ENET", "_", "Rheingold", "RemoteHost=" + device.IPAddress + ";DiagnosticPort=50160;ControlPort=50161", logging);
                                        */

                                        instructions.RemoveAt(patchIndex);  // ldstr	"ENET::remotehost="
                                        instructions.Insert(patchIndex, Instruction.Create(OpCodes.Ldstr, "ENET"));
                                        instructions.Insert(patchIndex + 1, Instruction.Create(OpCodes.Ldstr, "_"));
                                        instructions.Insert(patchIndex + 2, Instruction.Create(OpCodes.Ldstr, "Rheingold"));
                                        instructions.Insert(patchIndex + 3, Instruction.Create(OpCodes.Ldstr, "RemoteHost="));
                                        // Index 4: ldarg.1
                                        // Index 5: callvirt	instance string [RheingoldCoreContracts]BMW.Rheingold.CoreFramework.Contracts.Vehicle.IVciDevice::get_IPAddress()
                                        instructions.RemoveAt(patchIndex + 6);  // call	string [mscorlib]System.String::Concat(string, string)
                                        instructions.RemoveAt(patchIndex + 6);  // ldstr	"_"
                                        instructions.RemoveAt(patchIndex + 6);  // ldstr	"Rheingold"
                                        instructions.RemoveAt(patchIndex + 6);  // ldsfld	string [mscorlib]System.String::Empty
                                        instructions.Insert(patchIndex + 6, Instruction.Create(OpCodes.Ldstr, ";DiagnosticPort=50160;ControlPort=50161"));
                                        instructions.Insert(patchIndex + 7,
                                            Instruction.Create(OpCodes.Call,
                                                patcher.BuildCall(typeof(System.String), "Concat", typeof(string),
                                                    new[] { typeof(string), typeof(string), typeof(string) })));
                                        patched = true;
                                        //patcher.Save(file.Replace(".dll", "Test.dll"));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("'ENET::remotehost=' appears to have already been patched");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            Target target = new Target
                            {
                                Namespace = "BMW.ISPI.IstaOperation.Controller",
                                Class = "IstaOperationStarter",
                                Method = "Start",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("IstaOperationStarter.Start found");
                                int patchIndex = -1;
                                if (instructions.Count > 50)
                                {
                                    patchIndex = instructions.Count - 2;
                                }

                                if (patchIndex >= 0)
                                {
                                    if ((instructions[patchIndex].OpCode != OpCodes.Ldloc_S) ||     // ldloc.s	V_4 (4)
                                        (instructions[patchIndex + 1].OpCode != OpCodes.Ret))       // ret
                                    {
                                        Console.WriteLine("Start patch location invalid");
                                    }
                                    else
                                    {
                                        /*
                                        Add debug code:
                                        if (Debugger.IsAttached)
                                        {
                                            System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.Form { TopMost = true },
                                                "IstaOperation started. Attach to IstaOperation.exe now.", "ISTAGUI", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
                                        }
                                        */

                                        instructions.Insert(patchIndex,
                                            Instruction.Create(OpCodes.Call,
                                                patcher.BuildCall(typeof(System.Diagnostics.Debugger), "get_IsAttached", typeof(bool), null)));
                                        instructions.Insert(patchIndex + 1, Instruction.Create(OpCodes.Brfalse_S, instructions[patchIndex + 1]));
                                        instructions.Insert(patchIndex + 2,
                                            Instruction.Create(OpCodes.Newobj,
                                                patcher.BuildInstance(typeof(System.Windows.Forms.Form), null)));
                                        instructions.Insert(patchIndex + 3, Instruction.Create(OpCodes.Dup));
                                        instructions.Insert(patchIndex + 4, Instruction.Create(OpCodes.Ldc_I4_1));
                                        instructions.Insert(patchIndex + 5,
                                            Instruction.Create(OpCodes.Callvirt,
                                                patcher.BuildCall(typeof(System.Windows.Forms.Form), "set_TopMost", typeof(void), new []{ typeof(bool) })));
                                        instructions.Insert(patchIndex + 6, Instruction.Create(OpCodes.Ldstr, "IstaOperation started. Attach to IstaOperation.exe now."));
                                        instructions.Insert(patchIndex + 7, Instruction.Create(OpCodes.Ldstr, "ISTAGUI"));
                                        instructions.Insert(patchIndex + 8, Instruction.Create(OpCodes.Ldc_I4_0));
                                        instructions.Insert(patchIndex + 9, Instruction.Create(OpCodes.Ldc_I4_S, (sbyte) 0x40));
                                        instructions.Insert(patchIndex + 10,
                                            Instruction.Create(OpCodes.Call,
                                                patcher.BuildCall(typeof(System.Windows.Forms.MessageBox), "Show", typeof(System.Windows.Forms.DialogResult), 
                                                    new []{ typeof(System.Windows.Forms.IWin32Window), typeof(string), typeof(string), typeof(System.Windows.Forms.MessageBoxButtons), typeof(System.Windows.Forms.MessageBoxIcon) })));
                                        instructions.Insert(patchIndex + 11, Instruction.Create(OpCodes.Pop));
                                        //patcher.Save(file.Replace(".dll", "Test.dll"));
                                        patched = true;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

#if true
                        if (patched)
                        {
                            try
                            {
                                patcher.Save(true);
                                Console.WriteLine("Patched: {0}", file);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Patch exception: File={0}, Msg={1}", file, ex.Message);
                            }
                        }
#endif
                    }
                    catch (NullReferenceException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Patch exception: File={0}, Msg={1}", file, ex.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                return 1;
            }

            return 0;
        }

        static bool UpdateExeConfig(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("UpdateExeConfig Config file not existing: {0}", fileName);
                    return false;
                }

                string backupFile = fileName + ".bak";
                if (File.Exists(backupFile))
                {
                    Console.WriteLine("UpdateExeConfig Config file already modified: {0}", fileName);
                    return true;
                }

                var patchList = new List<(string Match, string Replace)>()
                {
                    ("\"BMW.Rheingold.ISTAGUI.enableENETprogramming\"", "    <add key=\"BMW.Rheingold.ISTAGUI.enableENETprogramming\" value=\"true\" />"),
                    ("\"DebugLevel\"", "    <add key=\"DebugLevel\" value=\"5\" />"),
                    ("\"TesterGUI.PreferEthernet\"", "    <add key=\"TesterGUI.PreferEthernet\" value=\"true\" />"),
                    ("\"BMW.Rheingold.Programming.Prodias.LogLevel\"", "    <add key=\"BMW.Rheingold.Programming.Prodias.LogLevel\" value=\"TRACE\" />"),
                    ("\"BMW.Rheingold.xVM.ICOM.Dirtyflag.Detection\"", "    <add key=\"BMW.Rheingold.xVM.ICOM.Dirtyflag.Detection\" value=\"false\" />"),
                };

                string[] fileLines = File.ReadAllLines(fileName);
                List<string> outputLines = new List<string>();
                foreach (string line in fileLines)
                {
                    int matchIdx = -1;
                    for (int i = 0; i < patchList.Count; i++)
                    {
                        if (line.Contains(patchList[i].Match))
                        {
                            matchIdx = i;
                            break;
                        }
                    }

                    if (matchIdx >= 0)
                    {
                        Console.WriteLine("UpdateExeConfig Modify: '{0}' to '{1}'", line, patchList[matchIdx].Replace);
                        outputLines.Add(patchList[matchIdx].Replace);
                        patchList.RemoveAt(matchIdx);
                        continue;
                    }

                    if (line.Contains("</appSettings>"))
                    {
                        foreach (var patch in patchList)
                        {
                            Console.WriteLine("UpdateExeConfig Add: '{0}''", patch.Replace);
                            outputLines.Add(patch.Replace);
                        }
                    }

                    outputLines.Add(line);
                }

                File.Copy(fileName, backupFile, true);
                File.WriteAllLines(fileName, outputLines);
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateExeConfig Exception: {0}", e.Message);
                return false;
            }
            return true;
        }
    }
}
