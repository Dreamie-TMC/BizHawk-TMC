using System;

namespace BizHawk.Client.Common.MinishCapToolsHelpers.Enumerables
{
    public static class EnumExtensions
    {
        public static string GetDomainAsString(this MemoryDomain domain)
        {
            return domain switch
            {
                MemoryDomain.IWRAM => "IWRAM",
                MemoryDomain.EWRAM => "EWRAM",
                MemoryDomain.BIOS => "BIOS",
                // ReSharper disable StringLiteralTypo
                MemoryDomain.PALRAM => "PALRAM",
                MemoryDomain.VRAM => "VRAM",
                MemoryDomain.OAM => "OAM",
                MemoryDomain.ROM => "ROM",
                MemoryDomain.SRAM => "SRAM",
                MemoryDomain.CombinedWRAM => "Combined WRAM",
                MemoryDomain.SystemBus => "System Bus", 
				_ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
			};
        }
    }
}