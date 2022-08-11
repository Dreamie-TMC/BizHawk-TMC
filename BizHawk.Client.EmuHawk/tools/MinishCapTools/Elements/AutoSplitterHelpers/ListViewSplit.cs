using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Elements.AutoSplitterHelpers
{
    public class ListViewSplit
    {
        public string Name { get; set; }
        
        public bool Enabled { get; set; }
        
        public SplitTypes SplitType { get; set; }
        
        public MemoryDomain Domain { get; set; }
        
    }
}