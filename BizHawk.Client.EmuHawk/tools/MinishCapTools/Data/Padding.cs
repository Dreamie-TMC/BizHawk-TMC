namespace MinishCapTools.Data
{
    public class Padding
    {
        public bool Enabled { get; set; }
        
        public int LeftWidth { get; set; }
        
        public int RightWidth { get; set; }
        
        public int TopHeight { get; set; }
        
        public int BottomHeight { get; set; }

        public void RestoreDefaults()
        {
            Enabled = true;
            LeftWidth = 100;
            RightWidth = 0;
            TopHeight = 0;
            BottomHeight = 0;
        }
    }
}