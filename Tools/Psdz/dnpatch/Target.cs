using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Emit;
using dnlib.DotNet;

namespace dnpatch
{
    public partial class Target
    {
        public Target() { }
        
        public Target(MethodDef method)
        {
            Namespace = method.DeclaringType.Namespace;
            Class = method.DeclaringType.Name;
            Method = method.Name;
        }

        /// <summary>
        /// Cast MethodDef to Target -> (Target)MethodDef
        /// </summary>
        /// <param name="value"></param>
        static public implicit operator Target(MethodDef value)
        {
            return new Target(value);
        }
    }

    public partial class Target
    {
        public string Namespace { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
    }

    public partial class Target
    {
        public int[] Indices { get; set; }
        public Instruction[] Instructions { get; set; }
    }

    public partial class Target
    {
        public int Index { get; set; } = -1;
        public Instruction Instruction { get; set; }
    }

    public partial class Target
    {
        public string[] NestedClasses { get; set; }
    }

    public partial class Target
    {
        public string NestedClass { get; set; }
    }

    public partial class Target
    {
        public string[] Parameters { get; set; } // String[] etc.. if null it means that you dont want to check it
    }

    public partial class Target
    {
        public string ReturnType { get; set; } // String[] etc.. if null or empty it means that you dont want to check it
    }

    public partial class Target
    {
        public string Property { get; set; }
        public PropertyMethod PropertyMethod { get; set; }
    }

    public partial class Target
    {
        public ParamDef[] ParameterDefs { get; set; }
    }

    public partial class Target
    {
        public MethodDef MethodDef { get; set; }
    }

    public partial class Target
    {
        public Local[] Locals { get; set; }
    }

    public enum PropertyMethod
    {
        Get,
        Set
    }
}
