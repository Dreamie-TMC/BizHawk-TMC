using System.Drawing;
using System.Globalization;
using BizHawk.Client.EmuHawk;
using MinishCapTools.Data;

namespace MinishCapTools.Elements
{
    public class BackgroundChroma
    {
        public void Draw(GuiApi gui, Settings config)
        {
            var color = Color.FromArgb(int.Parse(config.BackgroundChroma.Color, NumberStyles.HexNumber));

            var screenHeight = GlobalWin.MainForm.PresentationPanel.NativeSize.Height;
            var screenWidth = GlobalWin.MainForm.PresentationPanel.NativeSize.Width;
            var emuHeight = ClientApi.BufferHeight();
            var emuWidth = ClientApi.BufferWidth();
            
            if (config.BackgroundChroma.ShowOnLeft && config.Padding.LeftWidth > 0)
                gui.DrawRectangle(0, 0, config.Padding.LeftWidth - 1, screenHeight, color, color);
            
            if (config.BackgroundChroma.ShowOnRight && config.Padding.RightWidth > 0)
                gui.DrawRectangle(emuWidth + config.Padding.LeftWidth, 0, config.Padding.RightWidth - 1, screenHeight, color, color);
            
            if (config.BackgroundChroma.ShowOnTop && config.Padding.TopHeight > 0)
                gui.DrawRectangle(0, 0, screenWidth, config.Padding.TopHeight - 1, color, color);
            
            if (config.BackgroundChroma.ShowOnBottom && config.Padding.BottomHeight > 0)
                gui.DrawRectangle(0, emuHeight + config.Padding.TopHeight, screenWidth, config.Padding.BottomHeight - 1, color, color);
        }
    }
}