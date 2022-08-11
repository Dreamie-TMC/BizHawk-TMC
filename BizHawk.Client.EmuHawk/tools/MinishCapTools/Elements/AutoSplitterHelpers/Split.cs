using MinishCapTools.Elements.Enums;

namespace MinishCapTools.Elements.AutoSplitterHelpers
{
    public class Split
    {
        public SplitTypes SplitType { get; set; }
        
        public MemoryDomain Domain { get; set; }
        
        public string Name { get; set; }
        
        public int OrderId { get; set; }
        
        public uint Address { get; set; }
        
        public int Value { get; set; }
        
        public int AreaId { get; set; }
        
        public int RoomId { get; set; }
        
        public int Bit { get; set; }
        
        public bool Enabled { get; set; }

        public override string ToString()
        {
            return $"Enabled: {Enabled}, Name: {Name}, Split Type: {SplitType}, Domain: {Domain}";
        }

        public string[] ToStringArray()
        {
            return new[]
            {
                Name,
                Enabled.ToString(),
                SplitType.ToString(),
                Domain.ToString()
            };
        }
    }
}