using System.Collections.Generic;

namespace MinishCapTools.Data
{
    public class InputViewer
    {
        public int X { get; set; }
        
        public int Y { get; set; }
        
        public bool Show { get; set; }
        
        public bool InputBorder { get; set; }
		
		public bool UseCustomButtonImages { get; set; }
		
		public List<InputViewerCustomButton> CustomButtonImages { get; set; }

        public void RestoreDefaultPosition()
        {
            X = 2;
            Y = 134;
        }
        
        public void RestoreDefaults()
        {
            X = 2;
            Y = 134;
            Show = true;
            InputBorder = true;
        }
    }
}