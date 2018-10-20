using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimplyHttp.Client {
	public static class SocketExtensions {


		public static Task AsyncConnect(this Socket socket, EndPoint endPoint) {
			var source = new TaskCompletionSource<bool>();
			socket.BeginConnect(endPoint, ar => {
				try {
					socket.EndConnect(ar);
					source.SetResult(true);
				}
				catch (Exception ex) {
					source.SetException(ex);
				}
			}, null);
			return source.Task;
		}

		public static Task AsyncDisconnect(this Socket socket) {
			var source = new TaskCompletionSource<bool>();
			socket.BeginDisconnect(false, ar => {
				try {
					socket.EndDisconnect(ar);
					source.SetResult(true);
				}
				catch (Exception ex) {
					source.SetException(ex);
				}
			}, null);
			return source.Task;
		}

		public static Task<int> AsyncSend(this Socket socket, byte[] buffer, int offset, int count) {
			var source = new TaskCompletionSource<int>();
			socket.BeginSend(buffer, offset, count, SocketFlags.None, ar => {
				try {
					int sent = socket.EndSend(ar);
					source.SetResult(sent);
				}
				catch (Exception ex) {
					source.SetException(ex);
				}
			}, null);
			return source.Task;
		}

		public static Task<int> AsyncReceive(this Socket socket, byte[] buffer, int offset, int count) {
			var source = new TaskCompletionSource<int>();
			socket.BeginReceive(buffer, offset, count, SocketFlags.None, ar => {
				try {
					int read = socket.EndReceive(ar);
					source.SetResult(read);
				}
				catch (Exception ex) {
					source.SetException(ex);
				}
			}, null);
			return source.Task;
		}


	}
}
