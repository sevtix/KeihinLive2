using SAE.J2534;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using Message = SAE.J2534.Message;

namespace KeihinLive
{
    class J2534Client
    {

        // J2534 Driver
        // Severin
        API fac;
        Device dev;
        SAE.J2534.Channel channel;
        private byte[] fastInitSequence = new byte[] { 0x81, 0xD5, 0xF5, 0x81 };
        private int timeoutMS = 2500;

        public uint baudRate { get; private set; } = 10400;
        public bool ConnectionState { get; private set; }

        // Constructor values
        private byte ecu_addr;
        private byte tester_addr;
        private bool debug = false;

        #region TODO: Move to Protocol Layer instead of Transportation Layer

        // Timeout values
        private Stopwatch timer = new(); // Send frame throttling
        private long lastFrameTime = 0; // Send frame throttling
        public int p2timeout = 100; // Min. time to wait between each frame
        public int p3timeout = 2000; // Max. time between each frames before timeout of diag. session
        public int p2stimeout = 250; // Max. time ECU has to respond to command

        #endregion

        public J2534Client(string dllPath, byte ecu_addr, byte tester_addr, uint initialBaudRate = 10400, bool debug = false)
        {
            this.ecu_addr = ecu_addr;
            this.tester_addr = tester_addr;
            this.baudRate = initialBaudRate;
            this.debug = debug;

            // Using Tactrix Driver
            fac = APIFactory.GetAPI(dllPath);
        }

        public void Start()
        {
            Connect();
            ConnectionState = true;
        }

        public void Stop()
        {

            // J2534
            channel.StopAllMsgFilters();
            channel.Dispose();
            dev.Dispose();
            fac.Dispose();

            timer.Stop(); // Stop the timer when connection is closed
            ConnectionState = false;
        }

        public void SetBaudRate(int baudRate)
        {
            channel.SetConfig(Parameter.DATA_RATE, baudRate);
        }

        public class DeviceInfo
        {
            public string Serial { get; set; }
            public string Name { get; set; }
        }

        public DeviceInfo[] GetConnectedDevices()
        {
            List<DeviceInfo> ret = new();
            uint nDevices = 0;

            List<string> deviceNames = fac.GetDeviceList();
            foreach (var deviceName in deviceNames)
            {
                Device device = fac.GetDevice(deviceName);
                ret.Add(new DeviceInfo { Serial = "", Name = $"{device.DeviceName}" });
            }

            return ret.ToArray();
        }

        private void Connect()
        {
            dev = fac.GetDevice();

            channel = dev.GetChannel(Protocol.ISO14230, Baud.ISO14230_10400, ConnectFlag.NONE);
            MessageFilter emptyFilter = new MessageFilter(UserFilterType.PASSALL, new byte[] { });
            channel.StartMsgFilter(emptyFilter);
            
            channel.DefaultTxTimeout = timeoutMS;
            channel.DefaultRxTimeout = timeoutMS;

            channel.SetConfig(Parameter.DATA_RATE, 10400);
            channel.SetConfig(Parameter.LOOP_BACK, 0);

            channel.SetConfig(Parameter.P1_MAX, 10); // 40
            channel.SetConfig(Parameter.P3_MIN, 50); // 55
            channel.SetConfig(Parameter.P4_MIN, 0); // 10

            channel.SetConfig(Parameter.W0, 1); // 300
            channel.SetConfig(Parameter.W1, 10); // 300
            channel.SetConfig(Parameter.W2, 20); // 20
            channel.SetConfig(Parameter.W3, 20); // 50
            channel.SetConfig(Parameter.W4, 20); // 50
            channel.SetConfig(Parameter.W5, 100); // 300
            
            channel.SetConfig(Parameter.TIDLE, 300); // 300
            channel.SetConfig(Parameter.TINIL, 25);
            channel.SetConfig(Parameter.TWUP, 50);
            channel.SetConfig(Parameter.PARITY, 0);


        }

        public bool FastInit(byte maxRetries = 3)
        {
            Message fastInitReponse = channel.FastInit(new Message(fastInitSequence));
            bool success = fastInitReponse.Data[4] == 0xC1;
            byte tries = 0;
            while (!success && tries++ <= maxRetries)
            {
                fastInitReponse = channel.FastInit(new Message(fastInitSequence));
                success = fastInitReponse.Data[4] == 0xC1;
            }
            return success;
        }

        private byte[] SendRaw(byte[] command, bool ignoreTimeout = false)
        {
            // First try
            // Send message
            channel.SendMessage(new Message(command));
            // Get response
            GetMessageResults Response = channel.GetMessages(2, timeoutMS);

            // Additional tries
            byte tries = 1;
            while (Response.Messages.Length < 2 && tries < 3)
            {
                // Resend Message
                channel.SendMessage(new Message(command));

                // Get response
                Response = channel.GetMessages(2, timeoutMS);

                // Inc tries
                tries++;
            }

            foreach (Message message in Response.Messages)
            {

                // No SOM flag packets
                if (message.RxStatus != RxFlag.START_OF_MESSAGE)
                {
                    // Grab byte array from message
                    byte[] dataPacket = message.Data;

                    // Return packet without header
                    return dataPacket.Skip(4).ToArray();
                }
            }

            if (!ignoreTimeout)
            {
                throw new Exception("No response from ECU");
            }
            else
            {
                return new byte[] { 0 };
            }
        }

        public byte[] SendCommand(byte[] command, bool ignoreTimeout = false)
        {
            byte length = (byte)(command.Length);
            byte[] header = { 0x80, ecu_addr, tester_addr, length };
            byte[] packet = new byte[header.Length + command.Length];
            Array.Copy(header, 0, packet, 0, header.Length);
            Array.Copy(command, 0, packet, header.Length, command.Length);
            var res = SendRaw(packet, ignoreTimeout);
            int tries = 0;
            while (res.Length == 0 && tries++ < 5)
            {
                Thread.Sleep(50);
                res = SendRaw(packet, ignoreTimeout);
            }

            return res;
        }
    }
}