namespace MinishCapTools.Data
{
	public class DefaultSettings
	{
		public DefaultSettings() { }

		public DefaultSettings(bool initializeComponentsAsDefault)
		{
			BackgroundChroma = new BackgroundChroma();
			BackgroundChroma.RestoreDefaults();
			Padding = new Padding();
			Padding.RestoreDefaults();
			InputViewer = new InputViewer();
			InputViewer.RestoreDefaults();
			MovieMode = false;
			EnableAutoSplitter = false;
		}

		public DefaultSettings(Settings settingsToCopy, bool setAsDefaultInSettings = false)
		{
			MovieMode = settingsToCopy.MovieMode;
			EnableAutoSplitter = settingsToCopy.EnableAutoSplitter;
			BackgroundChroma = settingsToCopy.BackgroundChroma;
			Padding = settingsToCopy.Padding;
			InputViewer = settingsToCopy.InputViewer;

			if (setAsDefaultInSettings)
				settingsToCopy.DefaultSettings = this;
		}
		
		public bool MovieMode { get; set; }
        
		public bool EnableAutoSplitter { get; set; }
        
		public BackgroundChroma BackgroundChroma { get; set; }
        
		public Padding Padding { get; set; }
        
		public InputViewer InputViewer { get; set; }
	}
}