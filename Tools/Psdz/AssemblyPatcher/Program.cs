using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CommandLine;
using dnlib.DotNet.Emit;
using dnpatch;

// switch to artifacs directory layout:
// create dotnet new buildprops --use-artifacts
namespace AssemblyPatcher
{
    internal class Program
    {
        public const long FileVersion450 = (4 << 24) + (50 << 16) + 0;

        public class Options
        {
            public Options()
            {
                InputDir = string.Empty;
                DebugOpt = DebugOption.None;
                NoIcomCheck = false;
            }

            public enum DebugOption
            {
                None,
                MsgBox,
                Break,
            }

            [Option('i', "inputdir", Required = true, HelpText = "Input directory.")]
            public string InputDir { get; set; }

            [Option('d', "debug", Required = false, HelpText = "Option for debug code injection (MsgBox, Break)")]
            public DebugOption DebugOpt { get; set; }

            [Option('o', "overwrite_config", Required = false, HelpText = "Overwrite already patched config file")]
            public bool OverwriteConfig { get; set; }

            [Option('c', "no_icom_check", Required = false, HelpText = "Disable ICOM version check")]
            public bool NoIcomCheck { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                string inputDir = null;
                Options.DebugOption debugOpt = Options.DebugOption.None;
                bool overwriteConfig = false;
                bool noIcomVerCheck = true;
                bool hasErrors = false;
                Parser parser = new Parser(with =>
                {
                    //ignore case for enum values
                    with.CaseInsensitiveEnumValues = true;
                    with.EnableDashDash = true;
                    with.HelpWriter = Console.Out;
                });

                parser.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        inputDir = o.InputDir;
                        debugOpt = o.DebugOpt;
                        overwriteConfig = o.OverwriteConfig;
                        noIcomVerCheck = o.NoIcomCheck;
                    })
                    .WithNotParsed(errs =>
                    {
                        string errors = string.Join("\n", errs);
                        Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));
                        if (errors.IndexOf("BadFormatConversion", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine("Valid debug options are: {0}", string.Join(", ", Enum.GetNames(typeof(Options.DebugOption)).ToList()));
                        }

                        hasErrors = true;
                    });

                if (hasErrors)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(inputDir) || !Directory.Exists(inputDir))
                {
                    Console.WriteLine("Directory not existing: {0}", inputDir);
                    return 1;
                }

                Console.WriteLine("Input directory: '{0}'", inputDir);
                Console.WriteLine("Debug option: '{0}'", debugOpt.ToString());
                Console.WriteLine("Overwrite config: '{0}'", overwriteConfig.ToString());
                Console.WriteLine("Disable ICOM version check: '{0}'", noIcomVerCheck.ToString());

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

                string patchMethod1Namespace = ConfigurationManager.AppSettings["PatchMethod1Namespace"];
                if (string.IsNullOrEmpty(patchMethod1Namespace))
                {
                    Console.WriteLine("PatchMethod1Namespace not configured");
                    return 1;
                }

                string patchMethod1Class = ConfigurationManager.AppSettings["PatchMethod1Class"];
                if (string.IsNullOrEmpty(patchMethod1Class))
                {
                    Console.WriteLine("PatchMethod1Class not configured");
                    return 1;
                }

                string patchMethod1Name = ConfigurationManager.AppSettings["PatchMethod1Name"];
                if (string.IsNullOrEmpty(patchMethod1Name))
                {
                    Console.WriteLine("PatchMethod1Name not configured");
                    return 1;
                }

                string patchMethod1Name2 = ConfigurationManager.AppSettings["PatchMethod1Name2"];
                if (string.IsNullOrEmpty(patchMethod1Name2))
                {
                    Console.WriteLine("Warning: PatchMethod1Name2 not configured");
                }

                string patchMethod2Namespace = ConfigurationManager.AppSettings["PatchMethod2Namespace"];
                if (string.IsNullOrEmpty(patchMethod2Namespace))
                {
                    Console.WriteLine("PatchMethod2Namespace not configured");
                    return 1;
                }

                string patchMethod2Class = ConfigurationManager.AppSettings["PatchMethod2Class"];
                if (string.IsNullOrEmpty(patchMethod2Class))
                {
                    Console.WriteLine("PatchMethod2Class not configured");
                    return 1;
                }

                string patchMethod2Name = ConfigurationManager.AppSettings["PatchMethod2Name"];
                if (string.IsNullOrEmpty(patchMethod2Name))
                {
                    Console.WriteLine("PatchMethod2Name not configured");
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

                // Stored in HKEY_CURRENT_USER\Software\BMWGroup\ISPI\Rheingold\License
                string licFileSrc = Path.Combine(appDir, "Data", licFileName);
                string licFileDst = Path.Combine(inputDir, licFileName);
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

                string exeFile = Path.Combine(inputDir, "ISTAGUI.exe");
                if (!UpdateExeConfig(exeFile, noIcomVerCheck, overwriteConfig))
                {
                    Console.WriteLine("Update config file failed for: {0}", exeFile);
                    return 1;
                }

                string[] files = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string relPath = GetRelativePath(inputDir, file);
                    if (string.IsNullOrEmpty(relPath))
                    {
                        continue;
                    }

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

                    if (relPath.StartsWith("runtimes") ||
                        relPath.StartsWith("x86") ||
                        relPath.StartsWith("x64"))
                    {
                        continue;
                    }

                    if (baseName.StartsWith("Google", StringComparison.OrdinalIgnoreCase) ||
                        baseName.StartsWith("Grpc", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (baseName.EndsWith("interop", StringComparison.OrdinalIgnoreCase) ||
                        baseName.EndsWith("IDESKernel", StringComparison.OrdinalIgnoreCase) ||
                        baseName.EndsWith("procdump", StringComparison.OrdinalIgnoreCase) ||
                        baseName.EndsWith("WebView2Loader", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!File.Exists(file))
                    {
                        Console.WriteLine("Assembly not existing: {0}", file);
                        return 1;
                    }

                    string assemblyPathBak = Path.Combine(inputDir, file + ".bak");
                    if (File.Exists(assemblyPathBak))
                    {
                        Console.WriteLine("Assembly already patched: {0}", file);
                        continue;
                    }

                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(file);
                    long? fileVersion = null;
                    string companyName = fvi?.CompanyName ?? string.Empty;
                    string legalCopyright = fvi?.LegalCopyright ?? string.Empty;
                    string versionString = null;
                    if (!string.IsNullOrEmpty(fvi?.FileVersion))
                    {
                        if (companyName.IndexOf("BMW", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            legalCopyright.IndexOf("Bayerische", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            fileVersion = (fvi.FileMajorPart << 24) + (fvi.FileMinorPart << 16) + fvi.FileBuildPart;
                            versionString = string.Format(CultureInfo.InvariantCulture,  "{0}.{1}.{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
                        }
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
                                Console.WriteLine("{0}.ctor patched", patchCtorClass);
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
                                Namespace = patchMethod1Namespace,
                                Class = patchMethod1Class,
                                Method = patchMethod1Name,
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_0));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("{0}.{1} patched", patchMethod1Class, patchMethod1Name);
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (!string.IsNullOrEmpty(patchMethod1Name2))
                        {   // optional
                            try
                            {
                                Target target = new Target
                                {
                                    Namespace = patchMethod1Namespace,
                                    Class = patchMethod1Class,
                                    Method = patchMethod1Name2,
                                };
                                IList<Instruction> instructions = patcher.GetInstructionList(target);
                                if (instructions != null)
                                {
                                    instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_0));
                                    instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                    patched = true;
                                    Console.WriteLine("{0}.{1} patched", patchMethod1Class, patchMethod1Name2);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                        }

                        try
                        {
                            Target target = new Target
                            {
                                Namespace = patchMethod2Namespace,
                                Class = patchMethod2Class,
                                Method = patchMethod2Name,
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("{0}.{1} patched", patchMethod2Class, patchMethod2Name);
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
                                Namespace = "BMW.Rheingold.Programming.States",
                                Class = "TherapyPlanCalculated",
                                Method = "IsConnectedViaENETAndBrandIsToyota",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                // Hard coded "BMW.Rheingold.ISTAGUI.enableENETprogramming", not option required
                                Console.WriteLine("TherapyPlanCalculated.IsConnectedViaENETAndBrandIsToyota found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("IsConnectedViaENETAndBrandIsToyota patched");
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
                                Namespace = "BMW.iLean.CommonServices.Models.Helper",
                                Class = "Encryption",
                                Method = "EncryptSensitveContent",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("EncryptSensitveContent.EncryptSensitveContent found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("EncryptSensitveContent patched");
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
                                Namespace = "BMW.Rheingold.Programming",
                                Class = "ConnectionManager",
                                Method = "UseTheDoipPort",
                            };

                            Target targetTemplate = new Target
                            {
                                Namespace = "BMW.Rheingold.Programming",
                                Class = "ConnectionManager",
                                Method = "ConnectToProject",
                                Parameters = new[] { "ConnectionManager", "String", "String", "Boolean" },
                            };

                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            IList<Local> locals = patcher.GetVariableList(target);
                            IList<Instruction> instructionsTemplate = patcher.GetInstructionList(targetTemplate);
                            if (instructions != null && instructionsTemplate != null)
                            {
                                Console.WriteLine("ConnectionManager.UseTheDoipPort found");
                                int patchIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldarg_0
                                        && index + 2 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Callvirt)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 2].OpCode != OpCodes.Ret)
                                        {
                                            continue;
                                        }

                                        Console.WriteLine("get_IsEEES25Vehicle found at index: {0}", index);
                                        patchIndex = index + 1;
                                        break;
                                    }
                                }

                                int templateIndex = -1;
                                for (int index = 0; index < instructionsTemplate.Count; index++)
                                {
                                    Instruction instruction = instructionsTemplate[index];
                                    if (instruction.OpCode == OpCodes.Ldarg_0
                                        && index + 5 < instructionsTemplate.Count)
                                    {
                                        if (instructionsTemplate[index + 1].OpCode != OpCodes.Callvirt)
                                        {
                                            continue;
                                        }
                                        if (instructionsTemplate[index + 2].OpCode != OpCodes.Stloc_3)
                                        {
                                            continue;
                                        }
                                        if (instructionsTemplate[index + 3].OpCode != OpCodes.Ldloca_S)
                                        {
                                            continue;
                                        }
                                        if (instructionsTemplate[index + 4].OpCode != OpCodes.Call)
                                        {
                                            continue;
                                        }
                                        if (instructionsTemplate[index + 5].OpCode != OpCodes.Callvirt)
                                        {
                                            continue;
                                        }

                                        Console.WriteLine("get_IsDoIP found at index: {0}", index);
                                        templateIndex = index + 1;
                                        break;
                                    }
                                }

                                if (templateIndex < 0)
                                {
                                    Console.WriteLine("get_IsDoIP template not found");
                                }

                                if (patchIndex >= 0 && templateIndex >= 0)
                                {
                                    List<Instruction> insertInstructions = new List<Instruction>();
                                    insertInstructions.Add(instructionsTemplate[templateIndex + 0].Clone());    // callvirt get_IsDoIP()
                                    insertInstructions.Add(new Instruction(OpCodes.Stloc_0));

                                    Local localTemp1 = instructionsTemplate[templateIndex + 2].Operand as Local;
                                    Local local1 = new Local(localTemp1.Type);
                                    locals.Add(local1);
                                    insertInstructions.Add(new Instruction(OpCodes.Ldloca_S, local1));

                                    insertInstructions.Add(instructionsTemplate[templateIndex + 3].Clone());    // call get_Value()

                                    instructions.RemoveAt(patchIndex);  // callvirt
                                    int offset = 0;
                                    foreach (Instruction insertInstruction in insertInstructions)
                                    {
                                        instructions.Insert(patchIndex + offset, insertInstruction);
                                        offset++;
                                    }

                                    Console.WriteLine("ConnectionManager UseTheDoipPort patched");
                                    patched = true;
                                }
                                else
                                {
                                    Console.WriteLine("UseTheDoipPort appears to have already been patched or is not existing");
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
                                Namespace = "BMW.Rheingold.PresentationFramework.AuthenticationRefactored.Services",
                                Class = "LoginEnabledOptionProvider",
                                Method = "IsLoginEnabled",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                // Hard coded "BMW.Rheingold.ISTAGUI.enableENETprogramming", not option required
                                Console.WriteLine("LoginEnabledOptionProvider.IsLoginEnabled found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_0));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("IsLoginEnabled patched");
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
                                Namespace = "BMW.Rheingold.CoreFramework.InteropHelper",
                                Class = "VerifyAssemblyHelper",
                                Method = "VerifyStrongName",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("VerifyAssemblyHelper.VerifyStrongName found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("VerifyStrongName patched");
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
                                Namespace = "BMW.ISPI.IstaServices.Client",
                                Class = "IstaIcsServiceClient",
                                Method = "ValidateHost",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("IstaIcsServiceClient.ValidateHost found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("ValidateHost patched");
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
                                Namespace = "BMW.ISPI.IstaServices.Client",
                                Class = "IstaIcsServiceClient",
                                Method = "VerifyLicense",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("IstaIcsServiceClient.VerifyLicense found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("VerifyLicense patched");
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
                                Namespace = "BMW.Rheingold.ISTAGUI.Controller",
                                Class = "IstaInstallationRequirements",
                                Method = "CheckInstallationStatus",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("IstaInstallationRequirements.CheckInstallationStatus found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                                instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("CheckInstallationStatus patched");
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
                                Namespace = "BMW.Rheingold.ISTAGUI.Controller",
                                Class = "PackageValidityService",
                                Method = "CheckPackageValidity",
                                Parameters = new[] { "PackageValidityService", "Version" },
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("PackageValidityService.CheckPackageValidity found");
                                instructions.Insert(0, Instruction.Create(OpCodes.Ret));
                                patched = true;
                                Console.WriteLine("CheckPackageValidity patched");
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
                                if (fileVersion != null && fileVersion.Value < FileVersion450)
                                {
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
                                            Console.WriteLine("InitVCI patched");
                                            //patcher.Save(file.Replace(".dll", "Test.dll"));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("'ENET::remotehost=' appears to have already been patched or is not existing");
                                    }
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
                                Namespace = "BMW.Rheingold.VehicleCommunication",
                                Class = "ECUKom",
                                Method = "InitVCI",
                                Parameters = new[] { "ECUKom", "IVciDevice", "Boolean", "Boolean" },
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("ECUKom.InitVCI isDoIP found");
                                int patchIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldstr &&
                                        string.Compare(instruction.Operand.ToString(), "ENET", StringComparison.OrdinalIgnoreCase) == 0
                                        && index + 3 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Ldstr || string.Compare(instructions[index + 1].Operand.ToString(), "_", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 2].OpCode != OpCodes.Ldstr || string.Compare(instructions[index + 2].Operand.ToString(), "Rheingold", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 3].OpCode != OpCodes.Ldstr || string.Compare(instructions[index + 3].Operand.ToString(), "", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            continue;
                                        }

                                        Console.WriteLine("\"ENET\", \"_\", \"Rheingold\", \"\" found at index: {0}", index);
                                        patchIndex = index + 3;
                                        break;
                                    }
                                }

                                int templateIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldstr &&
                                        string.Compare(instruction.Operand.ToString(), "RPLUS:ICOM_P:Remotehost=", StringComparison.OrdinalIgnoreCase) == 0
                                        && index + 5 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Ldloc_0)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 2].OpCode != OpCodes.Ldfld)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 3].OpCode != OpCodes.Callvirt)     // get_IPAddress()
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 4].OpCode != OpCodes.Ldstr)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 5].OpCode != OpCodes.Call)         // Conact()
                                        {
                                            continue;
                                        }

                                        Console.WriteLine("Format template found at index: {0}", index);
                                        templateIndex = index;
                                        break;
                                    }
                                }

                                if (templateIndex < 0)
                                {
                                    Console.WriteLine("Format template not found");
                                }

                                if (patchIndex >= 0 && templateIndex >= 0)
                                {
                                    List<Instruction> insertInstructions = new List<Instruction>();
                                    insertInstructions.Add(new Instruction(OpCodes.Ldstr, "RemoteHost="));
                                    insertInstructions.Add(new Instruction(OpCodes.Ldloc_0));
                                    insertInstructions.Add(instructions[templateIndex + 2].Clone());    // get_IPAddress()
                                    insertInstructions.Add(instructions[templateIndex + 3].Clone());    // get_IPAddress()
                                    insertInstructions.Add(new Instruction(OpCodes.Ldstr, ";DiagnosticPort=6801;ControlPort=6811"));
                                    insertInstructions.Add(instructions[templateIndex + 5].Clone());    // Concat()

                                    instructions.RemoveAt(patchIndex);
                                    int offset = 0;
                                    foreach (Instruction insertInstruction in insertInstructions)
                                    {
                                        instructions.Insert(patchIndex + offset, insertInstruction);
                                        offset++;
                                    }
                                    patched = true;
                                    Console.WriteLine("InitVCI isDoIP patched");
                                }
                                else
                                {
                                    Console.WriteLine("\"ENET\", \"_\", \"Rheingold\", \"\" appears to have already been patched or is not existing");
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
                                Namespace = "BMW.Rheingold.Psdz.Client",
                                Class = "PsdzServiceStarter",
                                Method = "StartServerInstance",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("PsdzServiceStarter.StartServerInstance found");
                                bool alreadyCorrected = false;
                                int patchIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldstr &&
                                        string.Compare(instruction.Operand.ToString(), "\"{0}\" {1} \"{2}\"", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        Console.WriteLine("Arguments three param found at index: {0}", index);
                                        patchIndex = index;
                                        break;
                                    }

                                    if (instruction.OpCode == OpCodes.Ldstr &&
                                        string.Compare(instruction.Operand.ToString(), "\"{0}\" \"{1}\" \"{2}\"", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        alreadyCorrected = true;
                                        break;
                                    }
                                }

                                if (patchIndex >= 0)
                                {
                                    instructions[patchIndex].Operand = "\"{0}\" \"{1}\" \"{2}\"";
                                    patched = true;
                                    Console.WriteLine("StartServerInstance patched");
                                }
                                else
                                {
                                    if (alreadyCorrected)
                                    {
                                        Console.WriteLine("StartServerInstance already fixed");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Patching StartServerInstance failed");
                                    }
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
                                Namespace = "BMW.Rheingold.Psdz.Client",
                                Class = "PsdzServiceStarter",
                                Method = "checkForPsdzInstancesLogFile",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("PsdzServiceStarter.checkForPsdzInstancesLogFile found");
                                int patchIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldsfld &&
                                        index + 2 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Call)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 2].OpCode != OpCodes.Pop)
                                        {
                                            continue;
                                        }
                                        if (instructions[index + 3].OpCode != OpCodes.Ret)
                                        {
                                            continue;
                                        }

                                        Console.WriteLine("File.Create found at index: {0}", index);
                                        patchIndex = index + 2;
                                        break;
                                    }
                                }

                                if (patchIndex >= 0)
                                {
                                    instructions[patchIndex] = Instruction.Create(OpCodes.Callvirt,
                                        patcher.BuildCall(typeof(System.IO.Stream), "Close", typeof(void), null));
                                    patched = true;
                                    Console.WriteLine("checkForPsdzInstancesLogFile patched");
                                }
                                else
                                {
                                    Console.WriteLine("Pathing File.Create failed");
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
                                Namespace = "BMW.Rheingold.Diagnostics",
                                Class = "VehicleIdent",
                                Method = "doVehicleShortTest",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("VehicleIdent.doVehicleShortTest found");
                                int removeIndex = -1;
                                int getBnTypeIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Ldarg_0 && index + 12 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Call)         // get_VecInfo
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 2].OpCode != OpCodes.Callvirt)     // get_Classification
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 3].OpCode != OpCodes.Callvirt)     // IsPreDS2Vehicle
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 4].OpCode != OpCodes.Brtrue_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 5].OpCode != OpCodes.Ldarg_0)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 6].OpCode != OpCodes.Call)         // get_VecInfo
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 7].OpCode != OpCodes.Callvirt)     // get_BNType
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 8].OpCode != OpCodes.Ldc_I4_2)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 9].OpCode != OpCodes.Bne_Un_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 10].OpCode != OpCodes.Ldarg_0)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 11].OpCode != OpCodes.Ldc_I4_0)     // false
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 12].OpCode != OpCodes.Call)     // HandleMissingEcus
                                        {
                                            continue;
                                        }

                                        removeIndex = index;
                                        getBnTypeIndex = index + 5;
                                        break;
                                    }
                                }

                                if (removeIndex < 0 || getBnTypeIndex < 0)
                                {
                                    Console.WriteLine("HandleMissingEcus not found");
                                }

                                int insertIndex = -1;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Callvirt && index + 7 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Brtrue_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 2].OpCode != OpCodes.Ldloc_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 3].OpCode != OpCodes.Ldc_I4_0)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 4].OpCode != OpCodes.Callvirt)     // set_IDENT_SUCCESSFULLY
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 5].OpCode != OpCodes.Ldloc_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 6].OpCode != OpCodes.Ldc_I4_0)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 7].OpCode != OpCodes.Callvirt)     // set_FS_SUCCESSFULLY
                                        {
                                            continue;
                                        }

                                        insertIndex = index + 2;
                                        break;
                                    }
                                }

                                if (insertIndex < 0)
                                {
                                    Console.WriteLine("set_IDENT_SUCCESSFULLY not found");
                                }

                                if (removeIndex >= 0 && insertIndex >= 0)
                                {
                                    //  copy Ldarg_0, Call, Callvirt, Ldc_I4_2
                                    List<Instruction> insertInstructions = new List<Instruction>();
                                    int pos;
                                    for (pos = 0; pos < 4; pos++)
                                    {
                                        Instruction instruction = instructions[getBnTypeIndex + pos];
                                        insertInstructions.Add(instruction.Clone());
                                    }
                                    insertInstructions.Add(new Instruction(OpCodes.Beq_S, instructions[insertIndex + 3]));

                                    int offset = 0;
                                    foreach (Instruction insertInstruction in insertInstructions)
                                    {
                                        instructions.Insert(insertIndex + offset, insertInstruction);
                                        offset++;
                                    }

                                    for (int idx = 0; idx < 13; idx++)
                                    {
                                        instructions.RemoveAt(removeIndex + offset);
                                    }

                                    patched = true;
                                    Console.WriteLine("doVehicleShortTest patched");
                                }
                                else
                                {
                                    Console.WriteLine("Patching doVehicleShortTest failed");
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
                                Namespace = "BMW.Rheingold.Diagnostics",
                                Class = "VehicleIdent",
                                Method = "ClearAndReadErrorInfoMemory",
                            };
                            IList<Instruction> instructions = patcher.GetInstructionList(target);
                            if (instructions != null)
                            {
                                Console.WriteLine("VehicleIdent.ClearAndReadErrorInfoMemory found");
                                int patchIndex = -1;
                                object operand = null;
                                for (int index = 0; index < instructions.Count; index++)
                                {
                                    Instruction instruction = instructions[index];
                                    if (instruction.OpCode == OpCodes.Call && index + 7 < instructions.Count)
                                    {
                                        if (instructions[index + 1].OpCode != OpCodes.Callvirt)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 2].OpCode != OpCodes.Callvirt)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 3].OpCode != OpCodes.Stloc_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 4].OpCode != OpCodes.Ldloc_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 5].OpCode != OpCodes.Brfalse_S)    // copy offset from here
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 6].OpCode != OpCodes.Ldloca_S)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 7].OpCode != OpCodes.Call)
                                        {
                                            continue;
                                        }

                                        if (instructions[index + 8].OpCode != OpCodes.Brfalse_S)
                                        {
                                            continue;
                                        }

                                        patchIndex = index + 8;
                                        operand = instructions[index + 5].Operand;
                                        break;
                                    }
                                }

                                if (patchIndex >= 0 && operand != null)
                                {
                                    instructions[patchIndex].Operand = operand;
                                    patched = true;
                                    Console.WriteLine("Disabled NoClamp15ForErrorMemory message if clamp15 is not readable");
                                }
                                else
                                {
                                    Console.WriteLine("Patching ClearAndReadErrorInfoMemory failed");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (noIcomVerCheck)
                        {
                            try
                            {
                                Target target = new Target
                                {
                                    Namespace = "BMW.Rheingold.xVM",
                                    Class = "SLP",
                                    Method = "IsIcomUnsupported",
                                };
                                IList<Instruction> instructions = patcher.GetInstructionList(target);
                                if (instructions != null)
                                {
                                    Console.WriteLine("SLP.IsIcomUnsupported found");
                                    instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4_0));
                                    instructions.Insert(1, Instruction.Create(OpCodes.Ret));
                                    patched = true;
                                    Console.WriteLine("IsIcomUnsupported patched");
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
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
                                        switch (debugOpt)
                                        {
                                            case Options.DebugOption.MsgBox:
                                                if (!patcher.InsertDebugMessageBox(ref instructions, patchIndex, "IstaOperation started. Attach to IstaOperation.exe now.", "ISTAGUI"))
                                                {
                                                    Console.WriteLine("Patch InsertDebugMessageBox failed");
                                                }
                                                else
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine("To show the message box at startup:");
                                                    Console.WriteLine("In dnSpy disable the ignore options: IsDebuggerPresent and System.Diagnostics.Debugger");
                                                    Console.WriteLine();
                                                }
                                                break;

                                            case Options.DebugOption.Break:
                                                instructions.Insert(patchIndex,
                                                    Instruction.Create(OpCodes.Call,
                                                        patcher.BuildCall(typeof(System.Diagnostics.Debugger), "get_IsAttached", typeof(bool), null)));
                                                instructions.Insert(patchIndex + 1, Instruction.Create(OpCodes.Brfalse_S, instructions[patchIndex + 1]));
                                                instructions.Insert(patchIndex + 2, Instruction.Create(OpCodes.Break));
                                                Console.WriteLine();
                                                Console.WriteLine("When running in debugger attach IstaOperation.exe when the break point has been reached");
                                                Console.WriteLine();
                                                break;
                                        }

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

                        if (patched)
                        {
                            try
                            {
#if true
                                patcher.Save(true);
#endif
                                Console.WriteLine("Patched: {0} Version={1} '{2}'", relPath, versionString ?? string.Empty, companyName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Patch exception: File={0}, Msg={1}", relPath, ex.Message);
                            }
                        }
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

        static bool UpdateExeConfig(string exeFileName, bool noIcomVerCheck, bool overwriteConfig)
        {
            try
            {
                if (!File.Exists(exeFileName))
                {
                    Console.WriteLine("UpdateExeConfig Executable file not existing: {0}", exeFileName);
                    return false;
                }

                string configFileName = exeFileName + ".config";
                if (!File.Exists(configFileName))
                {
                    Console.WriteLine("UpdateExeConfig Config file not existing: {0}", configFileName);
                    return false;
                }

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(exeFileName);
                long? fileVersion = null;
                string companyName = fvi?.CompanyName ?? string.Empty;
                string legalCopyright = fvi?.LegalCopyright ?? string.Empty;
                if (!string.IsNullOrEmpty(fvi?.FileVersion))
                {
                    if (companyName.IndexOf("BMW", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        legalCopyright.IndexOf("Bayerische", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        fileVersion = (fvi.FileMajorPart << 24) + (fvi.FileMinorPart << 16) + fvi.FileBuildPart;
                    }
                }

                if (fileVersion == null)
                {
                    Console.WriteLine("UpdateExeConfig Missing file version for: {0}", exeFileName);
                    return false;
                }

                string backupFile = configFileName + ".bak";
                if (File.Exists(backupFile))
                {
                    if (overwriteConfig)
                    {
                        Console.WriteLine("UpdateExeConfig Overwriting modified config file: {0}", configFileName);
                    }
                    else
                    {
                        Console.WriteLine("UpdateExeConfig Config file already modified: {0}", configFileName);
                        return true;
                    }
                }

                string dirtyFlagValue = noIcomVerCheck ? "false" : "true";
                var patchList = new List<(string Match, string Replace)>()
                {
                    ("\"DebugLevel\"", "    <add key=\"DebugLevel\" value=\"5\" />"),
                    ("\"BMW.Rheingold.Programming.Prodias.LogLevel\"", "    <add key=\"BMW.Rheingold.Programming.Prodias.LogLevel\" value=\"TRACE\" />"),
                    ("\"BMW.Rheingold.RheingoldSessionController.FASTATransferMode\"", "    <add key=\"BMW.Rheingold.RheingoldSessionController.FASTATransferMode\" value=\"None\" />"),
                    ("\"BMW.Rheingold.Diagnostics.VehicleIdent.ReadFASTAData\"", "    <add key=\"BMW.Rheingold.Diagnostics.VehicleIdent.ReadFASTAData\" value=\"false\" />"),
                    ("\"BMW.Rheingold.OperationalMode\"", "    <add key=\"BMW.Rheingold.OperationalMode\" value=\"ISTA_PLUS\" />"),
                    ("\"BMW.Rheingold.ISTAGUI.Pages.StartPage.ShowDisclaimer\"", "    <add key=\"BMW.Rheingold.ISTAGUI.Pages.StartPage.ShowDisclaimer\" value=\"false\" />"),
                    ("\"BMW.Rheingold.ISTAGUI.App.DoInitialIpsAvailabilityCheck\"", "    <add key=\"BMW.Rheingold.ISTAGUI.App.DoInitialIpsAvailabilityCheck\" value=\"false\" />"),
                    ("\"BMW.Rheingold.Programming.ExpertMode\"", "    <add key=\"BMW.Rheingold.Programming.ExpertMode\" value=\"true\" />"),
                    ("\"BMW.Rheingold.ISTAGUI.ShowHiddenDiagnosticObjects\"", "    <add key=\"BMW.Rheingold.ISTAGUI.ShowHiddenDiagnosticObjects\" value=\"true\" />"),
                    ("\"BMW.Rheingold.OnlineMode\"", "    <add key=\"BMW.Rheingold.OnlineMode\" value=\"false\" />"),
                    ("\"BMW.Rheingold.UseIdentNuget\"", "    <add key=\"BMW.Rheingold.UseIdentNuget\" value=\"false\" />"),
                    ("\"BMW.Rheingold.Programming.PsdzWebservice.Enabled\"", "    <add key=\"BMW.Rheingold.Programming.PsdzWebservice.Enabled\" value=\"false\" />"),
                    ("\"BMW.Rheingold.PsdzWebservice_Activate\"", "    <add key=\"BMW.Rheingold.PsdzWebservice_Activate\" value=\"false\" />"),
                    ("\"BMW.Rheingold.UseJreWithTLS13Support_Activate\"", "    <add key=\"BMW.Rheingold.UseJreWithTLS13Support_Activate\" value=\"true\" />"),
                    ("\"BMW.Rheingold.Programming.Sdp.Patch.Enabled\"", "    <add key=\"BMW.Rheingold.Programming.Sdp.Patch.Enabled\" value=\"false\" />"),
                    ("\"BMW.Rheingold.Diagnostics.EnableRsuProcessHandling\"", "    <add key=\"BMW.Rheingold.Diagnostics.EnableRsuProcessHandling\" value=\"false\" />"),
                    ("\"BMW.Rheingold.ISTAGUI.Dialogs.AdministrationDialog.ShowPTTSelection\"", "    <add key=\"BMW.Rheingold.ISTAGUI.Dialogs.AdministrationDialog.ShowPTTSelection\" value=\"true\" />"),
                    ("\"BMW.Rheingold.CoreFramework.TRICZentralActive\"", "    <add key=\"BMW.Rheingold.CoreFramework.TRICZentralActive\" value=\"false\" />"),
                    ("\"EnableRelevanceFaultCode\"", "    <add key=\"EnableRelevanceFaultCode\" value=\"false\" />"),
                    ("\"BMW.Rheingold.Developer.guidebug\"", "    <add key=\"BMW.Rheingold.Developer.guidebug\" value=\"true\" />"),
                    ("\"BMW.Rheingold.ISTAGUI.App.MultipleInstancesAllowed\"", "    <add key=\"BMW.Rheingold.ISTAGUI.App.MultipleInstancesAllowed\" value=\"false\" />"),
                    ("\"BMW.Rheingold.xVM.ICOM.Dirtyflag.Detection\"", $"    <add key=\"BMW.Rheingold.xVM.ICOM.Dirtyflag.Detection\" value=\"{dirtyFlagValue}\" />"),
                };

                if (fileVersion.Value < FileVersion450)
                {
                    // hard coded, not option required
                    patchList.Add(("\"BMW.Rheingold.ISTAGUI.enableENETprogramming\"", "    <add key=\"BMW.Rheingold.ISTAGUI.enableENETprogramming\" value=\"true\" />"));
                    // not existing anymore
                    patchList.Add(("\"TesterGUI.PreferEthernet\"", "    <add key=\"TesterGUI.PreferEthernet\" value=\"true\" />"));
                }

                string[] fileLines = File.ReadAllLines(configFileName);
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

                if (!File.Exists(backupFile))
                {
                    File.Copy(configFileName, backupFile, false);
                }

                File.WriteAllLines(configFileName, outputLines);
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateExeConfig Exception: {0}", e.Message);
                return false;
            }
            return true;
        }
        public static string GetRelativePath(string basePath, string fullPath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
            {
                basePath += "\\";
            }

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");
        }
    }
}
