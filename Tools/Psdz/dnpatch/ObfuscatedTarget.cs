using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace dnpatch
{
    public class ObfuscatedTarget
    {
        public TypeDef Type { get; set; }
        public MethodDef Method { get; set; }
        public List<int> Indices { get; set; }
        public List<string> NestedTypes = new List<string>();
    }
}
