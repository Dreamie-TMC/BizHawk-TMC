namespace MinishCapTools.Data
{
    public class Settings
    {
        public Settings(bool initializeDefaults = true)
        {
            BackgroundChroma = new BackgroundChroma();
            Padding = new Padding();
            InputViewer = new InputViewer();
			
			if (!initializeDefaults) return;
			
			DefaultSettings = new Settings(false);
			DefaultSettings.BackgroundChroma.RestoreDefaults();
			DefaultSettings.Padding.RestoreDefaults();
			DefaultSettings.InputViewer.RestoreDefaults();
			DefaultSettings.MovieMode = false;
			DefaultSettings.EnableAutoSplitter = false;
		}
        
        public bool MovieMode { get; set; }
        
        public bool EnableAutoSplitter { get; set; }
        
        public BackgroundChroma BackgroundChroma { get; set; }
        
        public Padding Padding { get; set; }
        
        public InputViewer InputViewer { get; set; }
		
		public Settings DefaultSettings { get; set; }

        public void RestoreDefaults()
		{
			if (DefaultSettings != null)
			{
				BackgroundChroma = DefaultSettings.BackgroundChroma;
				Padding = DefaultSettings.Padding;
				InputViewer = DefaultSettings.InputViewer;
				MovieMode = DefaultSettings.MovieMode;
				EnableAutoSplitter = DefaultSettings.EnableAutoSplitter;
				return;
			}
            BackgroundChroma.RestoreDefaults();
            Padding.RestoreDefaults();
            InputViewer.RestoreDefaults();
            MovieMode = false;
            EnableAutoSplitter = false;
        }
    }
}