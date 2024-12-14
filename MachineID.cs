using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace KeihinLive
{
    class MachineID
    {
        public static string fingerPrint = string.Empty;

        public static string GetMachineID()
        {
            if (string.IsNullOrEmpty(fingerPrint))
            {
                fingerPrint = GetHash("CPU >> " + CPU_ID() + "\nBIOS >> " + BIOS_ID() + "\nBASE >> " + BASE_ID() +
                                      "\nDISK >> " + DISK_ID());
            }

            return fingerPrint;
        }

        private static string GetHash(string s)
        {
            MD5 sec = MD5.Create();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }

        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }

            return s;
        }

        private static string GetProperty(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            foreach (ManagementObject mo in new ManagementClass(wmiClass).GetInstances())
            {
                if (bool.Parse(mo[wmiMustBeTrue].ToString()))
                {
                    try
                    {
                        return mo[wmiProperty].ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while getting property {wmiClass}, {wmiProperty}. ex: {ex.Message}");
                    }
                }
            }

            return "";
        }

        private static string GetProperty(string wmiClass, string wmiProperty)
        {
            foreach (ManagementObject mo in new ManagementClass(wmiClass).GetInstances())
            {
                try
                {
                    return mo[wmiProperty].ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while getting property {wmiClass}, {wmiProperty}. ex: {ex.Message}");
                }
            }

            return "";
        }

        private static string GetProperty(ManagementObjectSearcher searcher, string wmiClass, string wmiProperty)
        {
            foreach (ManagementObject mo in searcher.Get())
            {
                if (mo[wmiProperty] != null) return mo[wmiProperty].ToString();
            }

            return "";
        }

        private static string search(string wmiClass, params string[] wmiProperty)
        {
            string query = string.Join(", ", wmiProperty);
            query = $"SELECT {query} FROM {wmiClass}";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);

            string id = "";

            foreach (string property in wmiProperty)
            {
                id += GetProperty(searcher, wmiClass, property);
            }

            return id;
        }

        private static string CPU_ID()
        {
            return search("Win32_Processor", "UniqueId", "ProcessorId", "Name", "Manufacturer", "MaxClockSpeed");
        }

        private static string BIOS_ID()
        {
            return search("Win32_BIOS", "Manufacturer", "SMBIOSBIOSVersion", "IdentificationCode", "SerialNumber",
                "ReleaseDate", "Version");
        }

        private static string DISK_ID()
        {
            return search("Win32_DiskDrive", "Model", "Manufacturer", "Signature", "TotalHeads");
        }

        private static string BASE_ID()
        {
            return search("Win32_BaseBoard", "Model", "Manufacturer", "Name", "SerialNumber");
        }

        private static string VIDEO_ID()
        {
            return GetProperty("Win32_VideoController", "DriverVersion") + GetProperty("Win32_VideoController", "Name");
        }

        private static string MAC_ID()
        {
            return GetProperty("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
    }

}
