using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Data
{
	public class InputViewerCustomButton
	{
		public InputViewerButton Button { get; set; }
		
		public string ButtonNotPressedImagePath { get; set; }
		
		public string ButtonPressedImagePath { get; set; }
		
		public int X { get; set; }
		
		public int Y { get; set; }
	}
}