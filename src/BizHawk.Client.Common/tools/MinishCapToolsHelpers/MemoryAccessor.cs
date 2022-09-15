using System;
using System.Collections.Generic;
using BizHawk.Client.Common.MinishCapToolsHelpers.Enumerables;

namespace BizHawk.Client.Common.MinishCapToolsHelpers
{
	public static class MemoryAccessor
    {
        public static List<byte> LoadMemoryRegionAsByteArray(long address, IMemoryApi memory, int length, MemoryDomain domain)
        {
            return memory.ReadByteRange(address, length, domain.GetDomainAsString());
        }
        
        public static string ReadString(long address, IMemoryApi memory, int length, MemoryDomain domain)
        {
            return System.Text.Encoding.ASCII.GetString(memory.ReadByteRange(address, length, domain.GetDomainAsString()).ToArray());
        }

        public static uint LoadAddress(long address, IMemoryApi memory, ReadType type, MemoryDomain domain)
        {
            var dom = domain.GetDomainAsString();

            return type switch
            {
                ReadType.Byte => memory.ReadByte(address, dom),
                ReadType.Signed8 => (uint)memory.ReadS8(address, dom),
                ReadType.Signed16 => (uint)memory.ReadS16(address, dom),
                ReadType.Signed24 => (uint)memory.ReadS24(address, dom),
                ReadType.Signed32 => (uint)memory.ReadS32(address, dom),
                ReadType.Unsigned8 => memory.ReadU8(address, dom),
                ReadType.Unsigned16 => memory.ReadU16(address, dom),
                ReadType.Unsigned24 => memory.ReadU24(address, dom),
                ReadType.Unsigned32 => memory.ReadU32(address, dom),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static IDictionary<string, uint> LoadCurrentAreaAndRoom(IMemoryApi memory)
        {
            return new Dictionary<string, uint>
            {
                { "Area", memory.ReadU8(0x0BF4, "IWRAM") },
                { "Room", memory.ReadU8(0x0BF5, "IWRAM") }
            };
        }

        public static IDictionary<string, int> LoadTextboxValues(IMemoryApi memory)
        {
            return new Dictionary<string, int>
            {
                { "Textbox", memory.ReadS8(0x000050, "EWRAM") },
                { "Render", memory.ReadS8(0x022809, "EWRAM") }
            };
        }
    }
}