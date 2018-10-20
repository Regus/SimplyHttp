using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimplyHttp.Client {
	public class HttpResponse {
		private HttpHeader _header;
		private byte[] _body;

		internal HttpResponse(HttpHeader header) {
			_header = header;
		}

		internal void SetBody(byte[] body) {
			_body = body;
		}

		public byte[] Body {
			get {
				if (_header.ContentEncoding == "identity") {
					return _body;
				}
				if (_header.ContentEncoding == "gzip") {
					byte[] buffer = new byte[_body.Length * 2];
					int offset = 0;
					using (var stream = new MemoryStream(_body)) {
						using (var zipStream = new GZipStream(stream, CompressionMode.Decompress)) {
							while (true) {
								if (buffer.Length - offset < 10) {
									byte[] old = buffer;
									buffer = new byte[buffer.Length + _body.Length];
									Buffer.BlockCopy(old, 0, buffer, 0, offset);
								}
								int read = zipStream.Read(buffer, offset, buffer.Length - offset);
								if (read == 0) {
									break;
								}
								offset += read;
							}
							byte[] result = new byte[offset];
							Buffer.BlockCopy(buffer, 0, result, 0, offset);
							return result;
						}
					}
				}
				if (_header.ContentEncoding == "deflate") {
					byte[] buffer = new byte[_body.Length * 2];
					int offset = 0;
					using (var stream = new MemoryStream(_body)) {
						using (var zipStream = new DeflateStream(stream, CompressionMode.Decompress)) {
							while (true) {
								if (buffer.Length - offset < 10) {
									byte[] old = buffer;
									buffer = new byte[buffer.Length + _body.Length];
									Buffer.BlockCopy(old, 0, buffer, 0, offset);
								}
								int read = zipStream.Read(buffer, offset, buffer.Length - offset);
								if (read == 0) {
									break;
								}
								offset += read;
							}
							byte[] result = new byte[offset];
							Buffer.BlockCopy(buffer, 0, result, 0, offset);
							return result;
						}
					}
				}
				throw new Exception($"Unsupported Content-Encoding: {_header.ContentEncoding}");
			}
		}

		public string BodyText {
			get {
				return _header.CharsetEncoding.GetString(Body);
			}
		}

	}
}
