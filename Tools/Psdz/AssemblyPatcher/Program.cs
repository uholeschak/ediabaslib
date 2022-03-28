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

                string patchNamespace = ConfigurationManager.AppSettings["PatchNamespace"];
                if (string.IsNullOrEmpty(patchNamespace))
                {
                    Console.WriteLine("PatchNamespace not configured");
                    return 1;
                }

                string patchClass = ConfigurationManager.AppSettings["PatchClass"];
                if (string.IsNullOrEmpty(patchClass))
                {
                    Console.WriteLine("PatchClass not configured");
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
                        Patcher patcher = new Patcher(file, true);
                        Instruction returnInstruction = Instruction.Create(OpCodes.Ret);
                        Target target = new Target
                        {
                            Namespace = patchNamespace,
                            Class = patchClass,
                            Method = ".ctor",
                            Instruction = returnInstruction,
                            Index = 0,
                        };

                        Instruction[] instructions = patcher.GetInstructions(target);
                        if (instructions != null)
                        {
                            patcher.ReplaceInstruction(target);
                            patcher.Save(true);
                            Console.WriteLine("Patched: {0}", file);
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
    }
}
