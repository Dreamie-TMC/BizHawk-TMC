using System.Collections.Generic;
using System.Drawing;
using BizHawk.Client.EmuHawk;
using BizHawk.Client.EmuHawk.Properties;
using MinishCapTools.Data;

namespace MinishCapTools.Elements
{
    public class InputViewer
    {
        #region Offsets
        private static int XOffset => -25;
        private static int YOffset => -12;

        private static int UnlockTextX => 3;
        private static int UnlockTextY => 3;

        private static int BorderOffsetX => -1;
        private static int BorderOffsetY => -1;
        #endregion

        public bool UnlockInputViewer { get; set; }

		public InputViewer(GuiApi gui)
		{
			gui.CacheImage("Layout.png", Resources.Layout);
			gui.CacheImage("Border.png", Resources.Border);
			gui.CacheImage("A.png", Resources.A);
			gui.CacheImage("B.png", Resources.B);
			gui.CacheImage("L.png", Resources.L);
			gui.CacheImage("R.png", Resources.R);
			gui.CacheImage("Up.png", Resources.Up);
			gui.CacheImage("Down.png", Resources.Down);
			gui.CacheImage("Left.png", Resources.Left);
			gui.CacheImage("Right.png", Resources.Right);
			gui.CacheImage("Start.png", Resources.Start);
			gui.CacheImage("Select.png", Resources.Select);
		}
        
        public void Draw(GuiApi gui, Settings config, IDictionary<string, object> mouse)
        {
			gui.DrawImageFromCache("Layout.png", config.InputViewer.X, config.InputViewer.Y);
                
            if (config.InputViewer.InputBorder)
                gui.DrawImageFromCache("Border.png", config.InputViewer.X + BorderOffsetX, config.InputViewer.Y + BorderOffsetY);

            if (UnlockInputViewer)
            {
                gui.DrawText(UnlockTextX, UnlockTextY, "Input Viewer Unlocked", Color.MediumVioletRed);
                if (mouse.TryGetValue("Left", out var left) && left is true)
                {
                    config.InputViewer.X = (int)mouse["X"] + (config.Padding.Enabled ?
                        config.Padding.LeftWidth : 0) + XOffset;
                    config.InputViewer.Y = (int)mouse["Y"] + (config.Padding.Enabled ?
                        config.Padding.TopHeight : 0) + YOffset;
                }
            }

            foreach (var input in InputHandler.Inputs)
            {
                if (input.Value is true)
                {
                    gui.DrawImageFromCache(@$"{input.Key}.png", config.InputViewer.X, config.InputViewer.Y);
                }
            }
        }
    }
}