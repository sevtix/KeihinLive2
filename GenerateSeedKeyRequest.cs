using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeihinLive
{
    class GenerateSeedKeyRequest
    {
        public byte[] seed { get; }
        public byte level { get; }
        public string vin { get; }
        public string softwareVersion { get; }
        public string partNumber { get; }
        public string mappingType { get; }

        public GenerateSeedKeyRequest(byte[] seed, byte level, string vin, string softwareVersion, string partNumber, string mappingType)
        {
            this.seed = seed;
            this.level = level;
            this.vin = vin;
            this.softwareVersion = softwareVersion;
            this.partNumber = partNumber;
            this.mappingType = mappingType;
        }

    }
}
