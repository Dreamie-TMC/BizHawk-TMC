using System;
using System.Collections.Generic;
using System.IO;
using BizHawk.Client.Common;
using BizHawk.Client.Common.MinishCapToolsHelpers;
using BizHawk.Client.Common.MinishCapToolsHelpers.Enumerables;
using BizHawk.Client.EmuHawk.AutoSplitter.Exceptions;
using BizHawk.Common.NumberExtensions;
using Newtonsoft.Json;

namespace BizHawk.Client.EmuHawk.AutoSplitter
{
	public class Splitter
	{
		private LivesplitServerConnector _livesplitServerConnector;
		
		internal IMemoryApi MemoryApi { get; set; }
		
		internal List<Split> Splits { get; private set; } = new();
		internal bool Started { get; private set; } = false;
		internal bool SplitsLoaded { get; private set; } = false;
		internal bool UpdateSplitText { get; private set; } = false;
		
		internal int CurrentSplitId { get; set; }
		internal int NextSplitId { get; set; }

		public Splitter()
		{
			_livesplitServerConnector = new LivesplitServerConnector();
		}

		public bool Start()
		{
			try
			{
				if (Started)
				{
					_livesplitServerConnector.Close();
					Started = false;
					return true;
				}

				_livesplitServerConnector.Start();
				Started = true;
				return true;
			}
			catch
			{
				Started = false;
				return false;
			}
		}

		public void LoadSplitsFile(string filepath)
		{
			Splits = JsonConvert.DeserializeObject<List<Split>>(File.ReadAllText($"{filepath}")) ??
				throw new AutosplitterConfigurationException("Failed to load valid splits file!");
			
			if (Splits.Count == 0)
				throw new AutosplitterConfigurationException("Failed to load valid splits file!");

			Splits.Sort((x, y) =>
			{
				if (x.OrderId == y.OrderId) return 0;
				return x.OrderId < y.OrderId ? -1 : 1;
			});

			SplitsLoaded = true;
			NextSplitId = 0;
			SetSplitIds();
		}
		
		public void SkipSplit(bool sendCommandToLivesplit)
		{
			try
			{
				SetSplitIds();

				if (!sendCommandToLivesplit || !Started) return;
				
				_livesplitServerConnector.SendSkipSplitCommand();

			}
			catch
			{
				Started = false;
				throw new AutosplitterConnectionException("Connection to Livesplit Server was Closed! Please " +
					"ensure the Server is running. If the error persists, " +
					"restart Bizhawk.");
			}
		}

		public void UndoSplit(bool sendCommandToLivesplit)
		{
			try
			{
				if (CurrentSplitId == 0) return;

				SetSplitIds(isUndo: true);

				if (!sendCommandToLivesplit || !Started) return;
				
				_livesplitServerConnector.SendUndoSplitCommand();
			}
			catch
			{
				Started = false;
				throw new AutosplitterConnectionException("Connection to Livesplit Server was Closed! Please " +
					"ensure the Server is running. If the error persists, " +
					"restart Bizhawk.");
			}
		}

		public void ResetSplits()
		{
			NextSplitId = 0;
			SetSplitIds();
			
			if (!Started) return;

			_livesplitServerConnector.SendResetCommand();
		}

		public void CheckSplit()
		{
			if (!Started) return;

			if (Splits == null)
			{
				Started = false;
				throw new AutosplitterConfigurationException("No splits file was loaded!");
			}

			UpdateSplitText = false;

			if (CurrentSplitId >= Splits.Count) return;

			var split = Splits[CurrentSplitId];
			switch (split.SplitType)
			{
				case SplitTypes.Start:
					ProcessStart(split);
					break;
				case SplitTypes.AreaEnter:
					ProcessAreaEnterFlag(split);
					break;
				case SplitTypes.Flag:
					ProcessFlag(split);
					break;
				case SplitTypes.Boss:
					ProcessBossSplit(split);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ProcessBossSplit(Split split)
		{
			var result = MemoryAccessor.LoadAddress(split.Address, MemoryApi, ReadType.Byte, split.Domain);
			if (result == split.Value)
				ProcessSplit();
		}

		private void ProcessFlag(Split split)
		{
			var result = (int)MemoryAccessor.LoadAddress(split.Address, MemoryApi, ReadType.Byte, split.Domain);
			if (result.Bit(split.Bit))
				ProcessSplit();
		}

		private void ProcessAreaEnterFlag(Split split)
		{
			var result = MemoryAccessor.LoadCurrentAreaAndRoom(MemoryApi);

			if (result.TryGetValue("Area", out var area) && area == split.AreaId && result.TryGetValue("Room", out var room) && room == split.RoomId)
				ProcessSplit();
		}

		private void ProcessStart(Split split)
		{
			var result = MemoryAccessor.LoadAddress(split.Address, MemoryApi, ReadType.Byte, split.Domain);

			if (result != split.Value) return;
			
			_livesplitServerConnector.SendStartCommand();
			SetSplitIds();
		}
		
		private void ProcessSplit()
		{
			try
			{
				_livesplitServerConnector.SendSplitCommand();
				SetSplitIds();
			}
			catch
			{
				Started = false;
				throw new AutosplitterConnectionException("Connection to Livesplit Server was Closed! Please " +
					"ensure the Server is running. If the error persists, " +
					"restart Bizhawk.");
			}
		}

		private void SetSplitIds(bool isUndo = false)
		{
			UpdateSplitText = true;
			if (isUndo)
			{
				NextSplitId = CurrentSplitId;
				--CurrentSplitId;
				while (CurrentSplitId > 0 && !Splits[CurrentSplitId].Enabled) --CurrentSplitId;
			}
			else
			{
				CurrentSplitId = NextSplitId;
				++NextSplitId;
				while (NextSplitId < Splits.Count && !Splits[NextSplitId].Enabled) ++NextSplitId;
			}
		}
	}
}