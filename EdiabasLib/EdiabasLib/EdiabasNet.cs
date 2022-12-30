#if Android
#define COMPRESS_TRACE
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
// ReSharper disable IntroduceOptionalParameters.Local
// ReSharper disable NotResolvedInText
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable RedundantCast
// ReSharper disable UseNameofExpression
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable NotAccessedVariable
// ReSharper disable InlineOutVariableDeclaration

namespace EdiabasLib
{
    using EdFloatType = Double;
    using EdValueType = UInt32;

    public partial class EdiabasNet : IDisposable
    {
        private delegate void OperationDelegate(EdiabasNet ediabas, OpCode oc, Operand arg0, Operand arg1);
        private delegate void VJobDelegate(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets);
        public delegate bool AbortJobDelegate();
        public delegate void ProgressJobDelegate(EdiabasNet ediabas);
        public delegate void ErrorRaisedDelegate(ErrorCodes error);

        private static readonly int MaxFiles = 5;

        private class OpCode
        {
            // ReSharper disable once NotAccessedField.Local
            private byte _oc;
            public readonly string Pneumonic;
            public readonly OperationDelegate OpFunc;
            public readonly bool Arg0IsNearAddress;

            public OpCode(byte oc, string pneumonic, OperationDelegate opFunc)
                : this(oc, pneumonic, opFunc, false)
            {
            }

            public OpCode(byte oc, string pneumonic, OperationDelegate opFunc, bool arg0IsNearAddress)
            {
                _oc = oc;
                Pneumonic = pneumonic;
                OpFunc = opFunc;
                Arg0IsNearAddress = arg0IsNearAddress;
            }
        }

        private class Operand
        {
            public Operand(EdiabasNet ediabas)
                : this(ediabas, OpAddrMode.None, null, null, null)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public Operand(EdiabasNet ediabas, OpAddrMode opAddrMode)
                : this(ediabas, opAddrMode, null, null, null)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public Operand(EdiabasNet ediabas, OpAddrMode opAddrMode, Object opData1)
                : this(ediabas, opAddrMode, opData1, null, null)
            {
            }

            // ReSharper disable once UnusedMember.Local
            public Operand(EdiabasNet ediabas, OpAddrMode opAddrMode, Object opData1, Object opData2)
                : this(ediabas, opAddrMode, opData1, opData2, null)
            {
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public Operand(EdiabasNet ediabas, OpAddrMode opAddrMode, Object opData1, Object opData2, Object opData3)
            {
                _ediabas = ediabas;
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
                _opAddrMode = opAddrMode;
                OpData1 = opData1;
                OpData2 = opData2;
                OpData3 = opData3;
            }

            public Type GetDataType()
            {
                switch (_opAddrMode)
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

            // ReSharper disable once MemberCanBePrivate.Local
            public EdValueType GetValueMask()
            {
                return GetValueMask(0);
            }

            // ReSharper disable once MemberCanBePrivate.Local
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
                switch (_opAddrMode)
                {
                    case OpAddrMode.RegS:
                    case OpAddrMode.ImmStr:
                        return (EdValueType)GetArrayData().Length;

                    case OpAddrMode.RegAb:
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
                switch (_opAddrMode)
                {
                    case OpAddrMode.RegS:
                    case OpAddrMode.RegAb:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData RegX: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
                            return arg1Data.GetRawData();
                        }

                    case OpAddrMode.Imm8:
                    case OpAddrMode.Imm16:
                    case OpAddrMode.Imm32:
                    case OpAddrMode.ImmStr:
                        {
                            return OpData1;
                        }

                    case OpAddrMode.IdxImm:
                    case OpAddrMode.IdxReg:
                    case OpAddrMode.IdxRegImm:
                        {
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxX: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
                            byte[] dataArray = arg1Data.GetArrayData(true);

                            EdValueType index;
                            if (_opAddrMode == OpAddrMode.IdxImm)
                            {
                                if (OpData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxX: Invalid data type2");
                                }
                                index = (EdValueType)OpData2;
                            }
                            else
                            {
                                if (OpData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxX: Invalid data type2");
                                }
                                Register arg2Data = (Register)OpData2;
                                index = arg2Data.GetValueData();
                            }

                            if (_opAddrMode == OpAddrMode.IdxRegImm)
                            {
                                if (OpData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.GetRawData IdxX: Invalid data type3");
                                }
                                index += (EdValueType)OpData3;
                            }

                            long requiredLength = (long)index + 1;
                            if (requiredLength > _ediabas.ArrayMaxSize)
                            {
                                _ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                                return ByteArray0;
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                return ByteArray0;
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
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxXLenX: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
                            byte[] dataArray = arg1Data.GetArrayData(true);

                            EdValueType index;
                            if ((_opAddrMode == OpAddrMode.IdxImmLenImm) || (_opAddrMode == OpAddrMode.IdxImmLenReg))
                            {
                                if (OpData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                index = (EdValueType)OpData2;
                            }
                            else
                            {
                                if (OpData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                Register arg2Data = (Register)OpData2;
                                index = arg2Data.GetValueData();
                            }

                            EdValueType len;
                            if ((_opAddrMode == OpAddrMode.IdxImmLenImm) || (_opAddrMode == OpAddrMode.IdxRegLenImm))
                            {
                                if (OpData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                len = (EdValueType)OpData3;
                            }
                            else
                            {
                                if (OpData1.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData1", "Operand.GetRawData IdxXLenX: Invalid data type");
                                }
                                Register arg3Data = (Register)OpData3;
                                len = arg3Data.GetValueData();
                            }

                            ulong requiredLength = (ulong) index + len;
                            if (requiredLength > _ediabas.ArrayMaxSize)
                            {
                                _ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                                return ByteArray0;
                            }
                            if (dataArray.Length < (long) requiredLength)
                            {
                                if (dataArray.Length <= index)
                                {
                                    return ByteArray0;
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
                if (rawData is EdValueType)
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
                return Encoding.GetString(data, 0, length);
            }

            public void SetRawData(Object data)
            {
                SetRawData(data, 1);
            }

            public void SetRawData(Object data, EdValueType dataLen)
            {
                switch (_opAddrMode)
                {
                    case OpAddrMode.RegS:
                        {
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData RegS: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
                            arg1Data.SetRawData(data);
                            return;
                        }

                    case OpAddrMode.RegAb:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData RegX: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
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
                            if (OpData1.GetType() != typeof(Register))
                            {
                                throw new ArgumentOutOfRangeException("opData1", "Operand.SetRawData IdxX: Invalid data type");
                            }
                            Register arg1Data = (Register)OpData1;
                            byte[] dataArray = arg1Data.GetArrayData();

                            EdValueType index;
                            if (_opAddrMode == OpAddrMode.IdxImm)
                            {
                                if (OpData2.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                index = (EdValueType)OpData2;
                            }
                            else
                            {
                                if (OpData2.GetType() != typeof(Register))
                                {
                                    throw new ArgumentOutOfRangeException("opData2", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                Register arg2Data = (Register)OpData2;
                                index = arg2Data.GetValueData();
                            }

                            if (_opAddrMode == OpAddrMode.IdxRegImm)
                            {
                                if (OpData3.GetType() != typeof(EdValueType))
                                {
                                    throw new ArgumentOutOfRangeException("opData3", "Operand.SetRawData IdxX: Invalid data type");
                                }
                                index += (EdValueType)OpData3;
                            }

                            EdValueType len;
                            byte[] sourceArray;
                            if (data is EdValueType)
                            {
                                len = dataLen;
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
                            if (requiredLength > _ediabas.ArrayMaxSize)
                            {
                                _ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
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
                if (OpData1.GetType() != typeof(Register))
                {
                    throw new ArgumentOutOfRangeException("opAddrMode", "Operand.SetArrayData: Invalid data type");
                }
                if (_opAddrMode != OpAddrMode.RegS)
                {
                    throw new ArgumentOutOfRangeException("opAddrMode", "Operand.SetArrayData: Invalid address mode");
                }
                Register arg1Data = (Register)OpData1;
                arg1Data.SetRawData(data);
            }

            public void SetStringData(string data)
            {
                byte[] dataArray = Encoding.GetBytes(data);
                int length = dataArray.Length;
                if (length > 0 && dataArray[length - 1] != 0)
                {
                    Array.Resize(ref dataArray, length + 1);
                    dataArray[length] = 0;
                }
                SetArrayData(dataArray);
            }

            private readonly EdiabasNet _ediabas;
            private OpAddrMode _opAddrMode;
            public OpAddrMode AddrMode
            {
                get
                {
                    return _opAddrMode;
                }
            }

            public Object OpData1;
            public Object OpData2;
            public Object OpData3;
        }

        public const int EdiabasVersion = 0x730;
        public const int TraceAppendDiffHours = 1;

        public enum ErrorCodes : uint
        {
            // ReSharper disable InconsistentNaming
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
            // ReSharper restore InconsistentNaming
        }

        private static readonly string[] ErrorDescription =
        {
            "IFH-0000: INTERNAL ERROR",
            "IFH-0001: UART ERROR",
            "IFH-0002: NO RESPONSE FROM INTERFACE",
            "IFH-0003: DATATRANSMISSION TO INTERFACE DISTURBED",
            "IFH-0004: ERROR IN INTERFACE COMMAND",
            "IFH-0005: INTERNAL INTERFACE ERROR",
            "IFH-0006: COMMAND NOT ACCEPTED",
            "IFH-0007: WRONG UBATT",
            "IFH-0008: CONTROLUNIT CONNECTION ERROR",
            "IFH-0009: NO RESPONSE FROM CONTROLUNIT",
            "IFH-0010: DATATRANSMISSION TO CONTROLUNIT DISTURBED",
            "IFH-0011: UNKNOWN INTERFACE",
            "IFH-0012: BUFFER OVERFLOW",
            "IFH-0013: COMMAND NOT IMPLEMENTED",
            "IFH-0014: CONCEPT NOT IMPLEMENTED",
            "IFH-0015: UBATT ON/OFF ERROR",
            "IFH-0016: IGNITION ON/OFF ERROR",
            "IFH-0017: INTERFACE DEADLOCK ERROR",
            "IFH-0018: INITIALIZATION ERROR",
            "IFH-0019: DEVICE ACCESS ERROR",
            "IFH-0020: DRIVER ERROR",
            "IFH-0021: ILLEGAL PORT",
            "IFH-0022: DRIVER STATUS ERROR",
            "IFH-0023: INTERFACE STATUS ERROR",
            "IFH-0024: CANCEL FAILED",
            "IFH-0025: INTERFACE APPLICATION ERROR",
            "IFH-0026: SIMULATION ERROR",
            "IFH-0027: IFH NOT FOUND",
            "IFH-0028: ILLEGAL IFH VERSION",
            "IFH-0029: ACCESS DENIED",
            "IFH-0030: TASK COMMUNICATION ERROR",
            "IFH-0031: DATA OVERFLOW",
            "IFH-0032: IGNITION IS OFF",
            "IFH-0033",
            "IFH-0034: CONFIGURATION FILE NOT FOUND",
            "IFH-0035: CONFIGURATION ERROR",
            "IFH-0036: LOAD ERROR",
            "IFH-0037: LOW UBATT",
            "IFH-0038: INTERFACE COMMAND NOT IMPLEMENTED",
            "IFH-0039: EDIC USER INTERFACE NOT FOUND",
            "IFH-0040: ILLEGAL EDIC USER INTERFACE VERSION",
            "IFH-0041: ILLEGAL PARAMETERS",
            "IFH-0042: CARD INSTALLATION ERROR",
            "IFH-0043: COMMUNICATION TRACE ERROR",
            "IFH-0044: FLASH ERROR",
            "IFH-0045: RUNBOARD ERROR",
            "IFH-0046: EDIC API ACCESS ERROR",
            "IFH-0047: PLUGIN ERROR",
            "IFH-0048: PLUGIN FUNCTION ERROR",
            "IFH-0049: CSS DEVICE DETECTION ERROR",
            "BIP-0000: INTERNAL ERROR",
            "BIP-0001: OUT OF RANGE",
            "BIP-0002: IFH FUNCTION ERROR",
            "BIP-0003: OBJECT FILE ERROR",
            "BIP-0004: ILLEGAL OPCODE",
            "BIP-0005: STACK OVERFLOW",
            "BIP-0006: BEST FILE ERROR",
            "BIP-0007: DIVISION BY ZERO",
            "BIP-0008: BEST BREAK",
            "BIP-0009: BEST VERSION ERROR",
            "BIP-0010: CONSTANT DATA ACCESS ERROR",
            "BIP-0011: REAL ERROR",
            "BIP-0012: PLUG IN NOT FOUND",
            "BIP-0013: PLUG IN ERROR",
            "BIP-0014: PLUG IN VERSION ERROR",
            "BIP-0015: PLUG IN STACK ERROR",
            "BIP-0016: PLUG IN FUNCTION NOT FOUND",
            "BIP-0017: IFH CHANNEL ERROR",
            "BIP-0018: SYSTEM ERROR",
            "BIP-0019",
            "BIP-0020",
            "BIP-0021",
            "BIP-0022",
            "BIP-0023",
            "BIP-0024",
            "BIP-0025",
            "BIP-0026",
            "BIP-0027",
            "BIP-0028",
            "BIP-0029",
            "SYS-0000: INTERNAL ERROR",
            "SYS-0001: ILLEGAL FUNCTION",
            "SYS-0002: ECU OBJECT FILE NOT FOUND",
            "SYS-0003: ECU OBJECT FILE ERROR",
            "SYS-0004: ILLEGAL FORMAT OF ECU OBJECTFILE",
            "SYS-0005: OBJECT FILE NOT FOUND",
            "SYS-0006: GROUP OBJECT FILE ERROR",
            "SYS-0007: ILLEGAL FORMAT OF GROUP OBJECT FILE",
            "SYS-0008: JOB NOT FOUND",
            "SYS-0009: NO INITIALIZATION JOB",
            "SYS-0010: INITIALIZATION ERROR",
            "SYS-0011: NO IDENTIFICATIONJOB",
            "SYS-0012: IDENTIFICATION ERROR",
            "SYS-0013: UNEXPECTED RESULT",
            "SYS-0014: ILLEGAL FORMAT",
            "SYS-0015: TASK COMMUNICATION ERROR",
            "SYS-0016: ILLEGAL CONFIGURATION",
            "SYS-0017",
            "SYS-0018: END JOB ERROR",
            "SYS-0019: TIMER ERROR",
            "SYS-0020: BASE OBJECT FILE NOT FOUND",
            "SYS-0021: BASE OBJECT FILE ERROR",
            "SYS-0022: ILLEGAL FORMAT OF BASE OBJECT FILE",
            "SYS-0023: PASSWORD ERROR",
            "SYS-0024: ILLEGAL PASSWORD",
            "SYS-0025",
            "SYS-0026",
            "SYS-0027",
            "SYS-0028",
            "SYS-0029",
            "API-0000: INTERNAL ERROR",
            "API-0001: USER BREAK",
            "API-0002: MEMORY ALLOCATION ERROR",
            "API-0003: RESULT SETS OVERFLOW",
            "API-0004: RESULTS OVERFLOW",
            "API-0005: ILLEGAL RESULT FORMAT",
            "API-0006: ACCESS DENIED",
            "API-0007: INCORRECT CONFIGURATION FILE",
            "API-0008: TASK COMMUNICATION ERROR",
            "API-0009: EDIABAS NOT FOUND",
            "API-0010: ILLEGAL EDIABAS VERSION",
            "API-0011: ILLEGAL ECU PATH",
            "API-0012: SIGNAL SERVER NOT FOUND",
            "API-0013: INITIALIZATION ERROR",
            "API-0014: RESULT NOT FOUND",
            "API-0015: HOST COMMUNICATION ERROR",
            "API-0016: RESULT OVERFLOW",
            "API-0017: ARGUMENT OVERFLOW",
            "API-0018",
            "API-0019",
            "API-0020",
            "API-0021",
            "API-0022",
            "API-0023",
            "API-0024",
            "API-0025",
            "API-0026",
            "API-0027",
            "API-0028",
            "API-0029",
            "NET-0000: INTERNAL ERROR",
            "NET-0001: UNKNOWN ERROR",
            "NET-0002: ILLEGAL VERSION",
            "NET-0003: INITIALIZATION ERROR",
            "NET-0004: ILLEGAL CALL",
            "NET-0005: NO SUPPORT",
            "NET-0006: ACCESS DENIED",
            "NET-0007: SYSTEM ERROR",
            "NET-0008: NETWORK ERROR",
            "NET-0009: TIMEOUT",
            "NET-0010: BUFFER OVERFLOW",
            "NET-0011: ALREADY CONNECTED",
            "NET-0012: NO CONNECTION",
            "NET-0013: CONNECTION DISTURBED",
            "NET-0014: CONNECTION ABORTED",
            "NET-0015: HOST NOT FOUND",
            "NET-0016: HOST ERROR",
            "NET-0017: PROTOCOL NOT AVAILABLE",
            "NET-0018: UNKNOWN PROTOCOL",
            "NET-0019: UNKNOWN SERVICE",
            "NET-0020: UNKNOWN HOST",
            "NET-0021: SERVER NOT FOUND",
            "NET-0022: SECURITY ERROR",
            "NET-0023",
            "NET-0024",
            "NET-0025",
            "NET-0026",
            "NET-0027",
            "NET-0028",
            "NET-0029",
            "NET-0030",
            "NET-0031",
            "NET-0032",
            "NET-0033",
            "NET-0034",
            "NET-0035",
            "NET-0036",
            "NET-0037",
            "NET-0038",
            "NET-0039",
            "NET-0040",
            "NET-0041",
            "NET-0042",
            "NET-0043",
            "NET-0044",
            "NET-0045",
            "NET-0046",
            "NET-0047",
            "NET-0048",
            "NET-0049",
            "IFH-0050: ENTRY IN DYNAMIC CONFIGURATION NOT FOUND",
            "IFH-0051: INTERNAL DYNAMIC PROTOCOL ERROR",
            "IFH-0052: CONCEPT NOT AVALIABLE",
            "IFH-0053: ILLEGAL CONCEPT ID",
            "IFH-0054: ILLEGAL FUNCTION PARAMETER",
            "IFH-0055: CANNOT LOAD PROTOCOL TABLES",
            "IFH-0056: ILLEGAL CHANNEL",
            "IFH-0057: ERROR READ DIGITAL INPUTS",
            "IFH-0058: ERROR SET DIGITAL OUTPUTS",
            "IFH-0059: ERROR READ ANALOG INPUTS",
            "IFH-0060: RESUME AFTER SUSPEND STATE",
            "IFH-0061: INVALID ECU PARAMETERS FORMAT",
            "IFH-0062: BAD ECU PARAMETERS BUFFER",
            "IFH-0063: INVALID BUS CONFIGURATION",
            "IFH-0064: INVALID CONNECTION SETTINGS",
            "IFH-0065: FIRMWARE UPDATE ERROR",
            "IFH-0066: CHANNEL ERROR",
            "IFH-0067: ECU RESPONSE PENDING",
            "IFH-0068: TESTER ADDRESS ERROR",
            "IFH-0069: GATEWAY ERROR",
            "IFH-0070: SYSTEM ERROR",
            "IFH-0071: TELEGRAM FORMAT ERROR",
            "IFH-0072: ECU ACCESS COLLISION",
            "IFH-0073: PROXY ERROR",
            "IFH-0074",
            "IFH-0075",
            "IFH-0076",
            "IFH-0077",
            "IFH-0078",
            "IFH-0079",
            "IFH-0080",
            "IFH-0081",
            "IFH-0082",
            "IFH-0083",
            "IFH-0084",
            "IFH-0085",
            "IFH-0086",
            "IFH-0087",
            "IFH-0088",
            "IFH-0089",
            "IFH-0090",
            "IFH-0091",
            "IFH-0092",
            "IFH-0093",
            "IFH-0094",
            "IFH-0095",
            "IFH-0096",
            "IFH-0097",
            "IFH-0098",
            "IFH-0099",
            "RUN-0000",
            "RUN-0001",
            "RUN-0002",
            "RUN-0003",
            "RUN-0004",
            "RUN-0005",
            "RUN-0006",
            "RUN-0007",
            "RUN-0008",
            "RUN-0009",
            "RUN-0010",
            "RUN-0011",
            "RUN-0012",
            "RUN-0013",
            "RUN-0014",
            "RUN-0015",
            "RUN-0016",
            "RUN-0017",
            "RUN-0018",
            "RUN-0019",
            "RUN-0020",
            "RUN-0021",
            "RUN-0022",
            "RUN-0023",
            "RUN-0024",
            "RUN-0025",
            "RUN-0026",
            "RUN-0027",
            "RUN-0028",
            "RUN-0029",
            "RUN-0030",
            "RUN-0031",
            "RUN-0032",
            "RUN-0033",
            "RUN-0034",
            "RUN-0035",
            "RUN-0036",
            "RUN-0037",
            "RUN-0038",
            "RUN-0039",
            "RUN-0040",
            "RUN-0041",
            "RUN-0042",
            "RUN-0043",
            "RUN-0044",
            "RUN-0045",
            "RUN-0046",
            "RUN-0047",
            "RUN-0048",
            "RUN-0049",
            "RUN-0050",
            "RUN-0051",
            "RUN-0052",
            "RUN-0053",
            "RUN-0054",
            "RUN-0055",
            "RUN-0056",
            "RUN-0057",
            "RUN-0058",
            "RUN-0059",
            "RUN-0060",
            "RUN-0061",
            "RUN-0062",
            "RUN-0063",
            "RUN-0064",
            "RUN-0065",
            "RUN-0066",
            "RUN-0067",
            "RUN-0068",
            "RUN-0069",
            "RUN-0070",
            "RUN-0071",
            "RUN-0072",
            "RUN-0073",
            "RUN-0074",
            "RUN-0075",
            "RUN-0076",
            "RUN-0077",
            "RUN-0078",
            "RUN-0079",
            "RUN-0080",
            "RUN-0081",
            "RUN-0082",
            "RUN-0083",
            "RUN-0084",
            "RUN-0085",
            "RUN-0086",
            "RUN-0087",
            "RUN-0088",
            "RUN-0089",
            "RUN-0090",
            "RUN-0091",
            "RUN-0092",
            "RUN-0093",
            "RUN-0094",
            "RUN-0095",
            "RUN-0096",
            "RUN-0097",
            "RUN-0098",
            "RUN-0099",
        };

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
        }

        private enum RegisterType : byte
        {
            RegAb,  // 8 bit
            RegI,   // 16 bit
            RegL,   // 32 bit
            RegF,   // float
            RegS,   // string
        }

        private enum OpAddrMode : byte
        {
            None = 0,
            RegS = 1,
            RegAb = 2,
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
            public StringData(EdiabasNet ediabas, EdValueType length)
            {
                _ediabas = ediabas;
                _length = 0;
                _data = new byte[length];
            }

            public void NewArrayLength(EdValueType length)
            {
                if (length > _data.Length)
                {
                    _data = new byte[length];
                }
            }

            // ReSharper disable once UnusedMember.Local
            public byte[] GetData()
            {
                return GetData(false);
            }

            public byte[] GetData(bool complete)
            {
                if (complete)
                {
                    return _data;
                }
                byte[] result = new byte[_length];
                Array.Copy(_data, 0, result, 0, (int)_length);
                return result;
            }

            public void SetData(byte[] value, bool keepLength)
            {
                if (value.Length > _data.Length)
                {
                    _ediabas.SetError(ErrorCodes.EDIABAS_BIP_0001);
                    return;
                }
                Array.Copy(value, 0, _data, 0, value.Length);
                if (!keepLength)
                {
                    _length = (EdValueType)value.Length;
                }
            }

            public void ClearData()
            {
                Array.Clear(_data, 0, _data.Length);
                _length = 0;
            }

            private EdValueType _length;
            private readonly EdiabasNet _ediabas;
            private byte[] _data;
        }

        private class Register
        {
            public Register(byte opcode, RegisterType type, uint index)
            {
                Opcode = opcode;
                _regType = type;
                _index = index;
                _ediabas = null;
            }

            public void SetEdiabas(EdiabasNet ediabas)
            {
                _ediabas = ediabas;
            }

            public string GetName()
            {
                switch (_regType)
                {
                    case RegisterType.RegAb:   // 8 bit
                        if (_index > 15)
                        {
                            return "A" + string.Format(Culture, "{0:X}", _index - 16);
                        }
                        return "B" + string.Format(Culture, "{0:X}", _index);

                    case RegisterType.RegI:   // 16 bit
                        return "I" + string.Format(Culture, "{0:X}", _index);

                    case RegisterType.RegL:   // 32 bit
                        return "L" + string.Format(Culture, "{0:X}", _index);

                    case RegisterType.RegF:
                        return "F" + string.Format(Culture, "{0:X}", _index);

                    case RegisterType.RegS:
                        return "S" + string.Format(Culture, "{0:X}", _index);
                }
                return string.Empty;
            }

            public Type GetDataType()
            {
                switch (_regType)
                {
                    case RegisterType.RegF:
                        return typeof(EdFloatType);

                    case RegisterType.RegS:
                        return typeof(byte[]);
                }
                return typeof(EdValueType);
            }

            // ReSharper disable once UnusedMember.Local
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

            // ReSharper disable once MemberCanBePrivate.Local
            public EdValueType GetDataLen()
            {
                switch (_regType)
                {
                    case RegisterType.RegAb:   // 8 bit
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
                switch (_regType)
                {
                    case RegisterType.RegAb:   // 8 bit
                        value = _ediabas._byteRegisters[_index + 0];
                        break;

                    case RegisterType.RegI:   // 16 bit
                        {
                            EdValueType offset = _index << 1;
                            value = _ediabas._byteRegisters[offset + 0] +
                                ((EdValueType)_ediabas._byteRegisters[offset + 1] << 8);
                            break;
                        }

                    case RegisterType.RegL:   // 32 bit
                        {
                            EdValueType offset = _index << 2;
                            value = _ediabas._byteRegisters[offset + 0] +
                                ((EdValueType)_ediabas._byteRegisters[offset + 1] << 8) +
                                ((EdValueType)_ediabas._byteRegisters[offset + 2] << 16) +
                                ((EdValueType)_ediabas._byteRegisters[offset + 3] << 24);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException("type", "Register.GetValueData: Invalid data type");
                }
                return value;
            }

            public object GetRawData()
            {
                switch (_regType)
                {
                    case RegisterType.RegAb:   // 8 bit
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

            // ReSharper disable once MemberCanBePrivate.Local
            public EdFloatType GetFloatData()
            {
                if (_regType != RegisterType.RegF)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.GetFloatData: Invalid data type");
                }
                if (GetDataLen() != sizeof(EdFloatType))
                {
                    throw new ArgumentOutOfRangeException("GetDataLen", "Register.GetFloatData: Invalid data length");
                }
                return _ediabas._floatRegisters[_index];
            }

            public byte[] GetArrayData()
            {
                return GetArrayData(false);
            }

            public byte[] GetArrayData(bool complete)
            {
                if (_regType != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.GetArrayData: Invalid data type");
                }
                return _ediabas._stringRegisters[_index].GetData(complete);
            }

            public void SetRawData(object value)
            {
                if (_regType == RegisterType.RegS)
                {
                    if (value.GetType() != typeof(byte[]))
                    {
                        throw new ArgumentOutOfRangeException("value", "Register.SetRawData: Invalid type");
                    }
                    SetArrayData((byte[])value);
                }
                else if (_regType == RegisterType.RegF)
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
                switch (_regType)
                {
                    case RegisterType.RegAb:   // 8 bit
                        _ediabas._byteRegisters[_index] = (byte) value;
                        break;

                    case RegisterType.RegI:   // 16 bit
                        {
                            EdValueType offset = _index << 1;
                            _ediabas._byteRegisters[offset + 0] = (byte)value;
                            _ediabas._byteRegisters[offset + 1] = (byte)(value >> 8);
                            break;
                        }

                    case RegisterType.RegL:   // 32 bit
                        {
                            EdValueType offset = _index << 2;
                            _ediabas._byteRegisters[offset + 0] = (byte)value;
                            _ediabas._byteRegisters[offset + 1] = (byte)(value >> 8);
                            _ediabas._byteRegisters[offset + 2] = (byte)(value >> 16);
                            _ediabas._byteRegisters[offset + 3] = (byte)(value >> 24);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException("type", "Register.SetValueData: Invalid data type");
                }
            }

            public void SetFloatData(EdFloatType value)
            {
                if (_regType != RegisterType.RegF)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetFloatData: Invalid data type");
                }
                _ediabas._floatRegisters[_index] = value;
            }

            public void SetArrayData(byte[] value)
            {
                SetArrayData(value, false);
            }

            public void SetArrayData(byte[] value, bool keepLength)
            {
                if (_regType != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetArrayData: Invalid data type");
                }
                _ediabas._stringRegisters[_index].SetData(value, keepLength);
            }

            public void ClearData()
            {
                if (_regType != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetArrayData: Invalid data type");
                }
                _ediabas._stringRegisters[_index].ClearData();
            }

            public byte Opcode
            {
                get;
                private set;
            }

            private readonly RegisterType _regType;
            private readonly uint _index;
            private EdiabasNet _ediabas;
        }

        private static readonly Register[] RegisterList =
        {
            new Register(0x00, RegisterType.RegAb, 0),
            new Register(0x01, RegisterType.RegAb, 1),
            new Register(0x02, RegisterType.RegAb, 2),
            new Register(0x03, RegisterType.RegAb, 3),
            new Register(0x04, RegisterType.RegAb, 4),
            new Register(0x05, RegisterType.RegAb, 5),
            new Register(0x06, RegisterType.RegAb, 6),
            new Register(0x07, RegisterType.RegAb, 7),
            new Register(0x08, RegisterType.RegAb, 8),
            new Register(0x09, RegisterType.RegAb, 9),
            new Register(0x0A, RegisterType.RegAb, 10),
            new Register(0x0B, RegisterType.RegAb, 11),
            new Register(0x0C, RegisterType.RegAb, 12),
            new Register(0x0D, RegisterType.RegAb, 13),
            new Register(0x0E, RegisterType.RegAb, 14),
            new Register(0x0F, RegisterType.RegAb, 15),
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
            new Register(0x80, RegisterType.RegAb, 16),
            new Register(0x81, RegisterType.RegAb, 17),
            new Register(0x82, RegisterType.RegAb, 18),
            new Register(0x83, RegisterType.RegAb, 19),
            new Register(0x84, RegisterType.RegAb, 20),
            new Register(0x85, RegisterType.RegAb, 21),
            new Register(0x86, RegisterType.RegAb, 22),
            new Register(0x87, RegisterType.RegAb, 23),
            new Register(0x88, RegisterType.RegAb, 24),
            new Register(0x89, RegisterType.RegAb, 25),
            new Register(0x8A, RegisterType.RegAb, 26),
            new Register(0x8B, RegisterType.RegAb, 27),
            new Register(0x8C, RegisterType.RegAb, 28),
            new Register(0x8D, RegisterType.RegAb, 29),
            new Register(0x8E, RegisterType.RegAb, 30),
            new Register(0x8F, RegisterType.RegAb, 31),
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

        private static readonly OpCode[] OcList =
        {
            new OpCode(0x00, "move", OpMove),
            new OpCode(0x01, "clear", OpClear),
            new OpCode(0x02, "comp", OpComp),
            new OpCode(0x03, "subb", OpSubb),
            new OpCode(0x04, "adds", OpAdds),
            new OpCode(0x05, "mult", OpMult),
            new OpCode(0x06, "divs", OpDivs),
            new OpCode(0x07, "and", OpAnd),
            new OpCode(0x08, "or", OpOr),
            new OpCode(0x09, "xor", OpXor),
            new OpCode(0x0A, "not", OpNot),
            new OpCode(0x0B, "jump", OpJump, true),
            new OpCode(0x0C, "jtsr", null, true),
            new OpCode(0x0D, "ret", null),
            new OpCode(0x0E, "jc", OpJc, true),      // identical to jb
            new OpCode(0x0F, "jae", OpJae, true),    // identical to jnc
            new OpCode(0x10, "jz", OpJz, true),
            new OpCode(0x11, "jnz", OpJnz, true),
            new OpCode(0x12, "jv", OpJv, true),
            new OpCode(0x13, "jnv", OpJnv, true),
            new OpCode(0x14, "jmi", OpJmi, true),
            new OpCode(0x15, "jpl", OpJpl, true),
            new OpCode(0x16, "clrc", OpClrc),
            new OpCode(0x17, "setc", OpSetc),
            new OpCode(0x18, "asr", OpAsr),
            new OpCode(0x19, "lsl", OpLsl),
            new OpCode(0x1A, "lsr", OpLsr),
            new OpCode(0x1B, "asl", OpAsl),
            new OpCode(0x1C, "nop", OpNop),
            new OpCode(0x1D, "eoj", OpEoj),
            new OpCode(0x1E, "push", OpPush),
            new OpCode(0x1F, "pop", OpPop),
            new OpCode(0x20, "scmp", OpScmp),
            new OpCode(0x21, "scat", OpScat),
            new OpCode(0x22, "scut", OpScut),
            new OpCode(0x23, "slen", OpSlen),
            new OpCode(0x24, "spaste", OpSpaste),
            new OpCode(0x25, "serase", OpSerase),
            new OpCode(0x26, "xconnect", OpXconnect),
            new OpCode(0x27, "xhangup", OpXhangup),
            new OpCode(0x28, "xsetpar", OpXsetpar),
            new OpCode(0x29, "xawlen", OpXawlen),
            new OpCode(0x2A, "xsend", OpXsend),
            new OpCode(0x2B, "xsendf", OpXsendf),
            new OpCode(0x2C, "xrequf", OpXrequf),
            new OpCode(0x2D, "xstopf", OpXstopf),
            new OpCode(0x2E, "xkeyb", OpXkeyb),
            new OpCode(0x2F, "xstate", OpXstate),
            new OpCode(0x30, "xboot", OpXboot),
            new OpCode(0x31, "xreset", OpXreset),
            new OpCode(0x32, "xtype", OpXtype),
            new OpCode(0x33, "xvers", OpXvers),
            new OpCode(0x34, "ergb", OpErgb),
            new OpCode(0x35, "ergw", OpErgw),
            new OpCode(0x36, "ergd", OpErgd),
            new OpCode(0x37, "ergi", OpErgi),
            new OpCode(0x38, "ergr", OpErgr),
            new OpCode(0x39, "ergs", OpErgs),
            new OpCode(0x3A, "a2flt", OpA2Flt),
            new OpCode(0x3B, "fadd", OpFadd),
            new OpCode(0x3C, "fsub", OpFsub),
            new OpCode(0x3D, "fmul", OpFmul),
            new OpCode(0x3E, "fdiv", OpFdiv),
            new OpCode(0x3F, "ergy", OpErgy),
            new OpCode(0x40, "enewset", OpEnewset),
            new OpCode(0x41, "etag", OpEtag, true),
            new OpCode(0x42, "xreps", OpXreps),
            new OpCode(0x43, "gettmr", OpGettmr),
            new OpCode(0x44, "settmr", OpSettmr),
            new OpCode(0x45, "sett", OpSett),
            new OpCode(0x46, "clrt", OpClrt),
            new OpCode(0x47, "jt", OpJt, true),
            new OpCode(0x48, "jnt", OpJnt, true),
            new OpCode(0x49, "addc", OpAddc),
            new OpCode(0x4A, "subc", OpSubc),
            new OpCode(0x4B, "break", OpBreak),
            new OpCode(0x4C, "clrv", OpClrv),
            new OpCode(0x4D, "eerr", OpEerr),
            new OpCode(0x4E, "popf", OpPopf),
            new OpCode(0x4F, "pushf", OpPushf),
            new OpCode(0x50, "atsp", OpAtsp),
            new OpCode(0x51, "swap", OpSwap),
            new OpCode(0x52, "setspc", OpSetspc),
            new OpCode(0x53, "srevrs", OpSrevrs),
            new OpCode(0x54, "stoken", OpStoken),
            new OpCode(0x55, "parb", OpParl),
            new OpCode(0x56, "parw", OpParl),
            new OpCode(0x57, "parl", OpParl),
            new OpCode(0x58, "pars", OpPars),
            new OpCode(0x59, "fclose", OpFclose),
            new OpCode(0x5A, "jg", OpJg, true),
            new OpCode(0x5B, "jge", OpJge, true),
            new OpCode(0x5C, "jl", OpJl, true),
            new OpCode(0x5D, "jle", OpJle, true),
            new OpCode(0x5E, "ja", OpJa, true),
            new OpCode(0x5F, "jbe", OpJbe, true),
            new OpCode(0x60, "fopen", OpFopen),
            new OpCode(0x61, "fread", OpFread),
            new OpCode(0x62, "freadln", OpFreadln),
            new OpCode(0x63, "fseek", OpFseek),
            new OpCode(0x64, "fseekln", OpFseekln),
            new OpCode(0x65, "ftell", OpFtell),
            new OpCode(0x66, "ftellln", OpFtellln),
            new OpCode(0x67, "a2fix", OpA2Fix),
            new OpCode(0x68, "fix2flt", OpFix2Flt),
            new OpCode(0x69, "parr", OpParr),
            new OpCode(0x6A, "test", OpTest),
            new OpCode(0x6B, "wait", OpWait),
            new OpCode(0x6C, "date", OpDate),
            new OpCode(0x6D, "time", OpTime),
            new OpCode(0x6E, "xbatt", OpXbatt),
            new OpCode(0x6F, "tosp", null),
            new OpCode(0x70, "xdownl", null),
            new OpCode(0x71, "xgetport", OpXgetport),
            new OpCode(0x72, "xignit", OpXignit),
            new OpCode(0x73, "xloopt", null),
            new OpCode(0x74, "xprog", null),
            new OpCode(0x75, "xraw", OpXraw),
            new OpCode(0x76, "xsetport", null),
            new OpCode(0x77, "xsireset", null),
            new OpCode(0x78, "xstoptr", null),
            new OpCode(0x79, "fix2hex", OpFix2Hex),
            new OpCode(0x7A, "fix2dez", OpFix2Dez),
            new OpCode(0x7B, "tabset", OpTabset),
            new OpCode(0x7C, "tabseek", OpTabseek),
            new OpCode(0x7D, "tabget", OpTabget),
            new OpCode(0x7E, "strcat", OpStrcat),
            new OpCode(0x7F, "pary", OpPary),
            new OpCode(0x80, "parn", OpParn),
            new OpCode(0x81, "ergc", OpErgc),
            new OpCode(0x82, "ergl", OpErgl),
            new OpCode(0x83, "tabline", OpTabline),
            new OpCode(0x84, "xsendr", null),
            new OpCode(0x85, "xrecv", null),
            new OpCode(0x86, "xinfo", null),
            new OpCode(0x87, "flt2a", OpFlt2A),
            new OpCode(0x88, "setflt", OpSetflt),
            new OpCode(0x89, "cfgig", OpCfgig),
            new OpCode(0x8A, "cfgsg", OpCfgsg),
            new OpCode(0x8B, "cfgis", OpCfgis),
            new OpCode(0x8C, "a2y", OpA2Y),
            new OpCode(0x8D, "xparraw", null),
            new OpCode(0x8E, "hex2y", OpHex2Y),
            new OpCode(0x8F, "strcmp", OpStrcmp),
            new OpCode(0x90, "strlen", OpStrlen),
            new OpCode(0x91, "y2bcd", OpY2Bcd),
            new OpCode(0x92, "y2hex", OpY2Hex),
            new OpCode(0x93, "shmset", OpShmset),
            new OpCode(0x94, "shmget", OpShmget),
            new OpCode(0x95, "ergsysi", OpErgsysi),
            new OpCode(0x96, "flt2fix", OpFlt2Fix),
            new OpCode(0x97, "iupdate", OpIupdate),
            new OpCode(0x98, "irange", OpIrange),
            new OpCode(0x99, "iincpos", OpIincpos),
            new OpCode(0x9A, "tabseeku", OpTabseeku),
            new OpCode(0x9B, "flt2y4", OpFlt2Y4),
            new OpCode(0x9C, "flt2y8", OpFlt2Y8),
            new OpCode(0x9D, "y42flt", OpY42Flt),
            new OpCode(0x9E, "y82flt", OpY82Flt),
            new OpCode(0x9F, "plink", OpPlink),
            new OpCode(0xA0, "pcall", null),
            new OpCode(0xA1, "fcomp", OpFcomp),
            new OpCode(0xA2, "plinkv", OpPlinkv),
            new OpCode(0xA3, "ppush", OpPpush),
            new OpCode(0xA4, "ppop", OpPpop),
            new OpCode(0xA5, "ppushflt", null),
            new OpCode(0xA6, "ppopflt", null),
            new OpCode(0xA7, "ppushy", OpPpushy),
            new OpCode(0xA8, "ppopy", OpPpopy),
            new OpCode(0xA9, "pjtsr", OpPjtsr),
            new OpCode(0xAA, "tabsetex", OpTabsetex),
            new OpCode(0xAB, "ufix2dez", OpUfix2Dez),
            new OpCode(0xAC, "generr", OpGenerr),
            new OpCode(0xAD, "ticks", OpTicks),
            new OpCode(0xAE, "waitex", OpWaitex),
            new OpCode(0xAF, "xopen", null),
            new OpCode(0xB0, "xclose", null),
            new OpCode(0xB1, "xcloseex", null),
            new OpCode(0xB2, "xswitch", null),
            new OpCode(0xB3, "xsendex", null),
            new OpCode(0xB4, "xrecvex", null),
            new OpCode(0xB5, "ssize", OpSsize),
            new OpCode(0xB6, "tabcols", OpTabcols),
            new OpCode(0xB7, "tabrows", OpTabrows),
        };

        private static readonly VJobInfo[] VJobList =
        {
            new VJobInfo("_JOBS", VJobJobs),
            new VJobInfo("_JOBCOMMENTS", VJobJobComments),
            new VJobInfo("_ARGUMENTS", VJobJobArgs),
            new VJobInfo("_RESULTS", VJobJobResults),
            new VJobInfo("_VERSIONINFO", VJobVerinfos),
            new VJobInfo("_TABLES", VJobTables),
            new VJobInfo("_TABLE", VJobTable),
        };

        public class ResultData
        {
            public ResultData(ResultType type, string name, object opData)
            {
                ResType = type;
                Name = name;
                OpData = opData;
            }
            public ResultType ResType { get; private set; }
            public string Name { get; private set; }
            public object OpData { get; private set; }
        }

        private class Flags
        {
            public Flags()
            {
                Init();
            }
            public void Init()
            {
                Carry = false;
                Zero = false;
                Sign = false;
                Overflow = false;
            }

            public void UpdateFlags(EdValueType value, EdValueType length)
            {
                EdValueType valueMask;
                EdValueType signMask;

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
                Zero = (value & valueMask) == 0;
                Sign = (value & signMask) != 0;
            }

            public void SetOverflow(UInt32 value1, UInt32 value2, UInt32 result, EdValueType length)
            {
                UInt64 signMask;

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
                    Overflow = false;
                }
                else if ((value1 & signMask) == (result & signMask))
                {
                    Overflow = false;
                }
                else
                {
                    Overflow = true;
                }
            }

            public void SetCarry(UInt64 value, EdValueType length)
            {
                UInt64 carryMask;

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
                Carry = (value & carryMask) != 0;
            }

            public EdValueType ToValue()
            {
                EdValueType value = 0;
                value |= (EdValueType)(Carry ? 0x01 : 0x00);
                value |= (EdValueType)(Zero ? 0x02 : 0x00);
                value |= (EdValueType)(Sign ? 0x04 : 0x00);
                value |= (EdValueType)(Overflow ? 0x08 : 0x00);

                return value;
            }

            public void FromValue(EdValueType value)
            {
                Carry = (value & 0x01) != 0;
                Zero = (value & 0x02) != 0;
                Sign = (value & 0x04) != 0;
                Overflow = (value & 0x08) != 0;
            }

            public bool Carry;
            public bool Zero;
            public bool Sign;
            public bool Overflow;
        }

        private class ArgInfo
        {
            public ArgInfo()
            {
                _binData = null;
                StringList = null;
            }

            public byte[] BinData
            {
                get
                {
                    return _binData;
                }
                set
                {
                    _binData = value;
                    StringList = null;
                }
            }

            public List<string> StringList { get; set; }

            private byte[] _binData;
        }

        private class VJobInfo
        {
            public VJobInfo(string jobName, VJobDelegate jobDelegate)
            {
                JobName = jobName;
                JobDelegate = jobDelegate;
            }

            public string JobName { get; private set; }
            public VJobDelegate JobDelegate { get; private set; }
        }

        private class JobInfo
        {
            public JobInfo(string jobName, UInt32 jobOffset, UInt32 jobSize, UInt32 arraySize, UsesInfo usesInfo)
            {
                JobName = jobName;
                JobOffset = jobOffset;
                JobSize = jobSize;
                ArraySize = arraySize;
                UsesInfo = usesInfo;
            }

            public string JobName { get; private set; }
            public UInt32 JobOffset { get; private set; }
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public UInt32 JobSize { get; private set; }
            public UInt32 ArraySize { get; private set; }
            public UsesInfo UsesInfo { get; private set; }
        }

        private class JobInfos
        {
            public JobInfo[] JobInfoArray { get; set; }
            public Dictionary<string, UInt32> JobNameDict { get; set; }
        }

        private class UsesInfo
        {
            public UsesInfo(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        private class UsesInfos
        {
            public UsesInfo[] UsesInfoArray { get; set; }
        }

        private class DescriptionInfos
        {
            public List<string> GlobalComments { get; set; }
            public Dictionary<string, List<string>> JobComments { get; set; }
        }

        private class TableInfo
        {
            public TableInfo(string name, UInt32 tableOffset, UInt32 tableColumnOffset, EdValueType columns, EdValueType rows)
            {
                Name = name;
                TableOffset = tableOffset;
                TableColumnOffset = tableColumnOffset;
                Columns = columns;
                Rows = rows;
            }

            public string Name { get; private set; }
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public UInt32 TableOffset { get; private set; }
            public UInt32 TableColumnOffset { get; private set; }
            public EdValueType Columns { get; private set; }
            public EdValueType Rows { get; private set; }
            public Dictionary<string, UInt32> ColumnNameDict { get; set; }
            public Dictionary<string, UInt32>[] SeekColumnStringDicts { get; set; }
            public Dictionary<EdValueType, UInt32>[] SeekColumnValueDicts { get; set; }
            public EdValueType[][] TableEntries { get; set; }
        }

        private class TableInfos
        {
            public TableInfo[] TableInfoArray { get; set; }
            public Dictionary<string, UInt32> TableNameDict { get; set; }
        }

        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        private static readonly byte[] ByteArray0 = new byte[0];
        private static Dictionary<ErrorCodes, UInt32> _trapBitDict;
        private static bool _firstLog = true;
        private static readonly object SharedDataLock = new object();
        private static readonly Dictionary<string, byte[]> SharedDataDict = new Dictionary<string, byte[]>();
        private static int _instanceCount;

        private const string JobNameInit = "INITIALISIERUNG";
        private const string JobNameExit = "ENDE";
        private const string JobNameIdent = "IDENTIFIKATION";

        public enum EdLogLevel
        {
            Off = 0,
            Ifh = 1,
            Error = 2,
            Info = 3,
        };

        private bool _disposed;
        private readonly object _apiLock = new object();
        private bool _jobRunning;
        private bool _jobStd;
        private bool _jobStdExit;
        private bool _closeSgbdFs;
        private readonly Stack<byte> _stackList = new Stack<byte>();
        private readonly ArgInfo _argInfo = new ArgInfo();
        private readonly ArgInfo _argInfoStd = new ArgInfo();
        private readonly Dictionary<string, ResultData> _resultDict = new Dictionary<string, ResultData>();
        private readonly Dictionary<string, ResultData> _resultSysDict = new Dictionary<string, ResultData>();
        private readonly Dictionary<string, bool> _resultsRequestDict = new Dictionary<string, bool>();
        private List<Dictionary<string, ResultData>> _resultSets;
        private List<Dictionary<string, ResultData>> _resultSetsTemp;
        private readonly Dictionary<string, string> _configDict = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _groupMappingDict = new Dictionary<string, string>();
        private long _infoProgressRange;
        private long _infoProgressPos;
        private string _infoProgressText = string.Empty;
        private string _resultJobStatus = string.Empty;
        private string _sgbdFileName = string.Empty;
        private string _sgbdFileResolveLast = string.Empty;
        private string _ecuPath = string.Empty;
        private readonly string _iniFileName = string.Empty;
        private readonly string _ecuPathDefault;
        private EdInterfaceBase _edInterfaceClass;
        private static long _timeMeas;
        private readonly byte[] _opArgBuffer = new byte[5];
        private AbortJobDelegate _abortJobFunc;
        private ProgressJobDelegate _progressJobFunc;
        private ErrorRaisedDelegate _errorRaisedFunc;
        private StreamWriter _swLog;
#if COMPRESS_TRACE
        private ICSharpCode.SharpZipLib.Zip.ZipOutputStream _zipStream;
#endif
        private readonly object _logLock = new object();
        private int _logLevelCached = -1;
        private readonly bool _lockTrace;
        private EdValueType _arrayMaxBufSize = 1024;
        private EdValueType _errorTrapMask;
        private int _errorTrapBitNr = -1;
        private ErrorCodes _errorCodeLast = ErrorCodes.EDIABAS_ERR_NONE;
        private readonly byte[] _byteRegisters;
        private readonly EdFloatType[] _floatRegisters;
        private readonly StringData[] _stringRegisters;
        private readonly Flags _flags = new Flags();
        private Stream _sgbdFs;
        private Stream _sgbdBaseFs;
        private EdValueType _pcCounter;
        private JobInfos _jobInfos;
        private UsesInfos _usesInfos;
        private DescriptionInfos _descriptionInfos;
        private TableInfos _tableInfos;
        private TableInfos _tableInfosExt;
        private Stream _tableFs;
        private Int32 _tableIndex = -1;
        private Int32 _tableRowIndex = -1;
        private readonly Stream[] _userFilesArray = new Stream[MaxFiles];
        private readonly byte[] _tableItemBuffer = new byte[1024];
        private string _tokenSeparator = string.Empty;
        private EdValueType _tokenIndex;
        private EdValueType _floatPrecision = 4;
        private bool _jobEnd;
        private bool _requestInit = true;

        public bool JobRunning
        {
            get
            {
                lock (_apiLock)
                {
                    return _jobRunning;
                }
            }
            private set
            {
                lock (_apiLock)
                {
                    _jobRunning = value;
                }
            }
        }

        public string ArgString
        {
            get
            {
                lock (_apiLock)
                {
                    if (_argInfo.BinData == null)
                    {
                        return string.Empty;
                    }
                    return Encoding.GetString(_argInfo.BinData, 0, _argInfo.BinData.Length);
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ArgString: Job is running");
                }
                if (!string.IsNullOrEmpty(value))
                {
                    LogFormat(EdLogLevel.Ifh, "ArgString: {0}", value);
                }
                lock (_apiLock)
                {
                    _argInfo.BinData = string.IsNullOrEmpty(value) ? null : Encoding.GetBytes(value);
                }
            }
        }

        public byte[] ArgBinary
        {
            get
            {
                lock (_apiLock)
                {
                    if (_argInfo.BinData == null)
                    {
                        return ByteArray0;
                    }
                    return _argInfo.BinData;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ArgBinary: Job is running");
                }
                if (value != null && value.Length > 0)
                {
                    LogData(EdLogLevel.Ifh, value, 0, value.Length, "ArgBinary");
                }
                lock (_apiLock)
                {
                    _argInfo.BinData = value;
                }
            }
        }

        public string ArgStringStd
        {
            get
            {
                lock (_apiLock)
                {
                    if (_argInfoStd.BinData == null)
                    {
                        return string.Empty;
                    }
                    return Encoding.GetString(_argInfoStd.BinData, 0, _argInfoStd.BinData.Length);
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ArgStringStd: Job is running");
                }
                if (!string.IsNullOrEmpty(value))
                {
                    LogFormat(EdLogLevel.Ifh, "ArgStringStd: {0}", value);
                }
                lock (_apiLock)
                {
                    _argInfoStd.BinData = string.IsNullOrEmpty(value) ? null : Encoding.GetBytes(value);
                }
            }
        }

        public byte[] ArgBinaryStd
        {
            get
            {
                lock (_apiLock)
                {
                    if (_argInfoStd.BinData == null)
                    {
                        return ByteArray0;
                    }
                    return _argInfoStd.BinData;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ArgBinaryStd: Job is running");
                }
                if (value != null && value.Length > 0)
                {
                    LogData(EdLogLevel.Ifh, value, 0, value.Length, "ArgBinaryStd");
                }
                lock (_apiLock)
                {
                    _argInfoStd.BinData = value;
                }
            }
        }

        public bool NoInitForVJobs { get; set; }

        private byte[] GetActiveArgBinary()
        {
            if (_jobStd)
            {
                return ArgBinaryStd;
            }
            return ArgBinary;
        }

        private List<string> GetActiveArgStrings()
        {
            string args = _jobStd ? ArgStringStd : ArgString;
            ArgInfo argInfo = _jobStd ? _argInfoStd : _argInfo;

            if (argInfo.StringList == null)
            {
                argInfo.StringList = new List<string>();
                if (args.Length > 0)
                {
                    string[] words = args.Split(';');
                    foreach (string word in words)
                    {
                        argInfo.StringList.Add(word);
                    }
                }
            }
            return argInfo.StringList;
        }

        public List<Dictionary<string, ResultData>> ResultSets
        {
            get
            {
                lock (_apiLock)
                {
                    return _resultSets;
                }
            }
        }

        public Dictionary<string, bool> ResultsRequestDict
        {
            get
            {
                lock (_apiLock)
                {
                    return _resultsRequestDict;
                }
            }
        }

        public string ResultsRequests
        {
            get
            {
                string result = string.Empty;
                lock (_apiLock)
                {
                    foreach (string arg in _resultsRequestDict.Keys)
                    {
                        if (result.Length > 0)
                        {
                            result += ";";
                        }
                        result += arg;
                    }
                }
                return result;
            }
            set
            {
                lock (_apiLock)
                {
                    _resultsRequestDict.Clear();
                    if (value.Length > 0)
                    {
                        string[] words = value.Split(';');
                        foreach (string word in words)
                        {
                            if (word.Length > 0)
                            {
                                _resultsRequestDict.Add(word.ToUpper(Culture), true);
                            }
                        }
                    }
                }
            }
        }

        public int InfoProgressPercent
        {
            get
            {
                lock (_apiLock)
                {
                    if ((_infoProgressPos < 0) || (_infoProgressRange <= 0))
                    {
                        return -1;
                    }
                    return (int)(_infoProgressPos * 100 / _infoProgressRange);
                }
            }
        }

        public string InfoProgressText
        {
            get
            {
                lock (_apiLock)
                {
                    return _infoProgressText;
                }
            }
        }

        public AbortJobDelegate AbortJobFunc
        {
            get
            {
                lock (_apiLock)
                {
                    return _abortJobFunc;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "AbortJobFunc: Job is running");
                }
                lock (_apiLock)
                {
                    _abortJobFunc = value;
                }
            }
        }

        public ProgressJobDelegate ProgressJobFunc
        {
            get
            {
                lock (_apiLock)
                {
                    return _progressJobFunc;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ProgressJobFunc: Job is running");
                }
                lock (_apiLock)
                {
                    _progressJobFunc = value;
                }
            }
        }

        public ErrorRaisedDelegate ErrorRaisedFunc
        {
            get
            {
                lock (_apiLock)
                {
                    return _errorRaisedFunc;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "ErrorRaisedFunc: Job is running");
                }
                lock (_apiLock)
                {
                    _errorRaisedFunc = value;
                }
            }
        }

        private EdValueType ArrayMaxSize
        {
            get
            {
                return _arrayMaxBufSize - 1;
            }
        }

        private EdValueType ArrayMaxBufSize
        {
            get
            {
                return _arrayMaxBufSize;
            }
            set
            {
                _arrayMaxBufSize = value;
                foreach (StringData data in _stringRegisters)
                {
                    data.NewArrayLength(_arrayMaxBufSize);
                }
            }
        }

        public string SgbdFileName
        {
            get
            {
                lock (_apiLock)
                {
                    return _sgbdFileName;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "SgbdFileName: Job is running");
                }
                bool changed;
                lock (_apiLock)
                {
                    changed = string.Compare(_sgbdFileName, value, StringComparison.OrdinalIgnoreCase) != 0;
                }
                if (changed)
                {
                    _closeSgbdFs = true;
                    lock (_apiLock)
                    {
                        _sgbdFileName = value;
                    }
                }
            }
        }

        public string EcuPath
        {
            get
            {
                lock (_apiLock)
                {
                    return _ecuPath;
                }
            }
        }

        public string IniFileName
        {
            get
            {
                lock (_apiLock)
                {
                    return _iniFileName;
                }
            }
        }

        public EdInterfaceBase EdInterfaceClass
        {
            get
            {
                lock (_apiLock)
                {
                    return _edInterfaceClass;
                }
            }
            set
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "EdInterfaceClass: Job is running");
                }
                lock (_apiLock)
                {
                    _edInterfaceClass = value;
                    _edInterfaceClass.Ediabas = this;
                    SetConfigProperty("Interface", _edInterfaceClass.InterfaceName);
                    SetConfigProperty("IfhVersion", _edInterfaceClass.InterfaceVerName);
                }
            }
        }

        public long TimeMeas
        {
            get
            {
                return _timeMeas;
            }
            set
            {
                _timeMeas = value;
            }
        }

        public ErrorCodes ErrorCodeLast
        {
            get
            {
                lock (_apiLock)
                {
                    return _errorCodeLast;
                }
            }
        }

#if !Android && !WindowsCE
        static EdiabasNet()
        {
            LoadAllResourceAssemblies();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string fullName = args.Name;
                if (!string.IsNullOrEmpty(fullName))
                {
                    Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly loadedAssembly in currentAssemblies)
                    {
                        if (string.Compare(loadedAssembly.FullName, fullName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return loadedAssembly;
                        }
                    }

                    string[] names = fullName.Split(',');
                    if (names.Length < 1)
                    {
                        return null;
                    }

                    string assemblyName = names[0];
                    string assemblyDllName = assemblyName + ".dll";
                    string assemblyDir = AssemblyDirectory;
                    if (string.IsNullOrEmpty(assemblyDir))
                    {
                        return null;
                    }

                    string assemblyFileName = Path.Combine(assemblyDir, assemblyDllName);

                    if (!File.Exists(assemblyFileName))
                    {
                        return null;
                    }

                    return Assembly.LoadFrom(assemblyFileName);
                }
                return null;
            };
        }
#endif

        public EdiabasNet() : this(null)
        {
        }

        public EdiabasNet(string config)
        {
            lock (SharedDataLock)
            {
                _instanceCount++;
            }
            if (_trapBitDict == null)
            {
                _trapBitDict = new Dictionary<ErrorCodes, UInt32>
                {
                    {ErrorCodes.EDIABAS_BIP_0002, 2},
                    {ErrorCodes.EDIABAS_BIP_0006, 6},
                    {ErrorCodes.EDIABAS_BIP_0009, 9},
                    {ErrorCodes.EDIABAS_BIP_0010, 10},
                    {ErrorCodes.EDIABAS_IFH_0001, 11},
                    {ErrorCodes.EDIABAS_IFH_0002, 12},
                    {ErrorCodes.EDIABAS_IFH_0003, 13},
                    {ErrorCodes.EDIABAS_IFH_0004, 14},
                    {ErrorCodes.EDIABAS_IFH_0005, 15},
                    {ErrorCodes.EDIABAS_IFH_0006, 16},
                    {ErrorCodes.EDIABAS_IFH_0007, 17},
                    {ErrorCodes.EDIABAS_IFH_0008, 18},
                    {ErrorCodes.EDIABAS_IFH_0009, 19},
                    {ErrorCodes.EDIABAS_IFH_0010, 20},
                    {ErrorCodes.EDIABAS_IFH_0011, 21},
                    {ErrorCodes.EDIABAS_IFH_0012, 22},
                    {ErrorCodes.EDIABAS_IFH_0013, 23},
                    {ErrorCodes.EDIABAS_IFH_0014, 24},
                    {ErrorCodes.EDIABAS_IFH_0015, 25},
                    {ErrorCodes.EDIABAS_IFH_0016, 26}
                };
            }

            _byteRegisters = new byte[32];
            _floatRegisters = new EdFloatType[16];
            _stringRegisters = new StringData[16];

            for (int i = 0; i < _stringRegisters.Length; i++)
            {
                _stringRegisters[i] = new StringData(this, ArrayMaxBufSize);
            }
            foreach (Register arg in RegisterList)
            {
                arg.SetEdiabas(this);
            }

            _jobRunning = false;
            _lockTrace = false;
            SetConfigProperty("EdiabasVersion", "7.3.0");
            SetConfigProperty("Simulation", "0");
            SetConfigProperty("BipDebugLevel", "0");
            SetConfigProperty("ApiTrace", "0");
            SetConfigProperty("IfhTrace", "0");
            SetConfigProperty("TraceBuffering", "0");
            SetConfigProperty("IfhTraceBuffering", "1");
            SetConfigProperty("AppendTrace", "0");
#if COMPRESS_TRACE
            SetConfigProperty("CompressTrace", "0");
#endif
            SetConfigProperty("LockTrace", "0");

            SetConfigProperty("UbattHandling", "0");
            SetConfigProperty("IgnitionHandling", "0");
            SetConfigProperty("ClampHandling", "0");

            SetConfigProperty("RetryComm", "1");
            SetConfigProperty("SystemResults", "1");
            SetConfigProperty("TaskPriority", "0");
            SetConfigProperty("EdicApiHandle", "0");

            string assemblyPath = AssemblyDirectory ?? string.Empty;
            SetConfigProperty("EcuPath", assemblyPath);

            string tracePath = Path.Combine(assemblyPath, "Trace");
            SetConfigProperty("TracePath", tracePath);

            bool withFile = false;
            string configFile = Path.Combine(assemblyPath, "EdiabasLib.config");
            string iniFile = Path.Combine(assemblyPath, "EDIABAS.INI");
            if (!string.IsNullOrEmpty(config) && (config[0] == '@'))
            {
                withFile = true;
                configFile = config.Substring(1);
            }

            if (File.Exists(iniFile))
            {
                _iniFileName = iniFile;
                ReadIniSettings(iniFile);
            }

            if (File.Exists(configFile))
            {
                XmlDocument xdocConfig = new XmlDocument();
                try
                {
                    xdocConfig.Load(configFile);
                    SetConfigProperty("EdiabasIniPath", Path.GetDirectoryName(configFile));
                    ReadAllSettingsProperties(xdocConfig);
                }
                catch
                {
                    // ignored
                }
            }
            _ecuPathDefault = _ecuPath;

            string lockTrace = GetConfigProperty("LockTrace");
            if (lockTrace != null)
            {
                if (StringToValue(lockTrace) != 0)
                {
                    _lockTrace = true;
                }
            }

            if (!withFile && !string.IsNullOrEmpty(config))
            {
                string[] words = config.Split(';');
                foreach (string word in words)
                {
                    int assignPos = word.IndexOf('=');
                    if (assignPos >= 0)
                    {
                        string key = word.Substring(0, assignPos);
                        string value = word.Substring(assignPos + 1);
                        SetConfigProperty(key, value);
                    }
                }
            }

            LogFormat(EdLogLevel.Ifh, "EDIABAS assembly path: {0}", assemblyPath);
            if (!string.IsNullOrEmpty(_iniFileName))
            {
                LogFormat(EdLogLevel.Ifh, "EDIABAS ini file: {0}", _iniFileName);
            }
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
                    _abortJobFunc = null;    // prevent abort of exitJob
                    try
                    {
                        CloseSgbdFs();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    CloseTableFs();
                    CloseAllUserFiles();
                    if (_edInterfaceClass != null)
                    {
                        _edInterfaceClass.Dispose();
                        _edInterfaceClass = null;
                    }
                    CloseLog(); // must be closed after interface class
                    lock (SharedDataLock)
                    {
                        _instanceCount--;
                        if (_instanceCount == 0)
                        {
                            SharedDataDict.Clear();
                        }
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public void ClearGroupMapping()
        {
            lock (_apiLock)
            {
                _sgbdFileResolveLast = string.Empty;
                _groupMappingDict.Clear();
            }
        }

        public bool ReadIniSettings(string iniFile)
        {
            try
            {
                IniFile ediabasIni = new IniFile(iniFile);
                SetConfigPropertyFromIni(ediabasIni, "Interface");
                SetConfigPropertyFromIni(ediabasIni, "EcuPath");
                SetConfigPropertyFromIni(ediabasIni, "ApiTrace");
                SetConfigPropertyFromIni(ediabasIni, "IfhTrace");
                SetConfigPropertyFromIni(ediabasIni, "TraceBuffering");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void SetConfigPropertyFromIni(IniFile ediabasIni, string property)
        {
            string value = ediabasIni.GetValue("Configuration", property, string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                SetConfigProperty(property, value);
            }
        }

        public void ReadAllSettingsProperties(XmlDocument xdocConfig)
        {
            try
            {
                if (xdocConfig != null)
                {
                    XmlNode xnodes = xdocConfig.SelectSingleNode("/configuration/appSettings");

                    if (xnodes != null)
                    {
                        foreach (XmlNode xnn in xnodes.ChildNodes)
                        {
                            try
                            {
                                if ((string.Compare(xnn.Name, "add", StringComparison.OrdinalIgnoreCase ) == 0) &&
                                    xnn.Attributes != null)
                                {
                                    XmlAttribute attribKey = xnn.Attributes["key"];
                                    XmlAttribute attribValue = xnn.Attributes["value"];
                                    if (attribKey != null && attribValue != null)
                                    {
                                        string key = attribKey.Value;
                                        string value = attribValue.Value;
                                        SetConfigProperty(key, value);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string GetConfigProperty(string name)
        {
            string key = name.ToUpper(Culture);
            string value;
            lock (_apiLock)
            {
                if (!_configDict.TryGetValue(key, out value))
                {
                    return null;
                }
            }
            return value;
        }

        public void SetConfigProperty(string name, string value)
        {
            string key = name.ToUpper(Culture);
            if (_lockTrace)
            {
                if (string.Compare(key, "ApiTrace", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "IfhTrace", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "TracePath", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "TraceBuffering", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "AppendTrace", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "IfhTraceName", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                if (string.Compare(key, "ApiTraceName", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
            }

            lock (_apiLock)
            {
                if (_configDict.ContainsKey(key))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _configDict[key] = value;
                    }
                    else
                    {
                        _configDict.Remove(key);
                    }
                }
                else
                {
                    _configDict.Add(key, value);
                }
            }

            if (string.Compare(key, "EcuPath", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (JobRunning)
                {
                    throw new ArgumentOutOfRangeException("JobRunning", "SetConfigProperty: Job is running");
                }
                string ecuPath;
                bool changed;
                lock (_apiLock)
                {
                    ecuPath = string.IsNullOrEmpty(value) ? _ecuPathDefault : value;
                    changed = string.Compare(_ecuPath, ecuPath, StringComparison.OrdinalIgnoreCase) != 0;
                }
                if (changed)
                {
                    lock (_apiLock)
                    {
                        _ecuPath = ecuPath;
                    }
                    _closeSgbdFs = true;
                }
            }
            if (string.Compare(key, "TracePath", StringComparison.OrdinalIgnoreCase) == 0)
            {
                CloseLog();
            }
            if (string.Compare(key, "IfhTrace", StringComparison.OrdinalIgnoreCase) == 0)
            {
                CloseLog();
            }
            if (string.Compare(key, "IfhTraceName", StringComparison.OrdinalIgnoreCase) == 0)
            {
                CloseLog();
            }
        }

        public void CloseSgbd()
        {
            _closeSgbdFs = true;
        }

        public bool IsJobExisting(string jobName)
        {
            return (_sgbdFs != null) && (GetJobInfo(jobName) != null);
        }

        private JobInfo GetJobInfo(string jobName)
        {
            UInt32 jobIndex;
            if (_jobInfos.JobNameDict.TryGetValue(jobName.ToUpper(Culture), out jobIndex))
            {
                return _jobInfos.JobInfoArray[jobIndex];
            }
            return null;
        }

        public static string AssemblyDirectory
        {
            get
            {
#if WindowsCE
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
#else
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
#endif
            }
        }

        public static bool LoadAllResourceAssemblies()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    if (!resourceName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                Assembly loadedAssembly = Assembly.Load(memoryStream.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
            if (_sgbdFs != null)
            {
                return true;
            }
            string fileName = Path.Combine(EcuPath, SgbdFileName);
            try
            {
                _sgbdFs = MemoryStreamReader.OpenRead(fileName);
            }
            catch (Exception)
            {
                LogString(EdLogLevel.Error, "OpenSgbdFs file not found: " + fileName);
                return false;
            }
            _usesInfos = ReadAllUses(_sgbdFs);
            _descriptionInfos = null;
            _jobInfos = ReadAllJobs(_sgbdFs);
            _tableInfos = ReadAllTables(_sgbdFs);
            _requestInit = true;
            return true;
        }

        private void CloseSgbdFs()
        {
            if (_sgbdFs != null)
            {
                try
                {
                    if (!_requestInit)
                    {
                        ExecuteExitJob();
                    }
                }
                finally
                {
                    if (_sgbdFs != null)
                    {
                        _sgbdFs.Dispose();
                        _sgbdFs = null;
                    }
                }
            }
        }

        private void CloseTableFs()
        {
            if (_tableFs != null)
            {
                if (_tableFs != _sgbdBaseFs)
                {
                    _tableFs.Dispose();
                }
                _tableFs = null;
            }
            _tableInfosExt = null;
            _tableIndex = -1;
            _tableRowIndex = -1;
        }

        private Stream GetTableFs()
        {
            if (_tableFs != null)
            {
                return _tableFs;
            }
            return _sgbdFs;
        }

        private void SetTableFs(Stream fs)
        {
            _tableFs = fs;
            _tableInfosExt = ReadAllTables(fs);
        }

        private TableInfos GetTableInfos(Stream fs)
        {
            if (_tableFs != null && _tableFs == fs)
            {
                return _tableInfosExt;
            }
            return _tableInfos;
        }

        private int StoreUserFile(Stream fs)
        {
            for (int i = 0; i < _userFilesArray.Length; i++)
            {
                if (_userFilesArray[i] == null)
                {
                    _userFilesArray[i] = fs;
                    return i;
                }
            }
            return -1;
        }

        private Stream GetUserFile(int index)
        {
            if ((index < 0) || (index >= _userFilesArray.Length))
            {
                return null;
            }
            return _userFilesArray[index];
        }

        private string ReadFileLine(Stream fs)
        {
            StringBuilder stringBuilder = new StringBuilder(100);
            int currByte;
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
            int currByte;
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
            if ((index < 0) || (index >= _userFilesArray.Length))
            {
                return false;
            }
            if (_userFilesArray[index] == null)
            {
                return true;
            }
            _userFilesArray[index].Dispose();
            _userFilesArray[index] = null;
            return true;
        }

        private void CloseAllUserFiles()
        {
            for (int i = 0; i < _userFilesArray.Length; i++)
            {
                if (_userFilesArray[i] != null)
                {
                    _userFilesArray[i].Dispose();
                    _userFilesArray[i] = null;
                }
            }
        }

        public void SetError(ErrorCodes error)
        {
            LogFormat(EdLogLevel.Error, "SetError: {0}", error);

            if (error != ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdValueType bitNumber;
                if (_trapBitDict.TryGetValue(error, out bitNumber))
                {
                    _errorTrapBitNr = (int) bitNumber;
                }
                else
                {
                    _errorTrapBitNr = 0;
                }

                _sgbdFileResolveLast = string.Empty;     // reset last file after error to force sgbd reload
                EdValueType activeErrors = (EdValueType)((1 << _errorTrapBitNr) & ~_errorTrapMask);
                if (activeErrors != 0)
                {
                    RaiseError(error);
                }
            }
            else
            {
                _errorTrapBitNr = -1;
            }
        }

        public void RaiseError(ErrorCodes error)
        {
            ErrorCodes errorCode = error;
            if (_jobStdExit)
            {
                errorCode = ErrorCodes.EDIABAS_SYS_0018;
            }
            lock (_apiLock)
            {
                _errorCodeLast = errorCode;
            }
            ErrorRaisedDelegate errorFunc = ErrorRaisedFunc;
            if (errorFunc != null)
            {
                errorFunc(errorCode);
            }
            throw new Exception(string.Format(Culture, "Error occurred: {0}", errorCode));
        }

        public static string GetErrorDescription(ErrorCodes errorCode)
        {
            if ((errorCode < ErrorCodes.EDIABAS_IFH_0000) ||
                (errorCode > ErrorCodes.EDIABAS_ERROR_LAST))
            {
                return string.Empty;
            }
            uint index = errorCode - ErrorCodes.EDIABAS_IFH_0000;
            return ErrorDescription[index];
        }

        public static string FormatResult(ResultData resultData, string format)
        {
            bool leftAlign = false;
            bool zeroPrexif = false;
            Int32 length1 = -1;
            Int32 length2 = -1;
            char convertType;
            char exponent = '\0';

            // remove whitespace
            string formatBare = Regex.Replace(format, @"\s+", "");
            // parse format
            if (string.IsNullOrEmpty(formatBare))
            {   // string
                convertType = 'T';
            }
            else
            {
                string parseString = formatBare;
                if (parseString[0] == '-')
                {
                    leftAlign = true;
                    parseString = parseString.Substring(1);
                    if (string.IsNullOrEmpty(parseString))
                    {
                        return null;
                    }
                }
                convertType = parseString[parseString.Length - 1];
                parseString = parseString.Remove(parseString.Length - 1, 1);

                if (!string.IsNullOrEmpty(parseString))
                {
                    if (convertType == 'R')
                    {
                        char lastChar = parseString[parseString.Length - 1];
                        if ((lastChar == 'E') || (lastChar == 'e'))
                        {
                            exponent = lastChar;
                            parseString = parseString.Remove(parseString.Length - 1, 1);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(parseString))
                {
                    string[] words = parseString.Split('.');
                    if ((words.Length < 1) || (words.Length > 2))
                    {
                        return null;
                    }
                    try
                    {
                        if (words[0].Length > 0)
                        {
                            length1 = Convert.ToInt32(words[0], 10);
                            if (words[0][0] == '0')
                            {
                                zeroPrexif = true;
                            }
                        }
                        if (words.Length > 1)
                        {
                            if (words[1].Length > 0)
                            {
                                length2 = Convert.ToInt32(words[1], 10);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            // convert format
            bool valueIsDouble = false;
            Int64 valueInt64 = 0;
            Double valueDouble = 0;
            string valueString = null;

            if (resultData.OpData is Int64)
            {
                valueInt64 = (Int64)resultData.OpData;
            }
            else if (resultData.OpData is Double)
            {
                valueDouble = (Double)resultData.OpData;
                valueIsDouble = true;
            }
            else if (resultData.OpData is string)
            {
                valueString = (string)resultData.OpData;
            }
            else
            {
                return null;
            }

            bool convIsDouble = false;
            bool hexValue = false;
            Int64 convInt64 = 0;
            Double convDouble = 0;
            string convString = null;
            bool validInt = true;

            if (valueString != null && convertType != 'T')
            {
                valueDouble = StringToFloat(valueString);
                valueIsDouble = true;
                StringToValue(valueString, out validInt);
            }

            switch (convertType)
            {
                case 'C':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (SByte)valueDouble;
                    }
                    else
                    {
                        convInt64 = (SByte)valueInt64;
                    }
                    break;

                case 'B':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (Byte)valueDouble;
                    }
                    else
                    {
                        convInt64 = (Byte)valueInt64;
                    }
                    break;

                case 'I':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (Int16)valueDouble;
                    }
                    else
                    {
                        convInt64 = (Int16)valueInt64;
                    }
                    break;

                case 'W':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (UInt16)valueDouble;
                    }
                    else
                    {
                        convInt64 = (UInt16)valueInt64;
                    }
                    break;

                case 'L':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (Int32)valueDouble;
                    }
                    else
                    {
                        convInt64 = (Int32)valueInt64;
                    }
                    break;

                case 'D':
                    if (!validInt)
                    {
                        return null;
                    }
                    if (valueIsDouble)
                    {
                        convInt64 = (UInt32)valueDouble;
                    }
                    else
                    {
                        convInt64 = (UInt32)valueInt64;
                    }
                    break;

                case 'R':
                    convDouble = valueIsDouble ? valueDouble : valueInt64;
                    convIsDouble = true;
                    break;

                case 'T':
                    if (valueString != null)
                    {
                        convString = valueString;
                        break;
                    }
                    if (valueIsDouble)
                    {
                        convDouble = valueDouble;
                        exponent = 'E';
                        convIsDouble = true;
                    }
                    else
                    {
                        convInt64 = valueInt64;
                    }
                    break;

                case 'X':
                    if (!validInt)
                    {
                        return null;
                    }
                    hexValue = true;
                    if (valueIsDouble)
                    {
                        convInt64 = (UInt32)valueDouble;
                    }
                    else
                    {
                        convInt64 = (UInt32)valueInt64;
                    }
                    break;

                default:
                    return null;
            }

            // format result
            try
            {
                if (convString == null)
                {
                    if (convIsDouble)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (length2 > 0)
                        {
                            sb.Append("0.");
                            sb.Append(new string('0', length2));
                        }
                        else
                        {
                            if ((length2 == 0) || (length1 >= 0))
                            {
                                sb.Append("0");
                            }
                            else
                            {
                                sb.Append("0.000000");
                            }
                        }
                        if (exponent != '\0')
                        {
                            sb.Append(exponent);
                            sb.Append("+000");
                        }
                        sb.Insert(0, "{0:");
                        sb.Append("}");

                        string formatString = sb.ToString();
                        string resultString = string.Format(Culture, formatString, convDouble);

                        if (length1 >= 0 && length1 > resultString.Length)
                        {
                            string fillString = new String(' ', length1 - resultString.Length);
                            if (leftAlign)
                            {
                                resultString += fillString;
                            }
                            else
                            {
                                resultString = fillString + resultString;
                            }
                        }
                        return resultString;
                    }

                    if (!hexValue)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (length1 >= 0)
                        {
                            sb.Append(",");
                            if (leftAlign)
                            {
                                sb.Append("-");
                            }
                            sb.Append(string.Format(Culture, "{0}", length1));
                        }
                        if (length2 > 0)
                        {
                            sb.Append(":");
                            sb.Append(new string('0', length2));
                        }
                        sb.Insert(0, "{0");
                        sb.Append("}");
                        string formatString = sb.ToString();
                        return string.Format(Culture, formatString, convInt64);
                    }
                    // hex value
                    {
                        StringBuilder sb = new StringBuilder();

                        if (length1 >= 0)
                        {
                            sb.Append(",");
                            if (leftAlign)
                            {
                                sb.Append("-");
                            }
                            sb.Append(string.Format(Culture, "{0}:X", length1));
                            if (zeroPrexif)
                            {
                                sb.Append(string.Format(Culture, "0{0}", length1));
                            }
                        }
                        else
                        {
                            sb.Append(":X");
                        }
                        sb.Insert(0, "{0");
                        sb.Append("}");
                        string formatString = sb.ToString();
                        return string.Format(Culture, formatString, convInt64);
                    }
                }
                else
                {
                    string resultString = convString;
                    if (length2 >= 0 && length2 < resultString.Length)
                    {
                        resultString = resultString.Remove(length2, resultString.Length - length2);
                    }
                    if (length1 >= 0 && length1 > resultString.Length)
                    {
                        string fillString = new String(' ', length1 - resultString.Length);
                        if (leftAlign)
                        {
                            resultString += fillString;
                        }
                        else
                        {
                            resultString = fillString + resultString;
                        }
                    }
                    return resultString;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        private void SetResultData(ResultData resultData)
        {
            string key = resultData.Name.ToUpper(Culture);
            if (_resultDict.ContainsKey(key))
            {
                _resultDict[key] = resultData;
            }
            else
            {
                _resultDict.Add(key, resultData);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private ResultData GetResultData(string name)
        {
            ResultData result;

            if (_resultDict.TryGetValue(name.ToUpper(Culture), out result))
            {
                return result;
            }
            return null;
        }

        private void SetSysResultData(ResultData resultData)
        {
            string key = resultData.Name.ToUpper(Culture);
            if (_resultSysDict.ContainsKey(key))
            {
                _resultSysDict[key] = resultData;
            }
            else
            {
                _resultSysDict.Add(key, resultData);
            }
        }

        private Dictionary<string, ResultData> CreateSystemResultDict(JobInfo jobInfo, int setCount)
        {
            Dictionary<string, ResultData> resultDictSystem = new Dictionary<string, ResultData>();

            string objectName = Path.GetFileNameWithoutExtension(SgbdFileName);
            if (jobInfo.UsesInfo != null)
            {
                objectName = jobInfo.UsesInfo.Name;
            }

            Int64 sysResults = 0;
            string sysResultStr = GetConfigProperty("SystemResults");
            if (sysResultStr != null)
            {
                sysResults = StringToValue(sysResultStr);
            }

            resultDictSystem.Add("VARIANTE", new ResultData(ResultType.TypeS, "VARIANTE", Path.GetFileNameWithoutExtension(SgbdFileName ?? string.Empty).ToUpper(Culture)));
            resultDictSystem.Add("OBJECT", new ResultData(ResultType.TypeS, "OBJECT", objectName));
            resultDictSystem.Add("JOBNAME", new ResultData(ResultType.TypeS, "JOBNAME", jobInfo.JobName));
            resultDictSystem.Add("SAETZE", new ResultData(ResultType.TypeW, "SAETZE", (Int64)setCount));
            if (sysResults != 0)
            {
                resultDictSystem.Add("JOBSTATUS", new ResultData(ResultType.TypeS, "JOBSTATUS", _resultJobStatus));
                resultDictSystem.Add("UBATTCURRENT", new ResultData(ResultType.TypeI, "UBATTCURRENT", (Int64)(-1)));
                resultDictSystem.Add("UBATTHISTORY", new ResultData(ResultType.TypeI, "UBATTHISTORY", (Int64)(-1)));
                resultDictSystem.Add("IGNITIONCURRENT", new ResultData(ResultType.TypeI, "IGNITIONCURRENT", (Int64)(-1)));
                resultDictSystem.Add("IGNITIONHISTORY", new ResultData(ResultType.TypeI, "IGNITIONHISTORY", (Int64)(-1)));
            }

            foreach (string key in _resultSysDict.Keys)
            {
                ResultData resultData = _resultSysDict[key];
                if (!resultDictSystem.ContainsKey(key))
                {
                    resultDictSystem.Add(key, resultData);
                }
            }

            return resultDictSystem;
        }

        private void JobProgressInform()
        {
            ProgressJobDelegate progressFunc = ProgressJobFunc;
            if (progressFunc != null)
            {
                progressFunc(this);
            }
        }

        private UsesInfos ReadAllUses(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x7C;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            Int32 usesOffset = BitConverter.ToInt32(buffer, 0);

            UsesInfos usesInfosLocal = new UsesInfos();
            if (usesOffset < 0)
            {
                usesInfosLocal.UsesInfoArray = new UsesInfo[0];
                return usesInfosLocal;
            }
            fs.Position = usesOffset;
            int usesCount = ReadInt32(fs);

            usesInfosLocal.UsesInfoArray = new UsesInfo[usesCount];

            byte[] usesBuffer = new byte[0x100];
            for (int i = 0; i < usesCount; i++)
            {
                ReadAndDecryptBytes(fs, usesBuffer, 0, 0x100);
                string usesName = Encoding.GetString(usesBuffer, 0, 0x100).TrimEnd('\0');
                usesInfosLocal.UsesInfoArray[i] = new UsesInfo(usesName);
            }

            return usesInfosLocal;
        }

        private DescriptionInfos ReadDescriptions(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x90;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            Int32 descriptionOffset = BitConverter.ToInt32(buffer, 0);

            DescriptionInfos descriptionInfosLocal = new DescriptionInfos
            {
                JobComments = new Dictionary<string, List<string>>()
            };
            if (descriptionOffset < 0)
            {
                return descriptionInfosLocal;
            }
            fs.Position = descriptionOffset;
            int numBytes = ReadInt32(fs);

            List<string> commentList = new List<string>();
            string previousJobName = null;

            byte[] recordBuffer = new byte[1100];
            int recordOffset = 0;
            for (int i = 0; i < numBytes; i++)
            {
                ReadAndDecryptBytes(fs, recordBuffer, recordOffset, 1);
                recordOffset += 1;

                if (recordOffset >= 1098)
                    recordBuffer[recordOffset++] = 10; //\n

                if (recordBuffer[recordOffset - 1] == 10) //\n
                {
                    recordBuffer[recordOffset] = 0;
                    string comment = Encoding.GetString(recordBuffer, 0, recordOffset - 1);
                    if (comment.StartsWith("JOBNAME:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (previousJobName == null)
                        {
                            descriptionInfosLocal.GlobalComments = commentList;
                        }
                        else
                        {
                            if (!descriptionInfosLocal.JobComments.ContainsKey(previousJobName))
                            {
                                descriptionInfosLocal.JobComments.Add(previousJobName, commentList);
                            }
                        }
                        commentList = new List<string>();
                        previousJobName = comment.Substring(8);
                    }

                    commentList.Add(comment);
                    recordOffset = 0;
                }
            }

            if (previousJobName == null)
            {
                descriptionInfosLocal.GlobalComments = commentList;
            }
            else
            {
                if (!descriptionInfosLocal.JobComments.ContainsKey(previousJobName))
                {
                    descriptionInfosLocal.JobComments.Add(previousJobName, commentList);
                }
            }

            return descriptionInfosLocal;
        }

        private JobInfos ReadAllJobs(Stream fs)
        {
            List<JobInfo> jobListComplete = GetJobList(fs, null);

            foreach (UsesInfo usesInfo in _usesInfos.UsesInfoArray)
            {
                string fileName = Path.Combine(EcuPath, usesInfo.Name.ToLower(Culture) + ".prg");
                try
                {
                    using (Stream tempFs = MemoryStreamReader.OpenRead(fileName))
                    {
                        try
                        {
                            List<JobInfo> jobListTemp = GetJobList(tempFs, usesInfo);
                            jobListComplete.AddRange(jobListTemp);
                        }
                        catch (Exception ex)
                        {
                            LogString(EdLogLevel.Error, "ReadAllJobs exception: " + GetExceptionText(ex));
                        }
                    }
                }
                catch (Exception)
                {
                    LogString(EdLogLevel.Error, "ReadAllJobs file not found: " + fileName);
                }
            }

            int numJobs = jobListComplete.Count;
            JobInfos jobInfosLocal = new JobInfos
            {
                JobNameDict = new Dictionary<string, UInt32>(),
                JobInfoArray = new JobInfo[numJobs]
            };

            EdValueType index = 0;
            foreach (JobInfo jobInfo in jobListComplete)
            {
                string key = jobInfo.JobName.ToUpper(Culture);
                bool addJob = true;
                if (jobInfo.UsesInfo != null)
                {
                    if ((string.Compare(key, JobNameInit, StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(key, JobNameExit, StringComparison.OrdinalIgnoreCase) == 0))
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

        private List<JobInfo> GetJobList(Stream fs, UsesInfo usesInfo)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x18;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            UInt32 arraySize = BitConverter.ToUInt32(buffer, 0);
            if (arraySize == 0)
            {
                arraySize = 1024;
            }

            fs.Position = 0x88;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            Int32 jobListOffset = BitConverter.ToInt32(buffer, 0);

            List<JobInfo> jobList = new List<JobInfo>();
            if (jobListOffset < 0)
            {
                return jobList;
            }
            fs.Position = jobListOffset;
            int numJobs = ReadInt32(fs);

            byte[] jobBuffer = new byte[0x44];
            UInt32 jobStart = (UInt32)fs.Position;
            for (int i = 0; i < numJobs; i++)
            {
                fs.Position = jobStart;
                ReadAndDecryptBytes(fs, jobBuffer, 0, jobBuffer.Length);
                string jobNameString = Encoding.GetString(jobBuffer, 0, 0x40).TrimEnd('\0');
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
                jobList.Add(new JobInfo(jobNameString, jobAddress, 0, arraySize, usesInfo));
#endif
                jobStart += 0x44;
            }
            return jobList;
        }

        private TableInfos ReadAllTables(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x84;
            fs.Read(buffer, 0, buffer.Length);

            TableInfos tableInfosLocal = new TableInfos
            {
                TableNameDict = new Dictionary<string, UInt32>()
            };

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            Int32 tableOffset = BitConverter.ToInt32(buffer, 0);
            if (tableOffset < 0)
            {
                tableInfosLocal.TableInfoArray = new TableInfo[0];
                return tableInfosLocal;
            }
            fs.Position = tableOffset;

            byte[] tableCountBuffer = new byte[4];
            ReadAndDecryptBytes(fs, tableCountBuffer, 0, tableCountBuffer.Length);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(tableCountBuffer, 0, 4);
            }
            int tableCount = BitConverter.ToInt32(tableCountBuffer, 0);
            tableInfosLocal.TableInfoArray = new TableInfo[tableCount];

            UInt32 tableStart = (UInt32)fs.Position;
            for (int i = 0; i < tableCount; ++i)
            {
                TableInfo tableInfo = ReadTable(fs, tableStart);
                tableInfosLocal.TableInfoArray[i] = tableInfo;
                tableInfosLocal.TableNameDict.Add(tableInfo.Name.ToUpper(Culture), (UInt32)i);
                tableStart += 0x50;
            }
            return tableInfosLocal;
        }

        private TableInfo ReadTable(Stream fs, UInt32 tableOffset)
        {
            fs.Position = tableOffset;

            byte[] tableBuffer = new byte[0x50];
            ReadAndDecryptBytes(fs, tableBuffer, 0, tableBuffer.Length);
            string name = Encoding.GetString(tableBuffer, 0, 0x40).TrimEnd('\0');

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

        private void IndexTable(Stream fs, TableInfo tableInfo)
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
                    for (l = 0; l < _tableItemBuffer.Length; l++)
                    {
                        ReadAndDecryptBytes(fs, _tableItemBuffer, l, 1);
                        if (_tableItemBuffer[l] == 0)
                            break;
                    }
                    if (j == 0)
                    {
                        string columnName = Encoding.GetString(_tableItemBuffer, 0, l).ToUpper(Culture);
                        columnNameDict.Add(columnName, (UInt32)k);
                    }
                }
            }

            tableInfo.ColumnNameDict = columnNameDict;
            tableInfo.TableEntries = tableEntries;
        }

        private string GetTableString(Stream fs, UInt32 stringOffset)
        {
            fs.Position = stringOffset;

            int l;
            for (l = 0; l < _tableItemBuffer.Length; ++l)
            {
                ReadAndDecryptBytes(fs, _tableItemBuffer, l, 1);
                if (_tableItemBuffer[l] == 0)
                    break;
            }
            return Encoding.GetString(_tableItemBuffer, 0, l);
        }

        private Int32 GetTableIndex(Stream fs, string tableName, out bool found)
        {
            found = false;
            TableInfos tableInfosLocal = GetTableInfos(fs);

            UInt32 tableIdx;
            if (tableInfosLocal.TableNameDict.TryGetValue(tableName.ToUpper(Culture), out tableIdx))
            {
                IndexTable(fs, tableInfosLocal.TableInfoArray[tableIdx]);
                found = true;
                return (Int32)tableIdx;
            }

            return tableInfosLocal.TableInfoArray.Length - 1;
        }

        private UInt32 GetTableColumns(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Columns;
        }

        private UInt32 GetTableRows(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfosLocal = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfosLocal.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Rows;
        }

        private Int32 GetTableLine(Stream fs, Int32 tableIdx, EdValueType line, out bool found)
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

        private Int32 SeekTable(Stream fs, Int32 tableIdx, string columnName, string valueName, out bool found)
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
                    string rowStr = GetTableString(fs, table.TableEntries[i][columnIndex]).ToUpper(Culture);
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
            if (columnDict.TryGetValue(valueName.ToUpper(Culture), out selectedRow))
            {
                found = true;
                return (Int32)selectedRow;
            }

            return (Int32)(table.Rows - 1); // select last line
        }

        private Int32 SeekTable(Stream fs, Int32 tableIdx, string columnName, EdValueType value, out bool found)
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

        // ReSharper disable once UnusedParameter.Local
        private Int32 GetTableColumnIdx(Stream fs, TableInfo table, string columnName)
        {
            UInt32 column;
            if (table.ColumnNameDict.TryGetValue(columnName.ToUpper(Culture), out column))
            {
                return (Int32)column;
            }
            return -1;
        }

        private string GetTableEntry(Stream fs, Int32 tableIdx, Int32 rowIdx, string columnName)
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
            LogFormat(EdLogLevel.Info, "ResolveSgbdFile: {0}", fileName);
            if (JobRunning)
            {
                throw new ArgumentOutOfRangeException("JobRunning", "ResolveSgbdFile: Job is running");
            }
            string baseFileName = Path.GetFileNameWithoutExtension(fileName ?? String.Empty).ToLower(Culture);
            if (string.Compare(_sgbdFileResolveLast, baseFileName, StringComparison.Ordinal) == 0)
            {   // same name specified
                LogFormat(EdLogLevel.Info, "ResolveSgbdFile resolved: {0}", SgbdFileName);
                return;
            }

            UInt32 fileType = GetFileType(Path.Combine(EcuPath, fileName ?? String.Empty));
            if (fileType == 0)
            {       // group file
                string key = baseFileName;
                string variantName;
                bool mappingFound;
                lock (_apiLock)
                {
                    mappingFound = _groupMappingDict.TryGetValue(key, out variantName);
                }
                if (!mappingFound)
                {
                    SgbdFileName = baseFileName + ".grp";
                    variantName = ExecuteIdentJob().ToLower(Culture);
                    if (variantName.Length == 0)
                    {
                        LogFormat(EdLogLevel.Error, "ResolveSgbdFile: No variant found");
                        throw new ArgumentOutOfRangeException("variantName", "ResolveSgbdFile: No variant found");
                    }
                    lock (_apiLock)
                    {
                        _groupMappingDict.Add(key, variantName);
                    }
                }
                SgbdFileName = variantName + ".prg";
            }
            else
            {
                SgbdFileName = baseFileName + ".prg";
            }
            LogFormat(EdLogLevel.Info, "ResolveSgbdFile resolved: {0}", SgbdFileName);
            _sgbdFileResolveLast = baseFileName;
        }

        public UInt32 GetFileType(string fileName)
        {
            UInt32 fileType;

            string baseFileName = Path.GetFileNameWithoutExtension(fileName);

            string dirName = Path.GetDirectoryName(fileName) ?? string.Empty;
            string prgFileName = Path.Combine(dirName, baseFileName + ".prg");
            string grpFileName = Path.Combine(dirName, baseFileName + ".grp");
            string localFileName = string.Empty;
            if (File.Exists(prgFileName))
            {
                localFileName = prgFileName;
            }
            else if (File.Exists(grpFileName))
            {
                localFileName = grpFileName;
            }
            if (string.IsNullOrEmpty(localFileName))
            {   // now try for case sensitive file systems
                try
                {
                    using (MemoryStreamReader.OpenRead(prgFileName))
                    {
                    }
                    localFileName = prgFileName;
                }
                catch (Exception)
                {
                    localFileName = grpFileName;
                }
            }

            try
            {
                using (Stream tempFs = MemoryStreamReader.OpenRead(localFileName))
                {
                    byte[] buffer = new byte[4];
                    tempFs.Position = 0x10;
                    tempFs.Read(buffer, 0, buffer.Length);

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer, 0, 4);
                    }
                    fileType = BitConverter.ToUInt32(buffer, 0);
                }
            }
            catch (Exception)
            {
                LogString(EdLogLevel.Error, "GetFileType file not found: " + fileName);
                throw new ArgumentOutOfRangeException(fileName, "GetFileType: Unable to read file");
            }
            return fileType;
        }

        private void ExecuteInitJob()
        {
            bool jobRunningOld = JobRunning;
            bool jobStdOld = _jobStd;
            try
            {
                JobRunning = true;
                _jobStd = true;

                try
                {
                    ExecuteJobPrivate(JobNameInit, true);
                }
                catch (Exception ex)
                {
                    LogString(EdLogLevel.Error, "executeInitJob Exception: " + GetExceptionText(ex));
                    CloseSgbdFs();  // close file to force a reload
                    throw;
                }
                if (_resultSets.Count > 1)
                {
                    ResultData result;
                    if (_resultSets[1].TryGetValue("DONE", out result))
                    {
                        if (result.OpData as Int64? == 1)
                        {
                            _requestInit = false;
                            LogString(EdLogLevel.Info, "executeInitJob ok");
                            return;
                        }
                    }
                }

                LogString(EdLogLevel.Error, "executeInitJob failed");
                CloseSgbdFs();  // close file to force a reload
                SetError(ErrorCodes.EDIABAS_SYS_0010);
            }
            finally
            {
                _jobStd = jobStdOld;
                JobRunning = jobRunningOld;
            }
        }

        private void ExecuteExitJob()
        {
            bool jobRunningOld = JobRunning;
            bool jobStdOld = _jobStd;
            try
            {
                JobRunning = true;
                _jobStd = true;
                _jobStdExit = true;

                try
                {
                    if (IsJobExisting(JobNameExit))
                    {
                        ExecuteJobPrivate(JobNameExit);
                    }
                }
                catch (Exception ex)
                {
                    LogString(EdLogLevel.Error, "ExecuteExitJob Exception: " + GetExceptionText(ex));
                    throw new Exception("ExecuteExitJob", ex);
                }
            }
            finally
            {
                _jobStd = jobStdOld;
                _jobStdExit = false;
                JobRunning = jobRunningOld;
            }
        }

        private string ExecuteIdentJob()
        {
            bool jobRunningOld = JobRunning;
            bool jobStdOld = _jobStd;
            try
            {
                JobRunning = true;
                _jobStd = true;

                _resultDict.Clear();
                try
                {
                    ExecuteJobPrivate(JobNameIdent);
                }
                catch (Exception ex)
                {
                    LogString(EdLogLevel.Error, "executeIdentJob Exception: " + GetExceptionText(ex));
                    throw new Exception("executeIdentJob", ex);
                }
                if (_resultSets.Count > 1)
                {
                    ResultData result;
                    if (_resultSets[1].TryGetValue("VARIANTE", out result))
                    {
                        if (result.OpData is string)
                        {
                            string variantName = (string)result.OpData;
                            LogString(EdLogLevel.Info, "executeIdentJob ok: " + variantName);
                            return variantName;
                        }
                    }
                }

                LogString(EdLogLevel.Error, "executeIdentJob failed");
            }
            finally
            {
                _jobStd = jobStdOld;
                JobRunning = jobRunningOld;
            }
            return string.Empty;
        }

        private void ExecuteJobPrivate(string jobName)
        {
            ExecuteJobPrivate(jobName, false);
        }

        private void ExecuteJobPrivate(string jobName, bool recursive)
        {
            LogFormat(EdLogLevel.Ifh, "executeJob({0}): {1}", SgbdFileName, jobName);

            if (_closeSgbdFs)
            {
                _closeSgbdFs = false;
                CloseSgbdFs();
            }
            if (!OpenSgbdFs())
            {
                throw new ArgumentOutOfRangeException("OpenSgbdFs", "ExecuteJobPrivate: Open SGBD failed");
            }
            JobInfo jobInfo = GetJobInfo(jobName);
            if (jobInfo == null)
            {
                foreach (VJobInfo vJobInfo in VJobList)
                {
                    if (string.Compare(vJobInfo.JobName, jobName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ExecuteVJob(_sgbdFs, vJobInfo);
                        LogString(EdLogLevel.Info, "executeJob successfull");
                        return;
                    }
                }
                SetError(ErrorCodes.EDIABAS_SYS_0008);
                return;
            }

            if (jobInfo.UsesInfo != null)
            {
                string fileName = Path.Combine(EcuPath, jobInfo.UsesInfo.Name.ToLower(Culture) + ".prg");
                try
                {
                    using (Stream tempFs = MemoryStreamReader.OpenRead(fileName))
                    {
                        _sgbdBaseFs = tempFs;
                        try
                        {
                            ExecuteJobPrivate(tempFs, jobInfo, recursive);
                        }
                        catch (Exception ex)
                        {
                            LogString(EdLogLevel.Error, "executeJob base job exception: " + GetExceptionText(ex));
                            throw new Exception("executeJob base job exception", ex);
                        }
                    }
                }
                catch (Exception)
                {
                    LogString(EdLogLevel.Error, "ExecuteJobPrivate file not found: " + fileName);
                    throw new ArgumentOutOfRangeException("fileName", "ExecuteJobPrivate: SGBD not found: " + fileName);
                }
                finally
                {
                    CloseTableFs();
                    _sgbdBaseFs = null;
                }
            }
            else
            {
                ExecuteJobPrivate(_sgbdFs, jobInfo, recursive);
            }
            LogString(EdLogLevel.Info, "executeJob successfull");
        }

        public void ExecuteJob(string jobName)
        {
            if (JobRunning)
            {
                throw new ArgumentOutOfRangeException("JobRunning", "ExecuteJob: Job is running");
            }
            try
            {
                JobRunning = true;
                _jobStd = false;
                _jobStdExit = false;
                ExecuteJobPrivate(jobName);
            }
            finally
            {
                _argInfo.BinData = null;
                _argInfoStd.BinData = null;
                _jobStd = false;
                _jobStdExit = false;
                JobRunning = false;
            }
        }

        private void ExecuteJobPrivate(Stream fs, JobInfo jobInfo, bool recursive)
        {
            if (_requestInit && !recursive)
            {
                ExecuteInitJob();
            }

            byte[] buffer = new byte[2];

            _resultSetsTemp = new List<Dictionary<string, ResultData>>();
            lock (_apiLock)
            {
                _resultSets = null;
            }
            _resultDict.Clear();
            _resultSysDict.Clear();
            _stackList.Clear();
            SetConfigProperty("BipEcuFile", Path.GetFileNameWithoutExtension(SgbdFileName));
            _flags.Init();
            foreach (StringData stringData in _stringRegisters)
            {
                stringData.ClearData();
            }
            _errorTrapBitNr = -1;
            _errorTrapMask = 0;
            _errorCodeLast = ErrorCodes.EDIABAS_ERR_NONE;
            _infoProgressRange = -1;
            _infoProgressPos = -1;
            _infoProgressText = string.Empty;
            _resultJobStatus = string.Empty;

            ArrayMaxBufSize = jobInfo.ArraySize;
            _pcCounter = jobInfo.JobOffset;
            CloseTableFs();
            CloseAllUserFiles();
            _jobEnd = false;

            Operand arg0 = new Operand(this);
            Operand arg1 = new Operand(this);
            try
            {
                while (!_jobEnd)
                {
                    //long startTime = Stopwatch.GetTimestamp();
                    EdValueType pcCounterOld = _pcCounter;
                    fs.Position = _pcCounter;
                    ReadAndDecryptBytes(fs, buffer, 0, buffer.Length);

                    byte opCodeVal = buffer[0];
                    byte opAddrMode = buffer[1];
                    OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
                    OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

                    if (opCodeVal >= OcList.Length)
                    {
                        throw new ArgumentOutOfRangeException("opCodeVal", "executeJob: Opcode out of range");
                    }
                    OpCode oc = OcList[opCodeVal];
                    GetOpArg(fs, opAddrMode0, ref arg0);
                    GetOpArg(fs, opAddrMode1, ref arg1);
                    _pcCounter = (EdValueType)fs.Position;

                    //special near address arg0 opcode handling mainly for jumps
                    if (oc.Arg0IsNearAddress && (opAddrMode0 == OpAddrMode.Imm32))
                    {
                        EdValueType labelAddress = _pcCounter + (EdValueType)arg0.OpData1;
                        arg0.OpData1 = labelAddress;
                    }

                    AbortJobDelegate abortFunc = AbortJobFunc;
                    if (abortFunc != null)
                    {
                        if (abortFunc())
                        {
                            throw new Exception("executeJob aborted");
                        }
                    }
                    //timeMeas += Stopwatch.GetTimestamp() - startTime;

                    if (oc.OpFunc != null)
                    {
                        if ((EdLogLevel)_logLevelCached >= EdLogLevel.Info)
                        {
                            LogString(EdLogLevel.Info, ">" + GetOpText(pcCounterOld, oc, arg0, arg1));
                        }
                        oc.OpFunc(this, oc, arg0, arg1);
                        if ((EdLogLevel)_logLevelCached >= EdLogLevel.Info)
                        {
                            LogString(EdLogLevel.Info, "<" + GetOpText(_pcCounter, oc, arg0, arg1));
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(oc.Pneumonic, "executeJob: Function not implemented");
                    }
                }
                if (_resultDict.Count > 0)
                {
                    _resultSetsTemp.Add(new Dictionary<string, ResultData>(_resultDict));
                }
                _resultDict.Clear();
            }
            catch (Exception ex)
            {
                LogString(EdLogLevel.Error, "executeJob Exception: " + GetExceptionText(ex));
                throw new Exception("executeJob", ex);
            }
            finally
            {
                CloseTableFs();
                CloseAllUserFiles();
                Dictionary<string, ResultData> systemResultDict = CreateSystemResultDict(jobInfo, _resultSetsTemp.Count);

                _resultSetsTemp.Insert(0, systemResultDict);
                lock (_apiLock)
                {
                    _resultSets = _resultSetsTemp;
                }
                SetConfigProperty("BipEcuFile", null);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void ExecuteVJob(Stream fs, VJobInfo vJobInfo)
        {
            if (_requestInit && !NoInitForVJobs)
            {
                ExecuteInitJob();
            }

            _resultSetsTemp = new List<Dictionary<string, ResultData>>();
            lock (_apiLock)
            {
                _resultSets = null;
            }
            SetConfigProperty("BipEcuFile", Path.GetFileNameWithoutExtension(SgbdFileName));

            try
            {
                vJobInfo.JobDelegate(this, _resultSetsTemp);
            }
            catch (Exception ex)
            {
                LogString(EdLogLevel.Error, "executeVJob Exception: " + GetExceptionText(ex));
                throw new Exception("executeVJob", ex);
            }
            finally
            {
                Dictionary<string, ResultData> systemResultDict = CreateSystemResultDict(new JobInfo(vJobInfo.JobName, 0, 0, 0, null), _resultSetsTemp.Count);

                _resultSetsTemp.Insert(0, systemResultDict);
                lock (_apiLock)
                {
                    _resultSets = _resultSetsTemp;
                }
                SetConfigProperty("BipEcuFile", null);
            }
        }

        private static void VJobJobs(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            const string entryJobName = "JOBNAME";

            List<string> argStrings = ediabas.GetActiveArgStrings();
            // the ALL argument is an EdiabasLib extensions
            bool allJobs = (argStrings.Count >= 1) && (string.Compare(argStrings[0], "ALL", StringComparison.OrdinalIgnoreCase) == 0);
            Dictionary<string, bool> jobNameDict = new Dictionary<string, bool>();
            foreach (JobInfo jobInfo in ediabas._jobInfos.JobInfoArray)
            {
                string key = jobInfo.JobName.ToUpper(Culture);
                if ((allJobs && !jobNameDict.ContainsKey(key)) || (jobInfo.UsesInfo == null))
                {
                    Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>
                    {
                        {entryJobName, new ResultData(ResultType.TypeS, entryJobName, jobInfo.JobName)}
                    };
                    if (resultDict.Count > 0)
                    {
                        resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                    }
                    jobNameDict.Add(key, true);
                }
            }
        }

        private static void VJobJobComments(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            Stream fs = ediabas._sgbdFs;

            List<string> argStrings = ediabas.GetActiveArgStrings();
            if (argStrings.Count < 1)
            {
                return;
            }

            if (ediabas._descriptionInfos == null)
            {
                ediabas._descriptionInfos = ediabas.ReadDescriptions(fs);
            }

            if (ediabas._descriptionInfos.JobComments == null)
            {
                return;
            }
            List<string> jobComments;
            if (!ediabas._descriptionInfos.JobComments.TryGetValue(argStrings[0].ToUpper(Culture), out jobComments))
            {
                return;
            }
            Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>();
            int commentCount = 0;
            foreach (string desc in jobComments)
            {
                int colon = desc.IndexOf(':');
                if (colon >= 0)
                {
                    string key = desc.Substring(0, colon);
                    string value = desc.Substring(colon + 1);
                    if (string.Compare(key, "JOBCOMMENT", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        key += commentCount.ToString(Culture);
                        commentCount++;
                        if (!resultDict.ContainsKey(key))
                        {
                            resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                        }
                    }
                }
            }
            if (resultDict.Count > 0)
            {
                resultSets.Add(new Dictionary<string, ResultData>(resultDict));
            }
        }

        private static void VJobJobArgs(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            Stream fs = ediabas._sgbdFs;

            List<string> argStrings = ediabas.GetActiveArgStrings();
            if (argStrings.Count < 1)
            {
                return;
            }

            if (ediabas._descriptionInfos == null)
            {
                ediabas._descriptionInfos = ediabas.ReadDescriptions(fs);
            }

            if (ediabas._descriptionInfos.JobComments == null)
            {
                return;
            }
            List<string> jobComments;
            if (!ediabas._descriptionInfos.JobComments.TryGetValue(argStrings[0].ToUpper(Culture), out jobComments))
            {
                return;
            }
            Dictionary<string, ResultData> resultDict = null;
            int commentCount = 0;
            foreach (string desc in jobComments)
            {
                int colon = desc.IndexOf(':');
                if (colon >= 0)
                {
                    string key = desc.Substring(0, colon);
                    string value = desc.Substring(colon + 1);

                    if (string.Compare(key, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (resultDict != null && resultDict.Count > 0)
                        {
                            resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                        }
                        resultDict = new Dictionary<string, ResultData>();
                        commentCount = 0;
                    }

                    if (resultDict != null)
                    {
                        if (string.Compare(key, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                        if (string.Compare(key, "ARGTYPE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                        if (string.Compare(key, "ARGCOMMENT", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            key += commentCount.ToString(Culture);
                            commentCount++;
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                    }
                }
            }
            if (resultDict != null && resultDict.Count > 0)
            {
                resultSets.Add(new Dictionary<string, ResultData>(resultDict));
            }
        }

        private static void VJobJobResults(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            Stream fs = ediabas._sgbdFs;

            List<string> argStrings = ediabas.GetActiveArgStrings();
            if (argStrings.Count < 1)
            {
                return;
            }

            if (ediabas._descriptionInfos == null)
            {
                ediabas._descriptionInfos = ediabas.ReadDescriptions(fs);
            }

            if (ediabas._descriptionInfos.JobComments == null)
            {
                return;
            }
            List<string> jobComments;
            if (!ediabas._descriptionInfos.JobComments.TryGetValue(argStrings[0].ToUpper(Culture), out jobComments))
            {
                return;
            }
            Dictionary<string, ResultData> resultDict = null;
            int commentCount = 0;
            foreach (string desc in jobComments)
            {
                int colon = desc.IndexOf(':');
                if (colon >= 0)
                {
                    string key = desc.Substring(0, colon);
                    string value = desc.Substring(colon + 1);

                    if (string.Compare(key, "RESULT", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (resultDict != null && resultDict.Count > 0)
                        {
                            resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                        }
                        resultDict = new Dictionary<string, ResultData>();
                        commentCount = 0;
                    }

                    if (resultDict != null)
                    {
                        if (string.Compare(key, "RESULT", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                        if (string.Compare(key, "RESULTTYPE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                        if (string.Compare(key, "RESULTCOMMENT", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            key += commentCount.ToString(Culture);
                            commentCount++;
                            if (!resultDict.ContainsKey(key))
                            {
                                resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                            }
                        }
                    }
                }
            }
            if (resultDict != null && resultDict.Count > 0)
            {
                resultSets.Add(new Dictionary<string, ResultData>(resultDict));
            }
        }

        private static void VJobVerinfos(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            Stream fs = ediabas._sgbdFs;

            if (ediabas._descriptionInfos == null)
            {
                ediabas._descriptionInfos = ediabas.ReadDescriptions(fs);
            }

            byte[] buffer = new byte[4];
            fs.Position = 0x94;
            fs.Read(buffer, 0, buffer.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            Int32 infoOffset = BitConverter.ToInt32(buffer, 0);
            if (infoOffset < 0)
            {
                return;
            }
            fs.Position = infoOffset;

            byte[] infoBuffer = new byte[0x6C];
            ReadAndDecryptBytes(fs, infoBuffer, 0, infoBuffer.Length);

            Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>();

            const string entryBipVersion = "BIP_VERSION";
            resultDict.Add(entryBipVersion, new ResultData(ResultType.TypeS, entryBipVersion, string.Format("{0}.{1}.{2}", infoBuffer[2], infoBuffer[1], infoBuffer[0])));

            const string entryAuthor = "AUTHOR";
            resultDict.Add(entryAuthor, new ResultData(ResultType.TypeS, entryAuthor, Encoding.GetString(infoBuffer, 0x08, 0x40).TrimEnd('\0')));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0x04, 2);
            }
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0x06, 2);
            }
            const string entryRevision = "REVISION";
            resultDict.Add(entryRevision, new ResultData(ResultType.TypeS, entryRevision, string.Format("{0}.{1}", BitConverter.ToInt16(infoBuffer, 0x06), BitConverter.ToInt16(infoBuffer, 0x04))));

            const string entryFrom = "FROM";
            resultDict.Add(entryFrom, new ResultData(ResultType.TypeS, entryFrom, Encoding.GetString(infoBuffer, 0x48, 0x20).TrimEnd('\0')));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0x68, 2);
            }
            const string entryPackage = "PACKAGE";
            resultDict.Add(entryPackage, new ResultData(ResultType.TypeL, entryPackage, (Int64)BitConverter.ToInt32(infoBuffer, 0x68)));

            if (ediabas._descriptionInfos.GlobalComments != null)
            {
                int descCount = 0;
                int usesCount = 0;
                foreach (string desc in ediabas._descriptionInfos.GlobalComments)
                {
                    int colon = desc.IndexOf(':');
                    if (colon >= 0)
                    {
                        string key = desc.Substring(0, colon);
                        string value = desc.Substring(colon + 1);
                        if (string.Compare(key, "ECUCOMMENT", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            key += descCount.ToString(Culture);
                            descCount++;
                        }
                        if (string.Compare(key, "USES", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            key += usesCount.ToString(Culture);
                            usesCount++;
                        }

                        if (!resultDict.ContainsKey(key))
                        {
                            resultDict.Add(key, new ResultData(ResultType.TypeS, key, value));
                        }
                    }
                }
            }
            if (resultDict.Count > 0)
            {
                resultSets.Add(new Dictionary<string, ResultData>(resultDict));
            }
        }

        private static void VJobTables(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            const string entryTableName = "TABLE";
            foreach (TableInfo tableInfo in ediabas._tableInfos.TableInfoArray)
            {
                Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>
                {
                    {entryTableName, new ResultData(ResultType.TypeS, entryTableName, tableInfo.Name)}
                };
                if (resultDict.Count > 0)
                {
                    resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                }
            }
        }

        private static void VJobTable(EdiabasNet ediabas, List<Dictionary<string, ResultData>> resultSets)
        {
            List<string> argStrings = ediabas.GetActiveArgStrings();
            if (argStrings.Count < 1)
            {
                return;
            }

            UInt32 tableIdx;
            if (!ediabas._tableInfos.TableNameDict.TryGetValue(argStrings[0].ToUpper(Culture), out tableIdx))
            {
                return;
            }

            TableInfo tableInfo = ediabas._tableInfos.TableInfoArray[tableIdx];
            ediabas.IndexTable(ediabas._sgbdFs, tableInfo);
            foreach (uint[] entries in tableInfo.TableEntries)
            {
                Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>();
                for (int j = 0; j < tableInfo.Columns; j++)
                {
                    string rowStr = ediabas.GetTableString(ediabas._sgbdFs, entries[j]);
                    string entryName = "COLUMN" + j.ToString(Culture);
                    resultDict.Add(entryName, new ResultData(ResultType.TypeS, entryName, rowStr));
                }
                if (resultDict.Count > 0)
                {
                    resultSets.Add(new Dictionary<string, ResultData>(resultDict));
                }
            }
        }

        public List<List<string>> GetTableLines(string tableName)
        {
            UInt32 tableIdx;
            if (!_tableInfos.TableNameDict.TryGetValue(tableName.ToUpper(Culture), out tableIdx))
            {
                return null;
            }

            List<List<string>> tableList = new List<List<string>>();
            TableInfo tableInfo = _tableInfos.TableInfoArray[tableIdx];
            IndexTable(_sgbdFs, tableInfo);
            foreach (uint[] entries in tableInfo.TableEntries)
            {
                List<string> rowList = new List<string>();
                for (int j = 0; j < tableInfo.Columns; j++)
                {
                    string rowStr = GetTableString(_sgbdFs, entries[j]);
                    rowList.Add(rowStr);
                }
                
                tableList.Add(rowList);
            }

            return tableList;
        }

        public List<string> GetTableColumn(List<List<string>> tableLines, string columnName)
        {
            if (tableLines == null || tableLines.Count < 1)
            {
                return null;
            }

            int columnIndex = -1;
            int index = 0;
            foreach (string name in tableLines[0])
            {
                if (string.Compare(name, columnName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    columnIndex = index;
                    break;
                }

                index++;
            }

            if (columnIndex < 0)
            {
                return null;
            }

            List<string> columnList = new List<string>();
            index = 0;
            foreach (List<string> line in tableLines)
            {
                if (index > 0 && line.Count >= columnIndex)
                {
                    columnList.Add(line[columnIndex]);
                }

                index++;
            }

            return columnList;
        }

        public void LogFormat(EdLogLevel logLevel, string format, params object[] args)
        {
            UpdateLogLevel();
            if ((int)logLevel > _logLevelCached)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                if (args[i] is string)
                {
                    args[i] = "'" + (string)args[i] + "'";
                }
                if (args[i] is byte[])
                {
                    byte[] argArray = (byte[])args[i];
                    StringBuilder stringBuilder = new StringBuilder(argArray.Length);
                    foreach (byte arg in argArray)
                    {
                        stringBuilder.Append(string.Format(Culture, "{0:X02} ", arg));
                    }

                    args[i] = "[" + stringBuilder + "]";
                }
            }
            LogString(logLevel, string.Format(Culture, format, args));
        }

        public void LogString(EdLogLevel logLevel, string info)
        {
            UpdateLogLevel();
            if ((int)logLevel > _logLevelCached)
            {
                return;
            }

            try
            {
                lock (_logLock)
                {
                    bool newFile = false;
                    if (_swLog == null)
                    {
                        string tracePath = GetConfigProperty("TracePath");
                        if (tracePath != null)
                        {
                            Directory.CreateDirectory(tracePath);

                            int appendTrace = 0;
                            string propAppend = GetConfigProperty("AppendTrace");
                            if (propAppend != null)
                            {
                                appendTrace = (int)StringToValue(propAppend);
                            }

                            string traceFileName = "ifh.trc";
                            string propName = GetConfigProperty("IfhTraceName");
                            if (!string.IsNullOrEmpty(propName))
                            {
                                traceFileName = propName;
                            }

                            string traceBuffering = GetConfigProperty("IfhTraceBuffering");
                            Int64 buffering = 0;
                            if (traceBuffering != null)
                            {
                                buffering = StringToValue(traceBuffering);
                            }
#if COMPRESS_TRACE
                            int compressTrace = 0;
                            string propCompress = GetConfigProperty("CompressTrace");
                            if (propCompress != null)
                            {
                                compressTrace = (int)StringToValue(propCompress);
                            }
                            if (compressTrace != 0)
                            {
                                if (_zipStream == null)
                                {
                                    string zipFileName = Path.Combine(tracePath, traceFileName + ".zip");
                                    string zipFileNameOld = Path.Combine(tracePath, traceFileName + ".old.zip");
                                    bool appendZip = (!_firstLog || appendTrace != 0) && File.Exists(zipFileName);
                                    if (appendZip && appendTrace == 0)
                                    {
                                        FileInfo fileInfo = new FileInfo(zipFileName);
                                        if (fileInfo.Length > 10000)
                                        {   // limit appended size
                                            appendZip = false;
                                        }
                                    }
                                    if (appendZip)
                                    {
                                        if (File.Exists(zipFileNameOld))
                                        {
                                            File.Delete(zipFileNameOld);
                                        }
                                        File.Move(zipFileName, zipFileNameOld);
                                    }
                                    FileStream fsOut = File.Create(zipFileName);
                                    _zipStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(fsOut);
                                    _zipStream.SetLevel(8);
                                    ICSharpCode.SharpZipLib.Zip.ZipEntry newEntry =
                                        new ICSharpCode.SharpZipLib.Zip.ZipEntry(traceFileName);
                                    _zipStream.UseZip64 = ICSharpCode.SharpZipLib.Zip.UseZip64.Off;
                                    _zipStream.PutNextEntry(newEntry);

                                    // copy old zip content to new one
                                    if (appendZip)
                                    {
                                        FileStream fs = File.OpenRead(zipFileNameOld);
                                        ICSharpCode.SharpZipLib.Zip.ZipFile zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs);
                                        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry in zf)
                                        {
                                            if (zipEntry.IsFile && string.Compare(zipEntry.Name, traceFileName, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zf.GetInputStream(zipEntry), _zipStream, new byte[4096]);
                                                break;
                                            }
                                        }
                                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                                        zf.Close(); // Ensure we release resources
                                        File.Delete(zipFileNameOld);
                                    }
                                }

                                newFile = true;
                                _swLog = new StreamWriter(_zipStream, Encoding, 1024, true);
                            }
                            else
#endif
                            {
                                string traceFile = Path.Combine(tracePath, traceFileName);
                                try
                                {
                                    if (appendTrace != 0 && File.Exists(traceFile))
                                    {
                                        DateTime lastWriteTime = File.GetLastWriteTime(traceFile);
                                        TimeSpan diffTime = DateTime.Now - lastWriteTime;
                                        if (diffTime.Hours > TraceAppendDiffHours)
                                        {
                                            appendTrace = 0;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }

                                FileMode fileMode = FileMode.Append;
                                if (_firstLog && appendTrace == 0)
                                {
                                    fileMode = FileMode.Create;
                                }

                                newFile = true;
                                _swLog = new StreamWriter(
                                    new FileStream(traceFile, fileMode, FileAccess.Write, FileShare.ReadWrite), Encoding)
                                    {
                                        AutoFlush = buffering == 0
                                    };
                            }
                            _firstLog = false;
                        }
                    }

                    if (_swLog != null)
                    {
                        if (newFile)
                        {
                            string currDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            _swLog.WriteLine(string.Format(CultureInfo.InvariantCulture, "Date: {0}", currDateTime));
                        }
                        _swLog.WriteLine(info);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void LogData(EdLogLevel logLevel, byte[] data, int offset, int length, string info)
        {
            UpdateLogLevel();
            if ((int)logLevel > _logLevelCached)
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(string.Format(Culture, "{0:X02} ", data[offset + i]));
            }

            LogString(logLevel, " (" + info + "): " + stringBuilder);
        }

        public void LogData(EdLogLevel logLevel, UInt32[] data, int offset, int length, string info)
        {
            UpdateLogLevel();
            if ((int)logLevel > _logLevelCached)
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(string.Format(Culture, "{0:X08} ", data[offset + i]));
            }

            LogString(logLevel, " (" + info + "): " + stringBuilder);
        }

        private void CloseLog()
        {
            lock (_logLock)
            {
                if (_swLog != null)
                {
                    try
                    {
                        _swLog.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    _swLog = null;
                }
#if COMPRESS_TRACE
                if (_zipStream != null)
                {
                    try
                    {
                        _zipStream.CloseEntry();
                        _zipStream.IsStreamOwner = true;
                        _zipStream.Close();
                        _zipStream.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    _zipStream = null;
                }
#endif
                _logLevelCached = -1;
            }
        }

        private void UpdateLogLevel()
        {
            if (_logLevelCached < 0)
            {
                lock (_logLock)
                {
                    string ifhTrace = GetConfigProperty("IfhTrace");
                    _logLevelCached = (int)StringToValue(ifhTrace);
                }
            }
        }

        private void GetOpArg(Stream fs, OpAddrMode opAddrMode, ref Operand oper)
        {
            try
            {
                switch (opAddrMode)
                {
                    case OpAddrMode.None:
                        oper.Init(opAddrMode);
                        return;

                    case OpAddrMode.RegS:
                    case OpAddrMode.RegAb:
                    case OpAddrMode.RegI:
                    case OpAddrMode.RegL:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 1);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            oper.Init(opAddrMode, oaReg);
                            return;
                        }
                    case OpAddrMode.Imm8:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 1);
                            oper.Init(opAddrMode, (EdValueType)_opArgBuffer[0]);
                            // string.Format(culture, "#${0:X}.B", buffer[0]);
                            return;
                        }
                    case OpAddrMode.Imm16:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 2);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 0, 2);
                            }
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt16(_opArgBuffer, 0));
                            // string.Format(culture, "#${0:X}.I", BitConverter.ToInt16(buffer, 0));
                            return;
                        }
                    case OpAddrMode.Imm32:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 4);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 0, 4);
                            }
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt32(_opArgBuffer, 0));
                            // string.Format(culture, "#${0:X}.L", BitConverter.ToInt32(buffer, 0));
                            return;
                        }
                    case OpAddrMode.ImmStr:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 2);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 0, 2);
                            }
                            short slen = BitConverter.ToInt16(_opArgBuffer, 0);
                            byte[] buffer = new byte[slen];
                            ReadAndDecryptBytes(fs, buffer, 0, slen);
                            oper.Init(opAddrMode, buffer);
                            return;
                        }
                    case OpAddrMode.IdxImm:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 3);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(_opArgBuffer, 1);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx);
                            // string.Format(culture, "{0}[#${1:X}]", oaReg.name, idx);
                            return;
                        }
                    case OpAddrMode.IdxReg:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 2);
                            Register oaReg0 = GetRegister(_opArgBuffer[0]);
                            Register oaReg1 = GetRegister(_opArgBuffer[1]);
                            oper.Init(opAddrMode, oaReg0, oaReg1);
                            // string.Format(culture, "{0}[{1}]", oaReg0.name, oaReg1.name);
                            return;
                        }
                    case OpAddrMode.IdxRegImm:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 4);
                            Register oaReg0 = GetRegister(_opArgBuffer[0]);
                            Register oaReg1 = GetRegister(_opArgBuffer[1]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 2, 2);
                            }
                            EdValueType inc = BitConverter.ToUInt16(_opArgBuffer, 2);
                            oper.Init(opAddrMode, oaReg0, oaReg1, (EdValueType)inc);
                            // string.Format(culture, "{0}[{1},#${2:X}]", oaReg0.name, oaReg1.name, inc);
                            return;
                        }
                    case OpAddrMode.IdxImmLenImm:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 5);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(_opArgBuffer, 1);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 3, 2);
                            }
                            EdValueType len = BitConverter.ToUInt16(_opArgBuffer, 3);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx, (EdValueType)len);
                            // string.Format(culture, "{0}[#${1:X}]#${2:X}", oaReg.name, idx, len);
                            return;
                        }
                    case OpAddrMode.IdxImmLenReg:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 4);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 1, 2);
                            }
                            EdValueType idx = BitConverter.ToUInt16(_opArgBuffer, 1);
                            Register oaLen = GetRegister(_opArgBuffer[3]);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx, oaLen);
                            // string.Format(culture, "{0}[#${1:X}]{2}", oaReg.name, idx, oaLen.name);
                            return;
                        }
                    case OpAddrMode.IdxRegLenImm:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 4);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            Register oaIdx = GetRegister(_opArgBuffer[1]);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(_opArgBuffer, 2, 2);
                            }
                            EdValueType len = BitConverter.ToUInt16(_opArgBuffer, 2);
                            oper.Init(opAddrMode, oaReg, oaIdx, (EdValueType)len);
                            // string.Format(culture, "{0}[{1}]#${2:X}", oaReg.name, oaIdx.name, len);
                            return;
                        }
                    case OpAddrMode.IdxRegLenReg:
                        {
                            ReadAndDecryptBytes(fs, _opArgBuffer, 0, 3);
                            Register oaReg = GetRegister(_opArgBuffer[0]);
                            Register oaIdx = GetRegister(_opArgBuffer[1]);
                            Register oaLen = GetRegister(_opArgBuffer[2]);
                            oper.Init(opAddrMode, oaReg, oaIdx, oaLen);
                            // string.Format(culture, "{0}[{1}]{2}", oaReg.name, oaIdx.name, oaLen.name);
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
                result = RegisterList[opcode];
            }
            else if (opcode >= 0x80)
            {
                int index = opcode - 0x80 + 0x34;
                if (index >= RegisterList.Length)
                {
                    throw new ArgumentOutOfRangeException("opcode", "GetRegister: Opcode out of range");
                }
                result = RegisterList[index];
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

        private static string GetOpText(EdValueType addr, OpCode oc, Operand arg0, Operand arg1)
        {
            string result = string.Format(Culture, "{0:X08}: ", addr);
            result += oc.Pneumonic;
            string text1 = GetOpArgText(arg0);
            string text2 = GetOpArgText(arg1);
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

        private static string GetOpArgText(Operand arg)
        {
            try
            {
                string regName1 = string.Empty;
                if (arg.OpData1 != null && arg.OpData1.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.OpData1;
                    regName1 = argValue.GetName();
                }

                string regName2 = string.Empty;
                if (arg.OpData2 != null && arg.OpData2.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.OpData2;
                    regName2 = argValue.GetName();
                }

                string regName3 = string.Empty;
                if (arg.OpData3 != null && arg.OpData3.GetType() == typeof(Register))
                {
                    Register argValue = (Register)arg.OpData3;
                    regName3 = argValue.GetName();
                }

                switch (arg.AddrMode)
                {
                    case OpAddrMode.None:
                        return string.Empty;

                    case OpAddrMode.RegS:
                        {
                            Object data = arg.GetRawData();
                            if (data is byte[])
                            {
                                return regName1 + ": " + GetStringText(arg.GetArrayData());
                            }
                            else if (data is EdFloatType)
                            {
                                return regName1 + string.Format(Culture, ": {0}", (EdFloatType)data);
                            }
                            return string.Empty;
                        }

                    case OpAddrMode.RegAb:
                        return regName1 + string.Format(Culture, ": ${0:X}", arg.GetValueData());

                    case OpAddrMode.RegI:
                        return regName1 + string.Format(Culture, ": ${0:X}", arg.GetValueData());

                    case OpAddrMode.RegL:
                        return regName1 + string.Format(Culture, ": ${0:X}", arg.GetValueData());

                    case OpAddrMode.Imm8:
                        return string.Format(Culture, "#${0:X}.B", arg.GetValueData());

                    case OpAddrMode.Imm16:
                        return string.Format(Culture, "#${0:X}.I", arg.GetValueData());

                    case OpAddrMode.Imm32:
                        return string.Format(Culture, "#${0:X}.L", arg.GetValueData());

                    case OpAddrMode.ImmStr:
                        return "#" + GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImm:
                        if (arg.OpData2 == null) return String.Empty;
                        return regName1 + string.Format(Culture, "[#${0:X}]: ", (EdValueType)arg.OpData2) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxReg:
                        if (arg.OpData2 == null) return String.Empty;
                        return regName1 + string.Format(Culture, "[{0}: ${1:X}]: ", regName2, ((Register)arg.OpData2).GetValueData()) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegImm:
                        if ((arg.OpData2 == null) || (arg.OpData3 == null)) return String.Empty;
                        return regName1 + string.Format(Culture, "[{0}: ${1:X},#${2:X}]: ", regName2, ((Register)arg.OpData2).GetValueData(), (EdValueType)arg.OpData3) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImmLenImm:
                        if ((arg.OpData2 == null) || (arg.OpData3 == null)) return String.Empty;
                        return regName1 + string.Format(Culture, "[#{0:X}],#${1:X}: ", (EdValueType)arg.OpData2, (EdValueType)arg.OpData3) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxImmLenReg:
                        if ((arg.OpData2 == null) || (arg.OpData3 == null)) return String.Empty;
                        return regName1 + string.Format(Culture, "[#{0:X}],{1}: #${2:X}: ", (EdValueType)arg.OpData2, regName3, ((Register)arg.OpData3).GetValueData()) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegLenImm:
                        if ((arg.OpData2 == null) || (arg.OpData3 == null)) return String.Empty;
                        return regName1 + string.Format(Culture, "[{0}: ${1:X}],#${2:X}: ", regName2, ((Register)arg.OpData2).GetValueData(), (EdValueType)arg.OpData3) +
                            GetStringText(arg.GetArrayData());

                    case OpAddrMode.IdxRegLenReg:
                        if ((arg.OpData2 == null) || (arg.OpData3 == null)) return String.Empty;
                        return regName1 + string.Format(Culture, "[{0}: ${1:X}],{2}: #${3:X}: ", regName2, ((Register)arg.OpData2).GetValueData(), regName3, ((Register)arg.OpData3).GetValueData()) +
                            GetStringText(arg.GetArrayData());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("opAddrMode", ex);
            }
            return string.Empty;
        }

        private static string GetStringText(byte[] dataArray)
        {
            bool printable = true;
            int length = dataArray.Length;
            if (length < 1)
            {
                return "{ }";
            }
            for (int i = 0; i < length - 1; ++i)
                if (!IsPrintable(dataArray[i]))
                {
                    printable = false;
                    break;
                }

            if (printable && (dataArray[length - 1] == 0))
                return "\"" + Encoding.GetString(dataArray, 0, length - 1) + "\"";

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < length; i++)
                sb.AppendFormat("${0:X}.B,", dataArray[i]);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("}");
            return sb.ToString();
        }

        private static bool IsPrintable(byte b)
        {
            return ((b == 9) || (b == 10) || (b == 13) || ((b >= 32) && (b < 127)));
        }

        private static void ReadAndDecryptBytes(Stream fs, byte[] buffer, int offset, int count)
        {
            fs.Read(buffer, offset, count);
            for (int i = offset; i < (offset + count); ++i)
                buffer[i] ^= 0xF7;
        }

        private static int ReadInt32(Stream fs)
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
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * Math.Round(value / scale, (int) digits);
        }

        public static Int64 StringToValue(string number)
        {
            bool valid;
            return StringToValue(number, out valid);
        }

        public static Int64 StringToValue(string number, out bool valid)
        {
            Int64 value = 0;
            valid = false;
            string numberLocal = number.TrimEnd();
            if (numberLocal.Length == 0)
            {
                return value;
            }
            string numberLower = numberLocal.ToLower(Culture);
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
                            valid = true;
                        }
                    }
                }
                else if (numberLower.StartsWith("0y", StringComparison.Ordinal))
                {   // binary
                    value = Convert.ToInt64(numberLocal.Substring(2, numberLocal.Length - 2), 2);
                    valid = true;
                }
                else
                {   // dec
                    if ((string.Compare(numberLower, "-", StringComparison.Ordinal) != 0) &&
                        (string.Compare(numberLower, "--", StringComparison.Ordinal) != 0))
                    {
                        if (!Char.IsLetter(numberLower[0]))
                        {
                            string numberConv = numberLocal.TrimStart();
                            int index = numberConv.IndexOfAny(new [] {'.', ','});
                            if (index >= 0)
                            {
                                numberConv = numberConv.Substring(0, index);
                            }
                            value = Convert.ToInt64(numberConv, 10);
                            valid = true;
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

        public static EdFloatType StringToFloat(string number)
        {
            bool valid;
            return StringToFloat(number, out valid);
        }

        public static EdFloatType StringToFloat(string number, out bool valid)
        {
            EdFloatType result = 0;
            valid = false;
            try
            {
                byte[] numberArray = Encoding.ASCII.GetBytes(number);
                if (string.Compare(number, Encoding.ASCII.GetString(numberArray, 0, numberArray.Length), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    number = number.Replace(",", ".");
                    result = EdFloatType.Parse(number, Culture);
                    valid = true;
                }
            }
            catch (Exception)
            {
                result = 0;
            }
            return result;
        }

        public static byte[] HexToByteArray(string valueStr)
        {
            byte[] result;
            try
            {
                int length = valueStr.Length - valueStr.Length % 2;
                result = Enumerable.Range(0, length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(valueStr.Substring(x, 2), 16))
                 .ToArray();
            }
            catch (Exception)
            {
                result = ByteArray0;
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
                result += string.Format(Culture, "{0:X}", part1);
            }

            if (part2 > 9)
            {
                result += "*";
            }
            else
            {
                result += string.Format(Culture, "{0:X}", part2);
            }
            return result;
        }

        // ReSharper disable once UnusedParameter.Local
        private static EdValueType GetArgsValueLength(Operand arg0, Operand arg1)
        {
            return arg0.GetDataLen(true);
        }
    }
}
