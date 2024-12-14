namespace KeihinLive
{

    class ISO14230_4(J2534Client client)
    {

        public byte[] EraseRequest(byte segment)
        {
            byte[] resp = client.SendCommand([0x31, 0x90, segment]); //71 90 00
            return resp;
        }
        public byte[] TransferData(byte[] da)
        {
            byte[] cmd = [0x36, 0x00, 0x00, 0x00, 0x80, .. da];
            byte[] resp = client.SendCommand(cmd);
            return resp;
        }
        public byte[] DownloadRequest()
        {
            byte[] resp = client.SendCommand([
                0x34,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00
            ]); //74 44
            return resp;
        }
        public byte[] EndDownloadRequest()
        {
            byte[] resp = client.SendCommand([0x37]);
            return resp;
        }

        public byte[] TesterPresent()
        {
            byte[] response = client.SendCommand([0x3E], true);
            return response;
        }
        public byte[] Hardreset()
        {
            byte[] response = client.SendCommand([0x11, 0x01]);
            return response;
        }
        public byte[] ReadByIdentifier(byte identifier)
        {
            return client.SendCommand([0x1A, identifier]);
        }
        public byte[] ReadByIdentifier2(byte byte0, byte byte1)
        {
            return client.SendCommand(new byte[] { 0x22, byte0, byte1 }).Skip(3).ToArray();
        }

        public byte[] SendReadMemoryByAddress(uint startMemoryAddress = 0x0, uint length = 0xFFFF,
            byte blockSize = 0x80, ECUMemory memory = ECUMemory.ROM, Action<int, string> progressCallback = null)
        {
            List<byte> data = new();
            uint remainingLength = length;
            uint address = startMemoryAddress;

            uint totalLength = length;
            int lastReportedPercentage = 0;

            while (remainingLength > 0)
            {
                byte currentBlockSize = (byte)Math.Min(blockSize, remainingLength);
                var read = ReadMemoryByAddress(address, currentBlockSize, (byte)memory);

                if (read.Length == currentBlockSize)
                {
                    data.AddRange(read);
                    address += currentBlockSize;
                    remainingLength -= currentBlockSize;
                }
                else if (read.Length == 0)
                {
                    for (int i = 0; i < currentBlockSize; i++)
                    {
                        var singleByteRead = ReadMemoryByAddress(address, 1, (byte)memory);
                        if (singleByteRead.Length == 1)
                        {
                            data.Add(singleByteRead[0]);
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Offset {address:X5} skipped! ({UtilFunctions.ByteArrayToString(singleByteRead)})");
                            data.Add(0x00);
                        }

                        address++;
                        remainingLength--;
                    }
                }
                else
                {
                    Console.WriteLine($"Unexpected response length at address {address:X}: {read.Length}");
                    address += currentBlockSize;
                    remainingLength -= currentBlockSize;
                }

                int percentageCompleted = (int)(((totalLength - remainingLength) / (double)totalLength) * 100);
                if (percentageCompleted != lastReportedPercentage)
                {
                    progressCallback?.Invoke(percentageCompleted, $"Reading memory at address {address:X}");
                    lastReportedPercentage = percentageCompleted;
                }
            }

            progressCallback?.Invoke(100, "Memory read completed");
            return data.ToArray();
        }
        private byte[] ReadMemoryByAddress(uint address, byte len, byte segment)
        {

            byte[] command =
            [
                0x23,
                (byte) ((address >> 16) & 0xFF),
                (byte) ((address >> 8) & 0xFF),
                (byte) (address & 0xFF),
                len,
                segment,
            ];
            byte[] response = client.SendCommand(command);
            if (response.Length > 0 && response[0] == 0x7F) return Array.Empty<byte>();
            return response.Skip(1).ToArray();
        }

        public byte[] RequestSeed(byte securityLevel) =>
            client.SendCommand(new byte[] { 0x27, securityLevel }).Skip(2).ToArray();
        public byte[] ProvideKey(byte securityLevel, byte[] key) =>
            client.SendCommand((new byte[] { 0x27, (byte)(securityLevel + 1) }).Concat(key).ToArray());
    }
}