using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace EdiabasLib
{
    using EdValueType = UInt32;
    using EdFloatType = Double;

    public partial class Ediabas : IDisposable
    {
        private delegate void OperationDelegate(Ediabas ediabas, OpCode oc, Operand arg0, Operand arg1);
        public delegate bool AbortJobDelegate();
        public delegate void ProgressJobDelegate(Ediabas ediabas);
        public delegate void ErrorRaisedDelegate(ErrorCodes error);

        public static readonly int MAX_ARRAY_LENGTH = 1024;
        public static readonly int MAX_STRING_LENGTH = 1023;
        public static readonly int MAX_FILES = 5;

        private class OpCode
        {
            public byte oc;
            public string pneumonic;
            public OperationDelegate opFunc;
            public bool arg0IsNearAddress;

            public OpCode(byte oc, string pneumonic, OperationDelegate opFunc)
                : this(oc, pneumonic, opFunc, false)
            {
            }

            public OpCode(byte oc, string pneumonic, OperationDelegate opFunc, bool arg0IsNearAddress)
            {
                this.oc = oc;
                this.pneumonic = pneumonic;
                this.opFunc = opFunc;
                this.arg0IsNearAddress = arg0IsNearAddress;
            }
        }

        private class Operand
        {
            public Operand(Ediabas ediabas)
                : this(ediabas, OpAddrMode.None, null, null, null)
            {
            }

            public Operand(Ediabas ediabas, OpAddrMode opAddrMode)
                : this(ediabas, opAddrMode, null, null, null)
            {
            }

            public Operand(Ediabas ediabas, OpAddrMode opAddrMode, Object opData1)
                : this(ediabas, opAddrMode, opData1, null, null)
            {
            }

            public Operand(Ediabas ediabas, OpAddrMode opAddrMode, Object opData1, Object opData2)
                : this(ediabas, opAddrMode, opData1, opData2, null)
            {
            }

            public Operand(Ediabas ediabas, OpAddrMode opAddrMode, Object opData1, Object opData2, Object opData3)
            {
                this.ediabas = ediabas;
                Init(opAddrMode, opData1, opData2, opData3);
            }

            public void Init(OpAddrMode opAddrMode)
            {
                Init(opAddrMode, null, null, null);
            }

            public void Init(OpAddrMode opAddrMode, Object opData1)
            {
                Init(opAddrMode, opData1, null, null);
            }

            public void Init(OpAddrMode opAddrMode, Object opData1, Object opData2)
            {
                Init(opAddrMode, opData1, opData2, null);
            }

            public void Init(OpAddrMode opAddrMode, Object opData1, Object opData2, Object opData3)
            {
                this.opAddrMode = opAddrMode;
                this.opData1 = opData1;
                this.opData2 = opData2;
                this.opData3 = opData3;
            }

            public Type GetDataType()
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.RegS:
                    case OpAddrMode.ImmStr:
                    case OpAddrMode.IdxImm:
                    case OpAddrMode.IdxReg:
                    case OpAddrMode.IdxRegImm:
                    case OpAddrMode.IdxImmLenImm:
                    case OpAddrMode.IdxImmLenReg:
                    case OpAddrMode.IdxRegLenImm:
                    case OpAddrMode.IdxRegLenReg:
                        return typeof(byte[]);
                }
                return typeof(EdValueType);
            }

            public EdValueType GetValueMask()
            {
                return GetValueMask(0);
            }

            public EdValueType GetValueMask(EdValueType dataLen)
            {
                if (dataLen == 0)
                {
                    dataLen = GetDataLen();
                }
                switch (dataLen)
                {
                    case 1:
                        return 0x000000FF;

                    case 2:
                        return 0x0000FFFF;

                    case 4:
                        return 0xFFFFFFFF;

                    default:
                        throw new ArgumentOutOfRangeException("dataLen", "Operand.GetValueMask: Invalid length");
                }
            }

            public EdValueType GetDataLen()
            {
                return GetDataLen(false);
            }

            public EdValueType GetDataLen(bool write)
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.RegS:
                    case OpAddrMode.ImmStr:
                        return (EdValueType)GetArrayData().Length;

                    case OpAddrMode.RegAB:
                        return 1;

                    case OpAddrMode.RegI:
                        return 2;

                    case OpAddrMode.RegL:
                        return 4;

                    case OpAddrMode.Imm8:
                        return 1;

                    case OpAddrMode.Imm16:
                        return 2;

                    case OpAddrMode.Imm32:
                        return 4;

                    case OpAddrMode.IdxImm:
                    case OpAddrMode.IdxReg:
                    case OpAddrMode.IdxRegImm:
                        if (write)
                        {
                            return 1;
                        }
                        return (EdValueType)GetArrayData().Length;

                    case OpAddrMode.IdxImmLenImm:
                    case OpAddrMode.IdxImmLenReg:
                    case OpAddrMode.IdxRegLenImm:
                    case OpAddrMode.IdxRegLenReg:
                        return (EdValueType)GetArrayData().Length;
                }
                return 0;
            }

            public Object GetRawData()
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.RegS:
                    case OpAddrMode.RegAB:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData RegX: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            return arg1Data.GetRawData();
                        }

                    case OpAddrMode.Imm8:
                    case OpAddrMode.Imm16:
                    case OpAddrMode.Imm32:
                    case OpAddrMode.ImmStr:
                        {
                            return opData1;
                        }

                    case OpAddrMode.IdxImm:
                    case OpAddrMode.IdxReg:
                    case OpAddrMode.IdxRegImm:
                        {
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxX: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            byte[] dataArray = arg1Data.GetArrayData(true);

                            EdValueType index = 0;
                            if (opAddrMode == OpAddrMode.IdxImm)
                            {
                                if (opData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxX: Invalid data type2");
                                }
                                index = (EdValueType)opData2;
                            }
                            else
                            {
                                if (opData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxX: Invalid data type2");
                                }
                                Register arg2Data = (Register)opData2;
                                index = arg2Data.GetValueData();
                            }

                            if (opAddrMode == OpAddrMode.IdxRegImm)
                            {
                                if (opData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.GetRawData IdxX: Invalid data type3");
                                }
                                index += (EdValueType)opData3;
                            }

                            EdValueType requiredLength = index + 1;
                            if (requiredLength > ediabas.ArrayMaxSize)
                            {
                                ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                                return Ediabas.byteArray0;
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                return Ediabas.byteArray0;
                            }

                            byte[] resultArray = new byte[dataArray.Length - index];
                            Array.Copy(dataArray, (int) index, resultArray, 0, resultArray.Length);
                            return resultArray;
                        }

                    case OpAddrMode.IdxImmLenImm:
                    case OpAddrMode.IdxImmLenReg:
                    case OpAddrMode.IdxRegLenImm:
                    case OpAddrMode.IdxRegLenReg:
                        {
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxXLenX: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            byte[] dataArray = arg1Data.GetArrayData(true);

                            EdValueType index = 0;
                            if ((opAddrMode == OpAddrMode.IdxImmLenImm) || (opAddrMode == OpAddrMode.IdxImmLenReg))
                            {
                                if (opData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                index = (EdValueType)opData2;
                            }
                            else
                            {
                                if (opData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                Register arg2Data = (Register)opData2;
                                index = arg2Data.GetValueData();
                            }

                            EdValueType len = 0;
                            if ((opAddrMode == OpAddrMode.IdxImmLenImm) || (opAddrMode == OpAddrMode.IdxRegLenImm))
                            {
                                if (opData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                len = (EdValueType)opData3;
                            }
                            else
                            {
                                if (opData1.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                Register arg3Data = (Register)opData3;
                                len = arg3Data.GetValueData();
                            }

                            EdValueType requiredLength = index + len;
                            if (requiredLength > ediabas.ArrayMaxSize)
                            {
                                ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                                return Ediabas.byteArray0;
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                if (dataArray.Length <= index)
                                {
                                    return Ediabas.byteArray0;
                                }
                                len = (EdValueType) (dataArray.Length - index);
                            }

                            byte[] resultArray = new byte[len];
                            Array.Copy(dataArray, (int)index, resultArray, 0, (int)len);
                            return resultArray;
                        }
                }
                throw new ArgumentOutOfRangeException("opAddrMode", "Operand.GetRawData: Invalid address mode");
            }

            public EdValueType GetValueData()
            {
                return GetValueData(0);
            }

            public EdValueType GetValueData(EdValueType dataLen)
            {
                Object rawData = GetRawData();
                EdValueType value;
                if (rawData.GetType() == typeof(EdValueType))
                {
                    value = (EdValueType)rawData;
                    return value & GetValueMask();
                }
                if (rawData.GetType() != typeof(byte[]))
                {
                    throw new ArgumentOutOfRangeException("rawData", "Operand.GetValueData: Invalid data type");
                }
                if (dataLen == 0)
                {
                    throw new ArgumentOutOfRangeException("dataLen", "Operand.GetValueData: Invalid data length");
                }
                byte[] arrayData = (byte[])rawData;
                if (arrayData.Length < dataLen)
                {
                    Array.Resize(ref arrayData, (int)dataLen);
                }
                value = 0;
                for (int i = (int) (dataLen - 1); i >= 0; i--)
                {
                    value <<= 8;
                    value |= arrayData[i];
                }
                return value;
            }

            public EdFloatType GetFloatData()
            {
                Object rawData = GetRawData();
                if (rawData.GetType() != typeof(EdFloatType))
                {
                    throw new ArgumentOutOfRangeException("rawData", "Operand.GetFloatData: Invalid data type");
                }
                return (EdFloatType)rawData;
            }

            public byte[] GetArrayData()
            {
                Object rawData = GetRawData();
                if (rawData.GetType() != typeof(byte[]))
                {
                    throw new ArgumentOutOfRangeException("rawData", "Operand.GetArrayData: Invalid data type");
                }
                return (byte[])rawData;
            }

            public string GetStringData()
            {
                byte[] data = GetArrayData();
                int length;
                for (length = 0; length < data.Length; length++)
                {
                    if (data[length] == 0)
                    {
                        break;
                    }
                }
                return encoding.GetString(data, 0, length);
            }

            public void SetRawData(Object data)
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.RegS:
                        {
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData RegS: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            arg1Data.SetRawData(data);
                            return;
                        }

                    case OpAddrMode.RegAB:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData RegX: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            arg1Data.SetRawData(data);
                            return;
                        }

                    case OpAddrMode.IdxImm:
                    case OpAddrMode.IdxReg:
                    case OpAddrMode.IdxRegImm:
                        {
                            if ((data.GetType() != typeof(EdValueType)) && (data.GetType() != typeof(byte[])))
                            {
                                throw new ArgumentOutOfRangeException("data", "Operand.SetRawData IdxX: Invalid input data type");
                            }
                            if (opData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData IdxX: Invalid data type");
                            }
                            Register arg1Data = (Register)opData1;
                            byte[] dataArray = arg1Data.GetArrayData();

                            EdValueType index = 0;
                            if (opAddrMode == OpAddrMode.IdxImm)
                            {
                                if (opData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                index = (EdValueType)opData2;
                            }
                            else
                            {
                                if (opData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                Register arg2Data = (Register)opData2;
                                index = arg2Data.GetValueData();
                            }

                            if (opAddrMode == OpAddrMode.IdxRegImm)
                            {
                                if (opData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                index += (EdValueType)opData3;
                            }

                            EdValueType len;
                            byte[] sourceArray;
                            if (data.GetType() == typeof(EdValueType))
                            {
                                len = 1;
                                EdValueType sourceValue = (EdValueType)data;
                                sourceArray = new byte[len];
                                for (int i = 0; i < len; i++)
                                {
                                    sourceArray[i] = (byte)(sourceValue >> (i << 3));
                                }
                            }
                            else
                            {
                                sourceArray = (byte[])data;
                                len = (EdValueType)sourceArray.Length;
                            }

                            EdValueType requiredLength = index + len;
                            if (requiredLength > ediabas.ArrayMaxSize)
                            {
                                ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                                return;
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                Array.Resize(ref dataArray, (int)requiredLength);
                            }

                            Array.Copy(sourceArray, 0, dataArray, (int)index, (int)len);
                            arg1Data.SetRawData(dataArray);
                            return;
                        }
                }
                throw new ArgumentOutOfRangeException("opAddrMode", "Operand.SetRawData: Invalid address mode");
            }

            public void SetArrayData(byte[] data)
            {
                if (opData1.GetType() != typeof(Register))
                {
                    throw new ArgumentOutOfRangeException("opAddrMode", "Operand.SetArrayData: Invalid data type");
                }
                if (opAddrMode != OpAddrMode.RegS)
                {
                    throw new ArgumentOutOfRangeException("opAddrMode", "Operand.SetArrayData: Invalid address mode");
                }
                Register arg1Data = (Register)opData1;
                arg1Data.SetRawData(data);
            }

            public void SetStringData(string data)
            {
                if (data.Length > MAX_STRING_LENGTH)
                {
                    ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                }
                byte[] dataArray = encoding.GetBytes(data);
                int length = dataArray.Length;
                if (length > 0 && dataArray[length - 1] != 0)
                {
                    Array.Resize(ref dataArray, length + 1);
                    dataArray[length] = 0;
                }
                SetArrayData(dataArray);
            }

            private Ediabas ediabas;
            private OpAddrMode opAddrMode;
            public OpAddrMode AddrMode
            {
                get
                {
                    return opAddrMode;
                }
            }

            public Object opData1;
            public Object opData2;
            public Object opData3;
        }

        public enum ErrorCodes : uint
        {
            EDIABAS_ERR_NONE = 0,
            EDIABAS_IFH_0000 = 10,
            EDIABAS_IFH_0001 = 11,
            EDIABAS_IFH_0002 = 12,
            EDIABAS_IFH_0003 = 13,
            EDIABAS_IFH_0004 = 14,
            EDIABAS_IFH_0005 = 15,
            EDIABAS_IFH_0006 = 16,
            EDIABAS_IFH_0007 = 17,
            EDIABAS_IFH_0008 = 18,
            EDIABAS_IFH_0009 = 19,
            EDIABAS_IFH_0010 = 20,
            EDIABAS_IFH_0011 = 21,
            EDIABAS_IFH_0012 = 22,
            EDIABAS_IFH_0013 = 23,
            EDIABAS_IFH_0014 = 24,
            EDIABAS_IFH_0015 = 25,
            EDIABAS_IFH_0016 = 26,
            EDIABAS_IFH_0017 = 27,
            EDIABAS_IFH_0018 = 28,
            EDIABAS_IFH_0019 = 29,
            EDIABAS_IFH_0020 = 30,
            EDIABAS_IFH_0021 = 31,
            EDIABAS_IFH_0022 = 32,
            EDIABAS_IFH_0023 = 33,
            EDIABAS_IFH_0024 = 34,
            EDIABAS_IFH_0025 = 35,
            EDIABAS_IFH_0026 = 36,
            EDIABAS_IFH_0027 = 37,
            EDIABAS_IFH_0028 = 38,
            EDIABAS_IFH_0029 = 39,
            EDIABAS_IFH_0030 = 40,
            EDIABAS_IFH_0031 = 41,
            EDIABAS_IFH_0032 = 42,
            EDIABAS_IFH_0033 = 43,
            EDIABAS_IFH_0034 = 44,
            EDIABAS_IFH_0035 = 45,
            EDIABAS_IFH_0036 = 46,
            EDIABAS_IFH_0037 = 47,
            EDIABAS_IFH_0038 = 48,
            EDIABAS_IFH_0039 = 49,
            EDIABAS_IFH_0040 = 50,
            EDIABAS_IFH_0041 = 51,
            EDIABAS_IFH_0042 = 52,
            EDIABAS_IFH_0043 = 53,
            EDIABAS_IFH_0044 = 54,
            EDIABAS_IFH_0045 = 55,
            EDIABAS_IFH_0046 = 56,
            EDIABAS_IFH_0047 = 57,
            EDIABAS_IFH_0048 = 58,
            EDIABAS_IFH_0049 = 59,
            EDIABAS_IFH_LAST = 59,
            EDIABAS_BIP_0000 = 60,
            EDIABAS_BIP_0001 = 61,
            EDIABAS_BIP_0002 = 62,
            EDIABAS_BIP_0003 = 63,
            EDIABAS_BIP_0004 = 64,
            EDIABAS_BIP_0005 = 65,
            EDIABAS_BIP_0006 = 66,
            EDIABAS_BIP_0007 = 67,
            EDIABAS_BIP_0008 = 68,
            EDIABAS_BIP_0009 = 69,
            EDIABAS_BIP_0010 = 70,
            EDIABAS_BIP_0011 = 71,
            EDIABAS_BIP_0012 = 72,
            EDIABAS_BIP_0013 = 73,
            EDIABAS_BIP_0014 = 74,
            EDIABAS_BIP_0015 = 75,
            EDIABAS_BIP_0016 = 76,
            EDIABAS_BIP_0017 = 77,
            EDIABAS_BIP_0018 = 78,
            EDIABAS_BIP_0019 = 79,
            EDIABAS_BIP_0020 = 80,
            EDIABAS_BIP_0021 = 81,
            EDIABAS_BIP_0022 = 82,
            EDIABAS_BIP_0023 = 83,
            EDIABAS_BIP_0024 = 84,
            EDIABAS_BIP_0025 = 85,
            EDIABAS_BIP_0026 = 86,
            EDIABAS_BIP_0027 = 87,
            EDIABAS_BIP_0028 = 88,
            EDIABAS_BIP_0029 = 89,
            EDIABAS_BIP_LAST = 89,
            EDIABAS_SYS_0000 = 90,
            EDIABAS_SYS_0001 = 91,
            EDIABAS_SYS_0002 = 92,
            EDIABAS_SYS_0003 = 93,
            EDIABAS_SYS_0004 = 94,
            EDIABAS_SYS_0005 = 95,
            EDIABAS_SYS_0006 = 96,
            EDIABAS_SYS_0007 = 97,
            EDIABAS_SYS_0008 = 98,
            EDIABAS_SYS_0009 = 99,
            EDIABAS_SYS_0010 = 100,
            EDIABAS_SYS_0011 = 101,
            EDIABAS_SYS_0012 = 102,
            EDIABAS_SYS_0013 = 103,
            EDIABAS_SYS_0014 = 104,
            EDIABAS_SYS_0015 = 105,
            EDIABAS_SYS_0016 = 106,
            EDIABAS_SYS_0017 = 107,
            EDIABAS_SYS_0018 = 108,
            EDIABAS_SYS_0019 = 109,
            EDIABAS_SYS_0020 = 110,
            EDIABAS_SYS_0021 = 111,
            EDIABAS_SYS_0022 = 112,
            EDIABAS_SYS_0023 = 113,
            EDIABAS_SYS_0024 = 114,
            EDIABAS_SYS_0025 = 115,
            EDIABAS_SYS_0026 = 116,
            EDIABAS_SYS_0027 = 117,
            EDIABAS_SYS_0028 = 118,
            EDIABAS_SYS_0029 = 119,
            EDIABAS_SYS_LAST = 119,
            EDIABAS_API_0000 = 120,
            EDIABAS_API_0001 = 121,
            EDIABAS_API_0002 = 122,
            EDIABAS_API_0003 = 123,
            EDIABAS_API_0004 = 124,
            EDIABAS_API_0005 = 125,
            EDIABAS_API_0006 = 126,
            EDIABAS_API_0007 = 127,
            EDIABAS_API_0008 = 128,
            EDIABAS_API_0009 = 129,
            EDIABAS_API_0010 = 130,
            EDIABAS_API_0011 = 131,
            EDIABAS_API_0012 = 132,
            EDIABAS_API_0013 = 133,
            EDIABAS_API_0014 = 134,
            EDIABAS_API_0015 = 135,
            EDIABAS_API_0016 = 136,
            EDIABAS_API_0017 = 137,
            EDIABAS_API_0018 = 138,
            EDIABAS_API_0019 = 139,
            EDIABAS_API_0020 = 140,
            EDIABAS_API_0021 = 141,
            EDIABAS_API_0022 = 142,
            EDIABAS_API_0023 = 143,
            EDIABAS_API_0024 = 144,
            EDIABAS_API_0025 = 145,
            EDIABAS_API_0026 = 146,
            EDIABAS_API_0027 = 147,
            EDIABAS_API_0028 = 148,
            EDIABAS_API_0029 = 149,
            EDIABAS_API_LAST = 149,
            EDIABAS_NET_0000 = 150,
            EDIABAS_NET_0001 = 151,
            EDIABAS_NET_0002 = 152,
            EDIABAS_NET_0003 = 153,
            EDIABAS_NET_0004 = 154,
            EDIABAS_NET_0005 = 155,
            EDIABAS_NET_0006 = 156,
            EDIABAS_NET_0007 = 157,
            EDIABAS_NET_0008 = 158,
            EDIABAS_NET_0009 = 159,
            EDIABAS_NET_0010 = 160,
            EDIABAS_NET_0011 = 161,
            EDIABAS_NET_0012 = 162,
            EDIABAS_NET_0013 = 163,
            EDIABAS_NET_0014 = 164,
            EDIABAS_NET_0015 = 165,
            EDIABAS_NET_0016 = 166,
            EDIABAS_NET_0017 = 167,
            EDIABAS_NET_0018 = 168,
            EDIABAS_NET_0019 = 169,
            EDIABAS_NET_0020 = 170,
            EDIABAS_NET_0021 = 171,
            EDIABAS_NET_0022 = 172,
            EDIABAS_NET_0023 = 173,
            EDIABAS_NET_0024 = 174,
            EDIABAS_NET_0025 = 175,
            EDIABAS_NET_0026 = 176,
            EDIABAS_NET_0027 = 177,
            EDIABAS_NET_0028 = 178,
            EDIABAS_NET_0029 = 179,
            EDIABAS_NET_0030 = 180,
            EDIABAS_NET_0031 = 181,
            EDIABAS_NET_0032 = 182,
            EDIABAS_NET_0033 = 183,
            EDIABAS_NET_0034 = 184,
            EDIABAS_NET_0035 = 185,
            EDIABAS_NET_0036 = 186,
            EDIABAS_NET_0037 = 187,
            EDIABAS_NET_0038 = 188,
            EDIABAS_NET_0039 = 189,
            EDIABAS_NET_0040 = 190,
            EDIABAS_NET_0041 = 191,
            EDIABAS_NET_0042 = 192,
            EDIABAS_NET_0043 = 193,
            EDIABAS_NET_0044 = 194,
            EDIABAS_NET_0045 = 195,
            EDIABAS_NET_0046 = 196,
            EDIABAS_NET_0047 = 197,
            EDIABAS_NET_0048 = 198,
            EDIABAS_NET_0049 = 199,
            EDIABAS_NET_LAST = 199,
            EDIABAS_IFH_0050 = 200,
            EDIABAS_IFH_0051 = 201,
            EDIABAS_IFH_0052 = 202,
            EDIABAS_IFH_0053 = 203,
            EDIABAS_IFH_0054 = 204,
            EDIABAS_IFH_0055 = 205,
            EDIABAS_IFH_0056 = 206,
            EDIABAS_IFH_0057 = 207,
            EDIABAS_IFH_0058 = 208,
            EDIABAS_IFH_0059 = 209,
            EDIABAS_IFH_0060 = 210,
            EDIABAS_IFH_0061 = 211,
            EDIABAS_IFH_0062 = 212,
            EDIABAS_IFH_0063 = 213,
            EDIABAS_IFH_0064 = 214,
            EDIABAS_IFH_0065 = 215,
            EDIABAS_IFH_0066 = 216,
            EDIABAS_IFH_0067 = 217,
            EDIABAS_IFH_0068 = 218,
            EDIABAS_IFH_0069 = 219,
            EDIABAS_IFH_0070 = 220,
            EDIABAS_IFH_0071 = 221,
            EDIABAS_IFH_0072 = 222,
            EDIABAS_IFH_0073 = 223,
            EDIABAS_IFH_0074 = 224,
            EDIABAS_IFH_0075 = 225,
            EDIABAS_IFH_0076 = 226,
            EDIABAS_IFH_0077 = 227,
            EDIABAS_IFH_0078 = 228,
            EDIABAS_IFH_0079 = 229,
            EDIABAS_IFH_0080 = 230,
            EDIABAS_IFH_0081 = 231,
            EDIABAS_IFH_0082 = 232,
            EDIABAS_IFH_0083 = 233,
            EDIABAS_IFH_0084 = 234,
            EDIABAS_IFH_0085 = 235,
            EDIABAS_IFH_0086 = 236,
            EDIABAS_IFH_0087 = 237,
            EDIABAS_IFH_0088 = 238,
            EDIABAS_IFH_0089 = 239,
            EDIABAS_IFH_0090 = 240,
            EDIABAS_IFH_0091 = 241,
            EDIABAS_IFH_0092 = 242,
            EDIABAS_IFH_0093 = 243,
            EDIABAS_IFH_0094 = 244,
            EDIABAS_IFH_0095 = 245,
            EDIABAS_IFH_0096 = 246,
            EDIABAS_IFH_0097 = 247,
            EDIABAS_IFH_0098 = 248,
            EDIABAS_IFH_0099 = 249,
            EDIABAS_IFH_LAST2 = 249,
            EDIABAS_RUN_0000 = 250,
            EDIABAS_RUN_0001 = 251,
            EDIABAS_RUN_0002 = 252,
            EDIABAS_RUN_0003 = 253,
            EDIABAS_RUN_0004 = 254,
            EDIABAS_RUN_0005 = 255,
            EDIABAS_RUN_0006 = 256,
            EDIABAS_RUN_0007 = 257,
            EDIABAS_RUN_0008 = 258,
            EDIABAS_RUN_0009 = 259,
            EDIABAS_RUN_0010 = 260,
            EDIABAS_RUN_0011 = 261,
            EDIABAS_RUN_0012 = 262,
            EDIABAS_RUN_0013 = 263,
            EDIABAS_RUN_0014 = 264,
            EDIABAS_RUN_0015 = 265,
            EDIABAS_RUN_0016 = 266,
            EDIABAS_RUN_0017 = 267,
            EDIABAS_RUN_0018 = 268,
            EDIABAS_RUN_0019 = 269,
            EDIABAS_RUN_0020 = 270,
            EDIABAS_RUN_0021 = 271,
            EDIABAS_RUN_0022 = 272,
            EDIABAS_RUN_0023 = 273,
            EDIABAS_RUN_0024 = 274,
            EDIABAS_RUN_0025 = 275,
            EDIABAS_RUN_0026 = 276,
            EDIABAS_RUN_0027 = 277,
            EDIABAS_RUN_0028 = 278,
            EDIABAS_RUN_0029 = 279,
            EDIABAS_RUN_0030 = 280,
            EDIABAS_RUN_0031 = 281,
            EDIABAS_RUN_0032 = 282,
            EDIABAS_RUN_0033 = 283,
            EDIABAS_RUN_0034 = 284,
            EDIABAS_RUN_0035 = 285,
            EDIABAS_RUN_0036 = 286,
            EDIABAS_RUN_0037 = 287,
            EDIABAS_RUN_0038 = 288,
            EDIABAS_RUN_0039 = 289,
            EDIABAS_RUN_0040 = 290,
            EDIABAS_RUN_0041 = 291,
            EDIABAS_RUN_0042 = 292,
            EDIABAS_RUN_0043 = 293,
            EDIABAS_RUN_0044 = 294,
            EDIABAS_RUN_0045 = 295,
            EDIABAS_RUN_0046 = 296,
            EDIABAS_RUN_0047 = 297,
            EDIABAS_RUN_0048 = 298,
            EDIABAS_RUN_0049 = 299,
            EDIABAS_RUN_0050 = 300,
            EDIABAS_RUN_0051 = 301,
            EDIABAS_RUN_0052 = 302,
            EDIABAS_RUN_0053 = 303,
            EDIABAS_RUN_0054 = 304,
            EDIABAS_RUN_0055 = 305,
            EDIABAS_RUN_0056 = 306,
            EDIABAS_RUN_0057 = 307,
            EDIABAS_RUN_0058 = 308,
            EDIABAS_RUN_0059 = 309,
            EDIABAS_RUN_0060 = 310,
            EDIABAS_RUN_0061 = 311,
            EDIABAS_RUN_0062 = 312,
            EDIABAS_RUN_0063 = 313,
            EDIABAS_RUN_0064 = 314,
            EDIABAS_RUN_0065 = 315,
            EDIABAS_RUN_0066 = 316,
            EDIABAS_RUN_0067 = 317,
            EDIABAS_RUN_0068 = 318,
            EDIABAS_RUN_0069 = 319,
            EDIABAS_RUN_0070 = 320,
            EDIABAS_RUN_0071 = 321,
            EDIABAS_RUN_0072 = 322,
            EDIABAS_RUN_0073 = 323,
            EDIABAS_RUN_0074 = 324,
            EDIABAS_RUN_0075 = 325,
            EDIABAS_RUN_0076 = 326,
            EDIABAS_RUN_0077 = 327,
            EDIABAS_RUN_0078 = 328,
            EDIABAS_RUN_0079 = 329,
            EDIABAS_RUN_0080 = 330,
            EDIABAS_RUN_0081 = 331,
            EDIABAS_RUN_0082 = 332,
            EDIABAS_RUN_0083 = 333,
            EDIABAS_RUN_0084 = 334,
            EDIABAS_RUN_0085 = 335,
            EDIABAS_RUN_0086 = 336,
            EDIABAS_RUN_0087 = 337,
            EDIABAS_RUN_0088 = 338,
            EDIABAS_RUN_0089 = 339,
            EDIABAS_RUN_0090 = 340,
            EDIABAS_RUN_0091 = 341,
            EDIABAS_RUN_0092 = 342,
            EDIABAS_RUN_0093 = 343,
            EDIABAS_RUN_0094 = 344,
            EDIABAS_RUN_0095 = 345,
            EDIABAS_RUN_0096 = 346,
            EDIABAS_RUN_0097 = 347,
            EDIABAS_RUN_0098 = 348,
            EDIABAS_RUN_0099 = 349,
            EDIABAS_RUN_LAST = 349,
            EDIABAS_ERROR_LAST = 349,
        }

        public enum ResultType : byte
        {
            TypeB,  // 8 bit
            TypeW,  // 16 bit
            TypeD,  // 32 bit
            TypeC,  // 8 bit char
            TypeI,  // 16 bit signed
            TypeL,  // 32 bit signed
            TypeR,  // float
            TypeS,  // string
            TypeY,  // array
            TypeSet,  // result set
        }

        private enum RegisterType : byte
        {
            RegAB,  // 8 bit
            RegI,   // 16 bit
            RegL,   // 32 bit
            RegF,   // float
            RegS,   // string
        }

        private enum OpAddrMode : byte
        {
            None = 0,
            RegS = 1,
            RegAB = 2,
            RegI = 3,
            RegL = 4,
            Imm8 = 5,
            Imm16 = 6,
            Imm32 = 7,
            ImmStr = 8,
            IdxImm = 9,
            IdxReg = 10,
            IdxRegImm = 11,
            IdxImmLenImm = 12,
            IdxImmLenReg = 13,
            IdxRegLenImm = 14,
            IdxRegLenReg = 15,
        }

        private class StringData
        {
            public StringData(Ediabas ediabas)
            {
                this.ediabas = ediabas;
                this.length = 0;
                this.data = new byte[MAX_ARRAY_LENGTH];
            }

            public byte[] GetData()
            {
                return GetData(false);
            }

            public byte[] GetData(bool complete)
            {
                if (complete)
                {
                    return data;
                }
                byte[] result = new byte[length];
                Array.Copy(data, 0, result, 0, (int)length);
                return result;
            }

            public void SetData(byte[] value, bool keepLength)
            {
                if (value.Length > MAX_ARRAY_LENGTH)
                {
                    ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                    return;
                }
                Array.Copy(value, 0, data, 0, value.Length);
                if (!keepLength)
                {
                    length = (EdValueType)value.Length;
                }
            }

            public EdValueType Length
            {
                get
                {
                    return length;
                }
                set
                {
                    length = value;
                }
            }

            private Ediabas ediabas;
            private EdValueType length;
            private byte[] data;
        }

        private class Register
        {
            public Register(byte opcode, RegisterType type, uint index)
            {
                this.Opcode = opcode;
                this.type = type;
                this.index = index;
                this.ediabas = null;
            }

            public void SetEdiabas(Ediabas ediabas)
            {
                this.ediabas = ediabas;
            }

            public string GetName()
            {
                switch (type)
                {
                    case RegisterType.RegAB:   // 8 bit
                        if (index > 15)
                        {
                            return "A" + string.Format("{0:X}", index - 16);
                        }
                        return "B" + string.Format("{0:X}", index);

                    case RegisterType.RegI:   // 16 bit
                        return "I" + string.Format("{0:X}", index);

                    case RegisterType.RegL:   // 32 bit
                        return "L" + string.Format("{0:X}", index);

                    case RegisterType.RegF:
                        return "F" + string.Format("{0:X}", index);

                    case RegisterType.RegS:
                        return "S" + string.Format("{0:X}", index);
                }
                return string.Empty;
            }

            public Type GetDataType()
            {
                switch (type)
                {
                    case RegisterType.RegF:
                        return typeof(EdFloatType);

                    case RegisterType.RegS:
                        return typeof(byte[]);
                }
                return typeof(EdValueType);
            }

            public EdValueType GetValueMask()
            {
                switch (GetDataLen())
                {
                    case 1:
                        return 0x000000FF;

                    case 2:
                        return 0x0000FFFF;

                    case 4:
                        return 0xFFFFFFFF;

                    default:
                        throw new ArgumentOutOfRangeException("GetDataLen", "Register.GetValueMask: Invalid length");
                }
            }

            public EdValueType GetDataLen()
            {
                switch (type)
                {
                    case RegisterType.RegAB:   // 8 bit
                        return 1;

                    case RegisterType.RegI:   // 16 bit
                        return 2;

                    case RegisterType.RegL:   // 32 bit
                        return 4;

                    case RegisterType.RegF:   // float
                        return sizeof(EdFloatType);

                    case RegisterType.RegS:   // string
                        return (EdValueType) GetArrayData().Length;
                }
                return 0;
            }

            public EdValueType GetValueData()
            {
                EdValueType value;
                switch (type)
                {
                    case RegisterType.RegAB:   // 8 bit
                        value = ediabas.byteRegisters[index + 0];
                        break;

                    case RegisterType.RegI:   // 16 bit
                        {
                            EdValueType offset = index << 1;
                            value = ediabas.byteRegisters[offset + 0] +
                                ((EdValueType)ediabas.byteRegisters[offset + 1] << 8);
                            break;
                        }

                    case RegisterType.RegL:   // 32 bit
                        {
                            EdValueType offset = index << 2;
                            value = ediabas.byteRegisters[offset + 0] +
                                ((EdValueType)ediabas.byteRegisters[offset + 1] << 8) +
                                ((EdValueType)ediabas.byteRegisters[offset + 2] << 16) +
                                ((EdValueType)ediabas.byteRegisters[offset + 3] << 24);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException("type", "Register.GetValueData: Invalid data type");
                }
                return value;
            }

            public object GetRawData()
            {
                switch (type)
                {
                    case RegisterType.RegAB:   // 8 bit
                    case RegisterType.RegI:   // 16 bit
                    case RegisterType.RegL:   // 32 bit
                        return GetValueData();

                    case RegisterType.RegF:   // float
                        return GetFloatData();

                    case RegisterType.RegS:   // string
                        return GetArrayData();

                    default:
                        throw new ArgumentOutOfRangeException("type", "Register.GetRawData: Invalid data type");
                }
            }

            public EdFloatType GetFloatData()
            {
                if (type != RegisterType.RegF)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.GetFloatData: Invalid data type");
                }
                if (GetDataLen() != sizeof(EdFloatType))
                {
                    throw new ArgumentOutOfRangeException("GetDataLen", "Register.GetFloatData: Invalid data length");
                }
                return ediabas.floatRegisters[index];
            }

            public byte[] GetArrayData()
            {
                return GetArrayData(false);
            }

            public byte[] GetArrayData(bool complete)
            {
                if (type != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.GetArrayData: Invalid data type");
                }
                return ediabas.stringRegisters[index].GetData(complete);
            }

            public void SetRawData(object value)
            {
                if (type == RegisterType.RegS)
                {
                    if (value.GetType() != typeof(byte[]))
                    {
                        throw new ArgumentOutOfRangeException("value", "Register.SetRawData: Invalid type");
                    }
                    SetArrayData((byte[])value);
                }
                else if (type == RegisterType.RegF)
                {
                    if (value.GetType() != typeof(EdFloatType))
                    {
                        throw new ArgumentOutOfRangeException("value", "Register.SetRawData: Invalid type");
                    }
                    SetFloatData((EdFloatType)value);
                }
                else
                {
                    if (value.GetType() != typeof(EdValueType))
                    {
                        throw new ArgumentOutOfRangeException("value", "Register.SetRawData: Invalid type");
                    }
                    SetValueData((EdValueType)value);
                }
            }

            public void SetValueData(EdValueType value)
            {
                switch (type)
                {
                    case RegisterType.RegAB:   // 8 bit
                        ediabas.byteRegisters[index] = (byte) value;
                        break;

                    case RegisterType.RegI:   // 16 bit
                        {
                            EdValueType offset = index << 1;
                            ediabas.byteRegisters[offset + 0] = (byte)value;
                            ediabas.byteRegisters[offset + 1] = (byte)(value >> 8);
                            break;
                        }

                    case RegisterType.RegL:   // 32 bit
                        {
                            EdValueType offset = index << 2;
                            ediabas.byteRegisters[offset + 0] = (byte)value;
                            ediabas.byteRegisters[offset + 1] = (byte)(value >> 8);
                            ediabas.byteRegisters[offset + 2] = (byte)(value >> 16);
                            ediabas.byteRegisters[offset + 3] = (byte)(value >> 24);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException("type", "Register.SetValueData: Invalid data type");
                }
            }

            public void SetFloatData(EdFloatType value)
            {
                if (type != RegisterType.RegF)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetFloatData: Invalid data type");
                }
                ediabas.floatRegisters[index] = value;
            }

            public void SetArrayData(byte[] value)
            {
                SetArrayData(value, false);
            }

            public void SetArrayData(byte[] value, bool keepLength)
            {
                if (type != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetArrayData: Invalid data type");
                }
                ediabas.stringRegisters[index].SetData(value, keepLength);
            }

            public byte Opcode
            {
                get;
                private set;
            }

            private RegisterType type;
            private uint index;
            private Ediabas ediabas;
        }

        static private Register[] registerList = new Register[]
        {
            new Register(0x00, RegisterType.RegAB, 0),
            new Register(0x01, RegisterType.RegAB, 1),
            new Register(0x02, RegisterType.RegAB, 2),
            new Register(0x03, RegisterType.RegAB, 3),
            new Register(0x04, RegisterType.RegAB, 4),
            new Register(0x05, RegisterType.RegAB, 5),
            new Register(0x06, RegisterType.RegAB, 6),
            new Register(0x07, RegisterType.RegAB, 7),
            new Register(0x08, RegisterType.RegAB, 8),
            new Register(0x09, RegisterType.RegAB, 9),
            new Register(0x0A, RegisterType.RegAB, 10),
            new Register(0x0B, RegisterType.RegAB, 11),
            new Register(0x0C, RegisterType.RegAB, 12),
            new Register(0x0D, RegisterType.RegAB, 13),
            new Register(0x0E, RegisterType.RegAB, 14),
            new Register(0x0F, RegisterType.RegAB, 15),
            new Register(0x10, RegisterType.RegI, 0),
            new Register(0x11, RegisterType.RegI, 1),
            new Register(0x12, RegisterType.RegI, 2),
            new Register(0x13, RegisterType.RegI, 3),
            new Register(0x14, RegisterType.RegI, 4),
            new Register(0x15, RegisterType.RegI, 5),
            new Register(0x16, RegisterType.RegI, 6),
            new Register(0x17, RegisterType.RegI, 7),
            new Register(0x18, RegisterType.RegL, 0),
            new Register(0x19, RegisterType.RegL, 1),
            new Register(0x1A, RegisterType.RegL, 2),
            new Register(0x1B, RegisterType.RegL, 3),
            new Register(0x1C, RegisterType.RegS, 0),
            new Register(0x1D, RegisterType.RegS, 1),
            new Register(0x1E, RegisterType.RegS, 2),
            new Register(0x1F, RegisterType.RegS, 3),
            new Register(0x20, RegisterType.RegS, 4),
            new Register(0x21, RegisterType.RegS, 5),
            new Register(0x22, RegisterType.RegS, 6),
            new Register(0x23, RegisterType.RegS, 7),
            new Register(0x24, RegisterType.RegF, 0),
            new Register(0x25, RegisterType.RegF, 1),
            new Register(0x26, RegisterType.RegF, 2),
            new Register(0x27, RegisterType.RegF, 3),
            new Register(0x28, RegisterType.RegF, 4),
            new Register(0x29, RegisterType.RegF, 5),
            new Register(0x2A, RegisterType.RegF, 6),
            new Register(0x2B, RegisterType.RegF, 7),
            new Register(0x2C, RegisterType.RegS, 8),
            new Register(0x2D, RegisterType.RegS, 9),
            new Register(0x2E, RegisterType.RegS, 10),
            new Register(0x2F, RegisterType.RegS, 11),
            new Register(0x30, RegisterType.RegS, 12),
            new Register(0x31, RegisterType.RegS, 13),
            new Register(0x32, RegisterType.RegS, 14),
            new Register(0x33, RegisterType.RegS, 15),
            new Register(0x80, RegisterType.RegAB, 16),
            new Register(0x81, RegisterType.RegAB, 17),
            new Register(0x82, RegisterType.RegAB, 18),
            new Register(0x83, RegisterType.RegAB, 19),
            new Register(0x84, RegisterType.RegAB, 20),
            new Register(0x85, RegisterType.RegAB, 21),
            new Register(0x86, RegisterType.RegAB, 22),
            new Register(0x87, RegisterType.RegAB, 23),
            new Register(0x88, RegisterType.RegAB, 24),
            new Register(0x89, RegisterType.RegAB, 25),
            new Register(0x8A, RegisterType.RegAB, 26),
            new Register(0x8B, RegisterType.RegAB, 27),
            new Register(0x8C, RegisterType.RegAB, 28),
            new Register(0x8D, RegisterType.RegAB, 29),
            new Register(0x8E, RegisterType.RegAB, 30),
            new Register(0x8F, RegisterType.RegAB, 31),
            new Register(0x90, RegisterType.RegI, 8),
            new Register(0x91, RegisterType.RegI, 9),
            new Register(0x92, RegisterType.RegI, 10),
            new Register(0x93, RegisterType.RegI, 11),
            new Register(0x94, RegisterType.RegI, 12),
            new Register(0x95, RegisterType.RegI, 13),
            new Register(0x96, RegisterType.RegI, 14),
            new Register(0x97, RegisterType.RegI, 15),
            new Register(0x98, RegisterType.RegL, 4),
            new Register(0x99, RegisterType.RegL, 5),
            new Register(0x9A, RegisterType.RegL, 6),
            new Register(0x9B, RegisterType.RegL, 7),
        };

        private static OpCode[] ocList = new OpCode[]
        {
            new OpCode(0x00, "move", new OperationDelegate(OpMove)),
            new OpCode(0x01, "clear", new OperationDelegate(OpClear)),
            new OpCode(0x02, "comp", new OperationDelegate(OpComp)),
            new OpCode(0x03, "subb", new OperationDelegate(OpSubb)),
            new OpCode(0x04, "adds", new OperationDelegate(OpAdds)),
            new OpCode(0x05, "mult", new OperationDelegate(OpMult)),
            new OpCode(0x06, "divs", new OperationDelegate(OpDivs)),
            new OpCode(0x07, "and", new OperationDelegate(OpAnd)),
            new OpCode(0x08, "or", new OperationDelegate(OpOr)),
            new OpCode(0x09, "xor", new OperationDelegate(OpXor)),
            new OpCode(0x0A, "not", new OperationDelegate(OpNot)),
            new OpCode(0x0B, "jump", new OperationDelegate(OpJump), true),
            new OpCode(0x0C, "jtsr", null, true),
            new OpCode(0x0D, "ret", null),
            new OpCode(0x0E, "jc", new OperationDelegate(OpJc), true),      // identical to jb
            new OpCode(0x0F, "jae", new OperationDelegate(OpJae), true),    // identical to jnc
            new OpCode(0x10, "jz", new OperationDelegate(OpJz), true),
            new OpCode(0x11, "jnz", new OperationDelegate(OpJnz), true),
            new OpCode(0x12, "jv", new OperationDelegate(OpJv), true),
            new OpCode(0x13, "jnv", new OperationDelegate(OpJnv), true),
            new OpCode(0x14, "jmi", new OperationDelegate(OpJmi), true),
            new OpCode(0x15, "jpl", new OperationDelegate(OpJpl), true),
            new OpCode(0x16, "clrc", new OperationDelegate(OpClrc)),
            new OpCode(0x17, "setc", new OperationDelegate(OpSetc)),
            new OpCode(0x18, "asr", new OperationDelegate(OpAsr)),
            new OpCode(0x19, "lsl", new OperationDelegate(OpLsl)),
            new OpCode(0x1A, "lsr", new OperationDelegate(OpLsr)),
            new OpCode(0x1B, "asl", new OperationDelegate(OpAsl)),
            new OpCode(0x1C, "nop", new OperationDelegate(OpNop)),
            new OpCode(0x1D, "eoj", new OperationDelegate(OpEoj)),
            new OpCode(0x1E, "push", new OperationDelegate(OpPush)),
            new OpCode(0x1F, "pop", new OperationDelegate(OpPop)),
            new OpCode(0x20, "scmp", new OperationDelegate(OpScmp)),
            new OpCode(0x21, "scat", new OperationDelegate(OpScat)),
            new OpCode(0x22, "scut", new OperationDelegate(OpScut)),
            new OpCode(0x23, "slen", new OperationDelegate(OpSlen)),
            new OpCode(0x24, "spaste", new OperationDelegate(OpSpaste)),
            new OpCode(0x25, "serase", new OperationDelegate(OpSerase)),
            new OpCode(0x26, "xconnect", new OperationDelegate(OpXconnect)),
            new OpCode(0x27, "xhangup", new OperationDelegate(OpXhangup)),
            new OpCode(0x28, "xsetpar", new OperationDelegate(OpXsetpar)),
            new OpCode(0x29, "xawlen", new OperationDelegate(OpXawlen)),
            new OpCode(0x2A, "xsend", new OperationDelegate(OpXsend)),
            new OpCode(0x2B, "xsendf", null),
            new OpCode(0x2C, "xrequf", null),
            new OpCode(0x2D, "xstopf", new OperationDelegate(OpXstopf)),
            new OpCode(0x2E, "xkeyb", null),
            new OpCode(0x2F, "xstate", null),
            new OpCode(0x30, "xboot", null),
            new OpCode(0x31, "xreset", null),
            new OpCode(0x32, "xtype", new OperationDelegate(OpXtype)),
            new OpCode(0x33, "xvers", new OperationDelegate(OpXvers)),
            new OpCode(0x34, "ergb", new OperationDelegate(OpErgb)),
            new OpCode(0x35, "ergw", new OperationDelegate(OpErgw)),
            new OpCode(0x36, "ergd", new OperationDelegate(OpErgd)),
            new OpCode(0x37, "ergi", new OperationDelegate(OpErgi)),
            new OpCode(0x38, "ergr", new OperationDelegate(OpErgr)),
            new OpCode(0x39, "ergs", new OperationDelegate(OpErgs)),
            new OpCode(0x3A, "a2flt", new OperationDelegate(OpA2flt)),
            new OpCode(0x3B, "fadd", new OperationDelegate(OpFadd)),
            new OpCode(0x3C, "fsub", new OperationDelegate(OpFsub)),
            new OpCode(0x3D, "fmul", new OperationDelegate(OpFmul)),
            new OpCode(0x3E, "fdiv", new OperationDelegate(OpFdiv)),
            new OpCode(0x3F, "ergy", new OperationDelegate(OpErgy)),
            new OpCode(0x40, "enewset", new OperationDelegate(OpEnewset)),
            new OpCode(0x41, "etag", new OperationDelegate(OpEtag), true),
            new OpCode(0x42, "xreps", new OperationDelegate(OpXreps)),
            new OpCode(0x43, "gettmr", new OperationDelegate(OpGettmr)),
            new OpCode(0x44, "settmr", new OperationDelegate(OpSettmr)),
            new OpCode(0x45, "sett", new OperationDelegate(OpSett)),
            new OpCode(0x46, "clrt", new OperationDelegate(OpClrt)),
            new OpCode(0x47, "jt", new OperationDelegate(OpJt), true),
            new OpCode(0x48, "jnt", new OperationDelegate(OpJnt), true),
            new OpCode(0x49, "addc", new OperationDelegate(OpAddc)),
            new OpCode(0x4A, "subc", new OperationDelegate(OpSubc)),
            new OpCode(0x4B, "break", new OperationDelegate(OpBreak)),
            new OpCode(0x4C, "clrv", new OperationDelegate(OpClrv)),
            new OpCode(0x4D, "eerr", new OperationDelegate(OpEerr)),
            new OpCode(0x4E, "popf", new OperationDelegate(OpPopf)),
            new OpCode(0x4F, "pushf", new OperationDelegate(OpPushf)),
            new OpCode(0x50, "atsp", new OperationDelegate(OpAtsp)),
            new OpCode(0x51, "swap", new OperationDelegate(OpSwap)),
            new OpCode(0x52, "setspc", new OperationDelegate(OpSetspc)),
            new OpCode(0x53, "srevrs", new OperationDelegate(OpSrevrs)),
            new OpCode(0x54, "stoken", new OperationDelegate(OpStoken)),
            new OpCode(0x55, "parb", new OperationDelegate(OpParl)),
            new OpCode(0x56, "parw", new OperationDelegate(OpParl)),
            new OpCode(0x57, "parl", new OperationDelegate(OpParl)),
            new OpCode(0x58, "pars", new OperationDelegate(OpPars)),
            new OpCode(0x59, "fclose", new OperationDelegate(OpFclose)),
            new OpCode(0x5A, "jg", new OperationDelegate(OpJg), true),
            new OpCode(0x5B, "jge", new OperationDelegate(OpJge), true),
            new OpCode(0x5C, "jl", new OperationDelegate(OpJl), true),
            new OpCode(0x5D, "jle", new OperationDelegate(OpJle), true),
            new OpCode(0x5E, "ja", new OperationDelegate(OpJa), true),
            new OpCode(0x5F, "jbe", new OperationDelegate(OpJbe), true),
            new OpCode(0x60, "fopen", new OperationDelegate(OpFopen)),
            new OpCode(0x61, "fread", new OperationDelegate(OpFread)),
            new OpCode(0x62, "freadln", new OperationDelegate(OpFreadln)),
            new OpCode(0x63, "fseek", new OperationDelegate(OpFseek)),
            new OpCode(0x64, "fseekln", new OperationDelegate(OpFseekln)),
            new OpCode(0x65, "ftell", new OperationDelegate(OpFtell)),
            new OpCode(0x66, "ftellln", new OperationDelegate(OpFtellln)),
            new OpCode(0x67, "a2fix", new OperationDelegate(OpA2fix)),
            new OpCode(0x68, "fix2flt", new OperationDelegate(OpFix2flt)),
            new OpCode(0x69, "parr", new OperationDelegate(OpParr)),
            new OpCode(0x6A, "test", new OperationDelegate(OpTest)),
            new OpCode(0x6B, "wait", new OperationDelegate(OpWait)),
            new OpCode(0x6C, "date", new OperationDelegate(OpDate)),
            new OpCode(0x6D, "time", new OperationDelegate(OpTime)),
            new OpCode(0x6E, "xbatt", null),
            new OpCode(0x6F, "tosp", null),
            new OpCode(0x70, "xdownl", null),
            new OpCode(0x71, "xgetport", null),
            new OpCode(0x72, "xignit", null),
            new OpCode(0x73, "xloopt", null),
            new OpCode(0x74, "xprog", null),
            new OpCode(0x75, "xraw", null),
            new OpCode(0x76, "xsetport", null),
            new OpCode(0x77, "xsireset", null),
            new OpCode(0x78, "xstoptr", null),
            new OpCode(0x79, "fix2hex", new OperationDelegate(OpFix2hex)),
            new OpCode(0x7A, "fix2dez", new OperationDelegate(OpFix2dez)),
            new OpCode(0x7B, "tabset", new OperationDelegate(OpTabset)),
            new OpCode(0x7C, "tabseek", new OperationDelegate(OpTabseek)),
            new OpCode(0x7D, "tabget", new OperationDelegate(OpTabget)),
            new OpCode(0x7E, "strcat", new OperationDelegate(OpStrcat)),
            new OpCode(0x7F, "pary", new OperationDelegate(OpPars)),
            new OpCode(0x80, "parn", new OperationDelegate(OpParn)),
            new OpCode(0x81, "ergc", new OperationDelegate(OpErgc)),
            new OpCode(0x82, "ergl", new OperationDelegate(OpErgl)),
            new OpCode(0x83, "tabline", new OperationDelegate(OpTabline)),
            new OpCode(0x84, "xsendr", null),
            new OpCode(0x85, "xrecv", null),
            new OpCode(0x86, "xinfo", null),
            new OpCode(0x87, "flt2a", new OperationDelegate(OpFlt2a)),
            new OpCode(0x88, "setflt", new OperationDelegate(OpSetflt)),
            new OpCode(0x89, "cfgig", new OperationDelegate(OpCfgig)),
            new OpCode(0x8A, "cfgsg", new OperationDelegate(OpCfgsg)),
            new OpCode(0x8B, "cfgis", null),
            new OpCode(0x8C, "a2y", new OperationDelegate(OpA2y)),
            new OpCode(0x8D, "xparraw", null),
            new OpCode(0x8E, "hex2y", new OperationDelegate(OpHex2y)),
            new OpCode(0x8F, "strcmp", new OperationDelegate(OpStrcmp)),
            new OpCode(0x90, "strlen", new OperationDelegate(OpStrlen)),
            new OpCode(0x91, "y2bcd", new OperationDelegate(OpY2bcd)),
            new OpCode(0x92, "y2hex", new OperationDelegate(OpY2hex)),
            new OpCode(0x93, "shmset", new OperationDelegate(OpShmset)),
            new OpCode(0x94, "shmget", new OperationDelegate(OpShmget)),
            new OpCode(0x95, "ergsysi", new OperationDelegate(OpErgsysi)),
            new OpCode(0x96, "flt2fix", new OperationDelegate(OpFlt2fix)),
            new OpCode(0x97, "iupdate", new OperationDelegate(OpIupdate)),
            new OpCode(0x98, "irange", new OperationDelegate(OpIrange)),
            new OpCode(0x99, "iincpos", new OperationDelegate(OpIincpos)),
            new OpCode(0x9A, "tabseeku", new OperationDelegate(OpTabseeku)),
            new OpCode(0x9B, "flt2y4", new OperationDelegate(OpFlt2y4)),
            new OpCode(0x9C, "flt2y8", new OperationDelegate(OpFlt2y8)),
            new OpCode(0x9D, "y42flt", new OperationDelegate(OpY42flt)),
            new OpCode(0x9E, "y82flt", new OperationDelegate(OpY82flt)),
            new OpCode(0x9F, "plink", null),
            new OpCode(0xA0, "pcall", null),
            new OpCode(0xA1, "fcomp", new OperationDelegate(OpFcomp)),
            new OpCode(0xA2, "plinkv", null),
            new OpCode(0xA3, "ppush", null),
            new OpCode(0xA4, "ppop", null),
            new OpCode(0xA5, "ppushflt", null),
            new OpCode(0xA6, "ppopflt", null),
            new OpCode(0xA7, "ppushy", null),
            new OpCode(0xA8, "ppopy", null),
            new OpCode(0xA9, "pjtsr", null),
            new OpCode(0xAA, "tabsetex", new OperationDelegate(OpTabsetex)),
            new OpCode(0xAB, "ufix2dez", new OperationDelegate(OpUfix2dez)),
            new OpCode(0xAC, "generr", new OperationDelegate(OpGenerr)),
            new OpCode(0xAD, "ticks", new OperationDelegate(OpTicks)),
            new OpCode(0xAE, "waitex", new OperationDelegate(OpWaitex)),
            new OpCode(0xAF, "xopen", null),
            new OpCode(0xB0, "xclose", null),
            new OpCode(0xB1, "xcloseex", null),
            new OpCode(0xB2, "xswitch", null),
            new OpCode(0xB3, "xsendex", null),
            new OpCode(0xB4, "xrecvex", null),
            new OpCode(0xB5, "ssize", null),
            new OpCode(0xB6, "tabcols", new OperationDelegate(OpTabcols)),
            new OpCode(0xB7, "tabrows", new OperationDelegate(OpTabrows)),
        };

        public class ResultData
        {
            public ResultData(ResultType type, string name, object opData)
            {
                this.type = type;
                this.name = name;
                this.opData = opData;
            }
            public ResultType type;
            public string name;
            public object opData;
        }

        private class Flags
        {
            public Flags()
            {
                Init();
            }
            public void Init()
            {
                this.carry = false;
                this.zero = false;
                this.sign = false;
                this.overflow = false;
            }

            public void UpdateFlags(EdValueType value, EdValueType length)
            {
                EdValueType valueMask = 0;
                EdValueType signMask = 0;

                switch (length)
                {
                    case 1:
                        valueMask = 0x000000FF;
                        signMask =  0x00000080;
                        break;

                    case 2:
                        valueMask = 0x0000FFFF;
                        signMask =  0x00008000;
                        break;

                    case 4:
                        valueMask = 0xFFFFFFFF;
                        signMask =  0x80000000;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("length", "Flags.UpdateFlags: Invalid length");
                }
                zero = (value & valueMask) == 0;
                sign = (value & signMask) != 0;
            }

            public void SetOverflow(UInt32 value1, UInt32 value2, UInt32 result, EdValueType length)
            {
                UInt64 signMask = 0;

                switch (length)
                {
                    case 1:
                        signMask = 0x00000080;
                        break;

                    case 2:
                        signMask = 0x00008000;
                        break;

                    case 4:
                        signMask = 0x80000000;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("length", "Flags.SetOverflow: Invalid length");
                }
                if ((value1 & signMask) != (value2 & signMask))
                {
                    overflow = false;
                }
                else if ((value1 & signMask) == (result & signMask))
                {
                    overflow = false;
                }
                else
                {
                    overflow = true;
                }
            }

            public void SetCarry(UInt64 value, EdValueType length)
            {
                UInt64 carryMask = 0;

                switch (length)
                {
                    case 1:
                        carryMask = 0x000000100;
                        break;

                    case 2:
                        carryMask = 0x000010000;
                        break;

                    case 4:
                        carryMask = 0x100000000;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("length", "Flags.SetCarry: Invalid length");
                }
                carry = (value & carryMask) != 0;
            }

            public EdValueType ToValue()
            {
                EdValueType value = 0;
                value |= (EdValueType)(carry ? 0x01 : 0x00);
                value |= (EdValueType)(zero ? 0x02 : 0x00);
                value |= (EdValueType)(sign ? 0x04 : 0x00);
                value |= (EdValueType)(overflow ? 0x08 : 0x00);

                return value;
            }

            public void FromValue(EdValueType value)
            {
                carry = (value & 0x01) != 0;
                zero = (value & 0x02) != 0;
                sign = (value & 0x04) != 0;
                overflow = (value & 0x08) != 0;
            }

            public bool carry;
            public bool zero;
            public bool sign;
            public bool overflow;
        }

        public class JobInfo
        {
            public JobInfo(string jobName, UInt32 jobOffset, UInt32 jobSize, UsesInfo usesInfo)
            {
                this.jobName = jobName;
                this.jobOffset = jobOffset;
                this.jobSize = jobSize;
                this.usesInfo = usesInfo;
            }

            public string JobName
            {
                get
                {
                    return jobName;
                }
            }

            public UInt32 JobOffset
            {
                get
                {
                    return jobOffset;
                }
            }

            public UInt32 JobSize
            {
                get
                {
                    return jobSize;
                }
            }

            public UsesInfo UsesInfo
            {
                get
                {
                    return usesInfo;
                }
            }

            private string jobName;
            private UInt32 jobOffset;
            private UInt32 jobSize;
            private UsesInfo usesInfo;
        }

        public class JobInfos
        {
            public JobInfo[] JobInfoArray
            {
                get
                {
                    return jobInfoArray;
                }
                set
                {
                    jobInfoArray = value;
                }
            }

            public Dictionary<string, UInt32> JobNameDict
            {
                get
                {
                    return jobNameDict;
                }
                set
                {
                    jobNameDict = value;
                }
            }

            private JobInfo[] jobInfoArray;
            private Dictionary<string, UInt32> jobNameDict;
        }

        public class UsesInfo
        {
            public UsesInfo(string name)
            {
                this.name = name;
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            private string name;
        }

        public class UsesInfos
        {
            public UsesInfo[] UsesInfoArray
            {
                get
                {
                    return usesInfoArray;
                }
                set
                {
                    usesInfoArray = value;
                }
            }

            private UsesInfo[] usesInfoArray;
        }

        public class TableInfo
        {
            public TableInfo(string name, UInt32 tableOffset, UInt32 tableColumnOffset, EdValueType columns, EdValueType rows)
            {
                this.name = name;
                this.tableOffset = tableOffset;
                this.tableColumnOffset = tableColumnOffset;
                this.columns = columns;
                this.rows = rows;
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public UInt32 TableOffset
            {
                get
                {
                    return tableOffset;
                }
            }

            public UInt32 TableColumnOffset
            {
                get
                {
                    return tableColumnOffset;
                }
            }

            public EdValueType Columns
            {
                get
                {
                    return columns;
                }
            }

            public EdValueType Rows
            {
                get
                {
                    return rows;
                }
            }

            public Dictionary<string, UInt32> ColumnNameDict
            {
                get
                {
                    return columnNameDict;
                }
                set
                {
                    columnNameDict = value;
                }
            }

            public Dictionary<string, UInt32>[] SeekColumnStringDicts
            {
                get
                {
                    return seekColumnStringDicts;
                }
                set
                {
                    seekColumnStringDicts = value;
                }
            }

            public Dictionary<EdValueType, UInt32>[] SeekColumnValueDicts
            {
                get
                {
                    return seekColumnValueDicts;
                }
                set
                {
                    seekColumnValueDicts = value;
                }
            }

            public EdValueType[][] TableEntries
            {
                get
                {
                    return tableEntries;
                }
                set
                {
                    tableEntries = value;
                }
            }

            private string name;
            private UInt32 tableOffset;
            private UInt32 tableColumnOffset;
            private EdValueType columns;
            private EdValueType rows;
            private Dictionary<string, UInt32> columnNameDict;
            private Dictionary<string, UInt32>[] seekColumnStringDicts;
            private Dictionary<EdValueType, UInt32>[] seekColumnValueDicts;
            private EdValueType[][] tableEntries;
        }

        public class TableInfos
        {
            public TableInfo[] TableInfoArray
            {
                get
                {
                    return tableInfoArray;
                }
                set
                {
                    tableInfoArray = value;
                }
            }

            public Dictionary<string, UInt32> TableNameDict
            {
                get
                {
                    return tableNameDict;
                }
                set
                {
                    tableNameDict = value;
                }
            }

            private TableInfo[] tableInfoArray;
            private Dictionary<string, UInt32> tableNameDict;
        }

        private static readonly Encoding encoding = Encoding.GetEncoding(1252);
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly byte[] byteArray0 = new byte[0];
        private static readonly byte[] byteArrayMaxZero = new byte[MAX_ARRAY_LENGTH];
        private static Dictionary<ErrorCodes, UInt32> trapBitDict;

        private const string jobNameInit = "INITIALISIERUNG";
        private const string jobNameExit = "ENDE";
        private const string jobNameIdent = "IDENTIFIKATION";

        private bool disposed = false;
        private Stack<byte> stackList = new Stack<byte>();
        private List<string> argList = new List<string>();
        private Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>();
        private Dictionary<string, bool> resultsRequestDict = new Dictionary<string, bool>();
        private List<Dictionary<string, ResultData>> resultSets = new List<Dictionary<string, ResultData>>();
        private Dictionary<string, string> configDict = new Dictionary<string, string>();
        private Dictionary<string, string> groupMappingDict = new Dictionary<string, string>();
        private Dictionary<string, byte[]> sharedDataDict = new Dictionary<string, byte[]>();
        private long infoProgressRange;
        private long infoProgressPos;
        private string infoProgressText = string.Empty;
        private string sgbdFileName = string.Empty;
        private string fileSearchDir = string.Empty;
        private EdCommBase edCommClass;
        private static long timeMeas = 0;
        private byte[] sendBuffer = new byte[MAX_ARRAY_LENGTH];
        private byte[] recBuffer = new byte[MAX_ARRAY_LENGTH];
        private byte[] opArgBuffer = new byte[5];
        private AbortJobDelegate abortJobFunc = null;
        private ProgressJobDelegate progressJobFunc = null;
        private ErrorRaisedDelegate errorRaisedFunc = null;
        private StreamWriter swLog = null;
        private EdValueType arraySize = 1024;
        private EdValueType[] commParameter = new EdValueType[0];
        private EdValueType commRepeats = 0;
        private Int16[] commAnswerLen = new Int16[0];
        private EdValueType errorTrapMask = 0;
        private int errorTrapBitNr = -1;
        private ErrorCodes errorCodeLast = ErrorCodes.EDIABAS_ERR_NONE;
        private byte[] byteRegisters;
        private EdFloatType[] floatRegisters;
        private StringData[] stringRegisters;
        private Flags flags = new Flags();
        private Stream sgbdFs = null;
        private Stream sgbdBaseFs = null;
        private EdValueType pcCounter = 0;
        private JobInfos jobInfos = null;
        private UsesInfos usesInfos = null;
        private TableInfos tableInfos = null;
        private TableInfos tableInfosExt = null;
        private Stream tableFs = null;
        private Int32 tableIndex = -1;
        private Int32 tableRowIndex = -1;
        private Stream[] userFilesArray = new Stream[MAX_FILES];
        private byte[] tableItemBuffer = new byte[1024];
        private string tokenSeparator = string.Empty;
        private EdValueType tokenIndex = 0;
        private EdValueType floatPrecision = 4;
        private bool jobEnd = false;
        private bool requestInit = true;

        public Dictionary<string, string> GroupMappingDict
        {
            get
            {
                return groupMappingDict;
            }
        }

        public List<string> ArgList
        {
            get
            {
                return argList;
            }
        }

        public string ArgString
        {
            get
            {
                string result = string.Empty;
                foreach (string arg in ArgList)
                {
                    if (result.Length > 0)
                    {
                        result += ";";
                    }
                    result += arg;
                }
                return result;
            }
            set
            {
                argList.Clear();
                if (value.Length > 0)
                {
                    string[] words = value.Split(';');
                    foreach (string word in words)
                    {
                        argList.Add(word);
                    }
                }
            }
        }

        public List<Dictionary<string, ResultData>> ResultSets
        {
            get
            {
                return resultSets;
            }
        }

        public Dictionary<string, bool> ResultsRequestDict
        {
            get
            {
                return resultsRequestDict;
            }
        }

        public string ResultsRequests
        {
            get
            {
                string result = string.Empty;
                foreach (string arg in resultsRequestDict.Keys)
                {
                    if (result.Length > 0)
                    {
                        result += ";";
                    }
                    result += arg;
                }
                return result;
            }
            set
            {
                resultsRequestDict.Clear();
                if (value.Length > 0)
                {
                    string[] words = value.Split(';');
                    foreach (string word in words)
                    {
                        if (word.Length > 0)
                        {
                            resultsRequestDict.Add(word, true);
                        }
                    }
                }
            }
        }

        public int InfoProgressPercent
        {
            get
            {
                if ((infoProgressPos < 0) || (infoProgressRange <= 0))
                {
                    return -1;
                }
                return (int)(infoProgressPos * 100 / infoProgressRange);
            }
        }

        public string InfoProgressText
        {
            get
            {
                return infoProgressText;
            }
        }

        public AbortJobDelegate AbortJobFunc
        {
            get
            {
                return abortJobFunc;
            }
            set
            {
                abortJobFunc = value;
            }
        }

        public ProgressJobDelegate ProgressJobFunc
        {
            get
            {
                return progressJobFunc;
            }
            set
            {
                progressJobFunc = value;
            }
        }

        public ErrorRaisedDelegate ErrorRaisedFunc
        {
            get
            {
                return errorRaisedFunc;
            }
            set
            {
                errorRaisedFunc = value;
            }
        }

        private EdValueType ArrayMaxSize
        {
            get
            {
                return arraySize - 1;
            }
        }

        public EdValueType[] CommParameter
        {
            get
            {
                return commParameter;
            }
        }

        public EdValueType CommRepeats
        {
            get
            {
                return commRepeats;
            }
        }

        public Int16[] CommAnswerLen
        {
            get
            {
                return commAnswerLen;
            }
        }

        public StreamWriter SwLog
        {
            get
            {
                return swLog;
            }
            set
            {
                swLog = value;
            }
        }

        public string SgbdFileName
        {
            get
            {
                return sgbdFileName;
            }
            set
            {
                CloseSgbdFs();
                sgbdFileName = value;
            }
        }

        public string FileSearchDir
        {
            get
            {
                return fileSearchDir;
            }
            set
            {
                CloseSgbdFs();
                fileSearchDir = value;
                SetConfigProperty("EcuPath", fileSearchDir.TrimEnd(Path.DirectorySeparatorChar));
            }
        }

        public EdCommBase EdCommClass
        {
            get
            {
                return edCommClass;
            }
            set
            {
                edCommClass = value;
            }
        }

        public long TimeMeas
        {
            get
            {
                return timeMeas;
            }
            set
            {
                timeMeas = value;
            }
        }

        public ErrorCodes ErrorCodeLast
        {
            get
            {
                return errorCodeLast;
            }
        }

        public Ediabas()
        {
            if (trapBitDict == null)
            {
                trapBitDict = new Dictionary<ErrorCodes, UInt32>();
                trapBitDict.Add(ErrorCodes.EDIABAS_BIP_0002, 2);
                trapBitDict.Add(ErrorCodes.EDIABAS_BIP_0006, 6);
                trapBitDict.Add(ErrorCodes.EDIABAS_BIP_0009, 9);
                trapBitDict.Add(ErrorCodes.EDIABAS_BIP_0010, 10);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0001, 11);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0002, 12);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0003, 13);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0004, 14);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0005, 15);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0006, 16);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0007, 17);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0008, 18);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0009, 19);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0010, 20);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0011, 21);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0012, 22);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0013, 23);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0014, 24);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0015, 25);
                trapBitDict.Add(ErrorCodes.EDIABAS_IFH_0016, 26);
            }

            byteRegisters = new byte[32];
            floatRegisters = new EdFloatType[16];
            stringRegisters = new StringData[16];

            for (int i = 0; i < stringRegisters.Length; i++)
            {
                stringRegisters[i] = new StringData(this);
            }
            foreach (Register arg in registerList)
            {
                arg.SetEdiabas(this);
            }
            SetConfigProperty("Simulation", "0");
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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    CloseSgbdFs();
                    CloseTableFs();
                    CloseAllUserFiles();
                    if (SwLog != null)
                    {
                        SwLog.Dispose();
                        SwLog = null;
                    }
                    if (edCommClass != null)
                    {
                        edCommClass.Dispose();
                        edCommClass = null;
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        public string GetConfigProperty(string name)
        {
            string key = name.ToUpper(culture);
            string value;
            if (!configDict.TryGetValue(key, out value))
            {
                return null;
            }
            return value;
        }

        public void SetConfigProperty(string name, string value)
        {
            string key = name.ToUpper(culture);
            if (configDict.ContainsKey(key))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    configDict[key] = value;
                }
                else
                {
                    configDict.Remove(key);
                }
            }
            else
            {
                configDict.Add(key, value);
            }
        }

        public JobInfo GetJobInfo(string jobName)
        {
            UInt32 jobIndex;
            if (jobInfos.JobNameDict.TryGetValue(jobName.ToUpper(culture), out jobIndex))
            {
                return jobInfos.JobInfoArray[jobIndex];
            }
            return null;
        }

        public static string GetExceptionText(Exception ex)
        {
            string text = ex.Message;
            Exception exIter = ex;
            while (exIter.InnerException != null)
            {
                text += "\r\n" + exIter.InnerException.Message;
                exIter = exIter.InnerException;
            }
            return text;
        }

        private bool OpenSgbdFs()
        {
            if (sgbdFs != null)
            {
                return true;
            }
            string fileName = Path.Combine(fileSearchDir, sgbdFileName);
            if (!File.Exists(fileName))
            {
                LogString("OpenSgbdFs file not found: " + fileName);
                return false;
            }
            try
            {
                sgbdFs = MemoryStreamReader.OpenRead(fileName);
            }
            catch (Exception ex)
            {
                LogString("OpenSgbdFs exception: " + GetExceptionText(ex));
                return false;
            }
            sharedDataDict.Clear();
            usesInfos = ReadAllUses(sgbdFs);
            jobInfos = ReadAllJobs(sgbdFs);
            tableInfos = ReadAllTables(sgbdFs);
            requestInit = true;
            return true;
        }

        private void CloseSgbdFs()
        {
            if (sgbdFs != null)
            {
                try
                {
                    ExecuteExitJob();
                }
                catch (Exception ex)
                {
                    LogString("CloseSgbdFs exception: " + GetExceptionText(ex));
                }
                sgbdFs.Dispose();
                sgbdFs = null;
            }
        }

        private void CloseTableFs()
        {
            if (tableFs != null)
            {
                if (tableFs != sgbdBaseFs)
                {
                    tableFs.Dispose();
                }
                tableFs = null;
            }
            tableInfosExt = null;
            tableIndex = -1;
            tableRowIndex = -1;
        }

        private Stream GetTableFs()
        {
            if (tableFs != null)
            {
                return tableFs;
            }
            return sgbdFs;
        }

        private void SetTableFs(Stream fs)
        {
            tableFs = fs;
            tableInfosExt = ReadAllTables(fs);
        }

        private TableInfos GetTableInfos(Stream fs)
        {
            if (tableFs != null && tableFs == fs)
            {
                return tableInfosExt;
            }
            return tableInfos;
        }

        private int StoreUserFile(Stream fs)
        {
            for (int i = 0; i < userFilesArray.Length; i++)
            {
                if (userFilesArray[i] == null)
                {
                    userFilesArray[i] = fs;
                    return i;
                }
            }
            return -1;
        }

        private Stream GetUserFile(int index)
        {
            if ((index < 0) || (index >= userFilesArray.Length))
            {
                return null;
            }
            return userFilesArray[index];
        }

        private string ReadFileLine(Stream fs)
        {
            StringBuilder stringBuilder = new StringBuilder(100);
            int currByte = -1;
            for (; ; )
            {
                currByte = fs.ReadByte();
                if (currByte < 0)
                {
                    break;
                }
                if (currByte == '\r' || currByte == '\n')
                {
                    break;
                }
                stringBuilder.Append((Char)currByte);
            }
            if (currByte < 0)
            {
                return null;
            }
            if (currByte == '\r')
            {
                int nextByte = fs.ReadByte();
                if (nextByte >= 0 && nextByte != '\n')
                {
                    fs.Position--;
                }
            }
            return stringBuilder.ToString();
        }

        private long ReadFileLineLength(Stream fs)
        {
            StringBuilder stringBuilder = new StringBuilder(100);
            int currByte = -1;
            long lineLength = 0;
            for (; ; )
            {
                currByte = fs.ReadByte();
                if (currByte < 0)
                {
                    break;
                }
                if (currByte == '\r' || currByte == '\n')
                {
                    break;
                }
                lineLength++;
            }
            if (currByte < 0)
            {
                return -1;
            }
            if (currByte == '\r')
            {
                int nextByte = fs.ReadByte();
                if (nextByte >= 0 && nextByte != '\n')
                {
                    fs.Position--;
                }
            }
            return lineLength;
        }

        private bool CloseUserFile(int index)
        {
            if ((index < 0) || (index >= userFilesArray.Length))
            {
                return false;
            }
            if (userFilesArray[index] == null)
            {
                return true;
            }
            userFilesArray[index].Dispose();
            userFilesArray[index] = null;
            return true;
        }

        private void CloseAllUserFiles()
        {
            for (int i = 0; i < userFilesArray.Length; i++)
            {
                if (userFilesArray[i] != null)
                {
                    userFilesArray[i].Dispose();
                    userFilesArray[i] = null;
                }
            }
        }

        public void SetError(ErrorCodes error)
        {
            if (swLog != null)
            {
                LogString(string.Format("SetError: {0}", error));
            }

            if (error != ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdValueType bitNumber;
                if (trapBitDict.TryGetValue(error, out bitNumber))
                {
                    this.errorTrapBitNr = (int) bitNumber;
                }
                else
                {
                    this.errorTrapBitNr = 0;
                }

                EdValueType activeErrors = (EdValueType)((1 << (int)this.errorTrapBitNr) & ~this.errorTrapMask);
                if (activeErrors != 0)
                {
                    RaiseError(error);
                }
            }
            else
            {
                this.errorTrapBitNr = -1;
            }
        }

        public void RaiseError(ErrorCodes error)
        {
            this.errorCodeLast = error;
            if (errorRaisedFunc != null)
            {
                errorRaisedFunc(error);
            }
            throw new Exception(string.Format("Error occured: {0}", error));
        }

        private void SetResultData(ResultData resultData)
        {
            string key = resultData.name;
            if (resultDict.ContainsKey(key))
            {
                resultDict[key] = resultData;
            }
            else
            {
                resultDict.Add(key, resultData);
            }
        }

        private ResultData GetResultData(string name)
        {
            ResultData result;

            if (resultDict.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }

        private void JobProgressInform()
        {
            if (progressJobFunc != null)
            {
                progressJobFunc(this);
            }
        }

        public UsesInfos ReadAllUses(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x7C;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            UInt32 usesOffset = BitConverter.ToUInt32(buffer, 0);
            fs.Position = usesOffset;
            int usesCount = readInt32(fs);

            UsesInfos usesInfosLocal = new UsesInfos();
            usesInfosLocal.UsesInfoArray = new UsesInfo[usesCount];

            byte[] usesBuffer = new byte[0x100];
            for (int i = 0; i < usesCount; i++)
            {
                readAndDecryptBytes(fs, usesBuffer, 0, 0x100);
                string usesName = encoding.GetString(usesBuffer, 0, 0x100).TrimEnd('\0');
                usesInfosLocal.UsesInfoArray[i] = new UsesInfo(usesName);
            }

            return usesInfosLocal;
        }

        public JobInfos ReadAllJobs(Stream fs)
        {
            List<JobInfo> jobListComplete = GetJobList(fs, null);

            foreach (UsesInfo usesInfo in usesInfos.UsesInfoArray)
            {
                string fileName = Path.Combine(fileSearchDir, usesInfo.Name.ToLower(culture) + ".prg");
                if (File.Exists(fileName))
                {
                    try
                    {
                        using (Stream tempFs = MemoryStreamReader.OpenRead(fileName))
                        {
                            List<JobInfo> jobListTemp = GetJobList(tempFs, usesInfo);
                            jobListComplete.AddRange(jobListTemp);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogString("ReadAllJobs exception: " + GetExceptionText(ex));
                    }
                }
            }

            int numJobs = jobListComplete.Count;
            JobInfos jobInfosLocal = new JobInfos();
            jobInfosLocal.JobNameDict = new Dictionary<string, UInt32>();
            jobInfosLocal.JobInfoArray = new JobInfo[numJobs];

            EdValueType index = 0;
            foreach (JobInfo jobInfo in jobListComplete)
            {
                string key = jobInfo.JobName.ToUpper(culture);
                bool addJob = true;
                if (jobInfo.UsesInfo != null)
                {
                    if ((string.Compare(key, jobNameInit, StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(key, jobNameExit, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        addJob = false;
                    }
                }
                if (addJob && !jobInfosLocal.JobNameDict.ContainsKey(key))
                {
                    jobInfosLocal.JobNameDict.Add(key, index);
                }
                jobInfosLocal.JobInfoArray[index] = jobInfo;
                index++;
            }

            return jobInfosLocal;
        }

        public List<JobInfo> GetJobList(Stream fs, UsesInfo usesInfo)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x88;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            UInt32 jobListOffset = BitConverter.ToUInt32(buffer, 0);
            fs.Position = jobListOffset;
            int numJobs = readInt32(fs);

            List<JobInfo> jobList = new List<JobInfo>();

            byte[] jobBuffer = new byte[0x44];
            UInt32 jobStart = (UInt32)fs.Position;
            for (int i = 0; i < numJobs; i++)
            {
                fs.Position = jobStart;
                readAndDecryptBytes(fs, jobBuffer, 0, jobBuffer.Length);
                string jobNameString = encoding.GetString(jobBuffer, 0, 0x40).TrimEnd('\0');
                UInt32 jobAddress = BitConverter.ToUInt32(jobBuffer, 0x40);
#if false
                //if (String.Compare(jobNameString, "STATUS_RAILDRUCK_IST", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    fs.Position = jobAddress;
                    bool foundFirstEoj = false;
                    byte[] ocBuffer = new byte[2];
                    Operand arg0 = new Operand(this);
                    Operand arg1 = new Operand(this);
                    long startTime = Stopwatch.GetTimestamp();
                    while (true)
                    {
                        readAndDecryptBytes(fs, ocBuffer, 0, ocBuffer.Length);

                        byte opCodeVal = ocBuffer[0];
                        byte opAddrMode = ocBuffer[1];
                        OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
                        OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

                        if (opCodeVal >= ocList.Length)
                        {
                            throw new ArgumentOutOfRangeException("opCodeVal", "ReadAllJobs: Opcode out of range");
                        }
                        getOpArg(fs, opAddrMode0, ref arg0);
                        getOpArg(fs, opAddrMode1, ref arg1);

                        if (opCodeVal == 0x1D)
                        {
                            if (foundFirstEoj)
                                break;
                            foundFirstEoj = true;
                        }
                        else
                        {
                            foundFirstEoj = false;
                        }
                    }
                    timeMeas += Stopwatch.GetTimestamp() - startTime;
                }
                UInt32 jobSize = (UInt32)(fs.Position - jobAddress);
                jobInfosLocal.JobInfoArray[i] = new JobInfo(jobAddress, jobSize);
#else
                jobList.Add(new JobInfo(jobNameString, jobAddress, 0, usesInfo));
#endif
                jobStart += 0x44;
            }
            return jobList;
        }

        public TableInfos ReadAllTables(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x84;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            UInt32 tableOffset = BitConverter.ToUInt32(buffer, 0);
            fs.Position = tableOffset;

            byte[] tableCountBuffer = new byte[4];
            readAndDecryptBytes(fs, tableCountBuffer, 0, tableCountBuffer.Length);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(tableCountBuffer, 0, 4);
            }
            int tableCount = BitConverter.ToInt32(tableCountBuffer, 0);

            TableInfos tableInfosLocal = new TableInfos();
            tableInfosLocal.TableNameDict = new Dictionary<string,UInt32>();
            tableInfosLocal.TableInfoArray = new TableInfo[tableCount];

            UInt32 tableStart = (UInt32)fs.Position;
            for (int i = 0; i < tableCount; ++i)
            {
                TableInfo tableInfo = ReadTable(fs, tableStart);
                tableInfosLocal.TableInfoArray[i] = tableInfo;
                tableInfosLocal.TableNameDict.Add(tableInfo.Name.ToUpper(culture), (UInt32)i);
                tableStart += 0x50;
            }
            return tableInfosLocal;
        }

        public TableInfo ReadTable(Stream fs, UInt32 tableOffset)
        {
            fs.Position = tableOffset;

            byte[] tableBuffer = new byte[0x50];
            readAndDecryptBytes(fs, tableBuffer, 0, tableBuffer.Length);
            string name = encoding.GetString(tableBuffer, 0, 0x40).TrimEnd('\0');

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(tableBuffer, 0x40, 4);
            }
            UInt32 tableColumnOffset = BitConverter.ToUInt32(tableBuffer, 0x40);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(tableBuffer, 0x48, 4);
            }
            UInt32 tableColumnCount = BitConverter.ToUInt32(tableBuffer, 0x48);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(tableBuffer, 0x4C, 4);
            }
            UInt32 tableRowCount = BitConverter.ToUInt32(tableBuffer, 0x4C);

            return new TableInfo(name, tableOffset, tableColumnOffset, tableColumnCount, tableRowCount);
        }

        public void IndexTable(Stream fs, TableInfo tableInfo)
        {
            if (tableInfo.TableEntries != null)
            {
                return;
            }
            fs.Position = tableInfo.TableColumnOffset;

            Dictionary<string, UInt32> columnNameDict = new Dictionary<string, UInt32>();
            EdValueType[][] tableEntries = new EdValueType[tableInfo.Rows + 1][];
            for (int j = 0; j < tableInfo.Rows + 1; j++)
            {
                EdValueType[] tableRow = new EdValueType[tableInfo.Columns];
                tableEntries[j] = tableRow;
                for (int k = 0; k < tableInfo.Columns; k++)
                {
                    tableRow[k] = (EdValueType)fs.Position;
                    int l;
                    for (l = 0; l < tableItemBuffer.Length; l++)
                    {
                        readAndDecryptBytes(fs, tableItemBuffer, l, 1);
                        if (tableItemBuffer[l] == 0)
                            break;
                    }
                    if (j == 0)
                    {
                        string columnName = encoding.GetString(tableItemBuffer, 0, l).ToUpper(culture);
                        columnNameDict.Add(columnName, (UInt32)k);
                    }
                }
            }

            tableInfo.ColumnNameDict = columnNameDict;
            tableInfo.TableEntries = tableEntries;
        }

        public string GetTableString(Stream fs, UInt32 stringOffset)
        {
            fs.Position = stringOffset;

            int l;
            for (l = 0; l < tableItemBuffer.Length; ++l)
            {
                readAndDecryptBytes(fs, tableItemBuffer, l, 1);
                if (tableItemBuffer[l] == 0)
                    break;
            }
            return encoding.GetString(tableItemBuffer, 0, l);
        }

        public Int32 GetTableIndex(Stream fs, string tableName, out bool found)
        {
            found = false;
            TableInfos tableInfosLocal = GetTableInfos(fs);

            UInt32 tableIdx;
            if (tableInfosLocal.TableNameDict.TryGetValue(tableName.ToUpper(culture), out tableIdx))
            {
                IndexTable(fs, tableInfosLocal.TableInfoArray[tableIdx]);
                found = true;
                return (Int32)tableIdx;
            }

            return tableInfosLocal.TableInfoArray.Count() - 1;
        }

        public UInt32 GetTableColumns(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Columns;
        }

        public UInt32 GetTableRows(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Rows;
        }

        public Int32 GetTableLine(Stream fs, Int32 tableIdx, EdValueType line, out bool found)
        {
            found = false;
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return -1;
            }
            TableInfo table = tableArray[tableIdx];
            if (line >= table.Rows)
            {   // select last line
                return (Int32)(table.Rows - 1);
            }
            found = true;
            return (Int32) line;
        }

        public Int32 SeekTable(Stream fs, Int32 tableIdx, string columnName, string valueName, out bool found)
        {
            found = false;
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return -1;
            }
            TableInfo table = tableArray[tableIdx];

            int columnIndex = GetTableColumnIdx(fs, table, columnName);
            if (columnIndex < 0)
            {
                return -1;
            }

            if (table.SeekColumnStringDicts == null)
            {
                table.SeekColumnStringDicts = new Dictionary<string, UInt32>[table.Columns];
            }

            Dictionary<string, UInt32> columnDict;
            if (table.SeekColumnStringDicts[columnIndex] == null)
            {   // create new dict
                table.SeekColumnStringDicts[columnIndex] = new Dictionary<string, UInt32>();
                columnDict = table.SeekColumnStringDicts[columnIndex];
                for (int i = 1; i < table.TableEntries.Length; i++)
                {
                    string rowStr = GetTableString(fs, table.TableEntries[i][columnIndex]).ToUpper(culture);
                    if (!columnDict.ContainsKey(rowStr))
                    {
                        columnDict.Add(rowStr, (UInt32)(i - 1));
                    }
                }
            }
            else
            {
                columnDict = table.SeekColumnStringDicts[columnIndex];
            }

            UInt32 selectedRow;
            if (columnDict.TryGetValue(valueName.ToUpper(culture), out selectedRow))
            {
                found = true;
                return (Int32)selectedRow;
            }

            return (Int32)(table.Rows - 1); // select last line
        }

        public Int32 SeekTable(Stream fs, Int32 tableIdx, string columnName, EdValueType value, out bool found)
        {
            found = false;
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return -1;
            }
            TableInfo table = tableArray[tableIdx];

            int columnIndex = GetTableColumnIdx(fs, table, columnName);
            if (columnIndex < 0)
            {
                return -1;
            }

            if (table.SeekColumnValueDicts == null)
            {
                table.SeekColumnValueDicts = new Dictionary<EdValueType, UInt32>[table.Columns];
            }

            Dictionary<EdValueType, UInt32> columnDict;
            if (table.SeekColumnValueDicts[columnIndex] == null)
            {   // create new dict
                table.SeekColumnValueDicts[columnIndex] = new Dictionary<EdValueType, UInt32>();
                columnDict = table.SeekColumnValueDicts[columnIndex];
                for (int i = 1; i < table.TableEntries.Length; i++)
                {
                    string rowStr = GetTableString(fs, table.TableEntries[i][columnIndex]);
                    EdValueType rowValue = (EdValueType)StringToValue(rowStr);
                    if (!columnDict.ContainsKey(rowValue))
                    {
                        columnDict.Add(rowValue, (UInt32)(i - 1));
                    }
                }
            }
            else
            {
                columnDict = table.SeekColumnValueDicts[columnIndex];
            }

            UInt32 selectedRow;
            if (columnDict.TryGetValue(value, out selectedRow))
            {
                found = true;
                return (Int32)selectedRow;
            }

            return (Int32)(table.Rows - 1); // select last line
        }

        public Int32 GetTableColumnIdx(Stream fs, TableInfo table, string columnName)
        {
            UInt32 column;
            if (table.ColumnNameDict.TryGetValue(columnName.ToUpper(culture), out column))
            {
                return (Int32)column;
            }
            return -1;
        }

        public string GetTableEntry(Stream fs, Int32 tableIdx, Int32 rowIdx, string columnName)
        {
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return null;
            }
            TableInfo table = tableArray[tableIdx];

            if ((rowIdx < 0) || rowIdx >= table.Rows)
            {
                return null;
            }

            int columnIndex = GetTableColumnIdx(fs, table, columnName);
            if (columnIndex < 0)
            {
                return null;
            }
            return GetTableString(fs, table.TableEntries[rowIdx + 1][columnIndex]);
        }

        public void ResolveSgbdFile(string fileName)
        {
            SgbdFileName = Path.GetFileName(fileName);
            if (String.Compare(Path.GetExtension(fileName), ".grp", StringComparison.OrdinalIgnoreCase) == 0)
            {       // group file
                string key = SgbdFileName.ToLower(culture);
                string variantName = string.Empty;
                if (!groupMappingDict.TryGetValue(key, out variantName))
                {
                    variantName = ExecuteIdentJob().ToLower(culture);
                    if (variantName.Length == 0)
                    {
                        throw new ArgumentOutOfRangeException("variantName", "ResolveSgbdFile: No variant found");
                    }
                    groupMappingDict.Add(key, variantName);
                }
                SgbdFileName = variantName + ".prg";
            }
        }

        public void ExecuteInitJob()
        {
            try
            {
                ExecuteJob(jobNameInit);
            }
            catch (Exception ex)
            {
                if (swLog != null)
                {
                    LogString("executeInitJob Exception: " + ex.Message);
                }
                throw new Exception("executeInitJob", ex);
            }
            if (resultSets.Count > 0)
            {
                ResultData result;
                if (resultSets[0].TryGetValue("DONE", out result))
                {
                    if (result.opData.GetType() == typeof(Int64))
                    {
                        if ((Int64)result.opData == 1)
                        {
                            if (swLog != null)
                            {
                                LogString("executeInitJob ok");
                            }
                            return;
                        }
                    }
                }
            }

            if (swLog != null)
            {
                LogString("executeInitJob failed");
            }
            throw new Exception("executeInitJob: Initialization failed");
        }

        public void ExecuteExitJob()
        {
            try
            {
                if (sgbdFs != null && GetJobInfo(jobNameExit) != null)
                {
                    ExecuteJob(jobNameExit);
                }
            }
            catch (Exception ex)
            {
                if (swLog != null)
                {
                    LogString("ExecuteExitJob Exception: " + ex.Message);
                }
                throw new Exception("ExecuteExitJob", ex);
            }
        }

        public string ExecuteIdentJob()
        {
            resultDict.Clear();
            try
            {
                ExecuteJob(jobNameIdent);
            }
            catch (Exception ex)
            {
                if (swLog != null)
                {
                    LogString("executeIdentJob Exception: " + ex.Message);
                }
                throw new Exception("executeInitJob", ex);
            }
            if (resultSets.Count > 0)
            {
                ResultData result;
                if (resultSets[0].TryGetValue("VARIANTE", out result))
                {
                    if (result.opData.GetType() == typeof(string))
                    {
                        string variantName = (string)result.opData;
                        if (swLog != null)
                        {
                            LogString("executeIdentJob ok: " + variantName);
                        }
                        return variantName;
                    }
                }
            }

            if (swLog != null)
            {
                LogString("executeIdentJob failed");
            }
            return string.Empty;
        }

        public void ExecuteJob(string jobName)
        {
            if (swLog != null)
            {
                LogString(string.Format("executeJob: {0}", jobName));
            }

            if (!OpenSgbdFs())
            {
                throw new ArgumentOutOfRangeException("OpenSgbdFs", "executeJob: Open SGBD failed");
            }
            JobInfo jobInfo = GetJobInfo(jobName);
            if (jobInfo == null)
            {
                throw new ArgumentOutOfRangeException("jobName", "executeJob: Job not found: " + jobName);
            }

            if (jobInfo.UsesInfo != null)
            {
                string fileName = Path.Combine(fileSearchDir, jobInfo.UsesInfo.Name.ToLower(culture) + ".prg");
                if (!File.Exists(fileName))
                {
                    throw new ArgumentOutOfRangeException("fileName", "executeJob: SGBD not found: " + fileName);
                }
                try
                {
                    using (Stream tempFs = MemoryStreamReader.OpenRead(fileName))
                    {
                        sgbdBaseFs = tempFs;
                        ExecuteJob(tempFs, jobInfo.JobOffset);
                    }
                }
                catch (Exception ex)
                {
                    LogString("executeJob base job exception: " + GetExceptionText(ex));
                    throw new Exception("executeJob base job exception", ex);
                }
                finally
                {
                    CloseTableFs();
                    sgbdBaseFs = null;
                }
            }
            else
            {
                ExecuteJob(sgbdFs, jobInfo.JobOffset);
            }
            if (swLog != null)
            {
                LogString("executeJob successfull");
            }
        }

        public void ExecuteJob(Stream fs, UInt32 jobAddress)
        {
            if (requestInit)
            {
                requestInit = false;
                ExecuteInitJob();
            }

            byte[] buffer = new byte[2];

            resultSets.Clear();
            resultDict.Clear();
            stackList.Clear();
            SetConfigProperty("BipEcuFile", Path.GetFileNameWithoutExtension(sgbdFileName));
            SetConfigProperty("BipDebugLevel", "0");
            flags.Init();
            errorTrapBitNr = -1;
            errorTrapMask = 0;
            errorCodeLast = ErrorCodes.EDIABAS_ERR_NONE;
            infoProgressRange = -1;
            infoProgressPos = -1;
            infoProgressText = string.Empty;

            pcCounter = jobAddress;
            CloseTableFs();
            CloseAllUserFiles();
            jobEnd = false;

            Operand arg0 = new Operand(this);
            Operand arg1 = new Operand(this);
            try
            {
                while (!jobEnd)
                {
                    //long startTime = Stopwatch.GetTimestamp();
                    EdValueType pcCounterOld = pcCounter;
                    fs.Position = pcCounter;
                    readAndDecryptBytes(fs, buffer, 0, buffer.Length);

                    byte opCodeVal = buffer[0];
                    byte opAddrMode = buffer[1];
                    OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
                    OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

                    if (opCodeVal >= ocList.Length)
                    {
                        throw new ArgumentOutOfRangeException("opCodeVal", "executeJob: Opcode out of range");
                    }
                    OpCode oc = ocList[opCodeVal];
                    getOpArg(fs, opAddrMode0, ref arg0);
                    getOpArg(fs, opAddrMode1, ref arg1);
                    pcCounter = (EdValueType)fs.Position;

                    //special near address arg0 opcode handling mainly for jumps
                    if (oc.arg0IsNearAddress && (opAddrMode0 == OpAddrMode.Imm32))
                    {
                        EdValueType labelAddress = pcCounter + (EdValueType)arg0.opData1;
                        arg0.opData1 = labelAddress;
                    }

                    if (abortJobFunc != null)
                    {
                        if (abortJobFunc())
                        {
                            throw new Exception("executeJob aborted");
                        }
                    }
                    //timeMeas += Stopwatch.GetTimestamp() - startTime;

                    if (oc.opFunc != null)
                    {
                        if (swLog != null)
                        {
                            LogString(">" + getOpText(pcCounterOld, oc, arg0, arg1));
                        }
                        oc.opFunc(this, oc, arg0, arg1);
                        if (swLog != null)
                        {
                            LogString("<" + getOpText(pcCounter, oc, arg0, arg1));
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(oc.pneumonic, "executeJob: Function not implemented");
                    }
                }
                if (resultDict.Count > 0)
                {
                    resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                }
                resultDict.Clear();
            }
            catch (Exception ex)
            {
                if (swLog != null)
                {
                    LogString("executeJob Exception: " + GetExceptionText(ex));
                }
                throw new Exception("executeJob", ex);
            }
            finally
            {
                CloseTableFs();
                CloseAllUserFiles();
                SetConfigProperty("BipEcuFile", null);
            }
        }

        public void LogString(string info)
        {
            if (swLog == null) return;
            try
            {
                swLog.WriteLine(info);
            }
            catch (Exception)
            {
            }
        }

        public void LogData(byte[] data, int length, string info)
        {
            if (swLog == null) return;
            string logString = "";

            for (int i = 0; i < length; i++)
            {
                logString += string.Format("{0:X02} ", data[i]);
            }
            try
            {
                swLog.Write(" (" + info + "): ");
                swLog.WriteLine(logString);
            }
            catch (Exception)
            {
            }
        }

        private void getOpArg(Stream fs, OpAddrMode opAddrMode, ref Operand oper)
        {
            try
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.None:
                        oper.Init(opAddrMode);
                        return;

                    case OpAddrMode.RegS:
                    case OpAddrMode.RegAB:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 1);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            oper.Init(opAddrMode, oaReg);
                            return;
                        }
                    case OpAddrMode.Imm8:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 1);
                            oper.Init(opAddrMode, (EdValueType)opArgBuffer[0]);
                            // string.Format("#${0:X}.B", buffer[0]);
                            return;
                        }
                    case OpAddrMode.Imm16:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 2);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 0, 2);
                            }
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt16(opArgBuffer, 0));
                            // string.Format("#${0:X}.I", BitConverter.ToInt16(buffer, 0));
                            return;
                        }
                    case OpAddrMode.Imm32:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 0, 4);
                            }
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt32(opArgBuffer, 0));
                            // string.Format("#${0:X}.L", BitConverter.ToInt32(buffer, 0));
                            return;
                        }
                    case OpAddrMode.ImmStr:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 2);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 0, 2);
                            }
                            short slen = BitConverter.ToInt16(opArgBuffer, 0);
                            byte[] buffer = new byte[slen];
                            readAndDecryptBytes(fs, buffer, 0, slen);
                            oper.Init(opAddrMode, buffer);
                            return;
                        }
                    case OpAddrMode.IdxImm:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 3);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(opArgBuffer, 1);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx);
                            // string.Format("{0}[#${1:X}]", oaReg.name, idx);
                            return;
                        }
                    case OpAddrMode.IdxReg:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 2);
                            Register oaReg0 = GetRegister(opArgBuffer[0]);
                            Register oaReg1 = GetRegister(opArgBuffer[1]);
                            oper.Init(opAddrMode, oaReg0, oaReg1);
                            // string.Format("{0}[{1}]", oaReg0.name, oaReg1.name);
                            return;
                        }
                    case OpAddrMode.IdxRegImm:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            Register oaReg0 = GetRegister(opArgBuffer[0]);
                            Register oaReg1 = GetRegister(opArgBuffer[1]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 2, 2);
                            }
                            EdValueType inc = BitConverter.ToUInt16(opArgBuffer, 2);
                            oper.Init(opAddrMode, oaReg0, oaReg1, (EdValueType)inc);
                            // string.Format("{0}[{1},#${2:X}]", oaReg0.name, oaReg1.name, inc);
                            return;
                        }
                    case OpAddrMode.IdxImmLenImm:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 5);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(opArgBuffer, 1);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 3, 2);
                            }
                            EdValueType len = BitConverter.ToUInt16(opArgBuffer, 3);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx, (EdValueType)len);
                            // string.Format("{0}[#${1:X}]#${2:X}", oaReg.name, idx, len);
                            return;
                        }
                    case OpAddrMode.IdxImmLenReg:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(opArgBuffer, 1);
                            Register oaLen = GetRegister(opArgBuffer[3]);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx, oaLen);
                            // string.Format("{0}[#${1:X}]{2}", oaReg.name, idx, oaLen.name);
                            return;
                        }
                    case OpAddrMode.IdxRegLenImm:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            Register oaIdx = GetRegister(opArgBuffer[1]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(opArgBuffer, 2, 2);
                            }
                            EdValueType len = BitConverter.ToUInt16(opArgBuffer, 2);
                            oper.Init(opAddrMode, oaReg, oaIdx, (EdValueType)len);
                            // string.Format("{0}[{1}]#${2:X}", oaReg.name, oaIdx.name, len);
                            return;
                        }
                    case OpAddrMode.IdxRegLenReg:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 3);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            Register oaIdx = GetRegister(opArgBuffer[1]);
                            Register oaLen = GetRegister(opArgBuffer[2]);
                            oper.Init(opAddrMode, oaReg, oaIdx, oaLen);
                            // string.Format("{0}[{1}]{2}", oaReg.name, oaIdx.name, oaLen.name);
                            return;
                        }
                    default:
                        throw new ArgumentOutOfRangeException("opAddrMode", "opAddrMode: Unsupported OpAddrMode");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("opAddrMode", ex);
            }
        }

        private static Register GetRegister(byte opcode)
        {
            Register result;
            if (opcode <= 0x33)
            {
                result = registerList[opcode];
            }
            else if (opcode >= 0x80)
            {
                int index = opcode - 0x80 + 0x34;
                if (index >= registerList.Length)
                {
                    throw new ArgumentOutOfRangeException("opcode", "GetRegister: Opcode out of range");
                }
                result = registerList[index];
            }
            else
            {
                throw new ArgumentOutOfRangeException("opcode", "GetRegister: Opcode out of range");
            }
            if (result.Opcode != opcode)
            {
                throw new ArgumentOutOfRangeException("opcode", "GetRegister: Opcode mapping invalid");
            }
            return result;
        }

        private static string getOpText(EdValueType addr, OpCode oc, Operand arg0, Operand arg1)
        {
            string result = string.Format("{0:X08}: ", addr);
            result += oc.pneumonic;
            string text1 = getOpArgText(arg0);
            string text2 = getOpArgText(arg1);
            if (text1.Length > 0)
            {
                result += " " + text1;
            }
            if (text2.Length > 0)
            {
                result += ", " + text2;
            }
            return result;
        }

        private static string getOpArgText(Operand arg)
        {
            try
            {
                string regName1 = string.Empty;
                if (arg.opData1 != null && arg.opData1.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.opData1;
                    regName1 = argValue.GetName();
                }

                string regName2 = string.Empty;
                if (arg.opData2 != null && arg.opData2.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.opData2;
                    regName2 = argValue.GetName();
                }

                string regName3 = string.Empty;
                if (arg.opData3 != null && arg.opData3.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.opData3;
                    regName3 = argValue.GetName();
                }

                switch (arg.AddrMode)
                {
                    case OpAddrMode.None:
                        return string.Empty;

                    case OpAddrMode.RegS:
                        {
                            Object data = arg.GetRawData();
                            if (data.GetType() == typeof(byte[]))
                            {
                                return regName1 + ": " + getStringText(arg.GetArrayData());
                            }
                            else if (data.GetType() == typeof(EdFloatType))
                            {
                                return regName1 + string.Format(": {0}", (EdFloatType)data);
                            }
                            return string.Empty;
                        }

                    case OpAddrMode.RegAB:
                        return regName1 + string.Format(": ${0:X}", arg.GetValueData());

                    case OpAddrMode.RegI:
                        return regName1 + string.Format(": ${0:X}", arg.GetValueData());

                    case OpAddrMode.RegL:
                        return regName1 + string.Format(": ${0:X}", arg.GetValueData());

                    case OpAddrMode.Imm8:
                        return string.Format("#${0:X}.B", arg.GetValueData());

                    case OpAddrMode.Imm16:
                        return string.Format("#${0:X}.I", arg.GetValueData());

                    case OpAddrMode.Imm32:
                        return string.Format("#${0:X}.L", arg.GetValueData());

                    case OpAddrMode.ImmStr:
                        return "#" + getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImm:
                        return regName1 + string.Format("[#${0:X}]: ", (EdValueType)arg.opData2) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxReg:
                        return regName1 + string.Format("[{0}: ${1:X}]: ", regName2, ((Register)arg.opData2).GetValueData()) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegImm:
                        return regName1 + string.Format("[{0}: ${1:X},#${2:X}]: ", regName2, ((Register)arg.opData2).GetValueData(), (EdValueType)arg.opData3) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImmLenImm:
                        return regName1 + string.Format("[#{0:X}],#${1:X}: ", (EdValueType)arg.opData2, (EdValueType)arg.opData3) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImmLenReg:
                        return regName1 + string.Format("[#{0:X}],{1}: #${2:X}: ", (EdValueType)arg.opData2, regName3, ((Register)arg.opData3).GetValueData()) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegLenImm:
                        return regName1 + string.Format("[{0}: ${1:X}],#${2:X}: ", regName2, ((Register)arg.opData2).GetValueData(), (EdValueType)arg.opData3) +
                            getStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegLenReg:
                        return regName1 + string.Format("[{0}: ${1:X}],{2}: #${3:X}: ", regName2, ((Register)arg.opData2).GetValueData(), regName3, ((Register)arg.opData3).GetValueData()) +
                            getStringText(arg.GetArrayData());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("opAddrMode", ex);
            }
            return string.Empty;
        }

        private static string getStringText(byte[] dataArray)
        {
            bool printable = true;
            int length = dataArray.Length;
            if (length < 1)
            {
                return "{ }";
            }
            for (int i = 0; i < length - 1; ++i)
                if (!isPrintable(dataArray[i]))
                {
                    printable = false;
                    break;
                }

            if (printable && (dataArray[length - 1] == 0))
                return "\"" + encoding.GetString(dataArray, 0, length - 1) + "\"";

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < length; i++)
                sb.AppendFormat("${0:X}.B,", dataArray[i]);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("}");
            return sb.ToString();
        }

        private static bool isPrintable(byte b)
        {
            return ((b == 9) || (b == 10) || (b == 13) || ((b >= 32) && (b < 127)));
        }

        private static void readAndDecryptBytes(Stream fs, byte[] buffer, int offset, int count)
        {
            fs.Read(buffer, offset, count);
            for (int i = offset; i < (offset + count); ++i)
                buffer[i] ^= 0xF7;
        }

        private static int readInt32(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        static double RoundToSignificantDigits(EdFloatType value, EdValueType digits)
        {
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * Math.Round(value / scale, (int) digits);
        }

        private static Int64 StringToValue(string number)
        {
            Int64 value = 0;
            string numberLocal = number.TrimEnd();
            if (numberLocal.Length == 0)
            {
                return value;
            }
            string numberLower = numberLocal.ToLower(culture);
            try
            {
                if (numberLower.StartsWith("0x", StringComparison.Ordinal))
                {   // hex
                    if (numberLower.Length > 2)
                    {
                        Char firstChar = numberLower[2];
                        if (Char.IsDigit(firstChar) || (firstChar >= 'a' && firstChar <= 'f'))
                        {
                            value = Convert.ToInt64(numberLocal.Substring(2, numberLocal.Length - 2), 16);
                        }
                    }
                }
                else if (numberLower.StartsWith("0y", StringComparison.Ordinal))
                {   // binary
                    value = Convert.ToInt64(numberLocal.Substring(2, numberLocal.Length - 2), 2);
                }
                else
                {   // dec
                    if ((string.Compare(numberLower, "-", StringComparison.Ordinal) != 0) &&
                        (string.Compare(numberLower, "--", StringComparison.Ordinal) != 0))
                    {
                        if (!Char.IsLetter(numberLower[0]))
                        {
                            value = Convert.ToInt64(numberLocal.TrimStart(), 10);
                        }
                    }
                }
            }
            catch (Exception)
            {
                value = 0;
            }
            return value;
        }

        private static EdFloatType StringToFloat(string number)
        {
            EdFloatType result = 0;
            try
            {
                number = number.Replace(",", ".");
                result = EdFloatType.Parse(number, culture);
            }
            catch (Exception)
            {
                result = Double.NaN;
            }
            return result;
        }

        private static string ValueToBcd(byte value)
        {
            string result = string.Empty;

            int part1 = (value >> 4) & 0x0F;
            int part2 = value & 0x0F;

            if (part1 > 9)
            {
                result += "*";
            }
            else
            {
                result += string.Format("{0:X}", part1);
            }

            if (part2 > 9)
            {
                result += "*";
            }
            else
            {
                result += string.Format("{0:X}", part2);
            }
            return result;
        }

        private static EdValueType GetArgsValueLength(Operand arg0, Operand arg1)
        {
            return arg0.GetDataLen(true);
        }
    }
}
