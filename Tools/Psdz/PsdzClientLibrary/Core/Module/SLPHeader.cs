using System;
using BMW.Rheingold.xVM;
using PsdzClient.Core;
using System.Net;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPHeader
    {
        public ushort curpos;

        public ushort encoding;

        public uint extoffset;

        public byte flags;

        public byte functionid;

        public string langtag;

        public ushort langtaglen;

        public uint length;

        public byte[] payload;

        public byte[] sendbuffer;

        public IPEndPoint sender;

        public bool validParsed;

        public byte version;

        public ushort xid;

        public SLPHeader()
        {
            sender = new IPEndPoint(IPAddress.Any, 0);
            langtag = string.Empty;
            payload = new byte[5];
            validParsed = false;
            functionid = 0;
            version = 2;
        }

        public bool ParseHeader(byte[] buffer)
        {
            try
            {
                version = buffer[0];
                functionid = buffer[1];
                switch (version)
                {
                    case 1:
                        {
                            length = SLP.AsUINT16(buffer, 2);
                            flags = buffer[4];
                            encoding = SLP.AsUINT16(buffer, 8);
                            payload = new byte[length];
                            for (int i = 0; i < length - 12; i++)
                            {
                                payload[i] = buffer[i + 12];
                            }
                            break;
                        }
                    case 2:
                        {
                            length = SLP.AsUINT24(buffer, 2);
                            if (buffer.Length < length)
                            {
                                validParsed = false;
                                return false;
                            }
                            flags = buffer[5];
                            extoffset = SLP.AsUINT24(buffer, 7);
                            xid = SLP.AsUINT16(buffer, 10);
                            langtaglen = SLP.AsUINT16(buffer, 12);
                            langtag = SLP.AsSTRING(buffer, 14, langtaglen);
                            payload = new byte[length];
                            for (int i = 0; i < length - 14 - langtaglen; i++)
                            {
                                payload[i] = buffer[i + 14 + langtaglen];
                            }
                            break;
                        }
                    default:
                        Log.Info("SLPHeader.ParseHeader()", "unknown protocol format found: {0}", (ushort)version);
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPHeader.ParseHeader()", exception);
                validParsed = false;
                return false;
            }
            validParsed = true;
            return true;
        }

        public string SLPToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (functionid)
            {
                case 1:
                    {
                        stringBuilder.Append("SrvRqst\n");
                        SLPSrvRqst arg7 = (SLPSrvRqst)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg7);
                        break;
                    }
                case 2:
                    {
                        stringBuilder.Append("SrvRply");
                        SLPSrvRply arg6 = (SLPSrvRply)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg6);
                        break;
                    }
                case 9:
                    {
                        stringBuilder.Append("SrvTypeRqst");
                        SLPSrvTypeRqst arg5 = (SLPSrvTypeRqst)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg5);
                        break;
                    }
                case 10:
                    {
                        stringBuilder.Append("SrvTypeRply");
                        SLPSrvTypeRply arg4 = (SLPSrvTypeRply)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg4);
                        break;
                    }
                case 6:
                    {
                        stringBuilder.Append("AttrRqst");
                        SLPAttrRqst arg3 = (SLPAttrRqst)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg3);
                        break;
                    }
                case 7:
                    {
                        stringBuilder.Append("AttrRply");
                        SLPAttrRply arg2 = (SLPAttrRply)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg2);
                        break;
                    }
                case 11:
                    {
                        stringBuilder.Append("SAAdvert");
                        SLPSAAdvert arg = (SLPSAAdvert)this;
                        stringBuilder.AppendFormat("Received from: {0}\n", sender);
                        stringBuilder.AppendFormat("{0}\n\n", arg);
                        break;
                    }
                default:
                    stringBuilder.AppendFormat("*** Function id:{0} ***", functionid);
                    stringBuilder.AppendFormat("Received from: {0}\n", sender);
                    break;
            }
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SLPHeader\n");
            stringBuilder.AppendFormat("Protocol version: {0}\n", version);
            stringBuilder.AppendFormat("Function id: {0}\n", functionid);
            stringBuilder.AppendFormat("Length: {0}\n", length);
            stringBuilder.AppendFormat("Flags: {0}\n", flags);
            stringBuilder.AppendFormat("Extension id: {0}\n", xid);
            stringBuilder.AppendFormat("Language taglen: {0}\n", langtaglen);
            stringBuilder.AppendFormat("Language tags: {0}\n", langtag);
            return stringBuilder.ToString();
        }

        public void ToUINT16(byte[] ucp, ushort val)
        {
            try
            {
                ucp[curpos] = (byte)((val >> 8) & 0xFF);
                ucp[1 + curpos] = (byte)(val & 0xFF);
                curpos += 2;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPHeader.ToUINT16()", exception);
            }
        }

        public void ToUINT24(byte[] ucp, uint val)
        {
            try
            {
                ucp[curpos] = (byte)((val >> 16) & 0xFF);
                ucp[1 + curpos] = (byte)((val >> 8) & 0xFF);
                ucp[2 + curpos] = (byte)(val & 0xFF);
                curpos += 3;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPHeader.ToUINT24()", exception);
            }
        }

        public void ToUINT32(byte[] ucp, uint val)
        {
            try
            {
                ucp[curpos] = (byte)((val >> 24) & 0xFF);
                ucp[1 + curpos] = (byte)((val >> 16) & 0xFF);
                ucp[2 + curpos] = (byte)((val >> 8) & 0xFF);
                ucp[3 + curpos] = (byte)(val & 0xFF);
                curpos += 4;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPHeader.ToUINT32()", exception);
            }
        }
    }
}
