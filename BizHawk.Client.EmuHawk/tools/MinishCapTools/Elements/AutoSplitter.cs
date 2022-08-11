using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using BizHawk.Client.Common;
using BizHawk.Common.NumberExtensions;
using MinishCapTools.Elements.AutoSplitterHelpers;
using MinishCapTools.Elements.Enums;
using MinishCapTools.Exceptions;
using Newtonsoft.Json;

namespace MinishCapTools.Elements
{
    public class AutoSplitter
    {
        public List<Split> Splits { get; private set; }
        public int SplitId { get; internal set; }
        public int LastSplitId { get; internal set; }
        public bool Started { get; private set; }

        private Socket _connection;

		public AutoSplitter()
		{
			Splits = new List<Split>();
			Started = false;
		}

        public void LoadSplitsFile(string filepath)
        {
            Splits = JsonConvert.DeserializeObject<List<Split>>(File.ReadAllText(@$"{filepath}")) ?? 
                     throw new AutosplitterConfigurationException("Failed to load valid splits file!");
            
            if (Splits.Count == 0)
                throw new AutosplitterConfigurationException("Failed to load valid splits file!");
            
            Splits.Sort((x, y) =>
            {
                if (x.OrderId == y.OrderId) return 0;
                return x.OrderId < y.OrderId ? -1 : 1;
            });
            SplitId = 0;
            LastSplitId = -1;
        }

		public void SkipSplit(bool sendCommandToLivesplit)
		{
			try
			{
				JumpToNextSplit();
				
				if (!sendCommandToLivesplit || !Started) return;
				
				const string message = "skipsplit\r\n";
				var data = new byte[message.Length];
				for (var i = 0; i < message.Length; ++i)
				{
					data[i] = Convert.ToByte(message[i]);
				}

				_connection.Send(data);
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
				if (SplitId == 0) return;
				
				RevertToPreviousSplit();
				
				if (!sendCommandToLivesplit || !Started) return;
				
				const string message = "unsplit\r\n";
				var data = new byte[message.Length];
				for (var i = 0; i < message.Length; ++i)
				{
					data[i] = Convert.ToByte(message[i]);
				}

				_connection.Send(data);
			}
			catch
			{
				Started = false;
				throw new AutosplitterConnectionException("Connection to Livesplit Server was Closed! Please " +
					"ensure the Server is running. If the error persists, " +
					"restart Bizhawk.");
			}
		}

        public bool Start()
        {
            try
            {
                if (Started)
                {
                    _connection.Close();
                    Started = false;
                    return true;
                }
                
                _connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _connection.Connect("localhost", 16834);
                Started = true;
                return true;
            }
            catch
            {
                Started = false;
                return false;
            }
        }

        public void CheckSplit(MemApi memApi)
        {
            if (!Started) return;

            if (Splits == null)
            {
                Started = false;
                throw new AutosplitterConfigurationException("No splits file was loaded!");
            }

            LastSplitId = SplitId;
            
            if (SplitId >= Splits.Count) return;

            var split = Splits[SplitId];
            switch (split.SplitType)
            {
                case SplitTypes.Start:
                    ProcessStart(split, memApi);
                    break;
                case SplitTypes.AreaEnter:
                    ProcessAreaEnterFlag(split, memApi);
                    break;
                case SplitTypes.Flag:
                    ProcessFlag(split, memApi);
                    break;
                case SplitTypes.Boss:
                    ProcessBossSplit(split, memApi);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessSplit()
        {
            try
            {
                const string message = "split\r\n";
                var data = new byte[message.Length];
                for (var i = 0; i < message.Length; ++i)
                {
                    data[i] = Convert.ToByte(message[i]);
                }

                _connection.Send(data);
				JumpToNextSplit();
            }
            catch
            {
                Started = false;
                throw new AutosplitterConnectionException("Connection to Livesplit Server was Closed! Please " +
                                                          "ensure the Server is running. If the error persists, " +
                                                          "restart Bizhawk.");
            }
        }

        private void ProcessBossSplit(Split split, MemApi memApi)
        {
            var result = MemoryAccessor.LoadAddress(split.Address, memApi, ReadType.Byte, split.Domain);
            if (result == split.Value)
                ProcessSplit();
        }

        private void ProcessFlag(Split split, MemApi memApi)
        {
            var result = (int)MemoryAccessor.LoadAddress(split.Address, memApi, ReadType.Byte, split.Domain);
            if (result.Bit(split.Bit))
                ProcessSplit();
        }

        private void ProcessAreaEnterFlag(Split split, MemApi memApi)
        {
            var result = MemoryAccessor.LoadCurrentAreaAndRoom(memApi);
            
            if (result.TryGetValue("Area", out var area) && area == split.AreaId && result.TryGetValue("Room", out var room) && room == split.RoomId)
                ProcessSplit();
        }

        private void ProcessStart(Split split, MemApi memApi)
        {
            var result = MemoryAccessor.LoadAddress(split.Address, memApi, ReadType.Byte, split.Domain);

            if (result != split.Value) return;
            
            const string message = "starttimer\r\n";
            var data = new byte[message.Length];
            for (var i = 0; i < message.Length; ++i)
            {
                data[i] = Convert.ToByte(message[i]);
            }
            _connection.Send(data);
			JumpToNextSplit();
        }

		private void JumpToNextSplit()
		{
			SplitId++;
			while (SplitId < Splits.Count && !Splits[SplitId].Enabled) SplitId++;
		}

		private void RevertToPreviousSplit()
		{
			SplitId--;
			while (SplitId > 0 && !Splits[SplitId].Enabled) SplitId--;
		}
    }
}