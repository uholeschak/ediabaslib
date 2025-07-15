using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace dnpatch
{
    internal class PatchHelper
    {
        public readonly ModuleDef Module;
        private readonly string _file;
        private readonly bool _keepOldMaxStack = false;

        public PatchHelper(string file)
        {
            _file = file;
            Module = ModuleDefMD.Load(file);
        }

        public PatchHelper(string file, bool keepOldMaxStack)
        {
            _file = file;
            Module = ModuleDefMD.Load(file);
            _keepOldMaxStack = keepOldMaxStack;
        }

        public PatchHelper(ModuleDefMD module, bool keepOldMaxStack)
        {
            Module = module;
            _keepOldMaxStack = keepOldMaxStack;
        }

        public PatchHelper(ModuleDef module, bool keepOldMaxStack)
        {
            Module = module;
            _keepOldMaxStack = keepOldMaxStack;
        }

        public PatchHelper(Stream stream, bool keepOldMaxStack)
        {
            Module = ModuleDefMD.Load(stream);
            _keepOldMaxStack = keepOldMaxStack;
        }

        public  void PatchAndClear(Target target)
        {
            string[] nestedClasses = { };
            if (target.NestedClasses != null)
            {
                nestedClasses = target.NestedClasses;
            }
            else if (target.NestedClass != null)
            {
                nestedClasses = new[] { target.NestedClass };
            }
            var type = FindType(target.Namespace + "." + target.Class, nestedClasses);
            var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                throw new Exception("Method not found");
            }
            var instructions = method.Body.Instructions;
            instructions.Clear();
            if (target.Instructions != null)
            {
                for (int i = 0; i < target.Instructions.Length; i++)
                {
                    instructions.Insert(i, target.Instructions[i]);
                }
            }
            else
            {
                instructions.Insert(0, target.Instruction);
            }
        }

        public  void PatchOffsets(Target target)
        {
            string[] nestedClasses = { };
            if (target.NestedClasses != null)
            {
                nestedClasses = target.NestedClasses;
            }
            else if (target.NestedClass != null)
            {
                nestedClasses = new[] { target.NestedClass };
            }
            var type = FindType(target.Namespace + "." + target.Class, nestedClasses);
            var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                throw new Exception("Method not found");
            }
            var instructions = method.Body.Instructions;
            if (target.Indices != null && target.Instructions != null)
            {
                for (int i = 0; i < target.Indices.Length; i++)
                {
                    instructions[target.Indices[i]] = target.Instructions[i];
                }
            }
            else if (target.Index != -1 && target.Instruction != null)
            {
                instructions[target.Index] = target.Instruction;
            }
            else if (target.Index == -1)
            {
                throw new Exception("No index specified");
            }
            else if (target.Instruction == null)
            {
                throw new Exception("No instruction specified");
            }
            else if (target.Indices == null)
            {
                throw new Exception("No Indices specified");
            }
            else if (target.Instructions == null)
            {
                throw new Exception("No instructions specified");
            }
        }

        public  TypeDef FindType(string classPath, string[] nestedClasses)
        {
            if (classPath.First() == '.')
                classPath = classPath.Remove(0, 1);
            foreach (var module in Module.Assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.FullName == classPath)
                    {
                        TypeDef t = null;
                        if (nestedClasses != null && nestedClasses.Length > 0)
                        {
                            foreach (var nc in nestedClasses)
                            {
                                if (t == null)
                                {
                                    if (!type.HasNestedTypes) continue;
                                    foreach (var typeN in type.NestedTypes)
                                    {
                                        if (typeN.Name == nc)
                                        {
                                            t = typeN;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!t.HasNestedTypes) continue;
                                    foreach (var typeN in t.NestedTypes)
                                    {
                                        if (typeN.Name == nc)
                                        {
                                            t = typeN;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            t = type;
                        }
                        return t;
                    }
                }
            }
            return null;
        }

        public PropertyDef FindProperty(TypeDef type, string property)
        {
            return type.Properties.FirstOrDefault(prop => prop.Name == property);
        }

        public  MethodDef FindMethod(TypeDef type, string methodName, string[] parameters, string returnType)
        {
            if (type == null)
            {
                return null;
            }

            bool checkParams = parameters != null;
            foreach (var m in type.Methods)
            {
                bool isMethod = true;
                if (checkParams && parameters.Length != m.Parameters.Count) continue;
                if (methodName != m.Name) continue;
                if (!string.IsNullOrEmpty(returnType) && returnType != m.ReturnType.TypeName) continue;
                if (checkParams)
                {
                    if (m.Parameters.Where((param, i) => param.Type.TypeName != parameters[i]).Any())
                    {
                        isMethod = false;
                    }
                }
                if(isMethod) return m;
            }
            return null;
        }

        public  Target FixTarget(Target target)
        {
            target.Indices = new int[] { };
            target.Index = -1;
            target.Instruction = null;
            return target;
        }

        public  void Save(string name)
        {
            if (_keepOldMaxStack)
            {
                Module.Write(name, new ModuleWriterOptions(Module)
                {
                    MetadataOptions =
                    {
                        Flags = MetadataFlags.KeepOldMaxStack
                    }
                });
            }
            else
            {
                Module.Write(name);
            }
        }

        public  void Save(bool backup)
        {
            if (string.IsNullOrEmpty(_file))
            {
                throw new Exception("Assembly/module was loaded in memory, and no file was specified. Use Save(string) method to save the patched assembly.");
            }

            if (_keepOldMaxStack)
            {
                Module.Write(_file + ".tmp", new ModuleWriterOptions(Module)
                {
                    MetadataOptions =
                    {
                        Flags = MetadataFlags.KeepOldMaxStack
                    }
                });
            }
            else
            {
                Module.Write(_file + ".tmp");
            }

            Module.Dispose();
            if (backup)
            {
                if (File.Exists(_file + ".bak"))
                {
                    File.Delete(_file + ".bak");
                }
                File.Move(_file, _file + ".bak");
            }
            else
            {
                File.Delete(_file);
            }
            File.Move(_file + ".tmp", _file);
        }

        public Target[] FindInstructionsByOperand(string[] operand)
        {
            List<ObfuscatedTarget> obfuscatedTargets = new List<ObfuscatedTarget>();
            List<string> operands = operand.ToList();
            foreach (var type in Module.Types)
            {
                if (!type.HasNestedTypes)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Body != null)
                        {
                            List<int> indexList = new List<int>();
                            var obfuscatedTarget = new ObfuscatedTarget()
                            {
                                Type = type,
                                Method = method
                            };
                            int i = 0;
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.Operand != null)
                                {
                                    if (operands.Contains(instruction.Operand.ToString()))
                                    {
                                        indexList.Add(i);
                                        operands.Remove(instruction.Operand.ToString());
                                    }
                                }
                                i++;
                            }
                            if (indexList.Count == operand.Length)
                            {
                                obfuscatedTarget.Indices = indexList;
                                obfuscatedTargets.Add(obfuscatedTarget);
                            }
                            operands = operand.ToList();
                        }
                    }
                }
                else
                {
                    var nestedTypes = type.NestedTypes;
                    NestedWorker:
                    foreach (var nestedType in nestedTypes)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.Body != null)
                            {
                                List<int> indexList = new List<int>();
                                var obfuscatedTarget = new ObfuscatedTarget()
                                {
                                    Type = type,
                                    Method = method
                                };
                                int i = 0;
                                obfuscatedTarget.NestedTypes.Add(nestedType.Name);
                                foreach (var instruction in method.Body.Instructions)
                                {
                                    if (instruction.Operand != null)
                                    {
                                        if (operands.Contains(instruction.Operand.ToString()))
                                        {
                                            indexList.Add(i);
                                            operands.Remove(instruction.Operand.ToString());
                                        }
                                    }
                                    i++;
                                }
                                if (indexList.Count == operand.Length)
                                {
                                    obfuscatedTarget.Indices = indexList;
                                    obfuscatedTargets.Add(obfuscatedTarget);
                                }
                                operands = operand.ToList();
                            }
                        }
                        if (nestedType.HasNestedTypes)
                        {
                            nestedTypes = nestedType.NestedTypes;
                            goto NestedWorker;
                        }
                    }
                }
            }
            List<Target> targets = new List<Target>();
            foreach (var obfuscatedTarget in obfuscatedTargets)
            {
                Target t = new Target()
                {
                    Namespace = obfuscatedTarget.Type.Namespace,
                    Class = obfuscatedTarget.Type.Name,
                    Method = obfuscatedTarget.Method.Name,
                    NestedClasses = obfuscatedTarget.NestedTypes.ToArray()
                };
                if (obfuscatedTarget.Indices.Count == 1)
                {
                    t.Index = obfuscatedTarget.Indices[0];
                }
                else if (obfuscatedTarget.Indices.Count > 1)
                {
                    t.Indices = obfuscatedTarget.Indices.ToArray();
                }

                targets.Add(t);
            }
            return targets.ToArray();
        }

        public  Target[] FindInstructionsByOperand(int[] operand)
        {
            List<ObfuscatedTarget> obfuscatedTargets = new List<ObfuscatedTarget>();
            List<int> operands = operand.ToList();
            foreach (var type in Module.Types)
            {
                if (!type.HasNestedTypes)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Body != null)
                        {
                            List<int> indexList = new List<int>();
                            var obfuscatedTarget = new ObfuscatedTarget()
                            {
                                Type = type,
                                Method = method
                            };
                            int i = 0;
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.Operand != null)
                                {
                                    if (operands.Contains(Convert.ToInt32(instruction.Operand.ToString())))
                                    {
                                        indexList.Add(i);
                                        operands.Remove(Convert.ToInt32(instruction.Operand.ToString()));
                                    }
                                }
                                i++;
                            }
                            if (indexList.Count == operand.Length)
                            {
                                obfuscatedTarget.Indices = indexList;
                                obfuscatedTargets.Add(obfuscatedTarget);
                            }
                            operands = operand.ToList();
                        }
                    }
                }
                else
                {
                    var nestedTypes = type.NestedTypes;
                    NestedWorker:
                    foreach (var nestedType in nestedTypes)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.Body != null)
                            {
                                List<int> indexList = new List<int>();
                                var obfuscatedTarget = new ObfuscatedTarget()
                                {
                                    Type = type,
                                    Method = method
                                };
                                int i = 0;
                                obfuscatedTarget.NestedTypes.Add(nestedType.Name);
                                foreach (var instruction in method.Body.Instructions)
                                {
                                    if (instruction.Operand != null)
                                    {
                                        if (operands.Contains(Convert.ToInt32(instruction.Operand.ToString())))
                                        {
                                            indexList.Add(i);
                                            operands.Remove(Convert.ToInt32(instruction.Operand.ToString()));
                                        }
                                    }
                                    i++;
                                }
                                if (indexList.Count == operand.Length)
                                {
                                    obfuscatedTarget.Indices = indexList;
                                    obfuscatedTargets.Add(obfuscatedTarget);
                                }
                                operands = operand.ToList();
                            }
                        }
                        if (nestedType.HasNestedTypes)
                        {
                            nestedTypes = nestedType.NestedTypes;
                            goto NestedWorker;
                        }
                    }
                }
            }
            List<Target> targets = new List<Target>();
            foreach (var obfuscatedTarget in obfuscatedTargets)
            {
                Target t = new Target()
                {
                    Namespace = obfuscatedTarget.Type.Namespace,
                    Class = obfuscatedTarget.Type.Name,
                    Method = obfuscatedTarget.Method.Name,
                    NestedClasses = obfuscatedTarget.NestedTypes.ToArray()
                };
                if (obfuscatedTarget.Indices.Count == 1)
                {
                    t.Index = obfuscatedTarget.Indices[0];
                }
                else if (obfuscatedTarget.Indices.Count > 1)
                {
                    t.Indices = obfuscatedTarget.Indices.ToArray();
                }

                targets.Add(t);
            }
            return targets.ToArray();
        }

        public  Target[] FindInstructionsByOpcode(OpCode[] opcode)
        {
            List<ObfuscatedTarget> obfuscatedTargets = new List<ObfuscatedTarget>();
            List<string> operands = opcode.Select(o => o.Name).ToList();
            foreach (var type in Module.Types)
            {
                if (!type.HasNestedTypes)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Body != null)
                        {
                            List<int> indexList = new List<int>();
                            var obfuscatedTarget = new ObfuscatedTarget()
                            {
                                Type = type,
                                Method = method
                            };
                            int i = 0;
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (operands.Contains(instruction.OpCode.Name))
                                {
                                    indexList.Add(i);
                                    operands.Remove(instruction.OpCode.Name);
                                }
                                i++;
                            }
                            if (indexList.Count == opcode.Length)
                            {
                                obfuscatedTarget.Indices = indexList;
                                obfuscatedTargets.Add(obfuscatedTarget);
                            }
                            operands = opcode.Select(o => o.Name).ToList();
                        }
                    }
                }
                else
                {
                    var nestedTypes = type.NestedTypes;
                    NestedWorker:
                    foreach (var nestedType in nestedTypes)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.Body != null)
                            {
                                List<int> indexList = new List<int>();
                                var obfuscatedTarget = new ObfuscatedTarget()
                                {
                                    Type = type,
                                    Method = method
                                };
                                int i = 0;
                                obfuscatedTarget.NestedTypes.Add(nestedType.Name);
                                foreach (var instruction in method.Body.Instructions)
                                {
                                    if (operands.Contains(instruction.OpCode.Name))
                                    {
                                        indexList.Add(i);
                                        operands.Remove(instruction.OpCode.Name);
                                    }
                                    i++;
                                }
                                if (indexList.Count == opcode.Length)
                                {
                                    obfuscatedTarget.Indices = indexList;
                                    obfuscatedTargets.Add(obfuscatedTarget);
                                }
                                operands = opcode.Select(o => o.Name).ToList();
                            }
                        }
                        if (nestedType.HasNestedTypes)
                        {
                            nestedTypes = nestedType.NestedTypes;
                            goto NestedWorker;
                        }
                    }
                }
            }
            List<Target> targets = new List<Target>();
            foreach (var obfuscatedTarget in obfuscatedTargets)
            {
                Target t = new Target()
                {
                    Namespace = obfuscatedTarget.Type.Namespace,
                    Class = obfuscatedTarget.Type.Name,
                    Method = obfuscatedTarget.Method.Name,
                    NestedClasses = obfuscatedTarget.NestedTypes.ToArray()
                };
                if (obfuscatedTarget.Indices.Count == 1)
                {
                    t.Index = obfuscatedTarget.Indices[0];
                }
                else if (obfuscatedTarget.Indices.Count > 1)
                {
                    t.Indices = obfuscatedTarget.Indices.ToArray();
                }

                targets.Add(t);
            }
            return targets.ToArray();
        }

        public  Target[] FindInstructionsByOperand(Target target, int[] operand, bool removeIfFound = false)
        {
            List<ObfuscatedTarget> obfuscatedTargets = new List<ObfuscatedTarget>();
            List<int> operands = operand.ToList();
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef m = null;
            if (target.Method != null)
                m = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (m != null)
            {
                List<int> indexList = new List<int>();
                var obfuscatedTarget = new ObfuscatedTarget()
                {
                    Type = type,
                    Method = m
                };
                int i = 0;
                foreach (var instruction in m.Body.Instructions)
                {
                    if (instruction.Operand != null)
                    {
                        if (operands.Contains(Convert.ToInt32(instruction.Operand.ToString())))
                        {
                            indexList.Add(i);
                            if (removeIfFound)
                                operands.Remove(Convert.ToInt32(instruction.Operand.ToString()));
                        }
                    }
                    i++;
                }
                if (indexList.Count == operand.Length || removeIfFound == false)
                {
                    obfuscatedTarget.Indices = indexList;
                    obfuscatedTargets.Add(obfuscatedTarget);
                }
                operands = operand.ToList();
            }
            else
            {
                foreach (var method in type.Methods)
                {
                    if (method.Body != null)
                    {
                        List<int> indexList = new List<int>();
                        var obfuscatedTarget = new ObfuscatedTarget()
                        {
                            Type = type,
                            Method = method
                        };
                        int i = 0;
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (instruction.Operand != null)
                            {
                                if (operands.Contains(Convert.ToInt32(instruction.Operand.ToString())))
                                {
                                    indexList.Add(i);
                                    if (removeIfFound)
                                        operands.Remove(Convert.ToInt32(instruction.Operand.ToString()));
                                }
                            }
                            i++;
                        }
                        if (indexList.Count == operand.Length || removeIfFound == false)
                        {
                            obfuscatedTarget.Indices = indexList;
                            obfuscatedTargets.Add(obfuscatedTarget);
                        }
                        operands = operand.ToList();
                    }
                }
            }

            List<Target> targets = new List<Target>();
            foreach (var obfuscatedTarget in obfuscatedTargets)
            {
                Target t = new Target()
                {
                    Namespace = obfuscatedTarget.Type.Namespace,
                    Class = obfuscatedTarget.Type.Name,
                    Method = obfuscatedTarget.Method.Name,
                    NestedClasses = obfuscatedTarget.NestedTypes.ToArray()
                };
                if (obfuscatedTarget.Indices.Count == 1)
                {
                    t.Index = obfuscatedTarget.Indices[0];
                }
                else if (obfuscatedTarget.Indices.Count > 1)
                {
                    t.Indices = obfuscatedTarget.Indices.ToArray();
                }

                targets.Add(t);
            }
            return targets.ToArray();
        }

        /// <summary>
        /// Find methods that contain a certain OpCode[] signature
        /// </summary>
        /// <returns></returns>
        public Target[] FindMethodsByOpCodeSignature(OpCode[] signature)
        {
            HashSet<MethodDef> found = new HashSet<MethodDef>();

            foreach (TypeDef td in Module.Types)
            {
                foreach (MethodDef md in td.Methods)
                {
                    if (md.HasBody)
                    {
                        if (md.Body.HasInstructions)
                        {
                            OpCode[] codes = md.Body.Instructions.GetOpCodes().ToArray();
                            if (codes.IndexOf<OpCode>(signature).Count() > 0)
                            {
                                found.Add(md);
                            }
                        }
                    }
                }
            }

            //cast each to Target
            return (from method in found select (Target)method).ToArray();
        }

        public  Target[] FindInstructionsByOpcode(Target target, OpCode[] opcode, bool removeIfFound = false)
        {
            List<ObfuscatedTarget> obfuscatedTargets = new List<ObfuscatedTarget>();
            List<string> operands = opcode.Select(o => o.Name).ToList();
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef m = null;
            if (target.Method != null)
                m = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (m != null)
            {
                List<int> indexList = new List<int>();
                var obfuscatedTarget = new ObfuscatedTarget()
                {
                    Type = type,
                    Method = m
                };
                int i = 0;
                foreach (var instruction in m.Body.Instructions)
                {
                    if (operands.Contains(instruction.OpCode.Name))
                    {
                        indexList.Add(i);
                        if (removeIfFound)
                            operands.Remove(instruction.OpCode.Name);
                    }
                    i++;
                }
                if (indexList.Count == opcode.Length || removeIfFound == false)
                {
                    obfuscatedTarget.Indices = indexList;
                    obfuscatedTargets.Add(obfuscatedTarget);
                }
            }
            else
            {
                foreach (var method in type.Methods)
                {
                    if (method.Body != null)
                    {
                        List<int> indexList = new List<int>();
                        var obfuscatedTarget = new ObfuscatedTarget()
                        {
                            Type = type,
                            Method = method
                        };
                        int i = 0;
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (operands.Contains(instruction.OpCode.Name))
                            {
                                indexList.Add(i);
                                if (removeIfFound)
                                    operands.Remove(instruction.OpCode.Name);
                            }
                            i++;
                        }
                        if (indexList.Count == opcode.Length || removeIfFound == false)
                        {
                            obfuscatedTarget.Indices = indexList;
                            obfuscatedTargets.Add(obfuscatedTarget);
                        }
                        operands = opcode.Select(o => o.Name).ToList();
                    }
                }
            }

            List<Target> targets = new List<Target>();
            foreach (var obfuscatedTarget in obfuscatedTargets)
            {
                Target t = new Target()
                {
                    Namespace = obfuscatedTarget.Type.Namespace,
                    Class = obfuscatedTarget.Type.Name,
                    Method = obfuscatedTarget.Method.Name,
                    NestedClasses = obfuscatedTarget.NestedTypes.ToArray()
                };
                if (obfuscatedTarget.Indices.Count == 1)
                {
                    t.Index = obfuscatedTarget.Indices[0];
                }
                else if (obfuscatedTarget.Indices.Count > 1)
                {
                    t.Indices = obfuscatedTarget.Indices.ToArray();
                }

                targets.Add(t);
            }
            return targets.ToArray();
        }

        public Target[] FindInstructionsByRegex(Target target, string pattern, bool ignoreOperand)
        {
            var targets = new List<Target>();
            if(target.Namespace != null)
            {
                var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
                if(target.Method != null) 
                {
                    string body = "";
                    var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
                    if (method == null)
                    {
                        throw new Exception("Method not found");
                    }
                    foreach (var instruction in method.Body.Instructions) 
                    {
                        if(!ignoreOperand) 
                        {
                            body += instruction.OpCode + " " + instruction.Operand + "\n";
                        }
                        else
                        {
                            body += instruction.OpCode + "\n";
                        }
                    }
                    foreach(Match match in Regex.Matches(body, pattern))
                    {
                        int startIndex = body.Split(new string[] {match.Value}, StringSplitOptions.None)[0].Split('\n').Length-1;
                        int[] indices = {};
                        for(int i = 0; i < match.Value.Split('\n').Length; i++)
                        {
                            indices[i] = startIndex + i;
                        }
                        var t = new Target()
                        {
                            Indices = indices,
                            Method = target.Method,
                            Class = target.Class,
                            Namespace = target.Namespace,
                            NestedClasses = target.NestedClasses,
                            NestedClass = target.NestedClass
                        };
                        targets.Add(t);
                    }
                }
            }
            return targets.ToArray();
        }

        private bool CheckParametersByType(ParameterInfo[] parameters, Type[] types)
        {
            return !parameters.Where((t, i) => types[i] != t.ParameterType).Any();
        }

        public IMethod BuildCall(Type type, string method, Type returnType, Type[] parameters)
        {
            Importer importer = new Importer(Module);
            foreach (var m in type.GetMethods())
            {
                if (m.Name == method && m.ReturnType == returnType)
                {
                    if (m.GetParameters().Length == 0  && parameters == null)
                    {
                        IMethod meth = importer.Import(m);
                        return meth;
                    }
                    if ( m.GetParameters().Length == parameters.Length && CheckParametersByType(m.GetParameters(), parameters))
                    {
                        IMethod meth = importer.Import(m);
                        return meth;
                    }
                }
            }
            return null;
        }

        public IMethod BuildInstance(Type type, Type[] parameters)
        {
            Importer importer = new Importer(Module);
            foreach (var c in type.GetConstructors())
            {
                if (c.GetParameters().Length == 0 && parameters == null)
                {
                    IMethod meth = importer.Import(c);
                    return meth;
                }
                if (c.GetParameters().Length == parameters.Length && CheckParametersByType(c.GetParameters(), parameters))
                {
                    IMethod meth = importer.Import(c);
                    return meth;
                }
            }
            return null;
        }

        public void ReplaceInstruction(Target target)
        {
            string[] nestedClasses = { };
            if (target.NestedClasses != null)
            {
                nestedClasses = target.NestedClasses;
            }
            else if (target.NestedClass != null)
            {
                nestedClasses = new[] { target.NestedClass };
            }
            var type = FindType(target.Namespace + "." + target.Class, nestedClasses);
            var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                throw new Exception("Method not found");
            }
            var instructions = method.Body.Instructions;
            if (target.Index != -1 && target.Instruction != null)
            {
                instructions[target.Index] = target.Instruction;
            }
            else if (target.Indices != null && target.Instructions != null)
            {
                for(int i = 0;i<target.Indices.Length;i++) {
                    var index = target.Indices[i];
                    instructions[index] = target.Instructions[i];
                }
            }
            else
            {
                throw new Exception("Target object built wrong");
            }
        }

        public  void RemoveInstruction(Target target)
        {
            string[] nestedClasses = { };
            if (target.NestedClasses != null)
            {
                nestedClasses = target.NestedClasses;
            }
            else if (target.NestedClass != null)
            {
                nestedClasses = new[] { target.NestedClass };
            }
            var type = FindType(target.Namespace + "." + target.Class, nestedClasses);
            var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                throw new Exception("Method not found");
            }
            var instructions = method.Body.Instructions;
            if (target.Index != -1 && target.Indices == null)
            {
                instructions.RemoveAt(target.Index);
            }
            else if (target.Index == -1 && target.Indices != null)
            {
                foreach (var index in target.Indices.OrderByDescending(v => v))
                {
                    instructions.RemoveAt(index);
                }
            }
            else
            {
                throw new Exception("Target object built wrong");
            }
        }

        public void InsertInstruction(Target target)
        {
            string[] nestedClasses = { };
            if (target.NestedClasses != null)
            {
                nestedClasses = target.NestedClasses;
            }
            else if (target.NestedClass != null)
            {
                nestedClasses = new[] { target.NestedClass };
            }
            var type = FindType(target.Namespace + "." + target.Class, nestedClasses);
            var method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                throw new Exception("Method not found");
            }
            var instructions = method.Body.Instructions;
            if (target.Index != -1 && target.Instruction != null)
            {
                instructions.Insert(target.Index, target.Instruction);
            }
            else if (target.Indices != null && target.Instructions != null)
            {
                for (int i = 0; i < target.Indices.Length; i++)
                {
                    var index = target.Indices[i];
                    instructions.Insert(index, target.Instructions[i]);
                }
            }
            else
            {
                throw new Exception("Target object built wrong");
            }
        }

        public Instruction[] GetInstructions(Target target)
        {
            var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                return null;
            }
            return method.Body.Instructions.ToArray();
        }

        public IList<Instruction> GetInstructionList(Target target)
        {
            var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                return null;
            }
            return method.Body.Instructions;
        }

        public IList<Local> GetVariableList(Target target)
        {
            var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            if (method == null)
            {
                return null;
            }
            return method.Body.Variables;
        }

        public void PatchOperand(Target target, string operand)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            var instructions = method.Body.Instructions;
            if (target.Indices == null && target.Index != -1)
            {
                instructions[target.Index].Operand = operand;
            }
            else if (target.Indices != null && target.Index == -1)
            {
                foreach (var index in target.Indices)
                {
                    instructions[index].Operand = operand;
                }
            }
            else
            {
                throw new Exception("Operand error");
            }
        }

        public  void PatchOperand(Target target, int operand)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            var instructions = method.Body.Instructions;
            if (target.Indices == null && target.Index != -1)
            {
                instructions[target.Index].Operand = operand;
            }
            else if (target.Indices != null && target.Index == -1)
            {
                foreach (var index in target.Indices)
                {
                    instructions[index].Operand = operand;
                }
            }
            else
            {
                throw new Exception("Operand error");
            }
        }

        public void PatchOperand(Target target, string[] operand)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            var instructions = method.Body.Instructions;
            if (target.Indices != null && target.Index == -1)
            {
                foreach (var index in target.Indices)
                {
                    instructions[index].Operand = operand[index];
                }
            }
            else
            {
                throw new Exception("Operand error");
            }
        }

        public  void PatchOperand(Target target, int[] operand)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            var instructions = method.Body.Instructions;
            if (target.Indices != null && target.Index == -1)
            {
                foreach (var index in target.Indices)
                {
                    instructions[index].Operand = operand[index];
                }
            }
            else
            {
                throw new Exception("Operand error");
            }
        }

        public  string GetOperand(Target target)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            return method.Body.Instructions[target.Index].Operand.ToString();
        }

        public int GetLdcI4Operand(Target target)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            return method.Body.Instructions[target.Index].GetLdcI4Value();
        }

        public  int FindInstruction(Target target, Instruction instruction, int occurence)
        {
            occurence--; // Fix the occurence, e.g. second occurence must be 1 but hoomans like to write like they speak so why don't assist them?
            var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
            var instructions = method.Body.Instructions;
            int index = 0;
            int occurenceCounter = 0;
            foreach (var i in instructions)
            {
                if (i.Operand == null && instruction.Operand == null)
                {
                    if (i.OpCode.Name == instruction.OpCode.Name && occurenceCounter < occurence)
                    {
                        occurenceCounter++;
                    }
                    else if (i.OpCode.Name == instruction.OpCode.Name && occurenceCounter == occurence)
                    {
                        return index;
                    }
                }
                else if (i.OpCode.Name == instruction.OpCode.Name && i.Operand.ToString() == instruction.Operand.ToString() &&
                         occurenceCounter < occurence)
                {
                    occurenceCounter++;
                }
                else if (i.OpCode.Name == instruction.OpCode.Name && i.Operand.ToString() == instruction.Operand.ToString() &&
                         occurenceCounter == occurence)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public void RewriteProperty(Target target)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            PropertyDef property = FindProperty(type, target.Property);
            IList<Instruction> instructions = null;
            if (target.PropertyMethod == PropertyMethod.Get)
            {
                instructions = property.GetMethod.Body.Instructions;
            }
            else
            {
                instructions = property.SetMethod.Body.Instructions;
            }
            instructions.Clear();
            foreach (var instruction in target.Instructions)
            {
                instructions.Add(instruction);
            }
        }

        // See this: https://github.com/0xd4d/dnlib/blob/master/Examples/Example2.cs
        public void InjectMethod(Target target)
        {
            var type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            type.Methods.Add(target.MethodDef);
            CilBody body = new CilBody();
            target.MethodDef.Body = body;
            if (target.ParameterDefs != null)
            {
                foreach (var param in target.ParameterDefs)
                {
                    target.MethodDef.ParamDefs.Add(param);
                }
            }
            if (target.Locals != null)
            {
                foreach (var local in target.Locals)
                {
                    body.Variables.Add(local);
                }
            }
            foreach (var il in target.Instructions)
            {
                body.Instructions.Add(il);
            }
        }

        public void AddCustomAttribute(Target target, CustomAttribute attribute)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            if (target.Method != null)
            {
                MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
                method.CustomAttributes.Add(attribute);
            }
            else
            {
                type.CustomAttributes.Add(attribute);
            }
        }
        public void RemoveCustomAttribute(Target target, CustomAttribute attribute)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            if (target.Method != null)
            {
                MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
                method.CustomAttributes.Remove(attribute);
            }
            else
            {
                type.CustomAttributes.Remove(attribute);
            }
        }

        public void RemoveCustomAttribute(Target target, int attributeIndex)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            if (target.Method != null)
            {
                MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
                method.CustomAttributes.RemoveAt(attributeIndex);
            }
            else
            {
                type.CustomAttributes.RemoveAt(attributeIndex);
            }
        }

        public void ClearCustomAttributes(Target target)
        {
            TypeDef type = FindType(target.Namespace + "." + target.Class, target.NestedClasses);
            if (target.Method != null)
            {
                MethodDef method = FindMethod(type, target.Method, target.Parameters, target.ReturnType);
                method.CustomAttributes.Clear();
            }
            else
            {
                type.CustomAttributes.Clear();
            }
        }

        public Target GetEntryPoint()
        {
            return new Target()
            {
                Namespace = Module.EntryPoint.DeclaringType.Namespace,
                Class = Module.EntryPoint.DeclaringType.Name,
                Method = Module.EntryPoint.Name
            };
        }
    }
}
