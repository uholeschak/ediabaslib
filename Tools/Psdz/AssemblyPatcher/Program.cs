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
                        Patcher patcher = new Patcher(file, true);

                        try
                        {
                            Instruction returnInstruction = Instruction.Create(OpCodes.Ret);
                            Target target = new Target
                            {
                                Namespace = patchCtorNamespace,
                                Class = patchCtorClass,
                                Method = ".ctor",
                                Instruction = returnInstruction,
                                Index = 0,
                            };
                            Instruction[] instructions = patcher.GetInstructions(target);
                            if (instructions != null)
                            {
                                patcher.InsertInstruction(target);
                                patched = true;
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            Instruction[] return0Instructions =
                            {
                                Instruction.Create(OpCodes.Ldc_I4_0),
                                Instruction.Create(OpCodes.Ret)
                            };

                            Target target = new Target
                            {
                                Namespace = patchMethodNamespace,
                                Class = patchMethodClass,
                                Method = patchMethodName,
                                Instructions = return0Instructions,
                                Indices = new [] { 0, 1 }
                            };
                            Instruction[] instructions = patcher.GetInstructions(target);
                            if (instructions != null)
                            {
                                patcher.InsertInstruction(target);
                                patched = true;
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
                                patcher.Save(true);
                                Console.WriteLine("Patched: {0}", file);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Patch exception: File={0}, Msg={1}", file, ex.Message);
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
    }
}
