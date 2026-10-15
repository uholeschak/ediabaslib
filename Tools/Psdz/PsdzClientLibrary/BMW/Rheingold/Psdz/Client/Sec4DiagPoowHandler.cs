using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PsdzClient;
using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    [ServiceKnownType(typeof(ECPrivateKeyParameters))]
    [DataContract]
    public class Sec4DiagPoowHandler
    {
        [DataMember]
        private readonly byte[] SerializedPrivateKey;

        public Sec4DiagPoowHandler(byte[] serializedPrivateKey)
        {
            SerializedPrivateKey = serializedPrivateKey;
        }

        internal byte[] CalculateProofOfOwnership(byte[] server_challenge)
        {
            ECPrivateKeyParameters privateKey = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(SerializedPrivateKey);
            byte[] array = new byte[16];
            RandomNumberGenerator.Create().GetBytes(array);
            int num = Encoding.ASCII.GetBytes("S29UNIPOO").Length;
            byte[] array2 = new byte[num + array.Length + server_challenge.Length + 2];
            Encoding.ASCII.GetBytes("S29UNIPOO").CopyTo(array2, 0);
            array.CopyTo(array2, num);
            server_challenge.CopyTo(array2, num + array.Length);
            array2[num + array.Length + server_challenge.Length + 2 - 2] = 0;
            array2[num + array.Length + server_challenge.Length + 2 - 1] = 16;
            BigInteger[] array3 = SignDataByte(array2, privateKey);
            byte[] array4 = array3[0].ToByteArrayUnsigned();
            byte[] array5 = array3[1].ToByteArrayUnsigned();
            byte[] array6 = new byte[array4.Length + array5.Length];
            Buffer.BlockCopy(array4, 0, array6, 0, array4.Length);
            Buffer.BlockCopy(array5, 0, array6, array4.Length, array5.Length);
            byte[] array7 = new byte[array.Length + array6.Length];
            Buffer.BlockCopy(array, 0, array7, 0, array.Length);
            Buffer.BlockCopy(array6, 0, array7, array.Length, array6.Length);
            _ = new byte[array7.Length];
            return array7;
        }

        private static BigInteger[] SignDataByte(byte[] message, ECPrivateKeyParameters privateKey)
        {
            Sha512Digest sha512Digest = new Sha512Digest();
            sha512Digest.BlockUpdate(message, 0, message.Length);
            byte[] array = new byte[sha512Digest.GetDigestSize()];
            sha512Digest.DoFinal(array, 0);
            ECDsaSigner eCDsaSigner = new ECDsaSigner();
            eCDsaSigner.Init(forSigning: true, privateKey);
            return eCDsaSigner.GenerateSignature(array);
        }
    }
}