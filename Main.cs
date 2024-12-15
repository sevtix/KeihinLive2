using SAE.J2534;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;

namespace KeihinLive
{
    public partial class Main : Form
    {

        string apiBaseURL = "https://api.sevtix.com:443";
        private bool vciConnected = false;
        private bool pollingData = false;

        private BackendClient backendClient;
        private string fingerprint;
        private J2534Client Client;
        private KeihinECU ecu;

        public Dictionary<int, Label> labels = new Dictionary<int, Label>();

        public Main()
        {
            InitializeComponent();
        }

        private void Main_LoadAsync(object sender, EventArgs e)
        {

            foreach (APIInfo apiInfo in APIFactory.GetAPIinfo().ToArray())
            {
                comboBox1.Items.Add(apiInfo.Filename);
            }

            comboBox1.SelectedIndex = 0;

            labels.Add(1, label1);
            labels.Add(2, label2);
            labels.Add(3, label3);
            labels.Add(4, label4);
            labels.Add(5, label5);
            labels.Add(6, label6);
            labels.Add(7, label7);
            labels.Add(8, label8);

            // 0x00 0xXX
            ReadIdentifier tpsReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x00 };
            ReadIdentifier thReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x01 };
            ReadIdentifier unknown0002 = new() { Identifier0 = 0x00, Identifier1 = 0x02 };
            ReadIdentifier mapReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x03 };
            ReadIdentifier unknown0006 = new() { Identifier0 = 0x00, Identifier1 = 0x06 };
            ReadIdentifier batteryVoltageIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x07 };
            ReadIdentifier cltAdcReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x08 };
            ReadIdentifier cltReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x09 };
            ReadIdentifier iatAdcReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x10 };
            ReadIdentifier iatReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x11 };
            ReadIdentifier unknown0012 = new() { Identifier0 = 0x00, Identifier1 = 0x12 };
            ReadIdentifier tpsRefReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x23 };
            ReadIdentifier unknown0024 = new() { Identifier0 = 0x00, Identifier1 = 0x24 };
            ReadIdentifier rollReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x28 };
            ReadIdentifier unknown0029 = new() { Identifier0 = 0x00, Identifier1 = 0x29 };
            ReadIdentifier gearAdcReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x30 };
            ReadIdentifier gearReadIdentifier = new() { Identifier0 = 0x00, Identifier1 = 0x31 };
            ReadIdentifier unknown0037 = new() { Identifier0 = 0x00, Identifier1 = 0x37 };
            ReadIdentifier unknown0044 = new() { Identifier0 = 0x00, Identifier1 = 0x44 };
            ReadIdentifier unknown0064 = new() { Identifier0 = 0x00, Identifier1 = 0x64 };
            ReadIdentifier unknown0066 = new() { Identifier0 = 0x00, Identifier1 = 0x66 };
            ReadIdentifier unknown0069 = new() { Identifier0 = 0x00, Identifier1 = 0x69 };
            ReadIdentifier unknown0078 = new() { Identifier0 = 0x00, Identifier1 = 0x78 };

            // 0x01 0xXX
            ReadIdentifier rpmReadIdentifier = new() { Identifier0 = 0x01, Identifier1 = 0x00 };
            ReadIdentifier unknown0102 = new() { Identifier0 = 0x01, Identifier1 = 0x02 };
            ReadIdentifier unknown0107 = new() { Identifier0 = 0x01, Identifier1 = 0x07 };
            ReadIdentifier unknown0110 = new() { Identifier0 = 0x01, Identifier1 = 0x10 };
            ReadIdentifier igaReadIdentifier = new() { Identifier0 = 0x01, Identifier1 = 0x20 };
            ReadIdentifier unknown0140 = new() { Identifier0 = 0x01, Identifier1 = 0x40 };
            ReadIdentifier unknown0142 = new() { Identifier0 = 0x01, Identifier1 = 0x42 };
            ReadIdentifier unknown0143 = new() { Identifier0 = 0x01, Identifier1 = 0x43 };
            ReadIdentifier unknown0149 = new() { Identifier0 = 0x01, Identifier1 = 0x49 };
            ReadIdentifier unknown0185 = new() { Identifier0 = 0x01, Identifier1 = 0x85 };
            ReadIdentifier unknown0188 = new() { Identifier0 = 0x01, Identifier1 = 0x88 };
            ReadIdentifier unknown0189 = new() { Identifier0 = 0x01, Identifier1 = 0x89 };
            ReadIdentifier unknown0197 = new() { Identifier0 = 0x01, Identifier1 = 0x97 };

            // 0x03 0xXX
            ReadIdentifier unknown0301 = new() { Identifier0 = 0x03, Identifier1 = 0x01 };

            // 0x04 0xXX
            ReadIdentifier unknown0400 = new() { Identifier0 = 0x04, Identifier1 = 0x00 };

            // 0x06 0xXX
            ReadIdentifier unknown0601 = new() { Identifier0 = 0x06, Identifier1 = 0x01 };
            ReadIdentifier adaptedGearNAdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x08 };
            ReadIdentifier unknown0609 = new() { Identifier0 = 0x06, Identifier1 = 0x09 };
            ReadIdentifier adaptedGear1AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x10 };
            ReadIdentifier adaptedGear2AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x11 };
            ReadIdentifier adaptedGear3AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x12 };
            ReadIdentifier adaptedGear4AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x13 };
            ReadIdentifier adaptedGear5AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x14 };
            ReadIdentifier adaptedGear6AdcReadIdentifier = new() { Identifier0 = 0x06, Identifier1 = 0x15 };
            ReadIdentifier unknown0616 = new() { Identifier0 = 0x06, Identifier1 = 0x16 };

            // 0x10 0xXX
            ReadIdentifier unknown1000 = new() { Identifier0 = 0x10, Identifier1 = 0x00 };
            ReadIdentifier unknown1001 = new() { Identifier0 = 0x10, Identifier1 = 0x01 };
            ReadIdentifier unknown1002 = new() { Identifier0 = 0x10, Identifier1 = 0x02 };
            ReadIdentifier unknown1004 = new() { Identifier0 = 0x10, Identifier1 = 0x04 };
            ReadIdentifier unknown1005 = new() { Identifier0 = 0x10, Identifier1 = 0x05 };
            ReadIdentifier unknown1006 = new() { Identifier0 = 0x10, Identifier1 = 0x06 };
            ReadIdentifier unknown1007 = new() { Identifier0 = 0x10, Identifier1 = 0x07 };
            ReadIdentifier unknown1009 = new() { Identifier0 = 0x10, Identifier1 = 0x09 };
            ReadIdentifier unknown1013 = new() { Identifier0 = 0x10, Identifier1 = 0x13 };
            ReadIdentifier unknown1014 = new() { Identifier0 = 0x10, Identifier1 = 0x14 };
            ReadIdentifier unknown1020 = new() { Identifier0 = 0x10, Identifier1 = 0x20 };
            ReadIdentifier unknown1030 = new() { Identifier0 = 0x10, Identifier1 = 0x30 };

            // 0x80 xXX
            ReadIdentifier unknown8000 = new() { Identifier0 = 0x80, Identifier1 = 0x00 };
            ReadIdentifier unknown8001 = new() { Identifier0 = 0x80, Identifier1 = 0x01 };
            ReadIdentifier unknown8004 = new() { Identifier0 = 0x80, Identifier1 = 0x04 };
            ReadIdentifier unknown8021 = new() { Identifier0 = 0x80, Identifier1 = 0x21 };
            ReadIdentifier unknown8031 = new() { Identifier0 = 0x80, Identifier1 = 0x31 };


            List<LiveValue> items = new List<LiveValue>
            {
                new() { Id = 1, Name = "RPM", ReadIdentifier = rpmReadIdentifier},
                new() { Id = 2, Name = "TPS", ReadIdentifier = tpsReadIdentifier},
                new() { Id = 3, Name = "TH", ReadIdentifier = thReadIdentifier, Formula = x => (x/2.55), Unit = "%"},
                new() { Id = 4, Name = "MAP", ReadIdentifier = mapReadIdentifier, Unit = "mbar"},
                new() { Id = 4, Name = "VBAT", ReadIdentifier = batteryVoltageIdentifier, Formula = x => (x/10), Unit = "V"},
                new() { Id = 5, Name = "GEAR", ReadIdentifier = gearReadIdentifier},
                new() { Id = 6, Name = "ROLL", ReadIdentifier = rollReadIdentifier},
                new() { Id = 6, Name = "GEAR_ADC", ReadIdentifier = gearAdcReadIdentifier},
                new() { Id = 7, Name = "IGA", ReadIdentifier = igaReadIdentifier, Formula = x => (x/2)-64, Unit = "°"},
                new() { Id = 8, Name = "CLT", ReadIdentifier = cltReadIdentifier, Formula = x => x - 40, Unit = "°C"},
                new() { Id = 9, Name = "IAT", ReadIdentifier = iatReadIdentifier, Formula = x => x - 40, Unit = "°C"},
                new() { Id = 10, Name = "TPSref", ReadIdentifier = tpsRefReadIdentifier},

                new() { Id = 11, Name = "unknown0002", ReadIdentifier = unknown0002},
                new() { Id = 12, Name = "unknown0006", ReadIdentifier = unknown0006},
                new() { Id = 13, Name = "CLT_ADC", ReadIdentifier = cltAdcReadIdentifier},
                new() { Id = 14, Name = "IAT_ADC", ReadIdentifier = iatAdcReadIdentifier},
                new() { Id = 38, Name = "GEAR_N", ReadIdentifier = adaptedGearNAdcReadIdentifier},
                new() { Id = 40, Name = "GEAR_1", ReadIdentifier = adaptedGear1AdcReadIdentifier},
                new() { Id = 41, Name = "GEAR_2", ReadIdentifier = adaptedGear2AdcReadIdentifier},
                new() { Id = 42, Name = "GEAR_3", ReadIdentifier = adaptedGear3AdcReadIdentifier},
                new() { Id = 43, Name = "GEAR_4", ReadIdentifier = adaptedGear4AdcReadIdentifier},
                new() { Id = 44, Name = "GEAR_5", ReadIdentifier = adaptedGear5AdcReadIdentifier},
                new() { Id = 45, Name = "GEAR_6", ReadIdentifier = adaptedGear6AdcReadIdentifier},
                new() { Id = 15, Name = "unknown0012", ReadIdentifier = unknown0012},
                new() { Id = 16, Name = "unknown0024", ReadIdentifier = unknown0024},
                new() { Id = 17, Name = "unknown0029", ReadIdentifier = unknown0029},
                new() { Id = 18, Name = "unknown0037", ReadIdentifier = unknown0037},
                new() { Id = 19, Name = "unknown0044", ReadIdentifier = unknown0044},
                new() { Id = 20, Name = "unknown0064", ReadIdentifier = unknown0064},
                new() { Id = 21, Name = "unknown0066", ReadIdentifier = unknown0066},
                new() { Id = 22, Name = "unknown0069", ReadIdentifier = unknown0069},
                new() { Id = 23, Name = "unknown0078", ReadIdentifier = unknown0078},
                new() { Id = 24, Name = "unknown0102", ReadIdentifier = unknown0102},
                new() { Id = 25, Name = "unknown0107", ReadIdentifier = unknown0107},
                new() { Id = 26, Name = "unknown0110", ReadIdentifier = unknown0110},
                new() { Id = 27, Name = "unknown0140", ReadIdentifier = unknown0140},
                new() { Id = 28, Name = "unknown0142", ReadIdentifier = unknown0142},
                new() { Id = 29, Name = "unknown0143", ReadIdentifier = unknown0143},
                new() { Id = 30, Name = "unknown0149", ReadIdentifier = unknown0149},
                new() { Id = 31, Name = "unknown0185", ReadIdentifier = unknown0185},
                new() { Id = 32, Name = "unknown0188", ReadIdentifier = unknown0188},
                new() { Id = 33, Name = "unknown0189", ReadIdentifier = unknown0189},
                new() { Id = 34, Name = "unknown0197", ReadIdentifier = unknown0197},
                new() { Id = 35, Name = "unknown0301", ReadIdentifier = unknown0301},
                new() { Id = 36, Name = "unknown0400", ReadIdentifier = unknown0400},
                new() { Id = 37, Name = "unknown0601", ReadIdentifier = unknown0601},
                new() { Id = 39, Name = "unknown0609", ReadIdentifier = unknown0609},
                new() { Id = 46, Name = "unknown0616", ReadIdentifier = unknown0616},
                new() { Id = 47, Name = "unknown1000", ReadIdentifier = unknown1000},
                new() { Id = 48, Name = "unknown1001", ReadIdentifier = unknown1001},
                new() { Id = 49, Name = "unknown1002", ReadIdentifier = unknown1002},
                new() { Id = 50, Name = "unknown1004", ReadIdentifier = unknown1004},
                new() { Id = 51, Name = "unknown1005", ReadIdentifier = unknown1005},
                new() { Id = 52, Name = "unknown1006", ReadIdentifier = unknown1006},
                new() { Id = 53, Name = "unknown1007", ReadIdentifier = unknown1007},
                new() { Id = 54, Name = "unknown1009", ReadIdentifier = unknown1009},
                new() { Id = 55, Name = "unknown1013", ReadIdentifier = unknown1013},
                new() { Id = 56, Name = "unknown1014", ReadIdentifier = unknown1014},
                new() { Id = 57, Name = "unknown1020", ReadIdentifier = unknown1020},
                new() { Id = 58, Name = "unknown1030", ReadIdentifier = unknown1030},
                new() { Id = 59, Name = "unknown8000", ReadIdentifier = unknown8000},
                new() { Id = 60, Name = "unknown8001", ReadIdentifier = unknown8001},
                new() { Id = 61, Name = "unknown8004", ReadIdentifier = unknown8004},
                new() { Id = 62, Name = "unknown8021", ReadIdentifier = unknown8021},
                new() { Id = 63, Name = "unknown8031", ReadIdentifier = unknown8031},
            };

            // Bind dataset to ComboBox
            checkedListBox1.DataSource = items;
            checkedListBox1.DisplayMember = "Name"; // Property to display
            checkedListBox1.ValueMember = "Id";    // Hidden value to retrieve
        }

        private void Init()
        {

        }

        bool polling = true;

        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                UpdateControls();

                if (checkBox1.Checked == true)
                {

                    if (checkedListBox1.CheckedItems.Count == 0)
                    {
                        checkBox1.Checked = false;
                        MessageBox.Show("Minimum data count is 1");
                        return;
                    }

                    if (checkedListBox1.CheckedItems.Count > 8)
                    {
                        checkBox1.Checked = false;
                        MessageBox.Show("Maximum data count is 8");
                        return;
                    }

                    ClearDataViews();

                    List<LiveValue> checkedLiveValues = new List<LiveValue>();
                    foreach (var checkedItem in checkedListBox1.CheckedItems)
                    {
                        checkedLiveValues.Add(checkedItem as LiveValue);
                    }

                    await Task.Run(async () =>
                    {
                        if (Client.FastInit(3))
                        {
                            UnlockLevel level = UnlockLevel.LEVEL_5;
                            if (!await ecu.UnlockECU(level, backendClient))
                            {
                                throw new Exception("ECU auth fail");
                            }
                            while (checkBox1.Checked)
                            {
                                int labelIndex = 1;
                                foreach (LiveValue val in checkedLiveValues)
                                {
                                    ReadIdentifier readIdentifier = val.ReadIdentifier;

                                    //Invoke(new Action(() => labels[labelIndex].BackColor = Color.Green));
                                    byte[] readResult = ecu.ReadByIdentifier2(readIdentifier.Identifier0, readIdentifier.Identifier1);
                                    byte[] resp16 = new byte[] { readResult[1], readResult[0] };
                                    var intValue = BitConverter.ToUInt16(resp16, 0);

                                    double displayedValue = intValue;
                                    if (val.Formula != null)
                                    {
                                        displayedValue = Math.Round(val.ApplyFormula(intValue), 1);
                                    }

                                    string displayedUnit = "";
                                    if (val.Unit != null)
                                    {
                                        displayedUnit = val.Unit;
                                    }

                                    Invoke(new Action(() => labels[labelIndex].Text = $"{val.Name}: {displayedValue}{displayedUnit}"));
                                    //Invoke(new Action(() => labels[labelIndex].BackColor = Color.LightGreen));
                                    labelIndex++;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                richTextBox1.AppendText(ex.Message + ex.StackTrace);
                richTextBox1.BackColor = Color.Yellow;
                Client.Stop();
            }
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                fingerprint = MachineID.GetMachineID();
                backendClient = new BackendClient(apiBaseURL, fingerprint);
                string DllFileName = comboBox1.Text;
                Client = new J2534Client(DllFileName, 0xD5, 0xF5);
                ecu = new KeihinECU(Client);
                Client.Start();

                vciConnected = true;
                UpdateControls();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(ex.Message + ex.StackTrace);
                richTextBox1.BackColor = Color.Yellow;
            }
        }

        private void UpdateControls()
        {
            checkBox1.Enabled = vciConnected;
            checkedListBox1.Enabled = vciConnected && !checkBox1.Checked;
            richTextBox2.Enabled = vciConnected;
        }

        private void ClearDataViews()
        {
            foreach (Label label in labels.Values) {
                label.Text = "No data";
            }
        }

        private async void richTextBox2_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                if (Client.FastInit(3))
                {
                    UnlockLevel level = UnlockLevel.LEVEL_5;
                    if (!await ecu.UnlockECU(level, backendClient))
                    {
                        throw new Exception("ECU auth fail");
                    }
                    var vin = ecu.ReadVIN();
                    var sw = ecu.ReadSoftwareVersion();
                    var mt = ecu.ReadMappingType();
                    var pn = ecu.ReadPartNumber();

                    Invoke(new Action(() =>
                        richTextBox2.Clear()
                    ));

                    Invoke(new Action(() =>
                        richTextBox2.AppendText($"VIN: {vin}\n")
                    ));

                    Invoke(new Action(() =>
                        richTextBox2.AppendText($"SW: {sw}\n")
                    ));

                    Invoke(new Action(() =>
                        richTextBox2.AppendText($"MT: {mt}\n")
                    ));

                    Invoke(new Action(() =>
                        richTextBox2.AppendText($"PN: {pn}\n")
                    ));
                }
            });
        }
    }
}
