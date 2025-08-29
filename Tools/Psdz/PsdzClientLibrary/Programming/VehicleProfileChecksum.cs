using PsdzClient.Programming;
using System;
using System.Linq;
using System.Text;

namespace PsdzClient.Programming
{
    internal class VehicleProfileChecksum : IVehicleProfileChecksum, ICloneable
    {
        public byte[] VpcCrc { get; internal set; }

        public long VpcVersion { get; internal set; }

        public string VpcCrcAsHex
        {
            get
            {
                byte[] vpcCrc = VpcCrc;
                if (vpcCrc == null || !vpcCrc.Any())
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                byte[] vpcCrc2 = VpcCrc;
                foreach (byte b in vpcCrc2)
                {
                    stringBuilder.Append(b.ToString("X2"));
                }
                return stringBuilder.ToString();
            }
        }

        public object Clone()
        {
            return new VehicleProfileChecksum
            {
                VpcVersion = VpcVersion,
                VpcCrc = ((VpcCrc != null) ? ((byte[])VpcCrc?.Clone()) : null)
            };
        }

        public override string ToString()
        {
            return $"VPC Version: {VpcVersion} - Crc: '{VpcCrcAsString()}' - VPC in hex format: '{VpcCrcAsHex}'";
        }

        private string VpcCrcAsString()
        {
            byte[] vpcCrc = VpcCrc;
            if (vpcCrc != null && vpcCrc.Any())
            {
                return string.Join("/", VpcCrc);
            }
            return string.Empty;
        }
    }
}