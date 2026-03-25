using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Runtime.CompilerServices;

namespace PsdzRpcServer
{
    internal static class PsdzRpcServiceValidation
    {
        [ModuleInitializer]
        internal static void ValidateEnumMappings()
        {
            AssertEnumCount<ProgrammingJobs.CacheType, PsdzRpcCacheType>();
            AssertEnumCount<ProgrammingJobs.OperationType, PsdzOperationType>();
            AssertEnumCount<PsdzDatabase.SwiRegisterEnum, PsdzRpcSwiRegisterEnum>();
        }

        private static void AssertEnumCount<TSource, TTarget>()
            where TSource : struct, Enum
            where TTarget : struct, Enum
        {
            int sourceCount = Enum.GetValues(typeof(TSource)).Length;
            int targetCount = Enum.GetValues(typeof(TTarget)).Length;
            if (sourceCount != targetCount)
            {
                throw new InvalidOperationException(
                    $"Enum count mismatch: {typeof(TSource).Name} has {sourceCount} values, " +
                    $"but {typeof(TTarget).Name} has {targetCount} values.");
            }
        }
    }
}