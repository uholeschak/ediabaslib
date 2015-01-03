using System;
using System.Runtime.InteropServices;

namespace Ftdi
{
    /// <summary>
    /// This class will deal with the USB communication
    /// </summary>
    /// Here we will cast all the ftd2xx.h functions and structure that we need
    /// and will add some other specific communaction protocols 
    class Ftd2xx
    {
        #region Stupid defines from the dll

        // FT_OpenEx Flags
        public const UInt16 FT_OPEN_BY_SERIAL_NUMBER = 1;
        public const UInt16 FT_OPEN_BY_DESCRIPTION = 2;
        public const UInt16 FT_OPEN_BY_LOCATION = 4;

        // FT_ListDevices Flags (used in conjunction with FT_OpenEx Flags
        public const UInt32 FT_LIST_NUMBER_ONLY = 0x80000000;
        public const UInt32 FT_LIST_BY_INDEX = 0x40000000;
        public const UInt32 FT_LIST_ALL = 0x20000000;
        public const UInt32 FT_LIST_MASK = (FT_LIST_NUMBER_ONLY | FT_LIST_BY_INDEX | FT_LIST_ALL);

        // Baud Rates
        public const UInt32 FT_BAUD_300 = 300;
        public const UInt32 FT_BAUD_600 = 600;
        public const UInt32 FT_BAUD_1200 = 1200;
        public const UInt32 FT_BAUD_2400 = 2400;
        public const UInt32 FT_BAUD_4800 = 4800;
        public const UInt32 FT_BAUD_9600 = 9600;
        public const UInt32 FT_BAUD_14400 = 14400;
        public const UInt32 FT_BAUD_19200 = 19200;
        public const UInt32 FT_BAUD_38400 = 38400;
        public const UInt32 FT_BAUD_57600 = 57600;
        public const UInt32 FT_BAUD_115200 = 115200;
        public const UInt32 FT_BAUD_230400 = 230400;
        public const UInt32 FT_BAUD_460800 = 460800;
        public const UInt32 FT_BAUD_921600 = 921600;

        // Word Lengths
        public const byte FT_BITS_8 = 8;
        public const byte FT_BITS_7 = 7;
        public const byte FT_BITS_6 = 6;
        public const byte FT_BITS_5 = 5;


        // Stop Bits
        public const byte FT_STOP_BITS_1 = 0;
        public const byte FT_STOP_BITS_1_5 = 1;
        public const byte FT_STOP_BITS_2 = 2;

        // Parity
        public const byte FT_PARITY_NONE = 0;
        public const byte FT_PARITY_ODD = 1;
        public const byte FT_PARITY_EVEN = 2;
        public const byte FT_PARITY_MARK = 3;
        public const byte FT_PARITY_SPACE = 4;

        // Flow Control
        public const UInt16 FT_FLOW_NONE = 0x0000;
        public const UInt16 FT_FLOW_RTS_CTS = 0x0100;
        public const UInt16 FT_FLOW_DTR_DSR = 0x0200;
        public const UInt16 FT_FLOW_XON_XOFF = 0x0400;

        // Purge rx and tx buffers
        public const byte FT_PURGE_RX = 1;
        public const byte FT_PURGE_TX = 2;

        // Events
        public const byte FT_EVENT_RXCHAR = 1;
        public const byte FT_EVENT_MODEM_STATUS = 2;
        public const byte FT_EVENT_LINE_STATUS = 4;

        // Timeouts
        private const UInt32 FT_DEFAULT_RX_TIMEOUT = 300;
        private const UInt32 FT_DEFAULT_TX_TIMEOUT = 300;

        #endregion Stupid defines

        #region Stupid Marshals

        /// <summary>
        /// marshalling the structure...
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LPFTDCB
        {
            public UInt32 DCBlength;      /* sizeof(FTDCB)                   */
            public UInt32 BaudRate;       /* Baudrate at which running       */

            public UInt32 fBits;            // bits layout is inportable so have the flag and take out the bits
            //ulong fBinary: 1;     /* Binary Mode (skip EOF check)    */
            //ulong fParity: 1;     /* Enable parity checking          */
            //ulong fOutxCtsFlow:1; /* CTS handshaking on output       */
            //ulong fOutxDsrFlow:1; /* DSR handshaking on output       */
            //ulong fDtrControl:2;  /* DTR Flow control                */
            //ulong fDsrSensitivity:1; /* DSR Sensitivity              */
            //ulong fTXContinueOnXoff: 1; /* Continue TX when Xoff sent */
            //ulong fOutX: 1;       /* Enable output X-ON/X-OFF        */
            //ulong fInX: 1;        /* Enable input X-ON/X-OFF         */
            //ulong fErrorChar: 1;  /* Enable Err Replacement          */
            //ulong fNull: 1;       /* Enable Null stripping           */
            //ulong fRtsControl:2;  /* Rts Flow control                */
            //ulong fAbortOnError:1; /* Abort all reads and writes on Error */
            //ulong fDummy2:17;     /* Reserved                        */

            public UInt16 wReserved;       /* Not currently used              */
            public UInt16 XonLim;          /* Transmit X-ON threshold         */
            public UInt16 XoffLim;         /* Transmit X-OFF threshold        */

            public byte ByteSize;        /* Number of bits/byte, 4-8        */
            public byte Parity;          /* 0-4=None,Odd,Even,Mark,Space    */
            public byte StopBits;        /* 0,1,2 = 1, 1.5, 2               */

            public char XonChar;         /* Tx and Rx X-ON character        */
            public char XoffChar;        /* Tx and Rx X-OFF character       */
            public char ErrorChar;       /* Error replacement char          */
            public char EofChar;         /* End of Input character          */
            public char EvtChar;         /* Received Event character        */

            public ushort wReserved1;    /* Fill for now.                   */
        } ;

        #endregion Stupid Marshals

        #region W32 Functions from the dll
        // note: W32 should not be mixed with non W32 unless explicitly stated


        /// <summary>
        /// This function gets the current device state. 
        /// </summary>
        /// <param name="ftHandle">Cast PVOID to IntPtr to have system specific handle pointer size</param>
        /// <param name="lpftDcb">the status</param>
        /// <returns>If the function is successful, the return value is nonzero</returns>
        [DllImport("ftd2xx")]
        public static extern bool FT_W32_GetCommState(IntPtr ftHandle, ref LPFTDCB lpftDcb);

        #endregion W32 Functions

        #region Normal Functions that we use from the dll

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_Open(UInt32 uiPort, out IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_OpenEx(IntPtr pArg1, UInt32 Flags, out IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_ListDevices(ref IntPtr pArg1, ref IntPtr pArg2, UInt32 Flags);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_Close(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_Read(IntPtr ftHandle, IntPtr lpBuffer, UInt32 nBufferSize, out UInt32 lpBytesReturned);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_Write(IntPtr ftHandle, IntPtr lpBuffer, UInt32 nBufferSize, out UInt32 lpBytesWritten);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetBaudRate(IntPtr ftHandle, UInt32 uBaudRate);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetDivisor(IntPtr ftHandle, UInt16 Divisor);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetDataCharacteristics(IntPtr ftHandle, Byte uWordLength, Byte uStopBits, Byte uParity);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetFlowControl(IntPtr ftHandle, UInt16 FlowControl, Byte XonChar, Byte XoffChar);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_ResetDevice(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetDtr(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_ClrDtr(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetRts(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_ClrRts(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_GetModemStatus(IntPtr ftHandle, ref UInt32 pModemStatus);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_Purge(IntPtr ftHandle, UInt32 Mask);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetTimeouts(IntPtr ftHandle, UInt32 ReadTimeout, UInt32 WriteTimeout);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_GetQueueStatus(IntPtr ftHandle, out UInt32 dwRxBytes);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_GetStatus(IntPtr ftHandle, out UInt32 dwRxBytes, out UInt32 dwTxBytes, out UInt32 dwEventDWord);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetBreakOn(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetBreakOff(IntPtr ftHandle);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetLatencyTimer(IntPtr ftHandle, Byte ucLatency);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_GetLatencyTimer(IntPtr ftHandle, out Byte pucLatency);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_SetResetPipeRetryCount(IntPtr ftHandle, UInt32 dwCount);

        [DllImport("ftd2xx")]
        public static extern FT_STATUS FT_ResetPort(IntPtr ftHandle);

        #endregion Normal Functions

        #region Helper Functions

        public static FT_STATUS FT_ReadWrapper(IntPtr ftHandle, byte[] buffer, int bytesToRead, int offset, out UInt32 bytesReturned)
        {
            FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
            IntPtr ptr = (IntPtr) 0;

            bytesReturned = 0;
            try
            {
                ptr = Marshal.AllocHGlobal(bytesToRead);
                ftStatus = FT_Read(ftHandle, ptr, (UInt32)bytesToRead, out bytesReturned);
                Marshal.Copy(ptr, buffer, offset, bytesToRead);
            }
            catch (Exception)
            {
                ftStatus = FT_STATUS.FT_OTHER_ERROR;
            }
            finally
            {
                if (ptr != (IntPtr)0)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            return ftStatus;
        }

        public static FT_STATUS FT_WriteWrapper(IntPtr ftHandle, byte[] buffer, int bufferSize, int offset, out UInt32 bytesWritten)
        {
            FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
            IntPtr ptr = (IntPtr) 0;

            bytesWritten = 0;
            try
            {
                ptr = Marshal.AllocHGlobal(bufferSize);
                Marshal.Copy(buffer, offset, ptr, bufferSize);
                ftStatus = FT_Write(ftHandle, ptr, (UInt32)bufferSize, out bytesWritten);
            }
            catch (Exception)
            {
                ftStatus = FT_STATUS.FT_OTHER_ERROR;
            }
            finally
            {
                if (ptr != (IntPtr)0)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            return ftStatus;
        }

        #endregion Functions
       
        #region other defines and enumerations for the communication namespace

        /// <summary>
        /// Enumaration containing the varios return status for the DLL functions.
        /// </summary>
        public enum FT_STATUS
        {
            FT_OK = 0,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,

            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR
        }

        #endregion other defines and enumerations for the communication namespace
    }
}
