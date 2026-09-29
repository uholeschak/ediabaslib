using System;
using System.Collections.Generic;

namespace BMW.Rheingold.Measurement.Common.Contract
{
    public interface IDeviceGeneric
    {
        uint RingBufferSize { get; set; }

        ExcecutionStatus LoadPlugin(string assemblyName, bool currentDomain);

        ExcecutionStatus LoadPluginGeneric(string nameSpace, string functionName, DateTime? operationStartTime);

        ExcecutionStatus UnloadPlugin();

        GenericDeviceStatus GetStatus();

        GenericDeviceStatus GetStatusGeneric();

        IDictionary<string, GenericDeviceStatusInfo> GetStatusStructured();

        GenericDeviceReadResult Read(KindOfResult kindOfResult = KindOfResult.HVA);

        GenericDeviceStatus Reset();

        GenericDeviceStatus Start();

        GenericDeviceStatus Stop();

        GenericDeviceStatus Command(string name, string[] parameters);

        GenericDeviceStatus GetProperty(string name);

        GenericDeviceStatus SetProperty(string name, string value);
    }
}
