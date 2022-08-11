using System.Collections.Generic;
using System.IO;
using MinishCapTools.Elements.AutoSplitterHelpers;
using MinishCapTools.Elements.Enums;
using MinishCapTools.Exceptions;
using Newtonsoft.Json;

namespace MinishCapTools.Elements
{
    public class AutoSplitterSplitEditor
    {
        public List<Split> Splits { get; private set; }
        
        public bool SplitsLoadedFromFile { get; private set; }

        public string LoadedFile { get; private set; } = "Splits.json";

		public AutoSplitterSplitEditor()
		{
			Splits = new List<Split>();
		}
        
        public void LoadSplits(string filepath)
        {
            Splits = JsonConvert.DeserializeObject<List<Split>>(File.ReadAllText(@$"{filepath}")) ?? 
                     throw new AutosplitterConfigurationException("Failed to load valid splits file!");

            if (Splits.Count == 0)
                throw new AutosplitterConfigurationException("Failed to load valid splits file!");

            LoadedFile = filepath;
            
            SplitsLoadedFromFile = true;
            
            Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });
        }

        public void ClearSplits()
        {
            Splits = new List<Split>();
            GenerateStartSplit();
        }

        public void CreateNewSplits()
        {
            Splits = new List<Split>();
            SplitsLoadedFromFile = false;
            GenerateStartSplit();
        }

        public void WriteSplits(string filename)
        {
            Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });
            File.WriteAllText(filename, JsonConvert.SerializeObject(Splits, Formatting.Indented));
            LoadedFile = filename;
            SplitsLoadedFromFile = true;
        }

        public void RefreshSplits()
        {
            if (!SplitsLoadedFromFile) throw new AutosplitterConfigurationException("Cannot reload splits without an associated file!");

            Splits = JsonConvert.DeserializeObject<List<Split>>(File.ReadAllText(@$"{LoadedFile}")) ?? 
                     throw new AutosplitterConfigurationException("Failed to load valid splits file!");

            if (Splits.Count == 0)
                throw new AutosplitterConfigurationException("Failed to load valid splits file!");
            
            Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });
        }

        private void GenerateStartSplit()
        {
            Splits.Add(new Split
            {
                Address = 0x1002,
                Value = 2,
                SplitType = SplitTypes.Start,
                Domain = MemoryDomain.IWRAM,
                Name = "Start Split",
                OrderId = 0,
                Enabled = true
            });
        }
    }
}