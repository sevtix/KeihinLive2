using SAE.J2534;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeihinLive
{
    public partial class Main : Form
    {

        string apiBaseURL = "https://api.sevtix.com:443";


        private BackendClient backendClient;
        private string fingerprint;
        private J2534Client Client;
        private KeihinECU ecu;

        public Dictionary<int, Label> labels = new Dictionary<int, Label>();
        public Dictionary<int, ComboBox> comboBoxes = new Dictionary<int, ComboBox>();

        public Main()
        {
            InitializeComponent();
        }

        private void Main_LoadAsync(object sender, EventArgs e)
        {
            labels.Add(1, label1);
            labels.Add(2, label2);
            labels.Add(3, label3);
            labels.Add(4, label4);
            labels.Add(5, label5);
            labels.Add(6, label6);
            labels.Add(7, label7);
            labels.Add(8, label8);

            ReadIdentifier tpsReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x00 };
            ReadIdentifier thReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x01 };
            ReadIdentifier mapReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x03 };
            ReadIdentifier cltReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x09 };
            ReadIdentifier iatReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x11 };
            ReadIdentifier tpsRefReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x23 };
            ReadIdentifier gearAdcReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x30 };
            ReadIdentifier gearReadIdentifier = new ReadIdentifier() { Identifier0 = 0x00, Identifiert1 = 0x31 };
            ReadIdentifier rpmReadIdentifier = new ReadIdentifier() { Identifier0 = 0x01, Identifiert1 = 0x00 };
            ReadIdentifier igaReadIdentifier = new ReadIdentifier() { Identifier0 = 0x01, Identifiert1 = 0x20 };

            List<LiveValue> items = new List<LiveValue>
            {
                new() { Id = 1, Name = "RPM", ReadIdentifier = rpmReadIdentifier},
                new() { Id = 1, Name = "TPS", ReadIdentifier = tpsReadIdentifier},
                new() { Id = 1, Name = "TH", ReadIdentifier = thReadIdentifier},
                new() { Id = 1, Name = "MAP", ReadIdentifier = mapReadIdentifier},
                new() { Id = 1, Name = "Gear", ReadIdentifier = gearReadIdentifier},
                new() { Id = 1, Name = "GPS", ReadIdentifier = gearAdcReadIdentifier},
                new() { Id = 1, Name = "IGA", ReadIdentifier = igaReadIdentifier},
                new() { Id = 1, Name = "CLT", ReadIdentifier = cltReadIdentifier},
                new() { Id = 1, Name = "IAT", ReadIdentifier = iatReadIdentifier},
                new() { Id = 1, Name = "TPSref", ReadIdentifier = tpsReadIdentifier},
            };

            // Bind dataset to ComboBox
            checkedListBox1.DataSource = items;
            checkedListBox1.DisplayMember = "Name"; // Property to display
            checkedListBox1.ValueMember = "Id";    // Hidden value to retrieve

            Init();
        }

        private void Init()
        {
            fingerprint = MachineID.GetMachineID();
            backendClient = new BackendClient(apiBaseURL, fingerprint);
            string DllFileName = APIFactory.GetAPIinfo().First().Filename;
            Client = new J2534Client(DllFileName, 0xD5, 0xF5);
            ecu = new KeihinECU(Client);
            Client.Start();
        }

        bool polling = true;

        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox1.Checked == true)
                {

                    if(checkedListBox1.CheckedItems.Count > 8)
                    {
                        MessageBox.Show("Maximum data count is 8");
                        return;
                    }

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

                                    Invoke(new Action(() => labels[labelIndex].BackColor = Color.Green));
                                    byte[] rawResult = ecu.ReadByIdentifier2(readIdentifier.Identifier0, readIdentifier.Identifiert1);
                                    byte[] resp16 = new byte[] { rawResult[1], rawResult[0] };
                                    var intValue = BitConverter.ToUInt16(resp16, 0);
                                    Invoke(new Action(() => labels[labelIndex].Text = $"{val.Name}: {intValue}"));
                                    Invoke(new Action(() => labels[labelIndex].BackColor = Color.LightGreen));
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
                Client.Stop();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (comboBox8.SelectedValue != null)
            {
                // Get the selected Id (hidden value)
                int selectedId = (int)comboBox8.SelectedValue;
                MessageBox.Show($"Selected Id: {selectedId}");

                // Find the corresponding LiveValue object
                var selectedItem = ((List<LiveValue>)comboBox8.DataSource)
                    .FirstOrDefault(x => x.Id == selectedId);

                if (selectedItem != null)
                {
                    MessageBox.Show($"Selected LiveValue: {selectedItem.Name}, Id: {selectedItem.Id}");
                }
            }*/
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
