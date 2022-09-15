using System;
using System.Net.Sockets;

namespace BizHawk.Client.EmuHawk.AutoSplitter
{
	public class LivesplitServerConnector
	{
		private Socket _connection;

		public void Start()
		{
			_connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_connection.Connect("localhost", 16834);
		}

		public void Close()
		{
			_connection.Close();
		}

		public void SendStartCommand()
		{
			const string message = "starttimer\r\n";
			var data = new byte[message.Length];
			for (var i = 0; i < message.Length; ++i)
			{
				data[i] = Convert.ToByte(message[i]);
			}
			_connection.Send(data);
		}

		public void SendSplitCommand()
		{
			const string message = "split\r\n";
			var data = new byte[message.Length];
			for (var i = 0; i < message.Length; ++i)
			{
				data[i] = Convert.ToByte(message[i]);
			}

			_connection.Send(data);
		}

		public void SendSkipSplitCommand()
		{
			const string message = "skipsplit\r\n";
			var data = new byte[message.Length];
			for (var i = 0; i < message.Length; ++i)
			{
				data[i] = Convert.ToByte(message[i]);
			}

			_connection.Send(data);
		}

		public void SendUndoSplitCommand()
		{
			const string message = "unsplit\r\n";
			var data = new byte[message.Length];
			for (var i = 0; i < message.Length; ++i)
			{
				data[i] = Convert.ToByte(message[i]);
			}

			_connection.Send(data);
		}

		public void SendResetCommand()
		{
			const string message = "reset\r\n";
			var data = new byte[message.Length];
			for (var i = 0; i < message.Length; ++i)
			{
				data[i] = Convert.ToByte(message[i]);
			}

			_connection.Send(data);
		}
	}
}