using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dnpatch
{
    public static class Extensions
    {
        /// <summary>
        /// Dynamic IndexOf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static IEnumerable<int> IndexOf<T>(this T[] haystack, T[] needle)
        {
            if ((needle != null) && (haystack.Length >= needle.Length))
            {
                for (int l = 0; l < haystack.Length - needle.Length + 1; l++)
                {
                    if (!needle.Where((data, index) => !haystack[l + index].Equals(data)).Any())
                    {
                        yield return l;
                    }
                }
            }
        }

        /// <summary>
        /// Get OpCode[] from Instruction[]
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public static IEnumerable<OpCode> GetOpCodes(this ICollection<Instruction> main)
        {
            return from instruction in main select instruction.OpCode;
        }
    }
}
