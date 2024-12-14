using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;

namespace KeihinLive
{
    enum ECUMemory
    {
        ROM = 0x0,
        EEPROM = 0x3,
        RAM = 0xFE, //placeholder
        ROM_FULL = 0xFF //placeholder, actual segment-id = 0
    }
    class KeihinECU(J2534Client Client) : ISO14230_4(Client)
    {

        #region Identification Commands
        public string ReadVIN() => Encoding.ASCII.GetString(ReadByIdentifier(0x01).Skip(2).ToArray());
        public string ReadPartNumber() => Encoding.ASCII.GetString(ReadByIdentifier(0x02).Skip(2).ToArray());
        public string ReadSoftwareVersion() => Encoding.ASCII.GetString(ReadByIdentifier(0x05).Skip(2).ToArray());
        public string ReadMappingType() => Encoding.ASCII.GetString(ReadByIdentifier(0x07).Skip(2).ToArray());
        #endregion

        public async Task<bool> UnlockECU(UnlockLevel level, BackendClient backendClient)
        {
            byte[] seedBytes = RequestSeed((byte)level);
            if (seedBytes.Length != 2)
                return false;

            var vin = ReadVIN();
            var softwareVersion = ReadSoftwareVersion();
            var partNumber = ReadPartNumber();
            var mappingType = ReadMappingType();

            GenerateSeedKeyRequest generateSeedKeyRequest = new GenerateSeedKeyRequest(seedBytes, (byte)level, vin, softwareVersion, partNumber, mappingType);
            GenerateSeedKeyResponse generateSeedKeyResponse = await backendClient.GenerateSeedKeyAsync(generateSeedKeyRequest);
            byte[] provideResp = ProvideKey((byte)level, generateSeedKeyResponse.key);
            if (provideResp[0] != 0x7F)
                return true;
            return false;
        }

        #region Exposed Methods
        public byte[] ReadECUBytes(ECUMemory mem, Action<int, string> progressCallback = null)
        {
            return mem switch
            {
                ECUMemory.ROM =>
                [
                    .. new byte[0x10000],
                    .. SendReadMemoryByAddress(0x10000, 0x3000, 0x80, mem, progressCallback),
                    .. new byte[0xAD000],
                    .. SendReadMemoryByAddress(0xC0000, 0x30000, 0x80, mem, progressCallback),
                    .. new byte[0x10000]
                ],
                ECUMemory.RAM =>
                [
                    .. SendReadMemoryByAddress(0, 0x10000, 0x80, ECUMemory.ROM, progressCallback),
                    .. new byte[0xF0000],
                ],
                ECUMemory.ROM_FULL => SendReadMemoryByAddress(0, 0x100000, 0x80, ECUMemory.ROM, progressCallback),
                ECUMemory.EEPROM => SendReadMemoryByAddress(0, 0x800, 0x80, mem, progressCallback),
                _ => throw new NotImplementedException($"{mem} read not implemented!")
            };
        }
        public bool FastMode()
        {
            var ret = Client.SendCommand([0xA5, 0x4A, 0x82]); //Switch Baud to 62500 resp=[E5 4A 82]
            if (ret[0] == 0xE5 && ret[1] == 0x4A && ret[2] == 0x82)
            {
                Client.SetBaudRate(62500);
                return true;
            }
            return false;
        }
        #endregion
    }
}