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

        public static readonly int MAX_ARRAY_LENGTH = 1024;

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
            public Operand()
                : this(OpAddrMode.None, null, null, null)
            {
            }

            public Operand(OpAddrMode opAddrMode)
                : this(opAddrMode, null, null, null)
            {
            }

            public Operand(OpAddrMode opAddrMode, Object opData1)
                : this(opAddrMode, opData1, null, null)
            {
            }

            public Operand(OpAddrMode opAddrMode, Object opData1, Object opData2)
                : this(opAddrMode, opData1, opData2, null)
            {
            }

            public Operand(OpAddrMode opAddrMode, Object opData1, Object opData2, Object opData3)
            {
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
                            if (requiredLength > MAX_ARRAY_LENGTH)
                            {
                                throw new ArgumentOutOfRangeException("index", "Operand.GetRawData IdxX: Index out of range");
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                return new byte[0];
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
                            if (requiredLength > MAX_ARRAY_LENGTH)
                            {
                                throw new ArgumentOutOfRangeException("requiredLength", "Operand.GetRawData IdxXLenX: Index out of range");
                            }
                            if (dataArray.Length < requiredLength)
                            {
                                if (dataArray.Length <= index)
                                {
                                    return new byte[0];
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
                            if (requiredLength > MAX_ARRAY_LENGTH)
                            {
                                throw new ArgumentOutOfRangeException("index", "Operand.SetRawData IdxX: Length out of range");
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
                byte[] dataArray = encoding.GetBytes(data);
                int length = dataArray.Length;
                if (length > 0 && dataArray[length - 1] != 0)
                {
                    Array.Resize(ref dataArray, length + 1);
                    dataArray[length] = 0;
                }
                SetArrayData(dataArray);
            }

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

        public enum ErrorNumbers : uint
        {
            BIP_0002 = 2,   // IFH Aufruf fehlerhaft
            BIP_0006 = 6,   // User File Fehler
            BIP_0009 = 9,   // Versionsfehler
            BIP_0010 = 10,  // Fehler bei Konstantenzugriff, Tabellenzugriffsfehler
            IFH_0001 = 11,  // Fehler an Schnittstelle Host-Interface
            IFH_0002 = 12,  // Interface meldet sich nicht
            IFH_0003 = 13,  // Datenübertragung zum Interface gestört
            IFH_0004 = 14,  // Kommando an Interface fehlerhaft
            IFH_0005 = 15,  // Interface interner Fehler (Defekt)
            IFH_0006 = 16,  // Interface nimmt Kommando nicht an
            IFH_0007 = 17,  // Falsche Versorgungsspannung am D-Bus
            IFH_0008 = 18,  // Fehler an Schnittstelle zum SG
            IFH_0009 = 19,  // Steuergerät meldet sich nicht
            IFH_0010 = 20,  // Datenübertragung Interface - SG gestört
            IFH_0011 = 21,  // Unbekanntes Interface
            IFH_0012 = 22,  // Datenpuffer Überlauf
            IFH_0013 = 23,  // Funktion im Interface nicht vorhanden
            IFH_0014 = 24,  // Konzept wird nicht unterstützt
            IFH_0015 = 25,  // U-Batt wurde kurz unterbrochen
            IFH_0019 = 29,  // Interface nicht initialisiert
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
            public StringData()
            {
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

            public void SetData(byte[] value)
            {
                if (value.Length > MAX_ARRAY_LENGTH)
                {
                    throw new ArgumentOutOfRangeException("value.Length", "StringData.SetData: Invalid length");
                }
                Array.Copy(value, 0, data, 0, value.Length);
                length = (EdValueType)value.Length;
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
                if (type != RegisterType.RegS)
                {
                    throw new ArgumentOutOfRangeException("type", "Register.SetArrayData: Invalid data type");
                }
                ediabas.stringRegisters[index].SetData(value);
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
            new OpCode(0x32, "xtype", null),
            new OpCode(0x33, "xvers", null),
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
            new OpCode(0x4B, "break", null),
            new OpCode(0x4C, "clrv", new OperationDelegate(OpClrv)),
            new OpCode(0x4D, "eerr", new OperationDelegate(OpEerr)),
            new OpCode(0x4E, "popf", null),
            new OpCode(0x4F, "pushf", null),
            new OpCode(0x50, "atsp", new OperationDelegate(OpAtsp)),
            new OpCode(0x51, "swap", new OperationDelegate(OpSwap)),
            new OpCode(0x52, "setspc", new OperationDelegate(OpSetspc)),
            new OpCode(0x53, "srevrs", null),
            new OpCode(0x54, "stoken", new OperationDelegate(OpStoken)),
            new OpCode(0x55, "parb", new OperationDelegate(OpParl)),
            new OpCode(0x56, "parw", new OperationDelegate(OpParl)),
            new OpCode(0x57, "parl", new OperationDelegate(OpParl)),
            new OpCode(0x58, "pars", new OperationDelegate(OpPars)),
            new OpCode(0x59, "fclose", null),
            new OpCode(0x5A, "jg", new OperationDelegate(OpJg), true),
            new OpCode(0x5B, "jge", new OperationDelegate(OpJge), true),
            new OpCode(0x5C, "jl", new OperationDelegate(OpJl), true),
            new OpCode(0x5D, "jle", new OperationDelegate(OpJle), true),
            new OpCode(0x5E, "ja", new OperationDelegate(OpJa), true),
            new OpCode(0x5F, "jbe", new OperationDelegate(OpJbe), true),
            new OpCode(0x60, "fopen", null),
            new OpCode(0x61, "fread", null),
            new OpCode(0x62, "freadln", null),
            new OpCode(0x63, "fseek", null),
            new OpCode(0x64, "fseekln", null),
            new OpCode(0x65, "ftell", null),
            new OpCode(0x66, "ftellln", null),
            new OpCode(0x67, "a2fix", new OperationDelegate(OpA2fix)),
            new OpCode(0x68, "fix2flt", new OperationDelegate(OpFix2flt)),
            new OpCode(0x69, "parr", new OperationDelegate(OpParr)),
            new OpCode(0x6A, "test", new OperationDelegate(OpTest)),
            new OpCode(0x6B, "wait", new OperationDelegate(OpWait)),
            new OpCode(0x6C, "date", null),
            new OpCode(0x6D, "time", null),
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
            new OpCode(0x8C, "a2y", null),
            new OpCode(0x8D, "xparraw", null),
            new OpCode(0x8E, "hex2y", new OperationDelegate(OpHex2y)),
            new OpCode(0x8F, "strcmp", new OperationDelegate(OpStrcmp)),
            new OpCode(0x90, "strlen", new OperationDelegate(OpStrlen)),
            new OpCode(0x91, "y2bcd", new OperationDelegate(OpY2bcd)),
            new OpCode(0x92, "y2hex", null),
            new OpCode(0x93, "shmset", new OperationDelegate(OpShmset)),
            new OpCode(0x94, "shmget", new OperationDelegate(OpShmget)),
            new OpCode(0x95, "ergsysi", new OperationDelegate(OpErgsysi)),
            new OpCode(0x96, "flt2fix", new OperationDelegate(OpFlt2fix)),
            new OpCode(0x97, "iupdate", null),
            new OpCode(0x98, "irange", null),
            new OpCode(0x99, "iincpos", null),
            new OpCode(0x9A, "tabseeku", new OperationDelegate(OpTabseeku)),
            new OpCode(0x9B, "flt2y4", null),
            new OpCode(0x9C, "flt2y8", null),
            new OpCode(0x9D, "y42flt", null),
            new OpCode(0x9E, "y82flt", null),
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
            new OpCode(0xAB, "ufix2dez", null),
            new OpCode(0xAC, "generr", new OperationDelegate(OpGenerr)),
            new OpCode(0xAD, "ticks", null),
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

            public void UpdateFlags(EdFloatType value)
            {
                zero = value == 0;
                sign = value < 0;
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
                else if ((value1 & signMask) == (value2 & signMask))
                {
                    overflow = false;
                }
                else
                {
                    overflow = true;
                }
            }

            public void SetOverflow(EdFloatType value1, EdFloatType value2, EdFloatType result)
            {
                if ((value1 < 0) != (value2 < 0))
                {
                    overflow = false;
                }
                else if ((value1 < 0) == (result < 0))
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

            public bool carry;
            public bool zero;
            public bool sign;
            public bool overflow;
        }

        public class JobInfo
        {
            public JobInfo(UInt32 jobOffset, UInt32 jobSize)
            {
                this.jobOffset = jobOffset;
                this.jobSize = jobSize;
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

            private UInt32 jobOffset;
            private UInt32 jobSize;
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

        private bool disposed = false;
        private Stack<byte> stackList = new Stack<byte>();
        private List<string> argList = new List<string>();
        private Dictionary<string, ResultData> resultDict = new Dictionary<string, ResultData>();
        private Dictionary<string, bool> resultsRequestDict = new Dictionary<string, bool>();
        private List<Dictionary<string, ResultData>> resultSets = new List<Dictionary<string, ResultData>>();
        private Dictionary<string, string> configDict = new Dictionary<string, string>();
        private Dictionary<string, byte[]> sharedDataDict = new Dictionary<string, byte[]>();
        private string sgdbFileName = string.Empty;
        private string fileSearchDir = string.Empty;
        private EdCommBase edCommClass;
        private static long timeMeas = 0;
        private byte[] sendBuffer = new byte[256];
        private byte[] recBuffer = new byte[256];
        private byte[] opArgBuffer = new byte[5];
        private AbortJobDelegate abortJobFunc = null;
        private StreamWriter swLog = null;
        private EdValueType[] commParameter = new EdValueType[0];
        private EdValueType commRepeats = 0;
        private Int16[] commAnswerLen = new Int16[0];
        private EdValueType trapMask = 0;
        private EdValueType trapBits = 0;
        private byte[] byteRegisters;
        private EdFloatType[] floatRegisters;
        private StringData[] stringRegisters;
        private Flags flags = new Flags();
        private Stream sgdbFs = null;
        private EdValueType pcCounter = 0;
        private JobInfos jobInfos = null;
        private TableInfos tableInfos = null;
        private TableInfos tableInfosExt = null;
        private Stream tableFs = null;
        private Int32 tableIndex = -1;
        private Int32 tableRowIndex = -1;
        private byte[] tableItemBuffer = new byte[1024];
        private string tokenSeparator = string.Empty;
        private EdValueType tokenIndex = 0;
        private EdValueType floatPrecision = 4;
        private bool jobEnd = false;
        private bool requestInit = true;

        public Dictionary<string, string> ConfigDict
        {
            get
            {
                return configDict;
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

        public string SgdbFileName
        {
            get
            {
                return sgdbFileName;
            }
            set
            {
                CloseSgdbFs();
                sgdbFileName = value;
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
                CloseSgdbFs();
                fileSearchDir = value;
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

        public Ediabas()
        {
            byteRegisters = new byte[32];
            floatRegisters = new EdFloatType[16];
            stringRegisters = new StringData[16];

            for (int i = 0; i < stringRegisters.Length; i++)
            {
                stringRegisters[i] = new StringData();
            }
            foreach (Register arg in registerList)
            {
                arg.SetEdiabas(this);
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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    CloseSgdbFs();
                    CloseTableFs();
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

        private bool OpenSgdbFs()
        {
            if (sgdbFs != null)
            {
                return true;
            }
            string fileName = Path.Combine(fileSearchDir, sgdbFileName);
            if (!File.Exists(fileName))
            {
                LogString("OpenSgdbFs file not found: " + fileName);
                return false;
            }
            try
            {
                sgdbFs = MemoryStreamReader.OpenRead(fileName);
            }
            catch (Exception ex)
            {
                LogString("OpenSgdbFs exception: " + GetExceptionText(ex));
                return false;
            }
            sharedDataDict.Clear();
            jobInfos = ReadAllJobs();
            tableInfos = ReadAllTables(sgdbFs);
            requestInit = true;
            return true;
        }

        private void CloseSgdbFs()
        {
            if (sgdbFs != null)
            {
                sgdbFs.Dispose();
                sgdbFs = null;
            }
        }

        private void CloseTableFs()
        {
            if (tableFs != null)
            {
                tableFs.Dispose();
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
            return sgdbFs;
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

        public void SetError(ErrorNumbers errorNumber)
        {
            if (swLog != null)
            {
                LogString(string.Format("SetError: {0}", errorNumber));
            }

            trapBits |= (EdValueType)(1 << (int)errorNumber);
            EdValueType activeErrors = trapBits & ~trapMask;
            if (activeErrors != 0)
            {
                throw new Exception(string.Format("SetError: Error not masked: {0:X08}", activeErrors));
            }
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

        public JobInfos ReadAllJobs()
        {
            byte[] buffer = new byte[4];
            sgdbFs.Position = 0x88;
            sgdbFs.Read(buffer, 0, buffer.Length);

            UInt32 jobListOffset = BitConverter.ToUInt32(buffer, 0);
            sgdbFs.Position = jobListOffset;
            int numJobs = readInt32(sgdbFs);

            JobInfos jobInfos = new JobInfos();
            jobInfos.JobNameDict = new Dictionary<string, UInt32>();
            jobInfos.JobInfoArray = new JobInfo[numJobs];

            UInt32 jobStart = (UInt32)sgdbFs.Position;
            for (int i = 0; i < numJobs; ++i)
            {
                sgdbFs.Position = jobStart;
                byte[] jobBuffer = new byte[0x44];
                readAndDecryptBytes(sgdbFs, jobBuffer, 0, jobBuffer.Length);
                string jobNameString = encoding.GetString(jobBuffer, 0, 0x40).TrimEnd('\0');
                UInt32 jobAddress = BitConverter.ToUInt32(jobBuffer, 0x40);
                jobInfos.JobNameDict.Add(jobNameString.ToUpper(culture), (UInt32)i);
#if false
                //if (String.Compare(jobNameString, "STATUS_RAILDRUCK_IST", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    sgdbFs.Position = jobAddress;
                    bool foundFirstEoj = false;
                    byte[] ocBuffer = new byte[2];
                    Operand arg0 = new Operand();
                    Operand arg1 = new Operand();
                    long startTime = Stopwatch.GetTimestamp();
                    while (true)
                    {
                        readAndDecryptBytes(sgdbFs, ocBuffer, 0, ocBuffer.Length);

                        byte opCodeVal = ocBuffer[0];
                        byte opAddrMode = ocBuffer[1];
                        OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
                        OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

                        if (opCodeVal >= ocList.Length)
                        {
                            throw new ArgumentOutOfRangeException("opCodeVal", "ReadAllJobs: Opcode out of range");
                        }
                        getOpArg(sgdbFs, opAddrMode0, ref arg0);
                        getOpArg(sgdbFs, opAddrMode1, ref arg1);

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
                UInt32 jobSize = (UInt32)(sgdbFs.Position - jobAddress);
                jobInfos.JobInfoArray[i] = new JobInfo(jobAddress, jobSize);
#else
                jobInfos.JobInfoArray[i] = new JobInfo(jobAddress, 0);
#endif
                jobStart += 0x44;
            }
            return jobInfos;
        }

        public TableInfos ReadAllTables(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Position = 0x84;
            fs.Read(buffer, 0, buffer.Length);

            UInt32 tableOffset = BitConverter.ToUInt32(buffer, 0);
            fs.Position = tableOffset;

            byte[] tableCountBuffer = new byte[4];
            readAndDecryptBytes(fs, tableCountBuffer, 0, tableCountBuffer.Length);
            int tableCount = BitConverter.ToInt32(tableCountBuffer, 0);

            TableInfos tableInfos = new TableInfos();
            tableInfos.TableNameDict = new Dictionary<string,UInt32>();
            tableInfos.TableInfoArray = new TableInfo[tableCount];

            UInt32 tableStart = (UInt32)fs.Position;
            for (int i = 0; i < tableCount; ++i)
            {
                TableInfo tableInfo = ReadTable(fs, tableStart);
                tableInfos.TableInfoArray[i] = tableInfo;
                tableInfos.TableNameDict.Add(tableInfo.Name.ToUpper(culture), (UInt32)i);
                tableStart += 0x50;
            }
            return tableInfos;
        }

        public TableInfo ReadTable(Stream fs, UInt32 tableOffset)
        {
            fs.Position = tableOffset;

            byte[] tableBuffer = new byte[0x50];
            readAndDecryptBytes(fs, tableBuffer, 0, tableBuffer.Length);
            string name = encoding.GetString(tableBuffer, 0, 0x40).TrimEnd('\0');

            UInt32 tableColumnOffset = BitConverter.ToUInt32(tableBuffer, 0x40);
            UInt32 tableColumnCount = BitConverter.ToUInt32(tableBuffer, 0x48);
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

        public Int32 GetTableIndex(Stream fs, string tableName)
        {
            TableInfos tableInfos = GetTableInfos(fs);

            UInt32 tableIdx;
            if (tableInfos.TableNameDict.TryGetValue(tableName.ToUpper(culture), out tableIdx))
            {
                IndexTable(fs, tableInfos.TableInfoArray[tableIdx]);
                return (Int32)tableIdx;
            }

            return -1;
        }

        public UInt32 GetTableColumns(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Columns;
        }

        public UInt32 GetTableRows(Stream fs, Int32 tableIdx)
        {
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
            if ((tableIdx < 0) || (tableIdx >= tableArray.Length))
            {
                return 0;
            }
            return tableArray[tableIdx].Rows;
        }

        public Int32 GetTableLine(Stream fs, Int32 tableIdx, EdValueType line, out bool found)
        {
            found = false;
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
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
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
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
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
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
                    EdValueType rowValue = StringToValue(rowStr);
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
            TableInfos tableInfos = GetTableInfos(fs);
            TableInfo[] tableArray = tableInfos.TableInfoArray;
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

        public void ResolveSgdbFile(string fileName)
        {
            SgdbFileName = Path.GetFileName(fileName);
            if (String.Compare(Path.GetExtension(fileName), ".GRP", StringComparison.OrdinalIgnoreCase) == 0)
            {       // group file
                string variantName = ExecuteIdentJob();
                if (variantName.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("variantName", "ResolveSgdbFile: No variant found");
                }
                SgdbFileName = variantName.ToLower(culture) + ".prg";
            }
        }

        public void ExecuteInitJob()
        {
            try
            {
                ExecuteJob("INITIALISIERUNG");
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

        public string ExecuteIdentJob()
        {
            resultDict.Clear();
            try
            {
                ExecuteJob("IDENTIFIKATION");
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

            if (!OpenSgdbFs())
            {
                throw new ArgumentOutOfRangeException("OpenSgdbFs", "executeJob: Open SGDB failed");
            }
            JobInfo jobInfo = GetJobInfo(jobName);
            if (jobInfo == null)
            {
                throw new ArgumentOutOfRangeException("jobName", "executeJob: Job not found");
            }
            ExecuteJob(jobInfo.JobOffset);
            if (swLog != null)
            {
                LogString("executeJob successfull");
            }
        }

        public void ExecuteJob(UInt32 jobAddress)
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
            flags.Init();
            pcCounter = jobAddress;
            CloseTableFs();
            jobEnd = false;

            Operand arg0 = new Operand();
            Operand arg1 = new Operand();
            try
            {
                while (!jobEnd)
                {
                    //long startTime = Stopwatch.GetTimestamp();
                    EdValueType pcCounterOld = pcCounter;
                    sgdbFs.Position = pcCounter;
                    readAndDecryptBytes(sgdbFs, buffer, 0, buffer.Length);

                    byte opCodeVal = buffer[0];
                    byte opAddrMode = buffer[1];
                    OpAddrMode opAddrMode0 = (OpAddrMode)((opAddrMode & 0xF0) >> 4);
                    OpAddrMode opAddrMode1 = (OpAddrMode)((opAddrMode & 0x0F) >> 0);

                    if (opCodeVal >= ocList.Length)
                    {
                        throw new ArgumentOutOfRangeException("opCodeVal", "executeJob: Opcode out of range");
                    }
                    OpCode oc = ocList[opCodeVal];
                    getOpArg(sgdbFs, opAddrMode0, ref arg0);
                    getOpArg(sgdbFs, opAddrMode1, ref arg1);
                    pcCounter = (EdValueType)sgdbFs.Position;

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
                resultSets.Add(new Dictionary<string, ResultData>(resultDict));
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
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt16(opArgBuffer, 0));
                            // string.Format("#${0:X}.I", BitConverter.ToInt16(buffer, 0));
                            return;
                        }
                    case OpAddrMode.Imm32:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            oper.Init(opAddrMode, (EdValueType)BitConverter.ToUInt32(opArgBuffer, 0));
                            // string.Format("#${0:X}.L", BitConverter.ToInt32(buffer, 0));
                            return;
                        }
                    case OpAddrMode.ImmStr:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 2);
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
                            EdValueType inc = BitConverter.ToUInt16(opArgBuffer, 2);
                            oper.Init(opAddrMode, oaReg0, oaReg1, (EdValueType)inc);
                            // string.Format("{0}[{1},#${2:X}]", oaReg0.name, oaReg1.name, inc);
                            return;
                        }
                    case OpAddrMode.IdxImmLenImm:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 5);
                            Register oaReg = GetRegister(opArgBuffer[0]);
                            EdValueType idx = BitConverter.ToUInt16(opArgBuffer, 1);
                            EdValueType len = BitConverter.ToUInt16(opArgBuffer, 3);
                            oper.Init(opAddrMode, oaReg, (EdValueType)idx, (EdValueType)len);
                            // string.Format("{0}[#${1:X}]#${2:X}", oaReg.name, idx, len);
                            return;
                        }
                    case OpAddrMode.IdxImmLenReg:
                        {
                            readAndDecryptBytes(fs, opArgBuffer, 0, 4);
                            Register oaReg = GetRegister(opArgBuffer[0]);
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
            return BitConverter.ToInt32(buffer, 0);
        }

        static double RoundToSignificantDigits(EdFloatType value, EdValueType digits)
        {
            if (value == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
            return scale * Math.Round(value / scale, (int) digits);
        }

        private static EdValueType StringToValue(string number)
        {
            EdValueType value = 0;
            if (number.Length == 0)
            {
                return value;
            }
            try
            {
                if (number.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                {   // hex
                    value = Convert.ToUInt32(number.Substring(2, number.Length - 2), 16);
                }
                else if (number.StartsWith("0y", StringComparison.InvariantCultureIgnoreCase))
                {   // binary
                    value = Convert.ToUInt32(number.Substring(2, number.Length - 2), 2);
                }
                else
                {   // dec
                    if (!Char.IsLetter(number[0]))
                    {
                        value = Convert.ToUInt32(number, 10);
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
                result = 0;
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
