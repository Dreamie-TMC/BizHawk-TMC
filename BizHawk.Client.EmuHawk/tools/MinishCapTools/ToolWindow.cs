using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Client.EmuHawk.Properties;
using BizHawk.Common.NumberExtensions;
using MinishCapTools.Data;
using MinishCapTools.Elements;
using MinishCapTools.Elements.SplitWindow;
using BackgroundChroma = MinishCapTools.Elements.BackgroundChroma;
using InputViewer = MinishCapTools.Elements.InputViewer;

namespace MinishCapTools
{
	[Tool(true, new[] { "GBA" })]
	public sealed class ToolWindow : ToolFormBase, IToolForm
    {
		[ConfigPersist]
		public Settings Settings { get; set; }

		private static Settings _modifiedSettings;
		
        private bool _initialized = false;
		private IDictionary<string, object> _mouse = new Dictionary<string, object>();

        private InputViewer _inputViewer;
        private BackgroundChroma _backgroundChroma;
        private AutoSplitter _splitter;
        private AutoSplitterSplitEditor _splitEditor;
		private bool _isExited = false;
		private bool _killSwitchOn = false;

        #region NativeImports
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        #endregion
        
        #region Bizhawk Apis
        [RequiredApi]
        public JoypadApi? Joypad { get; set; }
        [RequiredApi]
        public GuiApi? Gui { get; set; }
        [RequiredApi]
        public InputApi? Input { get; set; }
        [RequiredApi]
        public EmuApi? Emu { get; set; }
        [RequiredApi]
        public MovieApi? Movie { get; set; }
        [RequiredApi]
        public MemApi? Memory { get; set; }
        #endregion

        public ToolWindow()
        {
            InitializeComponent();
			_isExited = false;
            try
            {
                Icon = Resources.MinishCapToolsIcon;
            }
            catch
            {
                //ignored
            }
        }

		private void Close(object sender, FormClosingEventArgs e)
		{
			var result = MessageBox.Show(@"Would you like to save your changes?",
				@"Save Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes) return;

			try
			{
				Settings = _modifiedSettings;
				MessageBox.Show(@"Settings saved successfully!", @"Settings Saved", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch
			{
				//Silently catch
			}
			
			Gui.DrawNew("emu", true);
			Gui.DrawFinish();
			ClientApi.SetGameExtraPadding(0, 0, 0, 0);
			_isExited = true;
		}

        private void InitializeSettings()
		{
			if (Settings == null)
			{
				Settings = new Settings();
				Settings.RestoreDefaults();
			}

			_inputViewer = new InputViewer(Gui);
			_backgroundChroma = new BackgroundChroma();
			_splitter = new AutoSplitter();
			_splitEditor = new AutoSplitterSplitEditor();

			_modifiedSettings = Settings;

			InputViewerEnable.Checked = Settings.InputViewer.Show;
            InputBorder.Checked = Settings.InputViewer.InputBorder;
            ChromaEnable.Checked = Settings.BackgroundChroma.Enabled;
            EnablePadding.Checked = Settings.Padding.Enabled;
            MovieMode.Checked = Settings.MovieMode;
            EnableAutoSplitter.Checked = Settings.EnableAutoSplitter;
            
            ChromaSides.SelectedIndex = 0;
            EnableSpecificChroma.Checked = Settings.BackgroundChroma.ShowOnLeft;
            PaddingSides.SelectedIndex = 0;
            PaddingSize.Text = $@"{Settings.Padding.LeftWidth}";
            
            var color = Color.FromArgb(int.Parse(Settings.BackgroundChroma.Color, NumberStyles.HexNumber));
            ChromaColorEditor.Color = color;
            ColorDisplay.BackColor = color;

			if (_initialized) RenderPadding();
            
            var directory = Directory.GetCurrentDirectory();
            SelectSplitsLoader.InitialDirectory = directory;
            ConfigSplitsLoader.InitialDirectory = directory;
            ConfigSplitsSave.InitialDirectory = directory;
            ConfigSplitsSave.Filter = @"Split Files (*.json)|*.json";
        }
        
#region Windows Forms Actions
    #region General Settings
        private void InputViewerEnable_CheckedChanged(object sender, EventArgs e)
        {
			_modifiedSettings.InputViewer.Show = 
                InputBorder.Enabled = 
					InputViewerEnable.Checked;
        }
		
		private void DontClick_CheckedChanged(object sender, EventArgs e)
		{
			if (_killSwitchOn) return;
			_killSwitchOn = true;
			MessageBox.Show(@"Looks like someone doesn't know how to read instructions...", @"=(", MessageBoxButtons.YesNo,
				MessageBoxIcon.Error);
		}

		private void MovieMode_CheckedChanged(object sender, EventArgs e)
		{
			_modifiedSettings.MovieMode = MovieMode.Checked;
		}
        
        private void EnableAutoSplitter_CheckedChanged(object sender, EventArgs e)
        {
            var enabled = EnableAutoSplitter.Checked;
			_modifiedSettings.EnableAutoSplitter = enabled;
            
            SelectSplits.Enabled = enabled;
            StartAutosplitter.Enabled = enabled;
            StartAutosplitter.Text = @"Start Autosplitter";
            
            if (_splitter.Started)
                _splitter.Start();

            if (_splitter.Splits.Count <= 0) return;
            
            _splitter.SplitId = 0;
            _splitter.LastSplitId = -1;
        }

        private void StartAutosplitter_Click(object sender, EventArgs e)
        {
            if (!_splitter.Start())
            {
                MessageBox.Show(@"Failed to connect to livesplit server! Please ensure the server is running. If the error persists, try restarting BizHawk.", 
                    @"Failed to Connect to Livesplit",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetAutosplitter.Enabled = false;
                return;
            }
            
            StartAutosplitter.Text = _splitter.Started ? @"Stop Autosplitter" : @"Start Autosplitter";
        }

        private void ResetAutosplitter_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you wish to reset your current split to 0? This cannot be undone!",
                @"Reset Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;
            
            _splitter.SplitId = 0;
        }

        private void SelectSplits_Click(object sender, EventArgs e)
        {
            var result = SelectSplitsLoader.ShowHawkDialog();
            if (result != DialogResult.OK) return;
            
            try
            {
                _splitter.LoadSplitsFile(SelectSplitsLoader.FileName);
                SplitFile.Text = SelectSplitsLoader.SafeFileName;
            }
            catch
            {
                MessageBox.Show(@"Failed to load splits file!", @"AutoSplitter Error!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
		
		private void SkipSplit_Click(object sender, EventArgs e)
		{
			_splitter.SkipSplit(false);
		}

		private void SkipUpdateLivesplit_Click(object sender, EventArgs e)
		{
			_splitter.SkipSplit(true);
		}

		private void UndoUpdateLiveSplit_Click(object sender, EventArgs e)
		{
			_splitter.UndoSplit(true);
		}

		private void UndoSplit_Click(object sender, EventArgs e)
		{
			_splitter.UndoSplit(false);
		}
    #endregion
    
    #region Config Manip
        private void Save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you wish to save? This cannot be undone!",
                @"Save Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
			{
				Settings = _modifiedSettings;
                MessageBox.Show(@"Settings saved successfully!", @"Settings Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show(@"Failed to save settings! Please try again.", @"Settings Failed", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RestoreDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you want to restore default settings?",
                @"Restore Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
            if (result != DialogResult.Yes) return;

			Settings.RestoreDefaults();
			_modifiedSettings = Settings;
			
            InitializeSettings();
        }

		private void SaveSettingsAsDefault_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(@"Are you sure you want to update default settings?
This action cannot be undone!",
				@"Update Default Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
			if (result != DialogResult.Yes) return;

			Settings.DefaultSettings = _modifiedSettings;
			
			MessageBox.Show(@"Default settings saved successfully!", @"Settings Saved", MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}
            
        private void UndoChanges_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you want to undo changes? This cannot be undone!",
                @"Undo Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
            if (result != DialogResult.Yes) return;

			_modifiedSettings = Settings;
			
            InitializeSettings();
        }
    #endregion
    
    #region Bizhawk UI Settings
        private void ChromaEnable_CheckedChanged(object sender, EventArgs e)
        {
			_modifiedSettings.BackgroundChroma.Enabled = 
                ChromaSides.Enabled = 
                    EnableSpecificChroma.Enabled = 
                        EditChromaColor.Enabled = 
                            ChromaEnable.Checked;
        }

        private void ChromaSides_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableSpecificChroma.Checked = ChromaSides.SelectedItem switch
            {
                "Left" => _modifiedSettings.BackgroundChroma.ShowOnLeft,
                "Right" => _modifiedSettings.BackgroundChroma.ShowOnRight,
                "Top" => _modifiedSettings.BackgroundChroma.ShowOnTop,
                "Bottom" => _modifiedSettings.BackgroundChroma.ShowOnBottom,
                _ => EnableSpecificChroma.Checked
            };
        }

        private void EnableSpecificChroma_CheckedChanged(object sender, EventArgs e)
        {
            switch (ChromaSides.SelectedItem)
            {
                case "Left":
					_modifiedSettings.BackgroundChroma.ShowOnLeft = EnableSpecificChroma.Checked;
                    break;
                case "Right":
					_modifiedSettings.BackgroundChroma.ShowOnRight = EnableSpecificChroma.Checked;
                    break;
                case "Top":
					_modifiedSettings.BackgroundChroma.ShowOnTop = EnableSpecificChroma.Checked;
                    break;
                case "Bottom":
					_modifiedSettings.BackgroundChroma.ShowOnBottom = EnableSpecificChroma.Checked;
                    break;
            }
        }

        private void EditChromaColor_Click(object sender, EventArgs e)
        {
            var result = ChromaColorEditor.ShowHawkDialog();
            if (result != DialogResult.OK) return;
            
            var color = ChromaColorEditor.Color;
            ColorDisplay.BackColor = color;
			_modifiedSettings.BackgroundChroma.Color = color.ToArgb().ToHexString(8);
        }

        private void EnablePadding_CheckedChanged(object sender, EventArgs e)
        {
			_modifiedSettings.Padding.Enabled =
                PaddingSides.Enabled =
                    PaddingSize.Enabled =
                        ApplySize.Enabled =
                            EnablePadding.Checked;
            RenderPadding();
        }

        private void PaddingSides_SelectedIndexChanged(object sender, EventArgs e)
        {
            PaddingSize.Text = PaddingSides.SelectedItem switch
            {
                "Left" => $"{_modifiedSettings.Padding.LeftWidth}",
                "Right" => $"{_modifiedSettings.Padding.RightWidth}",
                "Top" => $"{_modifiedSettings.Padding.TopHeight}",
                "Bottom" => $"{_modifiedSettings.Padding.BottomHeight}",
                _ => PaddingSize.Text
            };
        }
        
        private void ApplySize_Click(object sender, EventArgs e)
        {
            switch (PaddingSides.SelectedItem)
            {
                case "Left":
                    var result = int.TryParse(PaddingSize.Text, out var size);
                    if (result)
						_modifiedSettings.Padding.LeftWidth = size;
					break;
                case "Right":
                    result = int.TryParse(PaddingSize.Text, out size);
                    if (result)
						_modifiedSettings.Padding.RightWidth = size;
                    break;
                case "Top":
                    result = int.TryParse(PaddingSize.Text, out size);
                    if (result)
						_modifiedSettings.Padding.TopHeight = size;
                    break;
                case "Bottom":
                    result = int.TryParse(PaddingSize.Text, out size);
                    if (result)
						_modifiedSettings.Padding.BottomHeight = size;
                    break;
            }
            
            RenderPadding();
        }
    #endregion
    
    #region Autosplitter Config
        private void LoadSplits_Click(object sender, EventArgs e)
        {
            var result = SelectSplitsLoader.ShowHawkDialog();
            if (result != DialogResult.OK) return;
            
            try
            {
                _splitEditor.LoadSplits(SelectSplitsLoader.FileName);
                Splits.Items.Clear();
                ReloadSplits.Enabled = true;           
                RemoveSplit.Enabled = false;
                SplitAddBefore.Enabled = false;
                SplitDown.Enabled = false;
                EditSplit.Enabled = false;
                SplitDisable.Enabled = false;
                SplitUp.Enabled = false;
                SplitAddAfter.Enabled = false;
                foreach (var split in _splitEditor.Splits)
                {
                    Splits.Items.Add(new ListViewItem(split.ToStringArray()));
                }
            }
            catch
            {
                MessageBox.Show(@"Failed to load splits file!", @"File Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Splits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0) return;
            var item = Splits.SelectedItems[0];
            
            switch (item.Index)
            {
                case 0:
                    RemoveSplit.Enabled = false;
                    SplitAddBefore.Enabled = false;
                    SplitDown.Enabled = false;
                    EditSplit.Enabled = false;
                    SplitDisable.Enabled = false;
                    SplitUp.Enabled = false;
                    SplitAddAfter.Enabled = true;
                    break;
                case 1:
                    RemoveSplit.Enabled = true;
                    SplitAddBefore.Enabled = true;
                    EditSplit.Enabled = true;
                    SplitDisable.Enabled = true;
                    SplitAddAfter.Enabled = true;
                    SplitUp.Enabled = false;
                    SplitDown.Enabled = item.Index != Splits.Items.Count - 1;
                    break;
                default:
                {
                    EditSplit.Enabled = true;
                    SplitDisable.Enabled = true;
                    SplitUp.Enabled = true;
                    SplitAddAfter.Enabled = true;
                    RemoveSplit.Enabled = true;
                    SplitAddBefore.Enabled = true;
                    SplitDown.Enabled = item.Index != Splits.Items.Count - 1;
                    break;
                }
            }

            SplitDisable.Text = _splitEditor.Splits[item.Index].Enabled ? @"Disable Split" : @"Enable Split";
        }

        private void SaveSplits_Click(object sender, EventArgs e)
        {
            if (Splits.Items.Count == 0) return;

            var name = _splitEditor.LoadedFile;
            var index = name.LastIndexOf('\\') + 1;
            index = index <= 0 ? name.LastIndexOf('/') + 1 : index;
            name = name.Substring(index);
            ConfigSplitsSave.FileName = name;
            var result = ConfigSplitsSave.ShowHawkDialog();

            if (result != DialogResult.OK) return;

            try
            {
                _splitEditor.WriteSplits(ConfigSplitsSave.FileName);
                MessageBox.Show(@"Splits file saved successfully!", @"Splits Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ReloadSplits.Enabled = true;
            }
            catch
            {
                MessageBox.Show(@"Failed to save Splits! Please try again.", @"Split Save Failed", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SplitClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you wish to clear splits? This cannot be undone!",
                @"Clear Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;
            
            _splitEditor.ClearSplits();
            Splits.Items.Clear();
            
            RemoveSplit.Enabled = false;
            SplitAddBefore.Enabled = false;
            SplitDown.Enabled = false;
            EditSplit.Enabled = false;
            SplitDisable.Enabled = false;
            SplitUp.Enabled = false;
            SplitAddAfter.Enabled = false;
            ReloadSplits.Enabled = false;
        }

        private void SplitUp_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == 1) return;

            var newId = id - 1;

            _splitEditor.Splits[newId].OrderId = id;
            _splitEditor.Splits[id].OrderId = newId;

            (Splits.Items[newId].Text, Splits.Items[id].Text) = (Splits.Items[id].Text, Splits.Items[newId].Text);
            (_splitEditor.Splits[newId], _splitEditor.Splits[id]) = (_splitEditor.Splits[id], _splitEditor.Splits[newId]);

            Splits.Items[id].Selected = false;
            Splits.Items[id].Focused = false;
            Splits.Items[newId].Selected = true;
            Splits.Items[newId].Focused = true;
        }

        private void SplitDown_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == Splits.Items.Count - 1) return;

            var newId = id + 1;

            _splitEditor.Splits[newId].OrderId = id;
            _splitEditor.Splits[id].OrderId = newId;

            (Splits.Items[newId].Text, Splits.Items[id].Text) = (Splits.Items[id].Text, Splits.Items[newId].Text);
            (_splitEditor.Splits[newId], _splitEditor.Splits[id]) = (_splitEditor.Splits[id], _splitEditor.Splits[newId]);

            Splits.Items[id].Selected = false;
            Splits.Items[id].Focused = false;
            Splits.Items[newId].Selected = true;
            Splits.Items[newId].Focused = true;
        }

        private void SplitDisable_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == 0) return;

            var enabled = !_splitEditor.Splits[id].Enabled;
            
            _splitEditor.Splits[id].Enabled = enabled;
            SplitDisable.Text = enabled ? @"Disable Split" : @"Enable Split";
            Splits.Items[id] = new ListViewItem(_splitEditor.Splits[id].ToStringArray());
            Splits.Items[id].Selected = true;
            Splits.Items[id].Focused = true;
        }

        private void ReloadSplits_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you wish to reload splits? This cannot be undone!",
                @"Reload Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;
            
            try
            {
                _splitEditor.RefreshSplits();
                Splits.Items.Clear();
                ReloadSplits.Enabled = true;           
                RemoveSplit.Enabled = false;
                SplitAddBefore.Enabled = false;
                SplitDown.Enabled = false;
                EditSplit.Enabled = false;
                SplitDisable.Enabled = false;
                SplitUp.Enabled = false;
                SplitAddAfter.Enabled = false;
                foreach (var split in _splitEditor.Splits)
                {
                    Splits.Items.Add(new ListViewItem(split.ToStringArray()));
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, @"Split Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateSplits_Click(object sender, EventArgs e)
        {
            if (_splitEditor.Splits.Count > 0)
            {
                var result = MessageBox.Show(@"Creating splits will clear your current splits, are you sure you want to continue?",
                    @"Create Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }
            
            _splitEditor.CreateNewSplits();
            Splits.Items.Clear();
            ReloadSplits.Enabled = false;        
            RemoveSplit.Enabled = false;
            SplitAddBefore.Enabled = false;
            SplitDown.Enabled = false;
            EditSplit.Enabled = false;
            SplitDisable.Enabled = false;
            SplitUp.Enabled = false;
            SplitAddAfter.Enabled = false;
            foreach (var split in _splitEditor.Splits)
            {
                Splits.Items.Add(new ListViewItem(split.ToStringArray()));
            }
        }

        private void SplitAddBefore_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == 0) return;

            using var child = new SplitAddWindow();
            
            child.ShowDialog(this);
            if (!child.SplitSaved)
            {
                child.Dispose();
                return;
            }

            var newSplit = child.LastUsedSplit;
            child.Dispose();

            _splitEditor.Splits.Insert(id, newSplit);
            for (var i = id; i < _splitEditor.Splits.Count; ++i)
            {
                _splitEditor.Splits[i].OrderId = i;
            }
            
            _splitEditor.Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });

            Splits.Items.Insert(id, new ListViewItem(newSplit.ToStringArray()));
        }

        private void SplitAddAfter_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index + 1;

            using var child = new SplitAddWindow();
            
            child.ShowDialog(this);
            if (!child.SplitSaved)
            {
                child.Dispose();
                return;
            }

            var newSplit = child.LastUsedSplit;
            child.Dispose();

            _splitEditor.Splits.Insert(id, newSplit);
            for (var i = id; i < _splitEditor.Splits.Count; ++i)
            {
                _splitEditor.Splits[i].OrderId = i;
            }
            
            _splitEditor.Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });

            Splits.Items.Insert(id, new ListViewItem(newSplit.ToStringArray()));
        }

        private void EditSplit_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == 0) return;

            using var child = new SplitAddWindow();
            
            child.LoadSplit(_splitEditor.Splits[id]);
            
            child.ShowDialog(this);
            if (!child.SplitSaved)
            {
                child.Dispose();
                return;
            }

            var newSplit = child.LastUsedSplit;
            child.Dispose();

            newSplit.OrderId = id;

            _splitEditor.Splits[id] = newSplit;
            for (var i = id; i < _splitEditor.Splits.Count; ++i)
            {
                _splitEditor.Splits[i].OrderId = i;
            }

            Splits.Items.RemoveAt(id);
            Splits.Items.Insert(id, new ListViewItem(newSplit.ToStringArray()));
        }

        private void RemoveSplit_Click(object sender, EventArgs e)
        {
            if (Splits.SelectedItems.Count == 0)
                return;

            var id = Splits.SelectedItems[0].Index;
            if (id == 0) return;
            
            var result = MessageBox.Show(@"Are you sure you wish to remove this split?",
                @"Remove Split?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            _splitEditor.Splits.RemoveAt(id);
            for (var i = id; i < _splitEditor.Splits.Count; ++i)
            {
                _splitEditor.Splits[i].OrderId = i;
            }
            
            _splitEditor.Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });
            
            Splits.Items.RemoveAt(id);
        }
    #endregion

    #region Input Config
        private void InputBorder_CheckedChanged(object sender, EventArgs e)
        {
			_modifiedSettings.InputViewer.InputBorder = InputBorder.Checked;
        }
        
        private void InputViewerRestorePos_Click(object sender, EventArgs e)
        {
			_modifiedSettings.InputViewer.RestoreDefaultPosition();
        }
    #endregion
#endregion

#region Per-Frame Actions
        
        public void UpdateAfter()
        {
			if (_isExited) return;

			if (!_initialized)
			{
				if (_modifiedSettings != null) Settings = _modifiedSettings;
				InitializeSettings();
				RenderPadding();
				_initialized = true;
			}

			if (_killSwitchOn && MemoryAccessor.LoadTextboxValues(Memory)["Textbox"] != 0)
			{
				Environment.Exit(255);
				return;
			}

			if (_modifiedSettings.MovieMode && Movie.IsLoaded() && Emu.FrameCount() < Movie.Length())
                InputHandler.SetInputs(Movie.GetInput(Emu.FrameCount()));
            else
                InputHandler.SetInputs(Joypad.GetImmediate());

            if (_modifiedSettings.EnableAutoSplitter)
            {
                try
                {
                    if (_splitter.LastSplitId != _splitter.SplitId)
                    {
                        SplitId.Text = $@"{_splitter.SplitId}";
                        LoadedSplit.Text = _splitter.Splits[_splitter.SplitId >= _splitter.Splits.Count ? _splitter.Splits.Count - 1 : _splitter.SplitId].Name;
                        ResetAutosplitter.Enabled = _splitter.SplitId != 0;
                    }
                    
                    _splitter.CheckSplit(Memory);
                }
                catch (Exception e)
                {
                    StartAutosplitter.Text = @"Start Autosplitter";
                    MessageBox.Show(e.Message, "AutoSplitter Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Draw();
        }

        /**
         * Draws UI elements to the screen
         * NOTE: Ui Elements are drawn such that the 2nd element drawn will display over the 1st, etc.
         * So draw all static background images, such as padding chroma, before drawing other UI elements, but after the
         * Gui.DrawNew call
         */
        private void Draw()
        {
			Gui.DrawNew("emu", true);
            
            if (IsBizhawkActiveWindow())
                _mouse = Input.GetMouse();
            
            if (_modifiedSettings.BackgroundChroma.Enabled && _modifiedSettings.Padding.Enabled)
                _backgroundChroma.Draw(Gui, _modifiedSettings);
            
            if (_modifiedSettings.InputViewer.Show)
                _inputViewer.Draw(Gui, _modifiedSettings, _mouse);

            Gui.DrawFinish();
        }

        private void RenderPadding()
        {
            if (_modifiedSettings.Padding.Enabled)
                ClientApi.SetGameExtraPadding(_modifiedSettings.Padding.LeftWidth,
					_modifiedSettings.Padding.TopHeight,
					_modifiedSettings.Padding.RightWidth,
					_modifiedSettings.Padding.BottomHeight);
            else
                ClientApi.SetGameExtraPadding(0, 0, 0, 0);
        }

        private bool IsBizhawkActiveWindow()
        { 
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();
            if (GetWindowText(handle, buff, nChars) <= 0) return false;
            
            var text = buff.ToString();
            return text.StartsWith("Gameboy Advance");
        }
#endregion

#region Required BizHawk Functions
        public bool UpdateBefore => false;

        public void NewUpdate(ToolFormUpdateType type)
        {
            switch (type) 
            {
                case ToolFormUpdateType.Legacy:
                    break;
                case ToolFormUpdateType.LegacyFast:
                    break;
                case ToolFormUpdateType.Reset:
                    Restart();
                    break;
                case ToolFormUpdateType.PreFrame:
                    UpdateValues();
                    break;
                case ToolFormUpdateType.PostFrame:
                    UpdateAfter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public void UpdateValues()
        {
        }

        public void FastUpdate()
        {
        }

        public void Restart()
        {
        }

        public bool AskSaveChanges() => true;
#endregion
        
#region Windows Forms Schtuff

/// <summary>
/// Required method for Designer support - do not modify
/// the contents of this method with the code editor.
/// </summary>
private void InitializeComponent()
{
	this.ChromaColorEditor = new System.Windows.Forms.ColorDialog();
	this.ButtonCounterTextColorEditor = new System.Windows.Forms.ColorDialog();
	this.label6 = new System.Windows.Forms.Label();
	this.button1 = new System.Windows.Forms.Button();
	this.ConfigSplitsLoader = new System.Windows.Forms.OpenFileDialog();
	this.ConfigSplitsSave = new System.Windows.Forms.SaveFileDialog();
	this.SelectSplitsLoader = new System.Windows.Forms.OpenFileDialog();
	this.tabPage7 = new System.Windows.Forms.TabPage();
	this.treeView1 = new System.Windows.Forms.TreeView();
	this.button2 = new System.Windows.Forms.Button();
	this.button3 = new System.Windows.Forms.Button();
	this.button4 = new System.Windows.Forms.Button();
	this.button5 = new System.Windows.Forms.Button();
	this.button6 = new System.Windows.Forms.Button();
	this.button7 = new System.Windows.Forms.Button();
	this.button8 = new System.Windows.Forms.Button();
	this.button9 = new System.Windows.Forms.Button();
	this.button10 = new System.Windows.Forms.Button();
	this.button11 = new System.Windows.Forms.Button();
	this.button12 = new System.Windows.Forms.Button();
	this.button13 = new System.Windows.Forms.Button();
	this.label9 = new System.Windows.Forms.Label();
	this.InputConfig = new System.Windows.Forms.TabPage();
	this.checkBox2 = new System.Windows.Forms.CheckBox();
	this.label48 = new System.Windows.Forms.Label();
	this.label37 = new System.Windows.Forms.Label();
	this.label38 = new System.Windows.Forms.Label();
	this.label39 = new System.Windows.Forms.Label();
	this.label40 = new System.Windows.Forms.Label();
	this.label41 = new System.Windows.Forms.Label();
	this.label42 = new System.Windows.Forms.Label();
	this.label43 = new System.Windows.Forms.Label();
	this.label44 = new System.Windows.Forms.Label();
	this.label45 = new System.Windows.Forms.Label();
	this.label46 = new System.Windows.Forms.Label();
	this.label47 = new System.Windows.Forms.Label();
	this.label36 = new System.Windows.Forms.Label();
	this.label35 = new System.Windows.Forms.Label();
	this.label34 = new System.Windows.Forms.Label();
	this.label33 = new System.Windows.Forms.Label();
	this.label32 = new System.Windows.Forms.Label();
	this.label31 = new System.Windows.Forms.Label();
	this.label30 = new System.Windows.Forms.Label();
	this.label29 = new System.Windows.Forms.Label();
	this.label28 = new System.Windows.Forms.Label();
	this.label27 = new System.Windows.Forms.Label();
	this.label26 = new System.Windows.Forms.Label();
	this.label21 = new System.Windows.Forms.Label();
	this.label20 = new System.Windows.Forms.Label();
	this.label19 = new System.Windows.Forms.Label();
	this.label18 = new System.Windows.Forms.Label();
	this.label17 = new System.Windows.Forms.Label();
	this.label16 = new System.Windows.Forms.Label();
	this.label15 = new System.Windows.Forms.Label();
	this.label14 = new System.Windows.Forms.Label();
	this.label13 = new System.Windows.Forms.Label();
	this.label11 = new System.Windows.Forms.Label();
	this.label25 = new System.Windows.Forms.Label();
	this.label24 = new System.Windows.Forms.Label();
	this.label23 = new System.Windows.Forms.Label();
	this.label22 = new System.Windows.Forms.Label();
	this.label3 = new System.Windows.Forms.Label();
	this.checkBox1 = new System.Windows.Forms.CheckBox();
	this.InputViewerRestorePos = new System.Windows.Forms.Button();
	this.InputBorder = new System.Windows.Forms.CheckBox();
	this.SplitterConfig = new System.Windows.Forms.TabPage();
	this.Splits = new System.Windows.Forms.ListView();
	this.header1 = new System.Windows.Forms.ColumnHeader();
	this.header2 = new System.Windows.Forms.ColumnHeader();
	this.header3 = new System.Windows.Forms.ColumnHeader();
	this.header4 = new System.Windows.Forms.ColumnHeader();
	this.SplitDisable = new System.Windows.Forms.Button();
	this.SplitClear = new System.Windows.Forms.Button();
	this.EditSplit = new System.Windows.Forms.Button();
	this.SplitDown = new System.Windows.Forms.Button();
	this.SplitUp = new System.Windows.Forms.Button();
	this.SplitAddAfter = new System.Windows.Forms.Button();
	this.ReloadSplits = new System.Windows.Forms.Button();
	this.CreateSplits = new System.Windows.Forms.Button();
	this.RemoveSplit = new System.Windows.Forms.Button();
	this.SplitAddBefore = new System.Windows.Forms.Button();
	this.SaveSplits = new System.Windows.Forms.Button();
	this.LoadSplits = new System.Windows.Forms.Button();
	this.label8 = new System.Windows.Forms.Label();
	this.UISettings = new System.Windows.Forms.TabPage();
	this.label5 = new System.Windows.Forms.Label();
	this.label4 = new System.Windows.Forms.Label();
	this.ChromaEnable = new System.Windows.Forms.CheckBox();
	this.EnablePadding = new System.Windows.Forms.CheckBox();
	this.PaddingSides = new System.Windows.Forms.ComboBox();
	this.label2 = new System.Windows.Forms.Label();
	this.PaddingSize = new System.Windows.Forms.TextBox();
	this.ApplySize = new System.Windows.Forms.Button();
	this.ChromaSides = new System.Windows.Forms.ComboBox();
	this.EnableSpecificChroma = new System.Windows.Forms.CheckBox();
	this.EditChromaColor = new System.Windows.Forms.Button();
	this.label1 = new System.Windows.Forms.Label();
	this.ColorDisplay = new System.Windows.Forms.PictureBox();
	this.GeneralSettings = new System.Windows.Forms.TabPage();
	this.UndoUpdateLiveSplit = new System.Windows.Forms.Button();
	this.SaveSettingsAsDefault = new System.Windows.Forms.Button();
	this.SkipUpdateLivesplit = new System.Windows.Forms.Button();
	this.UndoSplit = new System.Windows.Forms.Button();
	this.SkipSplit = new System.Windows.Forms.Button();
	this.MovieMode = new System.Windows.Forms.CheckBox();
	this.ResetAutosplitter = new System.Windows.Forms.Button();
	this.SplitId = new System.Windows.Forms.Label();
	this.label12 = new System.Windows.Forms.Label();
	this.LoadedSplit = new System.Windows.Forms.Label();
	this.label10 = new System.Windows.Forms.Label();
	this.SplitFile = new System.Windows.Forms.Label();
	this.label7 = new System.Windows.Forms.Label();
	this.SelectSplits = new System.Windows.Forms.Button();
	this.StartAutosplitter = new System.Windows.Forms.Button();
	this.UndoChanges = new System.Windows.Forms.Button();
	this.RestoreDefaults = new System.Windows.Forms.Button();
	this.InputViewerEnable = new System.Windows.Forms.CheckBox();
	this.Save = new System.Windows.Forms.Button();
	this.EnableAutoSplitter = new System.Windows.Forms.CheckBox();
	this.TabControl = new System.Windows.Forms.TabControl();
	this.tabPage7.SuspendLayout();
	this.InputConfig.SuspendLayout();
	this.SplitterConfig.SuspendLayout();
	this.UISettings.SuspendLayout();
	((System.ComponentModel.ISupportInitialize)(this.ColorDisplay)).BeginInit();
	this.GeneralSettings.SuspendLayout();
	this.TabControl.SuspendLayout();
	this.SuspendLayout();
	// 
	// ChromaColorEditor
	// 
	this.ChromaColorEditor.AnyColor = true;
	// 
	// label6
	// 
	this.label6.Location = new System.Drawing.Point(6, 34);
	this.label6.Name = "label6";
	this.label6.Size = new System.Drawing.Size(35, 23);
	this.label6.TabIndex = 13;
	this.label6.Text = "Side:";
	this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// button1
	// 
	this.button1.Location = new System.Drawing.Point(6, 96);
	this.button1.Name = "button1";
	this.button1.Size = new System.Drawing.Size(106, 23);
	this.button1.TabIndex = 30;
	this.button1.Text = "Select Splits File";
	this.button1.UseVisualStyleBackColor = true;
	// 
	// ConfigSplitsLoader
	// 
	this.ConfigSplitsLoader.FileName = "Splits.json";
	// 
	// tabPage7
	// 
	this.tabPage7.BackColor = System.Drawing.Color.White;
	this.tabPage7.Controls.Add(this.treeView1);
	this.tabPage7.Controls.Add(this.button2);
	this.tabPage7.Controls.Add(this.button3);
	this.tabPage7.Controls.Add(this.button4);
	this.tabPage7.Controls.Add(this.button5);
	this.tabPage7.Controls.Add(this.button6);
	this.tabPage7.Controls.Add(this.button7);
	this.tabPage7.Controls.Add(this.button8);
	this.tabPage7.Controls.Add(this.button9);
	this.tabPage7.Controls.Add(this.button10);
	this.tabPage7.Controls.Add(this.button11);
	this.tabPage7.Controls.Add(this.button12);
	this.tabPage7.Controls.Add(this.button13);
	this.tabPage7.Controls.Add(this.label9);
	this.tabPage7.Location = new System.Drawing.Point(4, 22);
	this.tabPage7.Name = "tabPage7";
	this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
	this.tabPage7.Size = new System.Drawing.Size(379, 320);
	this.tabPage7.TabIndex = 2;
	this.tabPage7.Text = "Autosplitter Config";
	// 
	// treeView1
	// 
	this.treeView1.LineColor = System.Drawing.Color.Empty;
	this.treeView1.Location = new System.Drawing.Point(0, 0);
	this.treeView1.Name = "treeView1";
	this.treeView1.Size = new System.Drawing.Size(121, 97);
	this.treeView1.TabIndex = 0;
	// 
	// button2
	// 
	this.button2.Location = new System.Drawing.Point(255, 229);
	this.button2.Name = "button2";
	this.button2.Size = new System.Drawing.Size(118, 23);
	this.button2.TabIndex = 46;
	this.button2.Text = "Disable Split";
	this.button2.UseVisualStyleBackColor = true;
	this.button2.Click += new System.EventHandler(this.SplitDisable_Click);
	// 
	// button3
	// 
	this.button3.Location = new System.Drawing.Point(255, 287);
	this.button3.Name = "button3";
	this.button3.Size = new System.Drawing.Size(118, 23);
	this.button3.TabIndex = 45;
	this.button3.Text = "Clear Splits";
	this.button3.UseVisualStyleBackColor = true;
	this.button3.Click += new System.EventHandler(this.SplitClear_Click);
	// 
	// button4
	// 
	this.button4.Location = new System.Drawing.Point(255, 113);
	this.button4.Name = "button4";
	this.button4.Size = new System.Drawing.Size(118, 23);
	this.button4.TabIndex = 44;
	this.button4.Text = "Edit Split";
	this.button4.UseVisualStyleBackColor = true;
	this.button4.Click += new System.EventHandler(this.EditSplit_Click);
	// 
	// button5
	// 
	this.button5.Location = new System.Drawing.Point(255, 200);
	this.button5.Name = "button5";
	this.button5.Size = new System.Drawing.Size(118, 23);
	this.button5.TabIndex = 43;
	this.button5.Text = "Move Split Down";
	this.button5.UseVisualStyleBackColor = true;
	this.button5.Click += new System.EventHandler(this.SplitDown_Click);
	// 
	// button6
	// 
	this.button6.Location = new System.Drawing.Point(255, 171);
	this.button6.Name = "button6";
	this.button6.Size = new System.Drawing.Size(118, 23);
	this.button6.TabIndex = 42;
	this.button6.Text = "Move Split Up";
	this.button6.UseVisualStyleBackColor = true;
	this.button6.Click += new System.EventHandler(this.SplitUp_Click);
	// 
	// button7
	// 
	this.button7.Location = new System.Drawing.Point(255, 84);
	this.button7.Name = "button7";
	this.button7.Size = new System.Drawing.Size(118, 23);
	this.button7.TabIndex = 41;
	this.button7.Text = "Add Split After";
	this.button7.UseVisualStyleBackColor = true;
	this.button7.Click += new System.EventHandler(this.SplitAddAfter_Click);
	// 
	// button8
	// 
	this.button8.Location = new System.Drawing.Point(255, 259);
	this.button8.Name = "button8";
	this.button8.Size = new System.Drawing.Size(118, 23);
	this.button8.TabIndex = 40;
	this.button8.Text = "Reload Splits";
	this.button8.UseVisualStyleBackColor = true;
	this.button8.Click += new System.EventHandler(this.ReloadSplits_Click);
	// 
	// button9
	// 
	this.button9.Location = new System.Drawing.Point(255, 26);
	this.button9.Name = "button9";
	this.button9.Size = new System.Drawing.Size(118, 23);
	this.button9.TabIndex = 39;
	this.button9.Text = "Create New Splits File";
	this.button9.UseVisualStyleBackColor = true;
	this.button9.Click += new System.EventHandler(this.CreateSplits_Click);
	// 
	// button10
	// 
	this.button10.Location = new System.Drawing.Point(255, 142);
	this.button10.Name = "button10";
	this.button10.Size = new System.Drawing.Size(118, 23);
	this.button10.TabIndex = 38;
	this.button10.Text = "Remove Split";
	this.button10.UseVisualStyleBackColor = true;
	this.button10.Click += new System.EventHandler(this.RemoveSplit_Click);
	// 
	// button11
	// 
	this.button11.Location = new System.Drawing.Point(255, 55);
	this.button11.Name = "button11";
	this.button11.Size = new System.Drawing.Size(118, 23);
	this.button11.TabIndex = 37;
	this.button11.Text = "Add Split Before";
	this.button11.UseVisualStyleBackColor = true;
	this.button11.Click += new System.EventHandler(this.SplitAddBefore_Click);
	// 
	// button12
	// 
	this.button12.Location = new System.Drawing.Point(131, 288);
	this.button12.Name = "button12";
	this.button12.Size = new System.Drawing.Size(118, 23);
	this.button12.TabIndex = 36;
	this.button12.Text = "Save Splits";
	this.button12.UseVisualStyleBackColor = true;
	this.button12.Click += new System.EventHandler(this.SaveSplits_Click);
	// 
	// button13
	// 
	this.button13.Location = new System.Drawing.Point(6, 288);
	this.button13.Name = "button13";
	this.button13.Size = new System.Drawing.Size(118, 23);
	this.button13.TabIndex = 35;
	this.button13.Text = "Load Splits";
	this.button13.UseVisualStyleBackColor = true;
	this.button13.Click += new System.EventHandler(this.LoadSplits_Click);
	// 
	// label9
	// 
	this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label9.Location = new System.Drawing.Point(3, 3);
	this.label9.Name = "label9";
	this.label9.Size = new System.Drawing.Size(246, 23);
	this.label9.TabIndex = 32;
	this.label9.Text = "Current Splits";
	this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// InputConfig
	// 
	this.InputConfig.BackColor = System.Drawing.Color.White;
	this.InputConfig.Controls.Add(this.checkBox2);
	this.InputConfig.Controls.Add(this.label48);
	this.InputConfig.Controls.Add(this.label37);
	this.InputConfig.Controls.Add(this.label38);
	this.InputConfig.Controls.Add(this.label39);
	this.InputConfig.Controls.Add(this.label40);
	this.InputConfig.Controls.Add(this.label41);
	this.InputConfig.Controls.Add(this.label42);
	this.InputConfig.Controls.Add(this.label43);
	this.InputConfig.Controls.Add(this.label44);
	this.InputConfig.Controls.Add(this.label45);
	this.InputConfig.Controls.Add(this.label46);
	this.InputConfig.Controls.Add(this.label47);
	this.InputConfig.Controls.Add(this.label36);
	this.InputConfig.Controls.Add(this.label35);
	this.InputConfig.Controls.Add(this.label34);
	this.InputConfig.Controls.Add(this.label33);
	this.InputConfig.Controls.Add(this.label32);
	this.InputConfig.Controls.Add(this.label31);
	this.InputConfig.Controls.Add(this.label30);
	this.InputConfig.Controls.Add(this.label29);
	this.InputConfig.Controls.Add(this.label28);
	this.InputConfig.Controls.Add(this.label27);
	this.InputConfig.Controls.Add(this.label26);
	this.InputConfig.Controls.Add(this.label21);
	this.InputConfig.Controls.Add(this.label20);
	this.InputConfig.Controls.Add(this.label19);
	this.InputConfig.Controls.Add(this.label18);
	this.InputConfig.Controls.Add(this.label17);
	this.InputConfig.Controls.Add(this.label16);
	this.InputConfig.Controls.Add(this.label15);
	this.InputConfig.Controls.Add(this.label14);
	this.InputConfig.Controls.Add(this.label13);
	this.InputConfig.Controls.Add(this.label11);
	this.InputConfig.Controls.Add(this.label25);
	this.InputConfig.Controls.Add(this.label24);
	this.InputConfig.Controls.Add(this.label23);
	this.InputConfig.Controls.Add(this.label22);
	this.InputConfig.Controls.Add(this.label3);
	this.InputConfig.Controls.Add(this.checkBox1);
	this.InputConfig.Controls.Add(this.InputViewerRestorePos);
	this.InputConfig.Controls.Add(this.InputBorder);
	this.InputConfig.Location = new System.Drawing.Point(4, 22);
	this.InputConfig.Name = "InputConfig";
	this.InputConfig.Padding = new System.Windows.Forms.Padding(3);
	this.InputConfig.Size = new System.Drawing.Size(600, 322);
	this.InputConfig.TabIndex = 3;
	this.InputConfig.Text = "Input Config";
	// 
	// checkBox2
	// 
	this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
	this.checkBox2.Location = new System.Drawing.Point(487, 56);
	this.checkBox2.Name = "checkBox2";
	this.checkBox2.Size = new System.Drawing.Size(106, 21);
	this.checkBox2.TabIndex = 81;
	this.checkBox2.UseVisualStyleBackColor = true;
	// 
	// label48
	// 
	this.label48.BackColor = System.Drawing.Color.Transparent;
	this.label48.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label48.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label48.Location = new System.Drawing.Point(486, 55);
	this.label48.Name = "label48";
	this.label48.Size = new System.Drawing.Size(108, 23);
	this.label48.TabIndex = 80;
	this.label48.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label37
	// 
	this.label37.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label37.Location = new System.Drawing.Point(275, 274);
	this.label37.Name = "label37";
	this.label37.Size = new System.Drawing.Size(212, 23);
	this.label37.TabIndex = 79;
	this.label37.Text = "No Image Selected";
	this.label37.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label38
	// 
	this.label38.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label38.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label38.Location = new System.Drawing.Point(275, 252);
	this.label38.Name = "label38";
	this.label38.Size = new System.Drawing.Size(212, 23);
	this.label38.TabIndex = 78;
	this.label38.Text = "Not Pressed Image Path";
	this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label39
	// 
	this.label39.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label39.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label39.Location = new System.Drawing.Point(275, 230);
	this.label39.Name = "label39";
	this.label39.Size = new System.Drawing.Size(212, 23);
	this.label39.TabIndex = 77;
	this.label39.Text = "Not Pressed Image Path";
	this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label40
	// 
	this.label40.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label40.Location = new System.Drawing.Point(275, 208);
	this.label40.Name = "label40";
	this.label40.Size = new System.Drawing.Size(212, 23);
	this.label40.TabIndex = 76;
	this.label40.Text = "Not Pressed Image Path";
	this.label40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label41
	// 
	this.label41.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label41.Location = new System.Drawing.Point(275, 186);
	this.label41.Name = "label41";
	this.label41.Size = new System.Drawing.Size(212, 23);
	this.label41.TabIndex = 75;
	this.label41.Text = "Not Pressed Image Path";
	this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label42
	// 
	this.label42.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label42.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label42.Location = new System.Drawing.Point(275, 165);
	this.label42.Name = "label42";
	this.label42.Size = new System.Drawing.Size(212, 23);
	this.label42.TabIndex = 74;
	this.label42.Text = "Not Pressed Image Path";
	this.label42.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label43
	// 
	this.label43.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label43.Location = new System.Drawing.Point(275, 143);
	this.label43.Name = "label43";
	this.label43.Size = new System.Drawing.Size(212, 23);
	this.label43.TabIndex = 73;
	this.label43.Text = "Not Pressed Image Path";
	this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label44
	// 
	this.label44.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label44.Location = new System.Drawing.Point(275, 121);
	this.label44.Name = "label44";
	this.label44.Size = new System.Drawing.Size(212, 23);
	this.label44.TabIndex = 72;
	this.label44.Text = "Not Pressed Image Path";
	this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label45
	// 
	this.label45.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label45.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label45.Location = new System.Drawing.Point(275, 99);
	this.label45.Name = "label45";
	this.label45.Size = new System.Drawing.Size(212, 23);
	this.label45.TabIndex = 71;
	this.label45.Text = "Not Pressed Image Path";
	this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label46
	// 
	this.label46.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label46.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label46.Location = new System.Drawing.Point(275, 77);
	this.label46.Name = "label46";
	this.label46.Size = new System.Drawing.Size(212, 23);
	this.label46.TabIndex = 70;
	this.label46.Text = "Not Pressed Image Path";
	this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label47
	// 
	this.label47.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label47.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label47.Location = new System.Drawing.Point(275, 55);
	this.label47.Name = "label47";
	this.label47.Size = new System.Drawing.Size(212, 23);
	this.label47.TabIndex = 69;
	this.label47.Text = "Not Pressed Image Path";
	this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label36
	// 
	this.label36.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label36.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label36.Location = new System.Drawing.Point(64, 274);
	this.label36.Name = "label36";
	this.label36.Size = new System.Drawing.Size(212, 23);
	this.label36.TabIndex = 67;
	this.label36.Text = "Not Pressed Image Path";
	this.label36.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label35
	// 
	this.label35.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label35.Location = new System.Drawing.Point(64, 252);
	this.label35.Name = "label35";
	this.label35.Size = new System.Drawing.Size(212, 23);
	this.label35.TabIndex = 66;
	this.label35.Text = "Not Pressed Image Path";
	this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label34
	// 
	this.label34.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label34.Location = new System.Drawing.Point(64, 230);
	this.label34.Name = "label34";
	this.label34.Size = new System.Drawing.Size(212, 23);
	this.label34.TabIndex = 65;
	this.label34.Text = "Not Pressed Image Path";
	this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label33
	// 
	this.label33.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label33.Location = new System.Drawing.Point(64, 208);
	this.label33.Name = "label33";
	this.label33.Size = new System.Drawing.Size(212, 23);
	this.label33.TabIndex = 64;
	this.label33.Text = "Not Pressed Image Path";
	this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label32
	// 
	this.label32.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label32.Location = new System.Drawing.Point(64, 186);
	this.label32.Name = "label32";
	this.label32.Size = new System.Drawing.Size(212, 23);
	this.label32.TabIndex = 63;
	this.label32.Text = "Not Pressed Image Path";
	this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label31
	// 
	this.label31.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label31.Location = new System.Drawing.Point(64, 165);
	this.label31.Name = "label31";
	this.label31.Size = new System.Drawing.Size(212, 23);
	this.label31.TabIndex = 62;
	this.label31.Text = "Not Pressed Image Path";
	this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label30
	// 
	this.label30.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label30.Location = new System.Drawing.Point(64, 143);
	this.label30.Name = "label30";
	this.label30.Size = new System.Drawing.Size(212, 23);
	this.label30.TabIndex = 61;
	this.label30.Text = "Not Pressed Image Path";
	this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label29
	// 
	this.label29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label29.Location = new System.Drawing.Point(64, 121);
	this.label29.Name = "label29";
	this.label29.Size = new System.Drawing.Size(212, 23);
	this.label29.TabIndex = 60;
	this.label29.Text = "Not Pressed Image Path";
	this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label28
	// 
	this.label28.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label28.Location = new System.Drawing.Point(64, 99);
	this.label28.Name = "label28";
	this.label28.Size = new System.Drawing.Size(212, 23);
	this.label28.TabIndex = 59;
	this.label28.Text = "Not Pressed Image Path";
	this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label27
	// 
	this.label27.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label27.Location = new System.Drawing.Point(64, 77);
	this.label27.Name = "label27";
	this.label27.Size = new System.Drawing.Size(212, 23);
	this.label27.TabIndex = 58;
	this.label27.Text = "Not Pressed Image Path";
	this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label26
	// 
	this.label26.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label26.Location = new System.Drawing.Point(64, 55);
	this.label26.Name = "label26";
	this.label26.Size = new System.Drawing.Size(212, 23);
	this.label26.TabIndex = 57;
	this.label26.Text = "Not Pressed Image Path";
	this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label21
	// 
	this.label21.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label21.Location = new System.Drawing.Point(6, 274);
	this.label21.Name = "label21";
	this.label21.Size = new System.Drawing.Size(59, 23);
	this.label21.TabIndex = 56;
	this.label21.Text = "Right";
	this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label20
	// 
	this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label20.Location = new System.Drawing.Point(6, 252);
	this.label20.Name = "label20";
	this.label20.Size = new System.Drawing.Size(59, 23);
	this.label20.TabIndex = 55;
	this.label20.Text = "Left";
	this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label19
	// 
	this.label19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label19.Location = new System.Drawing.Point(6, 230);
	this.label19.Name = "label19";
	this.label19.Size = new System.Drawing.Size(59, 23);
	this.label19.TabIndex = 54;
	this.label19.Text = "Down";
	this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label18
	// 
	this.label18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label18.Location = new System.Drawing.Point(6, 208);
	this.label18.Name = "label18";
	this.label18.Size = new System.Drawing.Size(59, 23);
	this.label18.TabIndex = 53;
	this.label18.Text = "Up";
	this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label17
	// 
	this.label17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label17.Location = new System.Drawing.Point(6, 186);
	this.label17.Name = "label17";
	this.label17.Size = new System.Drawing.Size(59, 23);
	this.label17.TabIndex = 52;
	this.label17.Text = "D-Pad";
	this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label16
	// 
	this.label16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label16.Location = new System.Drawing.Point(6, 165);
	this.label16.Name = "label16";
	this.label16.Size = new System.Drawing.Size(59, 23);
	this.label16.TabIndex = 51;
	this.label16.Text = "Select";
	this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label15
	// 
	this.label15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label15.Location = new System.Drawing.Point(6, 143);
	this.label15.Name = "label15";
	this.label15.Size = new System.Drawing.Size(59, 23);
	this.label15.TabIndex = 50;
	this.label15.Text = "Start";
	this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label14
	// 
	this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label14.Location = new System.Drawing.Point(6, 121);
	this.label14.Name = "label14";
	this.label14.Size = new System.Drawing.Size(59, 23);
	this.label14.TabIndex = 49;
	this.label14.Text = "R";
	this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label13
	// 
	this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label13.Location = new System.Drawing.Point(6, 99);
	this.label13.Name = "label13";
	this.label13.Size = new System.Drawing.Size(59, 23);
	this.label13.TabIndex = 48;
	this.label13.Text = "L";
	this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label11
	// 
	this.label11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label11.Location = new System.Drawing.Point(6, 77);
	this.label11.Name = "label11";
	this.label11.Size = new System.Drawing.Size(59, 23);
	this.label11.TabIndex = 47;
	this.label11.Text = "B";
	this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label25
	// 
	this.label25.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label25.Location = new System.Drawing.Point(6, 33);
	this.label25.Name = "label25";
	this.label25.Size = new System.Drawing.Size(59, 23);
	this.label25.TabIndex = 46;
	this.label25.Text = "Button";
	this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label24
	// 
	this.label24.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label24.Location = new System.Drawing.Point(486, 33);
	this.label24.Name = "label24";
	this.label24.Size = new System.Drawing.Size(108, 23);
	this.label24.TabIndex = 45;
	this.label24.Text = "Unlocked";
	this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label23
	// 
	this.label23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label23.Location = new System.Drawing.Point(275, 33);
	this.label23.Name = "label23";
	this.label23.Size = new System.Drawing.Size(212, 23);
	this.label23.TabIndex = 44;
	this.label23.Text = "Pressed Image Path";
	this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label22
	// 
	this.label22.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label22.Location = new System.Drawing.Point(64, 33);
	this.label22.Name = "label22";
	this.label22.Size = new System.Drawing.Size(212, 23);
	this.label22.TabIndex = 43;
	this.label22.Text = "Not Pressed Image Path";
	this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// label3
	// 
	this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label3.Location = new System.Drawing.Point(6, 55);
	this.label3.Name = "label3";
	this.label3.Size = new System.Drawing.Size(59, 23);
	this.label3.TabIndex = 32;
	this.label3.Text = "A";
	this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// checkBox1
	// 
	this.checkBox1.Enabled = false;
	this.checkBox1.Location = new System.Drawing.Point(173, 6);
	this.checkBox1.Name = "checkBox1";
	this.checkBox1.Size = new System.Drawing.Size(211, 24);
	this.checkBox1.TabIndex = 26;
	this.checkBox1.Text = "Use Custom Images For Input Viewer";
	this.checkBox1.UseVisualStyleBackColor = true;
	// 
	// InputViewerRestorePos
	// 
	this.InputViewerRestorePos.Location = new System.Drawing.Point(390, 6);
	this.InputViewerRestorePos.Name = "InputViewerRestorePos";
	this.InputViewerRestorePos.Size = new System.Drawing.Size(204, 23);
	this.InputViewerRestorePos.TabIndex = 25;
	this.InputViewerRestorePos.Text = "Restore Input Viewer Default Position";
	this.InputViewerRestorePos.UseVisualStyleBackColor = true;
	this.InputViewerRestorePos.Click += new System.EventHandler(this.InputViewerRestorePos_Click);
	// 
	// InputBorder
	// 
	this.InputBorder.Enabled = false;
	this.InputBorder.Location = new System.Drawing.Point(6, 6);
	this.InputBorder.Name = "InputBorder";
	this.InputBorder.Size = new System.Drawing.Size(161, 24);
	this.InputBorder.TabIndex = 1;
	this.InputBorder.Text = "Show Input Display Border";
	this.InputBorder.UseVisualStyleBackColor = true;
	this.InputBorder.CheckedChanged += new System.EventHandler(this.InputBorder_CheckedChanged);
	// 
	// SplitterConfig
	// 
	this.SplitterConfig.BackColor = System.Drawing.Color.White;
	this.SplitterConfig.Controls.Add(this.Splits);
	this.SplitterConfig.Controls.Add(this.SplitDisable);
	this.SplitterConfig.Controls.Add(this.SplitClear);
	this.SplitterConfig.Controls.Add(this.EditSplit);
	this.SplitterConfig.Controls.Add(this.SplitDown);
	this.SplitterConfig.Controls.Add(this.SplitUp);
	this.SplitterConfig.Controls.Add(this.SplitAddAfter);
	this.SplitterConfig.Controls.Add(this.ReloadSplits);
	this.SplitterConfig.Controls.Add(this.CreateSplits);
	this.SplitterConfig.Controls.Add(this.RemoveSplit);
	this.SplitterConfig.Controls.Add(this.SplitAddBefore);
	this.SplitterConfig.Controls.Add(this.SaveSplits);
	this.SplitterConfig.Controls.Add(this.LoadSplits);
	this.SplitterConfig.Controls.Add(this.label8);
	this.SplitterConfig.Location = new System.Drawing.Point(4, 22);
	this.SplitterConfig.Name = "SplitterConfig";
	this.SplitterConfig.Padding = new System.Windows.Forms.Padding(3);
	this.SplitterConfig.Size = new System.Drawing.Size(600, 322);
	this.SplitterConfig.TabIndex = 2;
	this.SplitterConfig.Text = "Autosplitter Config";
	// 
	// Splits
	// 
	this.Splits.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.header1, this.header2, this.header3, this.header4 });
	this.Splits.FullRowSelect = true;
	this.Splits.Location = new System.Drawing.Point(6, 25);
	this.Splits.MultiSelect = false;
	this.Splits.Name = "Splits";
	this.Splits.Size = new System.Drawing.Size(464, 257);
	this.Splits.TabIndex = 47;
	this.Splits.UseCompatibleStateImageBehavior = false;
	this.Splits.View = System.Windows.Forms.View.Details;
	this.Splits.SelectedIndexChanged += new System.EventHandler(this.Splits_SelectedIndexChanged);
	// 
	// header1
	// 
	this.header1.Text = "Split Name";
	this.header1.Width = 190;
	// 
	// header2
	// 
	this.header2.Text = "Enabled";
	// 
	// header3
	// 
	this.header3.Text = "Split Type";
	this.header3.Width = 100;
	// 
	// header4
	// 
	this.header4.Text = "Memory Domain";
	this.header4.Width = 100;
	// 
	// SplitDisable
	// 
	this.SplitDisable.Enabled = false;
	this.SplitDisable.Location = new System.Drawing.Point(476, 228);
	this.SplitDisable.Name = "SplitDisable";
	this.SplitDisable.Size = new System.Drawing.Size(118, 23);
	this.SplitDisable.TabIndex = 46;
	this.SplitDisable.Text = "Disable Split";
	this.SplitDisable.UseVisualStyleBackColor = true;
	this.SplitDisable.Click += new System.EventHandler(this.SplitDisable_Click);
	// 
	// SplitClear
	// 
	this.SplitClear.Location = new System.Drawing.Point(476, 288);
	this.SplitClear.Name = "SplitClear";
	this.SplitClear.Size = new System.Drawing.Size(118, 23);
	this.SplitClear.TabIndex = 45;
	this.SplitClear.Text = "Clear Splits";
	this.SplitClear.UseVisualStyleBackColor = true;
	this.SplitClear.Click += new System.EventHandler(this.SplitClear_Click);
	// 
	// EditSplit
	// 
	this.EditSplit.Enabled = false;
	this.EditSplit.Location = new System.Drawing.Point(476, 112);
	this.EditSplit.Name = "EditSplit";
	this.EditSplit.Size = new System.Drawing.Size(118, 23);
	this.EditSplit.TabIndex = 44;
	this.EditSplit.Text = "Edit Split";
	this.EditSplit.UseVisualStyleBackColor = true;
	this.EditSplit.Click += new System.EventHandler(this.EditSplit_Click);
	// 
	// SplitDown
	// 
	this.SplitDown.Enabled = false;
	this.SplitDown.Location = new System.Drawing.Point(476, 199);
	this.SplitDown.Name = "SplitDown";
	this.SplitDown.Size = new System.Drawing.Size(118, 23);
	this.SplitDown.TabIndex = 43;
	this.SplitDown.Text = "Move Split Down";
	this.SplitDown.UseVisualStyleBackColor = true;
	this.SplitDown.Click += new System.EventHandler(this.SplitDown_Click);
	// 
	// SplitUp
	// 
	this.SplitUp.Enabled = false;
	this.SplitUp.Location = new System.Drawing.Point(476, 170);
	this.SplitUp.Name = "SplitUp";
	this.SplitUp.Size = new System.Drawing.Size(118, 23);
	this.SplitUp.TabIndex = 42;
	this.SplitUp.Text = "Move Split Up";
	this.SplitUp.UseVisualStyleBackColor = true;
	this.SplitUp.Click += new System.EventHandler(this.SplitUp_Click);
	// 
	// SplitAddAfter
	// 
	this.SplitAddAfter.Enabled = false;
	this.SplitAddAfter.Location = new System.Drawing.Point(476, 84);
	this.SplitAddAfter.Name = "SplitAddAfter";
	this.SplitAddAfter.Size = new System.Drawing.Size(118, 23);
	this.SplitAddAfter.TabIndex = 41;
	this.SplitAddAfter.Text = "Add Split After";
	this.SplitAddAfter.UseVisualStyleBackColor = true;
	this.SplitAddAfter.Click += new System.EventHandler(this.SplitAddAfter_Click);
	// 
	// ReloadSplits
	// 
	this.ReloadSplits.Enabled = false;
	this.ReloadSplits.Location = new System.Drawing.Point(476, 259);
	this.ReloadSplits.Name = "ReloadSplits";
	this.ReloadSplits.Size = new System.Drawing.Size(118, 23);
	this.ReloadSplits.TabIndex = 40;
	this.ReloadSplits.Text = "Reload Splits";
	this.ReloadSplits.UseVisualStyleBackColor = true;
	this.ReloadSplits.Click += new System.EventHandler(this.ReloadSplits_Click);
	// 
	// CreateSplits
	// 
	this.CreateSplits.Location = new System.Drawing.Point(476, 26);
	this.CreateSplits.Name = "CreateSplits";
	this.CreateSplits.Size = new System.Drawing.Size(118, 23);
	this.CreateSplits.TabIndex = 39;
	this.CreateSplits.Text = "Create New Splits File";
	this.CreateSplits.UseVisualStyleBackColor = true;
	this.CreateSplits.Click += new System.EventHandler(this.CreateSplits_Click);
	// 
	// RemoveSplit
	// 
	this.RemoveSplit.Enabled = false;
	this.RemoveSplit.Location = new System.Drawing.Point(476, 141);
	this.RemoveSplit.Name = "RemoveSplit";
	this.RemoveSplit.Size = new System.Drawing.Size(118, 23);
	this.RemoveSplit.TabIndex = 38;
	this.RemoveSplit.Text = "Remove Split";
	this.RemoveSplit.UseVisualStyleBackColor = true;
	this.RemoveSplit.Click += new System.EventHandler(this.RemoveSplit_Click);
	// 
	// SplitAddBefore
	// 
	this.SplitAddBefore.Enabled = false;
	this.SplitAddBefore.Location = new System.Drawing.Point(476, 55);
	this.SplitAddBefore.Name = "SplitAddBefore";
	this.SplitAddBefore.Size = new System.Drawing.Size(118, 23);
	this.SplitAddBefore.TabIndex = 37;
	this.SplitAddBefore.Text = "Add Split Before";
	this.SplitAddBefore.UseVisualStyleBackColor = true;
	this.SplitAddBefore.Click += new System.EventHandler(this.SplitAddBefore_Click);
	// 
	// SaveSplits
	// 
	this.SaveSplits.Location = new System.Drawing.Point(241, 288);
	this.SaveSplits.Name = "SaveSplits";
	this.SaveSplits.Size = new System.Drawing.Size(229, 23);
	this.SaveSplits.TabIndex = 36;
	this.SaveSplits.Text = "Save Splits";
	this.SaveSplits.UseVisualStyleBackColor = true;
	this.SaveSplits.Click += new System.EventHandler(this.SaveSplits_Click);
	// 
	// LoadSplits
	// 
	this.LoadSplits.Location = new System.Drawing.Point(6, 288);
	this.LoadSplits.Name = "LoadSplits";
	this.LoadSplits.Size = new System.Drawing.Size(229, 23);
	this.LoadSplits.TabIndex = 35;
	this.LoadSplits.Text = "Load Splits";
	this.LoadSplits.UseVisualStyleBackColor = true;
	this.LoadSplits.Click += new System.EventHandler(this.LoadSplits_Click);
	// 
	// label8
	// 
	this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.label8.Location = new System.Drawing.Point(3, 3);
	this.label8.Name = "label8";
	this.label8.Size = new System.Drawing.Size(467, 23);
	this.label8.TabIndex = 32;
	this.label8.Text = "Current Splits";
	this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
	// 
	// UISettings
	// 
	this.UISettings.BackColor = System.Drawing.Color.White;
	this.UISettings.Controls.Add(this.label5);
	this.UISettings.Controls.Add(this.label4);
	this.UISettings.Controls.Add(this.ChromaEnable);
	this.UISettings.Controls.Add(this.EnablePadding);
	this.UISettings.Controls.Add(this.PaddingSides);
	this.UISettings.Controls.Add(this.label2);
	this.UISettings.Controls.Add(this.PaddingSize);
	this.UISettings.Controls.Add(this.ApplySize);
	this.UISettings.Controls.Add(this.ChromaSides);
	this.UISettings.Controls.Add(this.EnableSpecificChroma);
	this.UISettings.Controls.Add(this.EditChromaColor);
	this.UISettings.Controls.Add(this.label1);
	this.UISettings.Controls.Add(this.ColorDisplay);
	this.UISettings.Location = new System.Drawing.Point(4, 22);
	this.UISettings.Name = "UISettings";
	this.UISettings.Padding = new System.Windows.Forms.Padding(3);
	this.UISettings.Size = new System.Drawing.Size(600, 322);
	this.UISettings.TabIndex = 1;
	this.UISettings.Text = "Bizhawk UI Settings";
	// 
	// label5
	// 
	this.label5.Location = new System.Drawing.Point(178, 36);
	this.label5.Name = "label5";
	this.label5.Size = new System.Drawing.Size(35, 23);
	this.label5.TabIndex = 14;
	this.label5.Text = "Side:";
	this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label4
	// 
	this.label4.Location = new System.Drawing.Point(178, 6);
	this.label4.Name = "label4";
	this.label4.Size = new System.Drawing.Size(35, 23);
	this.label4.TabIndex = 13;
	this.label4.Text = "Side:";
	this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// ChromaEnable
	// 
	this.ChromaEnable.Location = new System.Drawing.Point(6, 36);
	this.ChromaEnable.Name = "ChromaEnable";
	this.ChromaEnable.Size = new System.Drawing.Size(166, 24);
	this.ChromaEnable.TabIndex = 3;
	this.ChromaEnable.Text = "Enable Background Chroma";
	this.ChromaEnable.UseVisualStyleBackColor = true;
	this.ChromaEnable.CheckedChanged += new System.EventHandler(this.ChromaEnable_CheckedChanged);
	// 
	// EnablePadding
	// 
	this.EnablePadding.Location = new System.Drawing.Point(6, 6);
	this.EnablePadding.Name = "EnablePadding";
	this.EnablePadding.Size = new System.Drawing.Size(125, 24);
	this.EnablePadding.TabIndex = 10;
	this.EnablePadding.Text = "Enable UI Extension";
	this.EnablePadding.UseVisualStyleBackColor = true;
	this.EnablePadding.CheckedChanged += new System.EventHandler(this.EnablePadding_CheckedChanged);
	// 
	// PaddingSides
	// 
	this.PaddingSides.Enabled = false;
	this.PaddingSides.FormattingEnabled = true;
	this.PaddingSides.Items.AddRange(new object[] { "Left", "Top", "Right", "Bottom" });
	this.PaddingSides.Location = new System.Drawing.Point(214, 6);
	this.PaddingSides.Name = "PaddingSides";
	this.PaddingSides.Size = new System.Drawing.Size(125, 21);
	this.PaddingSides.TabIndex = 9;
	this.PaddingSides.SelectedIndexChanged += new System.EventHandler(this.PaddingSides_SelectedIndexChanged);
	// 
	// label2
	// 
	this.label2.Location = new System.Drawing.Point(345, 6);
	this.label2.Name = "label2";
	this.label2.Size = new System.Drawing.Size(30, 23);
	this.label2.TabIndex = 12;
	this.label2.Text = "Size:";
	this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// PaddingSize
	// 
	this.PaddingSize.Enabled = false;
	this.PaddingSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.PaddingSize.Location = new System.Drawing.Point(381, 6);
	this.PaddingSize.Name = "PaddingSize";
	this.PaddingSize.Size = new System.Drawing.Size(117, 21);
	this.PaddingSize.TabIndex = 11;
	// 
	// ApplySize
	// 
	this.ApplySize.Enabled = false;
	this.ApplySize.Location = new System.Drawing.Point(504, 5);
	this.ApplySize.Name = "ApplySize";
	this.ApplySize.Size = new System.Drawing.Size(90, 22);
	this.ApplySize.TabIndex = 13;
	this.ApplySize.Text = "Apply";
	this.ApplySize.UseVisualStyleBackColor = true;
	this.ApplySize.Click += new System.EventHandler(this.ApplySize_Click);
	// 
	// ChromaSides
	// 
	this.ChromaSides.Enabled = false;
	this.ChromaSides.FormattingEnabled = true;
	this.ChromaSides.Items.AddRange(new object[] { "Left", "Top", "Right", "Bottom" });
	this.ChromaSides.Location = new System.Drawing.Point(214, 38);
	this.ChromaSides.Name = "ChromaSides";
	this.ChromaSides.Size = new System.Drawing.Size(217, 21);
	this.ChromaSides.TabIndex = 4;
	this.ChromaSides.SelectedIndexChanged += new System.EventHandler(this.ChromaSides_SelectedIndexChanged);
	// 
	// EnableSpecificChroma
	// 
	this.EnableSpecificChroma.Enabled = false;
	this.EnableSpecificChroma.Location = new System.Drawing.Point(437, 36);
	this.EnableSpecificChroma.Name = "EnableSpecificChroma";
	this.EnableSpecificChroma.Size = new System.Drawing.Size(157, 24);
	this.EnableSpecificChroma.TabIndex = 5;
	this.EnableSpecificChroma.Text = "Show on This Side";
	this.EnableSpecificChroma.UseVisualStyleBackColor = true;
	this.EnableSpecificChroma.CheckedChanged += new System.EventHandler(this.EnableSpecificChroma_CheckedChanged);
	// 
	// EditChromaColor
	// 
	this.EditChromaColor.Enabled = false;
	this.EditChromaColor.Location = new System.Drawing.Point(6, 66);
	this.EditChromaColor.Name = "EditChromaColor";
	this.EditChromaColor.Size = new System.Drawing.Size(270, 23);
	this.EditChromaColor.TabIndex = 6;
	this.EditChromaColor.Text = "Edit Chroma Color";
	this.EditChromaColor.UseVisualStyleBackColor = true;
	this.EditChromaColor.Click += new System.EventHandler(this.EditChromaColor_Click);
	// 
	// label1
	// 
	this.label1.Location = new System.Drawing.Point(282, 66);
	this.label1.Name = "label1";
	this.label1.Size = new System.Drawing.Size(149, 23);
	this.label1.TabIndex = 7;
	this.label1.Text = "Current Color: ";
	this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// ColorDisplay
	// 
	this.ColorDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
	this.ColorDisplay.Location = new System.Drawing.Point(437, 66);
	this.ColorDisplay.Name = "ColorDisplay";
	this.ColorDisplay.Size = new System.Drawing.Size(157, 23);
	this.ColorDisplay.TabIndex = 8;
	this.ColorDisplay.TabStop = false;
	// 
	// GeneralSettings
	// 
	this.GeneralSettings.BackColor = System.Drawing.Color.White;
	this.GeneralSettings.Controls.Add(this.UndoUpdateLiveSplit);
	this.GeneralSettings.Controls.Add(this.SaveSettingsAsDefault);
	this.GeneralSettings.Controls.Add(this.SkipUpdateLivesplit);
	this.GeneralSettings.Controls.Add(this.UndoSplit);
	this.GeneralSettings.Controls.Add(this.SkipSplit);
	this.GeneralSettings.Controls.Add(this.MovieMode);
	this.GeneralSettings.Controls.Add(this.ResetAutosplitter);
	this.GeneralSettings.Controls.Add(this.SplitId);
	this.GeneralSettings.Controls.Add(this.label12);
	this.GeneralSettings.Controls.Add(this.LoadedSplit);
	this.GeneralSettings.Controls.Add(this.label10);
	this.GeneralSettings.Controls.Add(this.SplitFile);
	this.GeneralSettings.Controls.Add(this.label7);
	this.GeneralSettings.Controls.Add(this.SelectSplits);
	this.GeneralSettings.Controls.Add(this.StartAutosplitter);
	this.GeneralSettings.Controls.Add(this.UndoChanges);
	this.GeneralSettings.Controls.Add(this.RestoreDefaults);
	this.GeneralSettings.Controls.Add(this.InputViewerEnable);
	this.GeneralSettings.Controls.Add(this.Save);
	this.GeneralSettings.Controls.Add(this.EnableAutoSplitter);
	this.GeneralSettings.Location = new System.Drawing.Point(4, 22);
	this.GeneralSettings.Name = "GeneralSettings";
	this.GeneralSettings.Padding = new System.Windows.Forms.Padding(3);
	this.GeneralSettings.Size = new System.Drawing.Size(600, 322);
	this.GeneralSettings.TabIndex = 0;
	this.GeneralSettings.Text = "General Settings";
	// 
	// UndoUpdateLiveSplit
	// 
	this.UndoUpdateLiveSplit.Enabled = false;
	this.UndoUpdateLiveSplit.Location = new System.Drawing.Point(451, 91);
	this.UndoUpdateLiveSplit.Name = "UndoUpdateLiveSplit";
	this.UndoUpdateLiveSplit.Size = new System.Drawing.Size(143, 23);
	this.UndoUpdateLiveSplit.TabIndex = 43;
	this.UndoUpdateLiveSplit.Text = "Undo Split Update Livesplit";
	this.UndoUpdateLiveSplit.UseVisualStyleBackColor = true;
	this.UndoUpdateLiveSplit.Click += new System.EventHandler(this.UndoUpdateLiveSplit_Click);
	// 
	// SaveSettingsAsDefault
	// 
	this.SaveSettingsAsDefault.Location = new System.Drawing.Point(303, 291);
	this.SaveSettingsAsDefault.Name = "SaveSettingsAsDefault";
	this.SaveSettingsAsDefault.Size = new System.Drawing.Size(143, 23);
	this.SaveSettingsAsDefault.TabIndex = 42;
	this.SaveSettingsAsDefault.Text = "Save Settings As Default";
	this.SaveSettingsAsDefault.UseVisualStyleBackColor = true;
	this.SaveSettingsAsDefault.Click += new System.EventHandler(this.SaveSettingsAsDefault_Click);
	// 
	// SkipUpdateLivesplit
	// 
	this.SkipUpdateLivesplit.Enabled = false;
	this.SkipUpdateLivesplit.Location = new System.Drawing.Point(154, 91);
	this.SkipUpdateLivesplit.Name = "SkipUpdateLivesplit";
	this.SkipUpdateLivesplit.Size = new System.Drawing.Size(143, 23);
	this.SkipUpdateLivesplit.TabIndex = 41;
	this.SkipUpdateLivesplit.Text = "Skip Split Update Livesplit";
	this.SkipUpdateLivesplit.UseVisualStyleBackColor = true;
	this.SkipUpdateLivesplit.Click += new System.EventHandler(this.SkipUpdateLivesplit_Click);
	// 
	// UndoSplit
	// 
	this.UndoSplit.Enabled = false;
	this.UndoSplit.Location = new System.Drawing.Point(303, 91);
	this.UndoSplit.Name = "UndoSplit";
	this.UndoSplit.Size = new System.Drawing.Size(143, 23);
	this.UndoSplit.TabIndex = 40;
	this.UndoSplit.Text = "Undo Split No Livesplit";
	this.UndoSplit.UseVisualStyleBackColor = true;
	this.UndoSplit.Click += new System.EventHandler(this.UndoSplit_Click);
	// 
	// SkipSplit
	// 
	this.SkipSplit.Enabled = false;
	this.SkipSplit.Location = new System.Drawing.Point(6, 91);
	this.SkipSplit.Name = "SkipSplit";
	this.SkipSplit.Size = new System.Drawing.Size(142, 23);
	this.SkipSplit.TabIndex = 39;
	this.SkipSplit.Text = "Skip Split No Livesplit";
	this.SkipSplit.UseVisualStyleBackColor = true;
	this.SkipSplit.Click += new System.EventHandler(this.SkipSplit_Click);
	// 
	// MovieMode
	// 
	this.MovieMode.Location = new System.Drawing.Point(156, 6);
	this.MovieMode.Name = "MovieMode";
	this.MovieMode.Size = new System.Drawing.Size(105, 24);
	this.MovieMode.TabIndex = 38;
	this.MovieMode.Text = "Movie Mode";
	this.MovieMode.UseVisualStyleBackColor = false;
	this.MovieMode.CheckedChanged += new System.EventHandler(this.MovieMode_CheckedChanged);
	// 
	// ResetAutosplitter
	// 
	this.ResetAutosplitter.Enabled = false;
	this.ResetAutosplitter.Location = new System.Drawing.Point(376, 62);
	this.ResetAutosplitter.Name = "ResetAutosplitter";
	this.ResetAutosplitter.Size = new System.Drawing.Size(218, 23);
	this.ResetAutosplitter.TabIndex = 37;
	this.ResetAutosplitter.Text = "Reset Autosplitter to Split 0";
	this.ResetAutosplitter.UseVisualStyleBackColor = true;
	this.ResetAutosplitter.Click += new System.EventHandler(this.ResetAutosplitter_Click);
	// 
	// SplitId
	// 
	this.SplitId.Location = new System.Drawing.Point(264, 62);
	this.SplitId.Name = "SplitId";
	this.SplitId.Size = new System.Drawing.Size(106, 23);
	this.SplitId.TabIndex = 36;
	this.SplitId.Text = "0";
	this.SplitId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label12
	// 
	this.label12.Location = new System.Drawing.Point(203, 62);
	this.label12.Name = "label12";
	this.label12.Size = new System.Drawing.Size(55, 23);
	this.label12.TabIndex = 35;
	this.label12.Text = "Split ID:";
	this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// LoadedSplit
	// 
	this.LoadedSplit.Location = new System.Drawing.Point(79, 62);
	this.LoadedSplit.Name = "LoadedSplit";
	this.LoadedSplit.Size = new System.Drawing.Size(118, 23);
	this.LoadedSplit.TabIndex = 34;
	this.LoadedSplit.Text = "No Splits Loaded";
	this.LoadedSplit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label10
	// 
	this.label10.Location = new System.Drawing.Point(6, 62);
	this.label10.Name = "label10";
	this.label10.Size = new System.Drawing.Size(67, 23);
	this.label10.TabIndex = 33;
	this.label10.Text = "Current Split:";
	this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// SplitFile
	// 
	this.SplitFile.Location = new System.Drawing.Point(264, 36);
	this.SplitFile.Name = "SplitFile";
	this.SplitFile.Size = new System.Drawing.Size(330, 23);
	this.SplitFile.TabIndex = 32;
	this.SplitFile.Text = "No File Selected - Please Select a File To Use Autosplitter";
	this.SplitFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// label7
	// 
	this.label7.Location = new System.Drawing.Point(156, 36);
	this.label7.Name = "label7";
	this.label7.Size = new System.Drawing.Size(67, 23);
	this.label7.TabIndex = 31;
	this.label7.Text = "Current File:";
	this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
	// 
	// SelectSplits
	// 
	this.SelectSplits.Enabled = false;
	this.SelectSplits.Location = new System.Drawing.Point(6, 36);
	this.SelectSplits.Name = "SelectSplits";
	this.SelectSplits.Size = new System.Drawing.Size(144, 23);
	this.SelectSplits.TabIndex = 30;
	this.SelectSplits.Text = "Select Splits File";
	this.SelectSplits.UseVisualStyleBackColor = true;
	this.SelectSplits.Click += new System.EventHandler(this.SelectSplits_Click);
	// 
	// StartAutosplitter
	// 
	this.StartAutosplitter.Enabled = false;
	this.StartAutosplitter.Location = new System.Drawing.Point(414, 6);
	this.StartAutosplitter.Name = "StartAutosplitter";
	this.StartAutosplitter.Size = new System.Drawing.Size(180, 23);
	this.StartAutosplitter.TabIndex = 29;
	this.StartAutosplitter.Text = "Start Autosplitter";
	this.StartAutosplitter.UseVisualStyleBackColor = true;
	this.StartAutosplitter.Click += new System.EventHandler(this.StartAutosplitter_Click);
	// 
	// UndoChanges
	// 
	this.UndoChanges.Location = new System.Drawing.Point(452, 291);
	this.UndoChanges.Name = "UndoChanges";
	this.UndoChanges.Size = new System.Drawing.Size(142, 23);
	this.UndoChanges.TabIndex = 28;
	this.UndoChanges.Text = "Undo Changes";
	this.UndoChanges.UseVisualStyleBackColor = true;
	this.UndoChanges.Click += new System.EventHandler(this.UndoChanges_Click);
	// 
	// RestoreDefaults
	// 
	this.RestoreDefaults.Location = new System.Drawing.Point(154, 291);
	this.RestoreDefaults.Name = "RestoreDefaults";
	this.RestoreDefaults.Size = new System.Drawing.Size(143, 23);
	this.RestoreDefaults.TabIndex = 27;
	this.RestoreDefaults.Text = "Restore Default Settings";
	this.RestoreDefaults.UseVisualStyleBackColor = true;
	this.RestoreDefaults.Click += new System.EventHandler(this.RestoreDefaults_Click);
	// 
	// InputViewerEnable
	// 
	this.InputViewerEnable.Location = new System.Drawing.Point(6, 6);
	this.InputViewerEnable.Name = "InputViewerEnable";
	this.InputViewerEnable.Size = new System.Drawing.Size(125, 24);
	this.InputViewerEnable.TabIndex = 0;
	this.InputViewerEnable.Text = "Enable Input Viewer";
	this.InputViewerEnable.UseVisualStyleBackColor = true;
	this.InputViewerEnable.CheckedChanged += new System.EventHandler(this.InputViewerEnable_CheckedChanged);
	// 
	// Save
	// 
	this.Save.Location = new System.Drawing.Point(6, 291);
	this.Save.Name = "Save";
	this.Save.Size = new System.Drawing.Size(142, 23);
	this.Save.TabIndex = 23;
	this.Save.Text = "Save Settings";
	this.Save.UseVisualStyleBackColor = true;
	this.Save.Click += new System.EventHandler(this.Save_Click);
	// 
	// EnableAutoSplitter
	// 
	this.EnableAutoSplitter.Location = new System.Drawing.Point(267, 6);
	this.EnableAutoSplitter.Name = "EnableAutoSplitter";
	this.EnableAutoSplitter.Size = new System.Drawing.Size(141, 24);
	this.EnableAutoSplitter.TabIndex = 26;
	this.EnableAutoSplitter.Text = "Use Auto Splitter";
	this.EnableAutoSplitter.UseVisualStyleBackColor = false;
	this.EnableAutoSplitter.CheckedChanged += new System.EventHandler(this.EnableAutoSplitter_CheckedChanged);
	// 
	// TabControl
	// 
	this.TabControl.Controls.Add(this.GeneralSettings);
	this.TabControl.Controls.Add(this.UISettings);
	this.TabControl.Controls.Add(this.SplitterConfig);
	this.TabControl.Controls.Add(this.InputConfig);
	this.TabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
	this.TabControl.Location = new System.Drawing.Point(12, 12);
	this.TabControl.Name = "TabControl";
	this.TabControl.SelectedIndex = 0;
	this.TabControl.Size = new System.Drawing.Size(608, 348);
	this.TabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
	this.TabControl.TabIndex = 27;
	// 
	// ToolWindow
	// 
	this.ClientSize = new System.Drawing.Size(632, 369);
	this.Controls.Add(this.TabControl);
	this.Name = "ToolWindow";
	this.Text = "Minish Cap Tools";
	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Close);
	this.tabPage7.ResumeLayout(false);
	this.InputConfig.ResumeLayout(false);
	this.SplitterConfig.ResumeLayout(false);
	this.UISettings.ResumeLayout(false);
	this.UISettings.PerformLayout();
	((System.ComponentModel.ISupportInitialize)(this.ColorDisplay)).EndInit();
	this.GeneralSettings.ResumeLayout(false);
	this.TabControl.ResumeLayout(false);
	this.ResumeLayout(false);
}

private System.Windows.Forms.Label label37;
private System.Windows.Forms.Label label38;
private System.Windows.Forms.Label label39;
private System.Windows.Forms.Label label40;
private System.Windows.Forms.Label label41;
private System.Windows.Forms.Label label42;
private System.Windows.Forms.Label label43;
private System.Windows.Forms.Label label44;
private System.Windows.Forms.Label label45;
private System.Windows.Forms.Label label46;
private System.Windows.Forms.Label label47;
private System.Windows.Forms.Label label48;

private System.Windows.Forms.CheckBox checkBox2;

private System.Windows.Forms.Label label26;
private System.Windows.Forms.Label label27;
private System.Windows.Forms.Label label28;
private System.Windows.Forms.Label label29;
private System.Windows.Forms.Label label30;
private System.Windows.Forms.Label label31;
private System.Windows.Forms.Label label32;
private System.Windows.Forms.Label label33;
private System.Windows.Forms.Label label34;
private System.Windows.Forms.Label label35;
private System.Windows.Forms.Label label36;

private System.Windows.Forms.Label label25;

private System.Windows.Forms.Label label11;
private System.Windows.Forms.Label label13;
private System.Windows.Forms.Label label14;
private System.Windows.Forms.Label label15;
private System.Windows.Forms.Label label16;
private System.Windows.Forms.Label label17;
private System.Windows.Forms.Label label18;
private System.Windows.Forms.Label label19;
private System.Windows.Forms.Label label20;
private System.Windows.Forms.Label label21;
private System.Windows.Forms.Label label22;
private System.Windows.Forms.Label label23;
private System.Windows.Forms.Label label24;

private System.Windows.Forms.CheckBox checkBox1;
private System.Windows.Forms.Label label3;

private System.Windows.Forms.Button UndoUpdateLiveSplit;

private System.Windows.Forms.Button SaveSettingsAsDefault;

private System.Windows.Forms.Button SkipSplit;
private System.Windows.Forms.Button UndoSplit;
private System.Windows.Forms.Button SkipUpdateLivesplit;

private System.Windows.Forms.ColumnHeader header1;
private System.Windows.Forms.ColumnHeader header2;
private System.Windows.Forms.ColumnHeader header3;
private System.Windows.Forms.ColumnHeader header4;

private System.Windows.Forms.ColumnHeader EName;

private System.Windows.Forms.ListView Splits;

private System.Windows.Forms.CheckBox MovieMode;

private System.Windows.Forms.Button InputViewerRestorePos;

private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label SplitId;
        private System.Windows.Forms.Button ResetAutosplitter;

        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label LoadedSplit;

        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Label label9;

        private System.Windows.Forms.OpenFileDialog SelectSplitsLoader;

        private System.Windows.Forms.SaveFileDialog ConfigSplitsSave;

        private System.Windows.Forms.OpenFileDialog ConfigSplitsLoader;

        private System.Windows.Forms.Button SplitDisable;

        private System.Windows.Forms.Button EditSplit;
        private System.Windows.Forms.Button SplitClear;

        private System.Windows.Forms.Button ReloadSplits;
        private System.Windows.Forms.Button SplitAddAfter;
        private System.Windows.Forms.Button SplitUp;
        private System.Windows.Forms.Button SplitDown;

        private System.Windows.Forms.Button SplitAddBefore;
        private System.Windows.Forms.Button RemoveSplit;
        private System.Windows.Forms.Button CreateSplits;

        private System.Windows.Forms.Button LoadSplits;
        private System.Windows.Forms.Button SaveSplits;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label SplitFile;

        private System.Windows.Forms.Button SelectSplits;

        private System.Windows.Forms.Button StartAutosplitter;

        private System.Windows.Forms.Button RestoreDefaults;
        private System.Windows.Forms.Button UndoChanges;

        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.Label label4;

        private System.Windows.Forms.TabPage InputConfig;

        private System.Windows.Forms.TabPage SplitterConfig;

        private System.Windows.Forms.TabControl TabControl;

        private System.Windows.Forms.TabPage GeneralSettings;
        private System.Windows.Forms.TabPage UISettings;

        private System.Windows.Forms.CheckBox EnableAutoSplitter;

        private System.Windows.Forms.Button Save;

        private System.Windows.Forms.ColorDialog ButtonCounterTextColorEditor;

        private System.Windows.Forms.Button ApplySize;

        private System.Windows.Forms.TextBox PaddingSize;
        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.ComboBox PaddingSides;
        private System.Windows.Forms.CheckBox EnablePadding;

        private System.Windows.Forms.PictureBox ColorDisplay;
        private System.Windows.Forms.ColorDialog ChromaColorEditor;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Button EditChromaColor;

        private System.Windows.Forms.ComboBox ChromaSides;
        private System.Windows.Forms.CheckBox EnableSpecificChroma;

        private System.Windows.Forms.CheckBox ChromaEnable;

        private System.Windows.Forms.CheckBox InputBorder;

		private System.Windows.Forms.CheckBox InputViewerEnable;
#endregion
	}
}