using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace PsdzRpcServer.Shared
{
    public static class PsdzRpcServiceConstants
    {
        public const string PipeName = "PsdzJsonRpcPipe";
        public const string DealerId = "40626";

        /// <summary>
        /// Berechnet eine Signatur aller Methoden des Interfaces.
        /// Ändert sich automatisch bei Methoden-Hinzufügungen, -Entfernungen oder Signaturänderungen.
        /// </summary>
        public static string ComputeInterfaceSignature(Type interfaceType)
        {
            StringBuilder sb = new StringBuilder();

            MethodInfo[] methods = interfaceType
                .GetMethods()
                .OrderBy(m => m.Name)
                .ThenBy(m => string.Join(",", m.GetParameters().Select(p => GetTypeName(p.ParameterType))))
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                sb.Append(GetTypeName(method.ReturnType));
                sb.Append(' ');
                sb.Append(method.Name);
                sb.Append('(');
                sb.Append(string.Join(",", method.GetParameters()
                    .Select(p => GetTypeName(p.ParameterType))));
                sb.Append(')');
                sb.Append(';');
            }

            string signature = sb.ToString();

            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signature));
            return BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
        }

        /// <summary>
        /// Framework-unabhängige Typbezeichnung ohne Assembly-Informationen.
        /// </summary>
        private static string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            // z.B. "Task`1[Boolean]" statt assembly-qualifiziertem Namen
            string genericName = type.Name; // "Task`1"
            string typeArgs = string.Join(",", type.GetGenericArguments().Select(GetTypeName));
            return $"{genericName}[{typeArgs}]";
        }

        public static string ServiceInterfaceSignature =>
            ComputeInterfaceSignature(typeof(IPsdzRpcService));

        public static string CallbackInterfaceSignature =>
            ComputeInterfaceSignature(typeof(IPsdzRpcServiceCallback));
    }
}
