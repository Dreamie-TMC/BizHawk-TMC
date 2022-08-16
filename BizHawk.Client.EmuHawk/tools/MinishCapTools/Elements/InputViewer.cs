using System.Collections.Generic;
using System.Drawing;
using BizHawk.Client.EmuHawk;
using BizHawk.Client.EmuHawk.Properties;
using MinishCapTools.Data;
using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Elements
{
    public class InputViewer
    {
        #region Offsets

        private static int UnlockTextX => 15;
        private static int UnlockTextY => 3;
		#endregion

        public bool UnlockButton { get; set; }
		public InputViewerButton UnlockedButtonId { get; set; }

		public InputViewer(GuiApi gui)
		{
			gui.CacheImage("A.png", Resources.A);
			gui.CacheImage("B.png", Resources.B);
			gui.CacheImage("L.png", Resources.L);
			gui.CacheImage("R.png", Resources.R);
			gui.CacheImage("Start.png", Resources.Start);
			gui.CacheImage("Select.png", Resources.Select);
			gui.CacheImage("DPad.png", Resources.DPad);
			gui.CacheImage("Down_Pressed.png", Resources.DownPressed);
			gui.CacheImage("Left_Pressed.png", Resources.LeftPressed);
			gui.CacheImage("Right_Pressed.png", Resources.RightPressed);
			gui.CacheImage("Up_Pressed.png", Resources.UpPressed);
			gui.CacheImage("A_Pressed.png", Resources.APressed);
			gui.CacheImage("B_Pressed.png", Resources.BPressed);
			gui.CacheImage("L_Pressed.png", Resources.LPressed);
			gui.CacheImage("R_Pressed.png", Resources.RPressed);
			gui.CacheImage("Start_Pressed.png", Resources.StartPressed);
			gui.CacheImage("Select_Pressed.png", Resources.SelectPressed);
		}
        
        public void Draw(GuiApi gui, Settings config, IDictionary<string, object> mouse)
        {
			if (UnlockButton)
			{
				gui.DrawText(UnlockTextX, UnlockTextY, $"{UnlockedButtonId} Button Unlocked", Color.MediumVioletRed);
				if (mouse.TryGetValue("Left", out var left) && left is true)
				{
					config.InputViewer.ButtonConfiguration[(short)UnlockedButtonId].X = 
						(int)mouse["X"] + (config.Padding.Enabled ? config.Padding.LeftWidth : 0);
					config.InputViewer.ButtonConfiguration[(short)UnlockedButtonId].Y = 
						(int)mouse["Y"] + (config.Padding.Enabled ? config.Padding.TopHeight : 0);
				}
			}
			
			gui.DrawImageFromCache("DPad.png", 
				config.InputViewer.ButtonConfiguration[(short)InputViewerButton.DPad].X, 
				config.InputViewer.ButtonConfiguration[(short)InputViewerButton.DPad].Y);
			
			foreach (var input in InputHandler.Inputs)
			{
				if (input.Value is true)
				{
					var button = config.InputViewer.GetButtonConfigFromString(input.Key);
					if (button == null) continue;
					
					gui.DrawImageFromCache($@"{input.Key}_Pressed.png", button.X, button.Y);
				}
				else
				{
					var button = config.InputViewer.GetButtonConfigFromString(input.Key);
					if (button == null) continue;
					
					gui.DrawImageFromCache($@"{input.Key}.png", button.X, button.Y);
				}
			}
			// gui.DrawImageFromCache("Layout.png", config.InputViewer.X, config.InputViewer.Y);
   //              
   //          if (config.InputViewer.UseBorderedVersionOfDefaultImages)
   //              gui.DrawImageFromCache("Border.png", config.InputViewer.X + BorderOffsetX, config.InputViewer.Y + BorderOffsetY);
   //
   //          if (UnlockInputViewer)
   //          {
   //              gui.DrawText(UnlockTextX, UnlockTextY, "Input Viewer Unlocked", Color.MediumVioletRed);
   //              if (mouse.TryGetValue("Left", out var left) && left is true)
   //              {
   //                  config.InputViewer.X = (int)mouse["X"] + (config.Padding.Enabled ?
   //                      config.Padding.LeftWidth : 0) + XOffset;
   //                  config.InputViewer.Y = (int)mouse["Y"] + (config.Padding.Enabled ?
   //                      config.Padding.TopHeight : 0) + YOffset;
   //              }
   //          }
   //
   //          foreach (var input in InputHandler.Inputs)
   //          {
   //              if (input.Value is true)
   //              {
   //                  gui.DrawImageFromCache(@$"{input.Key}.png", config.InputViewer.X, config.InputViewer.Y);
   //              }
   //          }
        }
    }
}