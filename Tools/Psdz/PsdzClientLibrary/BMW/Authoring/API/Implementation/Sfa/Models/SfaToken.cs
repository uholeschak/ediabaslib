using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.Implementation.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class SfaToken : SfaStatusInfo
    {
        public string TokenId { get; set; }
        public string TokenRaw { get; set; }
        public string SigningCert { get; set; }

        public byte[] GetEcuJobRawTokenRawBytes(string certFlag)
        {
            string hexTokenStringForEcuJob = GetHexTokenStringForEcuJob(certFlag);
            if (string.IsNullOrEmpty(hexTokenStringForEcuJob))
            {
                return null;
            }

            return FormatConverter.HexArray2ByteArray(hexTokenStringForEcuJob);
        }

        public string GetHexTokenStringForEcuJob(string certFlag)
        {
            return (Feature + certFlag + TokenRaw + SigningCert)?.Replace("0x", string.Empty);
        }
    }
}