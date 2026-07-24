using System;
using System.Reflection;

namespace BMW.Rheingold.CoreFramework.InteropHelper
{
    public static class VerifyAssemblyHelper
    {
        public static bool VerifyStrongName(string assemblyPath, bool force)
        {
            return true;
        }

        public static bool VerifyStrongName(Type t, bool force)
        {
            return VerifyStrongName(Assembly.GetAssembly(t).Location, force);
        }
    }
}
