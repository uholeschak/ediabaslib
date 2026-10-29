using System;
using System.Collections.Generic;
using UdsFileReader;

namespace BmwDeepObd
{
    public static class DtcUtils
    {
        public static int TelLengthKwp2000(byte[] dataBuffer, int offset, out int dataLength, out int dataOffset)
        {
            int bufferLen = dataBuffer.Length - offset;
            dataLength = 0;
            dataOffset = 0;
            if (bufferLen < 4)
            {
                return -1;
            }
            dataLength = dataBuffer[0 + offset] & 0x3F;
            int telLength = dataLength;
            if (telLength == 0)
            {   // with length byte
                dataLength = dataBuffer[3 + offset];
                telLength = dataLength + 5;
                dataOffset = 4;
            }
            else
            {
                telLength += 4;
                dataOffset = 3;
            }
            if (bufferLen < telLength)
            {
                return -1;
            }
            return telLength;
        }

        public static List<ActivityCommon.VagDtcEntry> ParseEcuDtcResponse(byte[] dataBuffer, bool saeMode)
        {
            if (dataBuffer == null)
            {
                return null;
            }
            List<ActivityCommon.VagDtcEntry> dtcList = new List<ActivityCommon.VagDtcEntry>();
            int dtcCount = -1;
            int offset = 0;
            for (;;)
            {
                int telLength = TelLengthKwp2000(dataBuffer, offset, out int dataLength, out int dataOffset);
                if (telLength < 0)
                {
                    return null;
                }
                if (dataLength < 1)
                {
                    return null;
                }

                int pos = 0;
                int responseCode = dataBuffer[offset + dataOffset + pos++];
                if (responseCode != 0x7F)
                {
                    if (dtcCount < 0)
                    {
                        if (dataLength < 2)
                        {
                            return null;
                        }
                        dtcCount = dataBuffer[offset + dataOffset + pos++];
                        if (dtcCount == 0)
                        {
                            return dtcList;
                        }
                    }

                    while (pos + 2 < dataLength)
                    {
                        int index = offset + dataOffset + pos;
                        uint dtcCode = (uint) ((dataBuffer[index] << 8) | dataBuffer[index + 1]);
                        byte dtcDetail = dataBuffer[index + 2];
                        pos += 3;

                        ActivityCommon.VagDtcEntry dtcEntry = new ActivityCommon.VagDtcEntry(dtcCode, dtcDetail, saeMode? DataReader.ErrorType.Sae : DataReader.ErrorType.Kwp2000);
                        dtcList.Add(dtcEntry);
                        if (dtcList.Count >= dtcCount)
                        {
                            break;
                        }
                    }
                }
                if (dtcList.Count >= dtcCount)
                {
                    break;
                }
                offset += telLength;
                if (offset == dataBuffer.Length)
                {
                    break;
                }
                if (offset > dataBuffer.Length)
                {
                    return null;
                }
            }

            return dtcList;
        }

        public static List<byte[]> ExtractValidEcuResponses(byte[] dataBuffer)
        {
            if (dataBuffer == null)
            {
                return null;
            }
            List<byte[]> responseList = new List<byte[]>();
            int offset = 0;
            for (; ; )
            {
                int telLength = TelLengthKwp2000(dataBuffer, offset, out int dataLength, out int dataOffset);
                if (telLength < 0)
                {
                    return null;
                }
                if (dataLength < 1)
                {
                    return null;
                }

                int responseCode = dataBuffer[offset + dataOffset];
                if (responseCode != 0x7F)
                {
                    byte[] response = new byte[dataLength];
                    Array.Copy(dataBuffer, offset + dataOffset, response, 0, dataLength);
                    responseList.Add(response);
                    offset += telLength;
                    if (offset == dataBuffer.Length)
                    {
                        break;
                    }

                    if (offset > dataBuffer.Length)
                    {
                        return null;
                    }
                }
            }

            return responseList;
        }

        public static byte[] ParseSaeDetailDtcResponse(byte[] dataBuffer)
        {
            List<byte[]> responseList = ExtractValidEcuResponses(dataBuffer);
            if (responseList == null)
            {
                return null;
            }

            foreach (byte[] response in responseList)
            {
                if (response.Length >= 3)
                {
                    int blockStart = 2;
                    int blockType = response[blockStart];
                    if (blockType == 0x6C)
                    {
                        byte[] blockData = new byte[response.Length - 2];
                        Array.Copy(response, blockStart, blockData, 0, blockData.Length);
                        return blockData;
                    }
                }
            }
            return null;
        }

        public static byte[] ExtractUdsEcuResponses(byte[] dataBuffer)
        {
            if (dataBuffer == null)
            {
                return null;
            }

            int dataOffset = 18;
            if (dataBuffer.Length < dataOffset + 2)
            {
                return null;
            }

            if ((dataBuffer[dataOffset] & 0x40) == 0x00 || dataBuffer[dataOffset + 1] == 0x7F)
            {
                return null;    // error response
            }

            byte[] data = new byte[dataBuffer.Length - dataOffset];
            Array.Copy(dataBuffer, dataOffset, data, 0, data.Length);
            return data;
        }

        public static string SaeCode16ToString(long code)
        {
            string locationName;
            switch ((code >> 14) & 0x03)
            {
                default:    // power train
                    locationName = "P";
                    break;

                case 0x1:  // chassis
                    locationName = "C";
                    break;

                case 0x2:  // body
                    locationName = "B";
                    break;

                case 0x3:  // network
                    locationName = "U";
                    break;
            }
            return string.Format("{0}{1:X04}", locationName, code & 0x3FFF);
        }
    }
}
