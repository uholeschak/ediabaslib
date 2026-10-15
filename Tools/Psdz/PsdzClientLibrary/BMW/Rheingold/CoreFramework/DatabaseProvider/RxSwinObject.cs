using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class RxSwinObject : IRxSwinObject
    {
        public string HexEcuAdress { get; private set; }

        public List<string> RxSwinList { get; private set; }

        public bool IsMasterEcu { get; set; }

        public bool IsNegativeJobStatus { get; private set; }

        public string NegativeJobResult { get; private set; }

        public RxSwinObject(string hexEcuAdres, List<string> rxsWinList, bool isMasterEcu)
        {
            HexEcuAdress = hexEcuAdres;
            RxSwinList = rxsWinList;
            IsMasterEcu = isMasterEcu;
        }

        public RxSwinObject(string hexEcuAdres, List<string> rxsWinList, bool isMasterEcu, bool isNegativeJobResult, string negativeJobResult)
        {
            HexEcuAdress = hexEcuAdres;
            RxSwinList = rxsWinList;
            IsMasterEcu = isMasterEcu;
            IsNegativeJobStatus = isNegativeJobResult;
            NegativeJobResult = negativeJobResult;
        }

        public override string ToString()
        {
            return "HexEcuAdress: " + HexEcuAdress + ", IsMasterEcu: " + (IsMasterEcu ? "true" : "false") + ", RxSwinList: " + string.Join(",", RxSwinList) + ", IsNegativeJobStatus: " + (IsNegativeJobStatus ? "true" : "false") + ", NegativeJobResult: " + NegativeJobResult + " ";
        }
    }
}
