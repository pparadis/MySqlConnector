﻿using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MySql.Data.Protocol.Serialization
{
	internal sealed class SocketByteHandler : IByteHandler
	{
		public SocketByteHandler(Socket socket)
		{
			m_socket = socket;
			var socketEventArgs = new SocketAsyncEventArgs();
			m_socketAwaitable = new SocketAwaitable(socketEventArgs);
		}

		public ValueTask<int> ReadBytesAsync(ArraySegment<byte> buffer, IOBehavior ioBehavior)
		{
			if (ioBehavior == IOBehavior.Asynchronous)
			{
				return new ValueTask<int>(DoReadBytesAsync(buffer));
			}
			else
			{
				var bytesRead = m_socket.Receive(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None);
				return new ValueTask<int>(bytesRead);
			}
		}

		public ValueTask<int> WriteBytesAsync(ArraySegment<byte> data, IOBehavior ioBehavior)
		{
			if (ioBehavior == IOBehavior.Asynchronous)
			{
				return new ValueTask<int>(DoWriteBytesAsync(data));
			}
			else
			{
				m_socket.Send(data.Array, data.Offset, data.Count, SocketFlags.None);
				return default(ValueTask<int>);
			}
		}

		private async Task<int> DoReadBytesAsync(ArraySegment<byte> buffer)
		{
			m_socketAwaitable.EventArgs.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
			await m_socket.ReceiveAsync(m_socketAwaitable);
			return m_socketAwaitable.EventArgs.BytesTransferred;
		}

		private async Task<int> DoWriteBytesAsync(ArraySegment<byte> payload)
		{
			m_socketAwaitable.EventArgs.SetBuffer(payload.Array, payload.Offset, payload.Count);
			await m_socket.SendAsync(m_socketAwaitable);
			return 0;
		}

		readonly Socket m_socket;
		readonly SocketAwaitable m_socketAwaitable;
	}
}
