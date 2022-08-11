namespace MinishCapTools.Data
{
    public class BackgroundChroma
    {
        public BackgroundChroma()
        {
            Color = "FFFFFFFF";
        }
        
        public bool Enabled { get; set; }
        
        public string Color { get; set; }
        
        public bool ShowOnLeft { get; set; }
        
        public bool ShowOnRight { get; set; }
        
        public bool ShowOnTop { get; set; }
        
        public bool ShowOnBottom { get; set; }

        public void RestoreDefaults()
        {
            Enabled = true;
            Color = "FF008CFF";
            ShowOnLeft = true;
            ShowOnRight = false;
            ShowOnTop = false;
            ShowOnBottom = false;
        }
    }
}