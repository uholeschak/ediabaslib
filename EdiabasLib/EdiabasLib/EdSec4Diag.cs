using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace EdiabasLib
{
    public static class EdSec4Diag
    {
        public const string S29ProofOfOwnershipPrefix = "S29UNIPOO";

        public class CertReqProfile
        {
            [DataContract]
            public enum EnumType
            {
                [EnumMember]
                crp_subCA_4ISTA,
                [EnumMember]
                crp_subCA_4ISTA_TISonly,
                [EnumMember]
                crp_M2M_3dParty_4_CUST_ControlOnly
            }
        }

        public class ProofOfPossession
        {
            [JsonProperty("signatureType")]
            public string SignatureType { get; set; }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }

        public class Sec4DiagRequestData
        {
            [JsonProperty("vin17")]
            public string Vin17 { get; set; }

            [JsonProperty("certReqProfile")]
            public string CertReqProfile { get; set; }

            [JsonProperty("publicKey")]
            public string PublicKey { get; set; }

            [JsonProperty("proofOfPossession")]
            public ProofOfPossession ProofOfPossession { get; set; }
        }

        public class Sec4DiagResponseData
        {
            [JsonProperty("vin17")]
            public string Vin17 { get; set; }

            [JsonProperty("certificate")]
            public string Certificate { get; set; }

            [JsonProperty("certificateChain")]
            public string[] CertificateChain { get; set; }
        }

        private static BigInteger[] SignDataBytes(byte[] message, ECPrivateKeyParameters privateKey)
        {
            Sha512Digest sha512Digest = new Sha512Digest();
            sha512Digest.BlockUpdate(message, 0, message.Length);
            byte[] array = new byte[sha512Digest.GetDigestSize()];
            sha512Digest.DoFinal(array, 0);
            ECDsaSigner eCDsaSigner = new ECDsaSigner();
            eCDsaSigner.Init(forSigning: true, privateKey);
            return eCDsaSigner.GenerateSignature(array);
        }

        private static bool VerifyDataSignature(byte[] message, BigInteger[] signatureInts, ECPublicKeyParameters publicKey)
        {
            Sha512Digest sha512Digest = new Sha512Digest();
            sha512Digest.BlockUpdate(message, 0, message.Length);
            byte[] array = new byte[sha512Digest.GetDigestSize()];
            sha512Digest.DoFinal(array, 0);
            ECDsaSigner eCDsaSigner = new ECDsaSigner();
            eCDsaSigner.Init(forSigning: false, publicKey);
            return eCDsaSigner.VerifySignature(array, signatureInts[0], signatureInts[1]);
        }

        public static byte[] CalculateProofOfOwnership(byte[] server_challenge, ECPrivateKeyParameters privateKey)
        {
            try
            {
                if (server_challenge == null || privateKey == null)
                {
                    return null;
                }

                byte[] randomData = new byte[16];
                RandomNumberGenerator.Create().GetBytes(randomData);
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipPrefix);
                int prefixLength = prefixBytes.Length;
                byte[] signData = new byte[prefixLength + randomData.Length + server_challenge.Length + 2];
                prefixBytes.CopyTo(signData, 0);
                randomData.CopyTo(signData, prefixLength);
                server_challenge.CopyTo(signData, prefixLength + randomData.Length);
                signData[signData.Length - 2] = 0;
                signData[signData.Length - 1] = 16;

                BigInteger[] signatureInts = SignDataBytes(signData, privateKey);
                byte[] integerPart1 = signatureInts[0].ToByteArrayUnsigned();
                byte[] integerPart2 = signatureInts[1].ToByteArrayUnsigned();
                byte[] integerData = new byte[integerPart1.Length + integerPart2.Length];
                Buffer.BlockCopy(integerPart1, 0, integerData, 0, integerPart1.Length);
                Buffer.BlockCopy(integerPart2, 0, integerData, integerPart1.Length, integerPart2.Length);

                byte[] resultData = new byte[randomData.Length + integerData.Length];
                Buffer.BlockCopy(randomData, 0, resultData, 0, randomData.Length);
                Buffer.BlockCopy(integerData, 0, resultData, randomData.Length, integerData.Length);

                return resultData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool VerifyProofOfOwnership(byte[] proofData, byte[] server_challenge, ECPublicKeyParameters publicKey)
        {
            try
            {
                if (proofData == null || server_challenge == null || publicKey == null)
                {
                    return false;
                }

                if (proofData.Length < 16 + 2 * 48)
                {
                    return false; // Minimum length check for random data and signature integers
                }

                byte[] randomData = new byte[16];
                Buffer.BlockCopy(proofData, 0, randomData, 0, randomData.Length);
                byte[] prefixBytes = Encoding.ASCII.GetBytes(S29ProofOfOwnershipPrefix);
                int prefixLength = prefixBytes.Length;
                byte[] signData = new byte[prefixLength + randomData.Length + server_challenge.Length + 2];
                prefixBytes.CopyTo(signData, 0);
                randomData.CopyTo(signData, prefixLength);
                server_challenge.CopyTo(signData, prefixLength + randomData.Length);
                signData[signData.Length - 2] = 0;
                signData[signData.Length - 1] = 16;

                BigInteger[] signatureInts = new BigInteger[2];
                int integerDataLength = (proofData.Length - randomData.Length) / 2;
                byte[] integerPart1 = new byte[integerDataLength];
                byte[] integerPart2 = new byte[integerDataLength];
                Buffer.BlockCopy(proofData, randomData.Length, integerPart1, 0, integerPart1.Length);
                Buffer.BlockCopy(proofData, randomData.Length + integerPart1.Length, integerPart2, 0, integerPart2.Length);
                signatureInts[0] = new BigInteger(1, integerPart1);
                signatureInts[1] = new BigInteger(1, integerPart2);

                if (!VerifyDataSignature(signData, signatureInts, publicKey))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
