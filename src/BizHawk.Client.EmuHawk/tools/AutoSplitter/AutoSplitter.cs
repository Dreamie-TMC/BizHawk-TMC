using System;
using System.IO;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Client.EmuHawk.AutoSplitter
{
	[Tool(true, new[] { "GBA" })]
	public partial class AutoSplitter : ToolFormBase, IToolForm
	{
		
		[RequiredService]
		private IEmulator Emulator { get; set; }

		private MemoryApi _memoryApi;
		
		private Splitter _splitter;

		private bool _initialized;
		
		protected override string WindowTitleStatic => "Autosplitter";

		public AutoSplitter()
		{
			InitializeComponent();
			
			OpenSplits.InitialDirectory = Directory.GetCurrentDirectory();
		}

		private void Initialize()
		{
			//Just log to the console for debugging purposes
			_memoryApi = new MemoryApi(Console.WriteLine);
			ServiceInjector.UpdateServices(Emulator.ServiceProvider, _memoryApi);
			_splitter = new Splitter();
			_splitter.MemoryApi = _memoryApi;

			LoadSplitFile.Enabled = StartAutosplitter.Enabled = true;
			
			_initialized = true;
		}

		public override bool AskSaveChanges() => false;

		protected override void UpdateAfter()
		{
			if (!_initialized) Initialize();
			
			try
			{
				if (_splitter.UpdateSplitText)
				{
					CurrentSplit.Text = _splitter.Splits[_splitter.CurrentSplitId >= _splitter.Splits.Count ? _splitter.Splits.Count - 1 : _splitter.CurrentSplitId].Name;
					NextSplit.Text = _splitter.Splits[_splitter.CurrentSplitId >= _splitter.Splits.Count ? _splitter.Splits.Count - 1 : _splitter.CurrentSplitId].Name;
					ResetSplits.Enabled = _splitter.CurrentSplitId != 0;
				}
                
				_splitter.CheckSplit();
			}
			catch (Exception e)
			{
				StartAutosplitter.Text = @"Start Autosplitter";
				MessageBox.Show(e.Message, "AutoSplitter Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public override void Restart()
		{
			//Just log to the console for debugging purposes
			_memoryApi = new MemoryApi(Console.WriteLine);

			ServiceInjector.UpdateServices(Emulator.ServiceProvider, _memoryApi);
			_splitter = new Splitter();
			_splitter.MemoryApi = _memoryApi;

			OpenSplits.InitialDirectory = Directory.GetCurrentDirectory();
		} 

		private void LoadSplitFile_Click(object sender, EventArgs e)
		{
			var result = OpenSplits.ShowDialog();
			if (result != DialogResult.OK) return;
            
			try
			{
				_splitter.LoadSplitsFile(OpenSplits.FileName);
			}
			catch
			{
				MessageBox.Show(@"Failed to load splits file!", @"File Load Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SkipSplitUpdate_Click(object sender, EventArgs e)
		{
			_splitter.SkipSplit(true);
		}

		private void SkipSplitNoUpdate_Click(object sender, EventArgs e)
		{
			_splitter.SkipSplit(false);
		}

		private void UndoSplitUpdate_Click(object sender, EventArgs e)
		{
			_splitter.UndoSplit(true);
		}

		private void UndoSplitNoUpdate_Click(object sender, EventArgs e)
		{
			_splitter.UndoSplit(false);
		}

		private void StartAutosplitter_Click(object sender, EventArgs e)
		{            
			if (!_splitter.Start())
			{
				MessageBox.Show(@"Failed to connect to livesplit server! Please ensure the server is running. If the error persists, try restarting BizHawk and Livesplit.", 
					@"Failed to Connect to Livesplit", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ResetSplits.Enabled = false;
				return;
			}
            
			StartAutosplitter.Text = _splitter.Started ? @"Stop Autosplitter" : @"Start Autosplitter";
		}

		private void ResetSplits_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(@"Are you sure you wish to reset your splits? This cannot be undone!",
				@"Reset Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes) return;

			_splitter.ResetSplits();
		}
	}
}