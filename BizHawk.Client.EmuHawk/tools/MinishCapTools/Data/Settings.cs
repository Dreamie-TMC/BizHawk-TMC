namespace MinishCapTools.Data
{
    public class Settings
    {
        public Settings()
        {
            BackgroundChroma = new BackgroundChroma();
            Padding = new Padding();
            InputViewer = new InputViewer();
			
			DefaultSettings = new DefaultSettings(true);
		}
        
        public bool MovieMode { get; set; }
        
        public bool EnableAutoSplitter { get; set; }
        
        public BackgroundChroma BackgroundChroma { get; set; }
        
        public Padding Padding { get; set; }
        
        public InputViewer InputViewer { get; set; }
		
		public DefaultSettings DefaultSettings { get; set; }

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