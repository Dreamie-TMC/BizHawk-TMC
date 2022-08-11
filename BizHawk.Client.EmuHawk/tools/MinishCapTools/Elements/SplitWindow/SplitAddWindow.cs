using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BizHawk.Client.EmuHawk.Properties;
using MinishCapTools.Elements.AutoSplitterHelpers;
using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Elements.SplitWindow
{
    public partial class SplitAddWindow : Form
    {
        public Split LastUsedSplit { get; private set; }
        public bool SplitSaved { get; private set; }
        
        public SplitAddWindow()
        {
            InitializeComponent();
            SplitSaved = false;
            try
            {
                Icon = Resources.MinishCapToolsIcon;
            }
            catch
            {
                //ignored
            }

            foreach (SplitTypes splitType in Enum.GetValues(typeof(SplitTypes)))
            {
                if (splitType == 0) continue;
                SplitTypes.Items.Add(splitType);
            }

            SplitTypes.SelectedIndex = 0;

            foreach (MemoryDomain domain in Enum.GetValues(typeof(MemoryDomain)))
            {
                MemoryDomains.Items.Add(domain);
            }

            MemoryDomains.SelectedIndex = 0;
            MemoryDomains.Enabled = false;
        }

        public void LoadSplit(Split split)
        {
            SplitName.Text = split.Name;
            SplitTypes.SelectedIndex = (int)split.SplitType - 1;
            MemoryDomains.SelectedIndex = (int)split.Domain;
            switch (split.SplitType)
            {
                case Enums.SplitTypes.Start:
                    break;
                case Enums.SplitTypes.AreaEnter:
                    Address.Text = Convert.ToString(split.AreaId, 16);
                    Value.Text = Convert.ToString(split.RoomId, 16);
                    break;
                case Enums.SplitTypes.Flag:
                    Address.Text = Convert.ToString(split.Address, 16);
                    Value.Text = $@"{split.Bit + 1}";
                    break;
                case Enums.SplitTypes.Boss:
                    Address.Text = Convert.ToString(split.Address, 16);
                    Value.Text = $@"{split.Value}";
                    break;
                default:
                    break;
            }

            Enabled.Checked = split.Enabled;
        }

        private void SplitTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (SplitTypes.SelectedIndex + 1)
            {
                case (int)Enums.SplitTypes.AreaEnter:
                    MemoryDomains.Enabled = false;
                    Address.Clear();
                    Value.Clear();
                    AddressText.Text = @"Area ID:";
                    ValueText.Text = @"Room ID:";
                    break;
                case (int)Enums.SplitTypes.Flag:
                    MemoryDomains.Enabled = true;
                    Address.Clear();
                    Value.Clear();
                    MemoryDomains.SelectedIndex = 1;
                    AddressText.Text = @"Address:";
                    ValueText.Text = @"Bit:";
                    break;
                case (int)Enums.SplitTypes.Boss:
                    MemoryDomains.Enabled = true;
                    Address.Clear();
                    Value.Clear();
                    MemoryDomains.SelectedIndex = 0;
                    AddressText.Text = @"Address:";
                    ValueText.Text = @"Value:";
                    break;
                default:
                    break;
            }
        }

        private void SaveSplit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Address.Text) || string.IsNullOrWhiteSpace(Value.Text) ||
                string.IsNullOrWhiteSpace(SplitName.Text))
            {
                MessageBox.Show(@"Required fields are missing, cannot save split!", @"Cannot Save Split!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var split = new Split();
                switch (SplitTypes.SelectedIndex + 1)
                {
                    case (int)Enums.SplitTypes.AreaEnter:
                        split.SplitType = Enums.SplitTypes.AreaEnter;
                        split.Domain = MemoryDomain.IWRAM;
                        split.AreaId = int.Parse(Address.Text, NumberStyles.HexNumber);
                        split.RoomId = int.Parse(Value.Text, NumberStyles.HexNumber);
                        split.Name = SplitName.Text;
                        break;
                    case (int)Enums.SplitTypes.Flag:
                        split.SplitType = Enums.SplitTypes.Flag;
                        split.Domain = (MemoryDomain)MemoryDomains.SelectedIndex;
                        split.Address = uint.Parse(Address.Text, NumberStyles.HexNumber);
                        split.Bit = int.Parse(Value.Text) - 1;
                        split.Name = SplitName.Text;
                        break;
                    case (int)Enums.SplitTypes.Boss:
                        split.SplitType = Enums.SplitTypes.Boss;
                        split.Domain = (MemoryDomain)MemoryDomains.SelectedIndex;
                        split.Address = uint.Parse(Address.Text, NumberStyles.HexNumber);
                        split.Value = int.Parse(Value.Text);
                        split.Name = SplitName.Text;
                        break;
                    default:
                        return;
                }
                split.Enabled = Enabled.Checked;
                LastUsedSplit = split;
                SplitSaved = true;
                Close();
            }
            catch
            {
                MessageBox.Show(@"Invalid data provided, cannot save split!
Make sure no values have 0x at the beginning!", @"Cannot Save Split!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you want to cancel?", @"Cancel?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes) return;
            
            Close();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            using var help = new HelpWindow();
            help.Show(@"How to use:
Area and Room ID's can be found here:
    https://docs.google.com/spreadsheets/d/1FSWWUMHTdHmIKghCwsY_BEjOFSZWz3x_kX4co34qgvw/
Addresses for Flag and Boss Splits can be found here:
    https://docs.google.com/spreadsheets/d/11Ve770jjf7Y1dgf0kqWKlCjBpaNyw0XBkp-ayxeXJvg/
For Flag splits, the 'Bit' value gotten by the index where you see '1' from right to left.
For example: Ezlo is 00010000 which is the 5th bit, 10000000 would be 8th bit, and so on.
Note: Make sure you do not include the '0x' before hex numbers!", this);
        }
    }
}