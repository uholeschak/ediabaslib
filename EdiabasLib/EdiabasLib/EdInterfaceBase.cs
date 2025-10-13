using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public abstract class EdInterfaceBase : IDisposable
    {
        protected class TransmitStorage
        {
            public TransmitStorage(byte[] request, byte[] response, EdiabasNet.ErrorCodes errorCode)
            {
                Request = request;
                ResponseList = new List<byte[]>();
                if (response != null && response.Length > 0)
                {
                    byte[] buffer = new byte[response.Length];
                    Array.Copy(response, buffer, response.Length);
                    ResponseList.Add(buffer);
                }
                ErrorCode = errorCode;
            }

            public byte[] Request
            {
                set
                {
                    _request = value;
                    Key = BitConverter.ToString(value);
                }
                get { return _request; }
            }
            public string Key { get; private set; }
            private byte[] _request;
            public List<byte[]> ResponseList { get; set; }
            public EdiabasNet.ErrorCodes ErrorCode { get; set; }
        }

        // automatic baud rate detection
        // don't change this value, it's also used in the adapter.
        public static int BaudAuto = 2;

        private bool _disposed;
        private long _responseCounter;
        private object _responseCounterLock = new object();
        public const string SimFileExtension = ".sim";
        public const string PortIdSimulation = "SIMULATION";
        protected EdSimFile EdSimFileInterface;
        protected EdSimFile EdSimFileSgbd;
        protected bool SimulationConnected;
        protected Queue<byte[]> SimulationRecQueue = new Queue<byte[]>();
        protected byte[] SimFrequentResponse;
        protected int? SimEcuAddr;
        protected int? SimWakeAddr;
        protected string IfhNameProtected;
        protected string UnitNameProtected;
        protected string ApplicationNameProtected;
        protected EdiabasNet EdiabasProtected;
        protected object ConnectParameterProtected;
        protected object MutexLock = new object();
        protected bool MutexAquired;
        protected UInt32 CommRepeatsProtected;
        protected UInt32[] CommParameterProtected;
        protected Int16[] CommAnswerLenProtected = new Int16[2];
        protected bool EnableTransCacheProtected;
        protected TransmitStorage ReadTransaction;
        protected TransmitStorage WriteTransaction;
        protected int ReadTransactionPos;
        protected Dictionary<string, TransmitStorage> TransmitCacheDict = new Dictionary<string, TransmitStorage>();

        protected virtual Mutex InterfaceMutex
        {
            get { return null; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        protected virtual string InterfaceMutexName
        {
            get { return string.Empty; }
        }


        public abstract bool IsValidInterfaceName(string name);

        public virtual bool InterfaceLock(int timeout = 0)
        {
            lock (MutexLock)
            {
                if (InterfaceMutex == null)
                {
                    return false;
                }
                try
                {
                    if (!InterfaceMutex.WaitOne(timeout))
                    {
                        return false;
                    }
                    MutexAquired = true;
                }
                catch (AbandonedMutexException)
                {
                    MutexAquired = true;
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public virtual bool InterfaceUnlock()
        {
            lock (MutexLock)
            {
                if (InterfaceMutex == null)
                {
                    return true;
                }
                if (MutexAquired)
                {
                    try
                    {
                        InterfaceMutex.ReleaseMutex();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            InterfaceMutex.Dispose();
                            InterfaceMutex = null;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
#if ANDROID
                        InterfaceMutex = new Mutex(false);
#else
                        InterfaceMutex = new Mutex(false, InterfaceMutexName);
#endif
                    }

                    MutexAquired = false;
                }

                return true;
            }
        }

        public virtual bool InterfaceConnect()
        {
            CommRepeatsProtected = 0;
            CommParameterProtected = null;
            CommAnswerLenProtected[0] = 0;
            CommAnswerLenProtected[1] = 0;
            ResponseCount = 0;
            LoadInterfaceSimFile();
            return true;
        }

        public virtual bool InterfaceDisconnect()
        {
            UnloadInterfaceSimFile();
            return true;
        }

        public virtual bool IsSimulationMode()
        {
            if (EdiabasProtected == null)
            {
                return false;
            }

            if (!EdiabasProtected.Simulation)
            {
                return false;
            }

            if (string.IsNullOrEmpty(EdiabasProtected.SimulationPath) ||
                !Directory.Exists(EdiabasProtected.SimulationPath))
            {
                return false;
            }

            return true;
        }

        public virtual bool LoadInterfaceSimFile()
        {
            UnloadInterfaceSimFile();

            if (!IsSimulationMode())
            {
                return false;
            }

            try
            {
                List<string> simInterfaceList = new List<string>();
                string simInterfaces = EdiabasProtected.GetConfigProperty("SimulationInterfaces");
                if (!string.IsNullOrEmpty(simInterfaces))
                {
                    string[] simInterfaceArray = simInterfaces.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string simInterface in simInterfaceArray)
                    {
                        simInterfaceList.Add(simInterface.Trim().ToLowerInvariant());
                    }
                }

                if (!simInterfaceList.Contains(InterfaceType, StringComparer.OrdinalIgnoreCase))
                {
                    simInterfaceList.Add(InterfaceType.ToLowerInvariant());
                }

                string simFileUse = null;
                foreach (string simInterface in simInterfaceList)
                {
                    if (simInterface.Contains("*"))
                    {
                        try
                        {
                            string[] simFiles = Directory.GetFiles(EdiabasProtected.SimulationPath, simInterface + SimFileExtension, SearchOption.TopDirectoryOnly);
                            if (simFiles.Length == 0)
                            {
                                continue;
                            }

                            foreach (string simFile in simFiles)
                            {
                                if (File.Exists(simFile))
                                {
                                    simFileUse = simFile;
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        continue;
                    }

                    string simFileName = simInterface + SimFileExtension;
                    string simFilePath = Path.Combine(EdiabasProtected.SimulationPath, simFileName.ToLowerInvariant());
                    if (File.Exists(simFilePath))
                    {
                        simFileUse = simFilePath;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(simFileUse))
                {
                    EdSimFileInterface = new EdSimFile(simFileUse);
                    if (!EdSimFileInterface.FileValid)
                    {
                        // Simulation error
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0026);
                        return false;
                    }
                }

                SimulationConnected = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual bool UnloadInterfaceSimFile()
        {
            if (EdSimFileInterface != null)
            {
                SimulationRecQueue.Clear();
            }

            EdSimFileInterface = null;
            SimFrequentResponse = null;
            SimEcuAddr = null;
            SimulationConnected = false;
            return true;
        }

        public virtual bool LoadSgbdSimFile(string fileName)
        {
            UnloadSgbdSimFile();

            if (!IsSimulationMode())
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            try
            {
                string simFileName = Path.ChangeExtension(fileName, SimFileExtension);
                string simFilePath = Path.Combine(EdiabasProtected.SimulationPath, simFileName.ToLowerInvariant());
                if (!File.Exists(simFilePath))
                {
                    if (EdSimFileInterface == null)
                    {
                        // Simulation error
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0026);
                        return false;
                    }

                    return true;
                }

                EdSimFileSgbd = new EdSimFile(simFilePath);
                if (!EdSimFileSgbd.FileValid)
                {
                    // Simulation error
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0026);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual bool UnloadSgbdSimFile()
        {
            if (EdSimFileSgbd != null)
            {
                SimulationRecQueue.Clear();
            }

            EdSimFileSgbd = null;
            SimFrequentResponse = null;
            return true;
        }

        public virtual bool TransmitSimulationData(byte[] sendData, out byte[] receiveData, int? simEcuAddr = null, int? simWakeAddr = null, bool bmwFast = false)
        {
            receiveData = null;
            List<byte> recDataInternal;
            if (!SimulationConnected)
            {
                EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                return false;
            }

            if (sendData.Length == 0)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Simulation request len zero");
            }

            if (!bmwFast)
            {
                SimulationRecQueue.Clear();
                if (!TransmitSimulationInternal(sendData, out recDataInternal, simEcuAddr, simWakeAddr))
                {
                    return false;
                }

                receiveData = recDataInternal.ToArray();
                return true;
            }

            if (sendData.Length > 0)
            {
                SimulationRecQueue.Clear();
                if (!TransmitSimulationInternal(sendData, out recDataInternal, simEcuAddr, simWakeAddr))
                {
                    return false;
                }

                List<byte> recDataList = recDataInternal;
                for (; ; )
                {
                    int telLength = TelLengthBmwFast(recDataList.ToArray());
                    if (telLength == 0)
                    {
                        return false;
                    }

                    if (telLength + 1 > recDataList.Count)
                    {
                        return false;
                    }

                    List<byte> responseBytes = recDataList.GetRange(0, telLength + 1);  // including checksum
                    bool filterResponse = false;
                    if (responseBytes.Count == 7)
                    {
                        if (responseBytes[3] == 0x7F)
                        {
                            switch (responseBytes[5])
                            {
                                case 0x22:
                                case 0x23:
                                case 0x78:
                                    filterResponse = true;
                                    break;
                            }
                        }
                    }

                    if (!filterResponse)
                    {
                        responseBytes[responseBytes.Count - 1] = CalcChecksumBmwFast(responseBytes.ToArray(), telLength);    // fix checksum
                        SimulationRecQueue.Enqueue(responseBytes.ToArray());
                    }

                    recDataList.RemoveRange(0, telLength + 1);
                    if (recDataList.Count == 0)
                    {
                        break;
                    }
                }

                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BMW FAST simulation responses queued: {0}", SimulationRecQueue.Count);
            }
            else
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "BMW FAST simulation queue count: {0}", SimulationRecQueue.Count);
            }

            if (SimulationRecQueue.Count == 0)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "BMW FAST simulation queue empty");
                return false;
            }

            receiveData = SimulationRecQueue.Dequeue();
            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, receiveData.Length, "BMW FAST sim rec");
            return true;
        }

        protected bool TransmitSimulationInternal(byte[] sendData, out List<byte> receiveData, int? simEcuAddr = null, int? simWakeAddr = null)
        {
            receiveData = null;
            EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Transmit sim ECU={0:X02}, Wake={0:X02}", simEcuAddr ?? 0, simWakeAddr ?? 0);
            EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendData.Length, "Send sim");

            if (EdSimFileSgbd != null)
            {
                List<byte> response = EdSimFileSgbd.GetResponse(sendData.ToList(), simEcuAddr, simWakeAddr);
                if (response == null && (simEcuAddr != null || simWakeAddr != null))
                {   // try without ecu address
                    response = EdSimFileSgbd.GetResponse(sendData.ToList());
                }

                if (response != null)
                {
                    receiveData = response;
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData.ToArray(), 0, receiveData.Count, "Rec sim SGBD");
                    return true;
                }
            }

            if (EdSimFileInterface != null)
            {
                List<byte> response = EdSimFileInterface.GetResponse(sendData.ToList(), simEcuAddr, simWakeAddr);
                if (response == null && (simEcuAddr != null || simWakeAddr != null))
                {   // try without ecu address
                    response = EdSimFileInterface.GetResponse(sendData.ToList());
                }

                if (response != null)
                {
                    receiveData = response;
                    EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData.ToArray(), 0, receiveData.Count, "Rec sim interface");
                    return true;
                }
            }

            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No simulation data");
            return false;
        }

        public virtual byte[] GetKeyBytesSimulation(int? simEcuAddr = null, int? simWakeAddr = null)
        {
            if (!SimulationConnected)
            {
                EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                return null;
            }

            EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "KeyBytes sim ECU={0:X02}, Wake={0:X02}", simEcuAddr ?? 0, simWakeAddr ?? 0);

            if (EdSimFileSgbd != null)
            {
                List<byte> keyBytes = EdSimFileSgbd.GetKeyBytes();
                if (keyBytes == null && (simEcuAddr != null || simWakeAddr != null))
                {
                    keyBytes = EdSimFileSgbd.GetKeyBytes(simEcuAddr, simWakeAddr);
                }

                if (keyBytes != null)
                {
                    return keyBytes.ToArray();
                }
            }

            if (EdSimFileInterface != null)
            {
                List<byte> keyBytes = EdSimFileInterface.GetKeyBytes();
                if (keyBytes == null && (simEcuAddr != null || simWakeAddr != null))
                {
                    keyBytes = EdSimFileInterface.GetKeyBytes(simEcuAddr, simWakeAddr);
                }

                if (keyBytes != null)
                {
                    return keyBytes.ToArray();
                }
            }

            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No KeyBytes simulation data");
            return null;
        }

        public virtual Int64 IgnitionVoltageSimulation
        {
            get
            {
                if (!SimulationConnected)
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }

                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.IgnitionVolt;
                }

                return EdSimFile.DefaultIgnitionVolt;
            }
        }

        public virtual Int64 UbatVoltageSimulation
        {
            get
            {
                if (!SimulationConnected)
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }

                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.UBatVolt;
                }

                return EdSimFile.DefaultBatteryVolt;
            }
        }

        public virtual Int64 UbatCurrentSimulation
        {
            get
            {
                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.UBatCurrent;
                }

                return -1;
            }
        }

        public virtual Int64 UbatHistorySimulation
        {
            get
            {
                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.UBatHistory;
                }

                return -1;
            }
        }

        public virtual Int64 IgnitionCurrentSimulation
        {
            get
            {
                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.IgnitionCurrent;
                }

                return -1;
            }
        }

        public virtual Int64 IgnitionHistorySimulation
        {
            get
            {
                if (EdSimFileInterface != null)
                {
                    return EdSimFileInterface.IgnitionHistory;
                }

                return -1;
            }
        }

        public abstract bool InterfaceReset();

        public abstract bool InterfaceBoot();

        public abstract bool TransmitData(byte[] sendData, out byte[] receiveData);

        public abstract bool TransmitFrequent(byte[] sendData);

        public abstract bool ReceiveFrequent(out byte[] receiveData);

        public abstract bool StopFrequent();

        public abstract bool RawData(byte[] sendData, out byte[] receiveData);

        public abstract bool TransmitCancel(bool cancel);

        public virtual string IfhName
        {
            get { return IfhNameProtected; }
            set { IfhNameProtected = value; }
        }

        public virtual string UnitName
        {
            get { return UnitNameProtected; }
            set { UnitNameProtected = value; }
        }

        public virtual string ApplicationName
        {
            get { return ApplicationNameProtected; }
            set { ApplicationNameProtected = value; }
        }

        public virtual EdiabasNet Ediabas
        {
            get { return EdiabasProtected; }
            set { EdiabasProtected = value; }
        }

        public virtual object ConnectParameter
        {
            get { return ConnectParameterProtected; }
            set { ConnectParameterProtected = value; }
        }

        public UInt32 CommRepeats
        {
            get { return CommRepeatsProtected; }
            set { CommRepeatsProtected = value; }
        }

        public virtual UInt32[] CommParameter { get; set; }

        public virtual Int16[] CommAnswerLen
        {
            get { return CommAnswerLenProtected; }
            set
            {
                if (value != null && value.Length >= 2)
                {
                    CommAnswerLenProtected[0] = value[0];
                    CommAnswerLenProtected[1] = value[1];
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Answer length: {0:X04} {1:X04}", CommAnswerLenProtected[0], CommAnswerLenProtected[1]);
                }
            }
        }

        public abstract bool BmwFastProtocol { get; }

        public abstract string InterfaceType { get; }

        public abstract UInt32 InterfaceVersion { get; }

        public abstract string InterfaceName { get; }

        public virtual string InterfaceVerName
        {
            get { return "IFH-STD Version 7.6.0"; }
        }

        public abstract byte[] KeyBytes { get; }

        public abstract byte[] State { get; }

        public abstract Int64 BatteryVoltage { get; }

        public abstract Int64 IgnitionVoltage { get; }

        public abstract int? AdapterVersion { get; }

        public abstract byte[] AdapterSerial { get; }

        public abstract double? AdapterVoltage { get; }

        public abstract Int64 GetPort(UInt32 index);

        public abstract void SetPort(UInt32 index, UInt32 value);

        public abstract bool Connected { get; }

        public abstract bool IsEcuConnected { get; }

        public virtual long ResponseCount
        {
            get
            {
                lock (_responseCounterLock)
                {
                    return _responseCounter;
                }
            }
            set
            {
                lock (_responseCounterLock)
                {
                    _responseCounter = value;
                }
            }
        }

        public virtual long IncResponseCount(long offset)
        {
            lock (_responseCounterLock)
            {
                _responseCounter += offset;
                return _responseCounter;
            }
        }

        public bool EnableTransmitCache
        {
            get
            {
                return EnableTransCacheProtected;
            }
            set
            {
                EnableTransCacheProtected = value;
                if (!EnableTransCacheProtected)
                {
                    WriteTransaction = null;
                    ReadTransaction = null;
                    ReadTransactionPos = 0;
                    TransmitCacheDict.Clear();
                }
            }
        }

        protected void CacheTransmission(byte[] request, byte[] response, EdiabasNet.ErrorCodes errorCode)
        {
            if (!EnableTransmitCache)
            {
                return;
            }
            if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
            {
                return;
            }
            if (request.Length > 0)
            {
                StoreWriteTransaction();
                WriteTransaction = new TransmitStorage(request, response, errorCode);
            }
            else
            {
                if (WriteTransaction != null)
                {
                    if (response != null && response.Length > 0)
                    {
                        byte[] buffer = new byte[response.Length];
                        Array.Copy(response, buffer, response.Length);
                        WriteTransaction.ResponseList.Add(buffer);
                    }
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE && WriteTransaction.ErrorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        WriteTransaction.ErrorCode = errorCode;
                    }
                }
            }
        }

        protected void StoreWriteTransaction()
        {
            if (WriteTransaction != null)
            {
                if (TransmitCacheDict.ContainsKey(WriteTransaction.Key))
                {
                    TransmitCacheDict.Remove(WriteTransaction.Key);
                }
                TransmitCacheDict.Add(WriteTransaction.Key, WriteTransaction);
                WriteTransaction = null;
            }
        }

        protected bool ReadCachedTransmission(byte[] request, out byte[] response, out EdiabasNet.ErrorCodes errorCode)
        {
            response = null;
            errorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            if (!EnableTransmitCache)
            {
                return false;
            }
            if (request.Length > 0)
            {
                StoreWriteTransaction();
                ReadTransaction = null;
                ReadTransactionPos = 0;
                TransmitCacheDict.TryGetValue(BitConverter.ToString(request), out ReadTransaction);
            }
            if (ReadTransaction == null)
            {
                return false;
            }
            if (request.Length > 0)
            {
                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, request, 0, request.Length, "Send Cache");
            }
            if ((ReadTransaction.Request.Length > 0) && ((ReadTransaction.Request[0] & 0xC0) == 0xC0))
            {   // functional address
                if (ReadTransaction.ErrorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {   // transmission not completed
                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Incomplete cached response");
                    return false;
                }
            }
            if (ReadTransaction.ResponseList.Count > ReadTransactionPos)
            {
                response = ReadTransaction.ResponseList[ReadTransactionPos++];
                errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, response, 0, response.Length, "Resp Cache");
            }
            else
            {
                if (ReadTransaction.ErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    errorCode = ReadTransaction.ErrorCode;
                }
            }
            return true;
        }

        // telegram length without checksum
        public static int TelLengthBmwFast(byte[] dataBuffer)
        {
            int telLength = dataBuffer[0] & 0x3F;
            if (telLength == 0)
            {   // with length byte
                if (dataBuffer[3] == 0)
                {
                    telLength = (dataBuffer[4] << 8) + dataBuffer[5] + 6;
                }
                else
                {
                    telLength = dataBuffer[3] + 4;
                }
            }
            else
            {
                telLength += 3;
            }

            if (telLength > dataBuffer.Length)
            {
                telLength = dataBuffer.Length;
            }
            return telLength;
        }

        public static int DataLengthBmwFast(byte[] dataBuffer, out int dataOffset)
        {
            dataOffset = 3;
            int telLength = dataBuffer[0] & 0x3F;
            if (telLength == 0)
            {   // with length byte
                if (dataBuffer[3] == 0)
                {
                    dataOffset = 6;
                    telLength = (dataBuffer[4] << 8) + dataBuffer[5];
                }
                else
                {
                    dataOffset = 4;
                    telLength = dataBuffer[3];
                }
            }

            if (telLength + dataOffset > dataBuffer.Length)
            {
                return -1;
            }

            return telLength;
        }

        public static byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        public static byte[] Int16ByteArrayToLe(Int16[] intArray)
        {
            if (intArray == null)
            {
                return null;
            }

            byte[] byteArray = new byte[intArray.Length * 2];

            int index = 0;
            for (int i = 0; i < intArray.Length; i++)
            {
                Int16 value = intArray[i];
                byteArray[index] = (byte)value;
                byteArray[index + 1] = (byte)(value >> 8);

                index += 2;
            }

            return byteArray;
        }

        public static byte[] UInt32ByteArrayToLe(UInt32[] uintArray)
        {
            if (uintArray == null)
            {
                return null;
            }

            byte[] byteArray = new byte[uintArray.Length * 4];

            int index = 0;
            for (int i = 0; i < uintArray.Length; i++)
            {
                UInt32 value = uintArray[i];
                byteArray[index] = (byte)value;
                byteArray[index + 1] = (byte)(value >> 8);
                byteArray[index + 2] = (byte)(value >> 16);
                byteArray[index + 3] = (byte)(value >> 24);

                index += 4;
            }

            return byteArray;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

   }
}
