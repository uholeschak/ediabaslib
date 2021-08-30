using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using EdiabasLib;

namespace EdiabasLib
{
    public class EdSimInterface : EdInterfaceBase
    {
        //private byte[] debugArray = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        public override string InterfaceType { get { return "OBD"; } }
        public override UInt32 InterfaceVersion { get { return 209; } }
        public override string InterfaceName { get { return "STD:OBD"; } }
        public override bool Connected { get { return true; }}
        public override long IgnitionVoltage { get { return 12000; } }
        public override long BatteryVoltage { get { return 12000; } }
        public override byte[] State { get { return new byte[2]; } }
        public override byte[] KeyBytes { get { return new byte[0]; } }

        private string Sgbd;
        private string EcuPath;
        private string SimFile;
        private Dictionary<string, byte[]> SimRequests;
        private Dictionary<string, byte[]> SimResponses;
        private Dictionary<string, List<int>> SimRequestsAnyL;
        private Dictionary<string, List<int>> SimRequestsAnyR;

        public EdSimInterface()
        {
            SimRequests = new Dictionary<string, byte[]>();
            SimResponses = new Dictionary<string, byte[]>();
            SimRequestsAnyL = new Dictionary<string, List<int>>();
            SimRequestsAnyR = new Dictionary<string, List<int>>();
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            RefreshSim();

            foreach(KeyValuePair<string, byte[]> kvp in SimRequests)
            {
                List<int> anyL = SimRequestsAnyL[kvp.Key];
                List<int> anyR = SimRequestsAnyR[kvp.Key];

                bool match = true;
                for(int i = 0; i < sendData.Length; i++)
                {
                    if(sendData[i] != kvp.Value[i] && !anyL.Contains(i) && !anyR.Contains(i))
                    {
                        match = false;
                        break;
                    }

                    else if(sendData[i] != kvp.Value[i])
                    {
                        byte l = (byte)(sendData[i] >> 4);
                        byte r = (byte)(sendData[i] << 4);

                        if (kvp.Value[i] != l && !anyL.Contains(i))
                            match = false;
                        if (kvp.Value[i] != r && !anyR.Contains(i))
                            match = false;
                    }
                }

                if(match)
                {
                    receiveData = SimResponses[kvp.Key];
                    return true;
                }
            }


            receiveData = new byte[0];
            return false;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            receiveData = new byte[] { 0xFF };
            return true;
        }

        public override bool StopFrequent()
        {
            return true;
        }

        public override bool InterfaceReset()
        {
            return true;
        }

        public override bool IsValidInterfaceName(string name)
        {
            return true;
        }

        private void RefreshSim()
        {
            try
            {
                if(SimFile == null || Sgbd != Ediabas.SgbdFileName || EcuPath != Ediabas.EcuPath)
                {
                    Sgbd = Ediabas.SgbdFileName;
                    EcuPath = Ediabas.EcuPath;
                    SimFile = Directory.GetParent(Ediabas.EcuPath).FullName + "\\Sim\\" + Path.GetFileNameWithoutExtension(Ediabas.SgbdFileName) + ".SIM";

                    if(!File.Exists(SimFile))
                        SimFile = Ediabas.EcuPath + "\\" + Path.GetFileNameWithoutExtension(Ediabas.SgbdFileName) + ".SIM";

                    if(!File.Exists(SimFile))
                        throw new FileNotFoundException("SIM file not found");
                }

                string[] lines = File.ReadAllLines(SimFile);

                int mode = 0; // 1 = request; 2 = response

                foreach (string rawLine in lines)
                {
                    string line;

                    int comment = rawLine.IndexOf(';');

                    if (comment >= 0)
                        line = rawLine.Substring(0, comment);
                    else
                        line = rawLine;

                    line = Regex.Replace(line, @"\s+", "");

                    if (string.Compare("[REQUEST]", line, true) == 0)
                        mode = 1;
                    else if (string.Compare("[RESPONSE]", line, true) == 0)
                        mode = 2;

                    string id;
                    byte[] data;
                    List<int> anyIndicesL = new List<int>();
                    List<int> anyIndicesR = new List<int>();

                    if (line.IndexOf('=') >= 1)
                    {
                        string[] split = line.Split('=');
                        string[] strBytes = split[1].Split(',');

                        id = split[0];
                        data = new byte[strBytes.Length];



                        for (int i = 0; i < strBytes.Length; i++)
                        {
                            if (strBytes[i] == "")
                                continue;

                            string l = strBytes[i].Substring(0, 1);
                            string r = strBytes[i].Substring(1, 1);

                            if (string.Compare(l, "X", true) == 0 && string.Compare(r, "X", true) == 0)
                            {
                                anyIndicesL.Add(i);
                                anyIndicesR.Add(i);
                            }
                            else if (string.Compare(l, "X", true) == 0 && string.Compare(r, "X", true) != 0)
                            {
                                anyIndicesL.Add(i);
                                data[i] = byte.Parse("0" + r, System.Globalization.NumberStyles.HexNumber);
                            }
                            else if (string.Compare(l, "X", true) != 0 && string.Compare(r, "X", true) == 0)
                            {
                                anyIndicesR.Add(i);
                                data[i] = byte.Parse(l + "0" , System.Globalization.NumberStyles.HexNumber);
                            }
                            else
                                data[i] = byte.Parse(strBytes[i], System.Globalization.NumberStyles.HexNumber);
                        }


                        if (mode == 1)
                        {
                            SimRequests[id] = data;
                            SimRequestsAnyL[id] = anyIndicesL;
                            SimRequestsAnyR[id] = anyIndicesR;
                        }
                        else if (mode == 2)
                        {
                            SimResponses[id] = data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("code", EdiabasNet.ErrorCodes.EDIABAS_IFH_0026);
                throw ex;
            }
        }
    }
}
