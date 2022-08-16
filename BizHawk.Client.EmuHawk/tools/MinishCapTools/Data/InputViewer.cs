using System;
using System.Collections.Generic;
using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Data
{
    public class InputViewer
    {
		public bool Show { get; set; }
        
        public bool UseBorderedVersionOfDefaultImages { get; set; }
		
		public bool UseCustomButtonImages { get; set; }
		
		public List<InputViewerButtonConfig> ButtonConfiguration { get; set; }

		public InputViewerButtonConfig GetButtonConfigFromString(string button)
		{
			return button switch
			{
				"A" => ButtonConfiguration[(short)InputViewerButton.A],
				"B" => ButtonConfiguration[(short)InputViewerButton.B],
				"Start" => ButtonConfiguration[(short)InputViewerButton.Start],
				"Select" => ButtonConfiguration[(short)InputViewerButton.Select],
				"L" => ButtonConfiguration[(short)InputViewerButton.L],
				"R" => ButtonConfiguration[(short)InputViewerButton.R],
				"Up" => ButtonConfiguration[(short)InputViewerButton.Up],
				"Down" => ButtonConfiguration[(short)InputViewerButton.Down],
				"Left" => ButtonConfiguration[(short)InputViewerButton.Left],
				"Right" => ButtonConfiguration[(short)InputViewerButton.Right],
				_ => null
			};
		}
        
        public void RestoreDefaults()
        {
            const int baseX = 1;
            const int baseY = 133;
            Show = true;
            UseBorderedVersionOfDefaultImages = true;
			ButtonConfiguration = new List<InputViewerButtonConfig>
			{
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.A, X = baseX + 45, Y = baseY + 9
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.B, X = baseX + 34, Y = baseY + 14
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.L, X = baseX + 1, Y = baseY
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.R, X = baseX + 37, Y = baseY
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Start, X = baseX + 16, Y = baseY + 15
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Select, X = baseX + 25, Y = baseY + 15
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.DPad, X = baseX, Y = baseY + 10
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Up, X = baseX + 4, Y = baseY + 10
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Down, X = baseX + 4, Y = baseY + 20
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Left, X = baseX, Y = baseY + 14
				},
				new InputViewerButtonConfig
				{
					Button = InputViewerButton.Right, X = baseX + 10, Y = baseY + 14
				},
			};
		}
    }
}