using BMW.Rheingold.Psdz.Model.SecureCoding;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class NcdMapper
    {
        public static IPsdzNcd Map(NcdModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzNcd
            {
                CafId = model.CafId,
                CodingArea = model.CodingArea,
                CodingProofStamp = model.CodingProofStamp.ToArray(),
                CodingVersion = model.CodingVersion,
                IsSigned = model.Signed,
                IsValid = model.Valid,
                MotorolaSString = model.MotorolaSString,
                OBDCRC32 = model.Obdcrc32,
                ObdRelevantBytes = model.ObdRelevantBytes,
                Signature = model.Signature,
                SignatureBlockAddress = model.SignatureBlockAddress,
                SignatureLength = model.SignatureLength,
                TlIdBlockAddress = model.TlIdBlockAddress,
                UserDataCoding1 = model.UserDataCoding1?.Select(Coding1NcdEntryMapper.Map).ToList()
            };
        }

        internal static NcdModel Map(IPsdzNcd ncd)
        {
            if (ncd == null)
            {
                return null;
            }
            return new NcdModel
            {
                UserDataCoding2 = ncd.UserDataCoding2,
                CafId = ncd.CafId,
                CodingArea = ncd.CodingArea,
                CodingVersion = ncd.CodingVersion,
                Signed = ncd.IsSigned,
                Obdcrc32 = ncd.OBDCRC32,
                ObdRelevantBytes = ncd.ObdRelevantBytes,
                SignatureLength = ncd.SignatureLength
            };
        }
    }
}