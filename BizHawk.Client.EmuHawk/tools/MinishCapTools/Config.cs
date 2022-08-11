using System.Collections.Generic;

namespace MinishCapTools
{
    public static class Config
    {
		private static readonly Dictionary<string, string> GameHashes = new Dictionary<string, string>
		{
			{ "B4BD50E4131B027C334547B4524E2DBBD4227130", "USA" },
			{ "6C5404A1EFFB17F481F352181D0F1C61A2765C5D", "JP" },
			{ "CFF199B36FF173FB6FAF152653D1BCCF87C26FB7", "EU" },
			{ "63FCAD218F9047B6A9EDBB68C98BD0DEC322D7A1", "USA (DEMO)" },
			{ "9CDB56FA79BBA13158B81925C1F3641251326412", "JP (DEMO)" },
			{ "9800F27A317339DF67F3A3B580178D2F9CFBD895", "JP (Firerod Speedrun Hack)" },
			{ "29278F6E6F0A0C7716057B3AE919150939650543", "JP (New Game Plus Speedrun Rom)"},
		};
		
		public static bool IsValidMinishCapRom(string hash)
		{
			return GameHashes.ContainsKey(hash);
		}
    }
}