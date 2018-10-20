using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimplyHttp.Client {
	public class HttpStream {
		private Socket _socket;
		private NetworkStream _rawStream;
		private SslStream _sslStream;
		private Stream _stream;
		private byte[] _data;
		private int _available = 0;

		public HttpStream() {

		}

		public async Task Close() {
			if (_sslStream != null) {
				_sslStream.Dispose();
			}
			if (_rawStream != null) {
				_rawStream.Dispose();
			}
			try {
				await _socket.AsyncDisconnect();
			}
			finally {
				_socket.Close();
			}
		}

		public async Task ConnectAsync(string host, int port, bool ssl) {
			Console.WriteLine(host);
			var address = (await Dns.GetHostAddressesAsync(host))[0];
			var endPoint = new IPEndPoint(address, port);
			Console.WriteLine(endPoint);
			_socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			await _socket.AsyncConnect(endPoint);
			_rawStream = new NetworkStream(_socket, false);
			if (ssl) {
				_sslStream = new SslStream(_rawStream);
				await _sslStream.AuthenticateAsClientAsync(host, null, SslProtocols.Tls12, false);
				_stream = _sslStream;
			}
			else {
				_stream = _rawStream;
			}
		}

		public async Task<int> ReadAsync(byte[] buffer, int offset, int count) {
			return await _stream.ReadAsync(buffer, offset, count);
		}

		public async Task WriteAsync(byte[] buffer, int offset, int count) {
			await _stream.WriteAsync(buffer, offset, count);
		}

		public async Task<int> ReadIntoBuffer() {
			if (_data == null) {
				throw new Exception("No buffer assigned.");
			}
			if (_available >= _data.Length) {
				throw new Exception("Cannot read into full buffer");
			}
			int read = await _stream.ReadAsync(_data, _available, _data.Length - _available);
			if (read == 0) {
				throw new IOException("Connection reset by peer.");
			}
			_available += read;
			return read;
		}

		public void Consume(int consume) {
			if (consume > _available) {
				throw new ArgumentException($"consume ({consume}) cannot be greater than Available ({_available})");
			}
			if (consume == _available) {
				_available = 0;
			}
			else {
				_available -= consume;
				Buffer.BlockCopy(_data, consume, _data, 0, _available);
			}
		}

		public byte[] Data {
			get {
				return _data;
			}
		}

		public int Available {
			get {
				return _available;
			}
		}

		public int RemainingCapacity {
			get {
				return _data.Length - _available;
			}
		}

		public int Capacity {
			get {
				return _data?.Length ?? 0;
			}
			set {
				if (value < _available) {
					throw new ArgumentException("Cannot set Capacity to less than Available");
				}
				if (_available > 0) {
					var old = _data;
					_data = new byte[value];
					Buffer.BlockCopy(old, 0, _data, 0, _available);
				}
				else {
					_data = new byte[value];
				}
			}
		}
	}
}
