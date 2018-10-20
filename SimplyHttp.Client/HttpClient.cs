using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimplyHttp.Client {
	public class HttpClient {
		public async Task<HttpResponse> Get(string url) {
			return await Send(new HttpRequest(new Uri(url)));
		}

		public async Task<HttpResponse> Post(string url, string content, string mediaType) {
			var request = new HttpRequest(new Uri(url));
			request.Header.MediaType = mediaType;
			request.Body = content;
			return await Send(request);
		}

		public async Task<HttpResponse> Send(HttpRequest request) {
			var stream = new HttpStream();
			await stream.ConnectAsync(request.Url.Host, request.Url.Port, request.Url.Scheme == "https");

			byte[] toSend = request.ToByteArray();
			await stream.WriteAsync(toSend, 0, toSend.Length);

			if (stream.Capacity == 0) {
				stream.Capacity = 40960;
			}
			int bodyStart = 0;
			HttpResponse response = null;
			HttpHeader header = null;
			while (header == null) {
				if (stream.RemainingCapacity < 100) {
					stream.Capacity += stream.Capacity < 102400 ? stream.Capacity : 102400;
				}
				await stream.ReadIntoBuffer();
				int index = IndexOfHeaderEnd(stream.Data, 0, stream.Available);
				if (index != -1) {
					header = new HttpHeader(stream.Data, 0, index);
					response = new HttpResponse(header);
				}
				bodyStart = index + 4;
			}
			if (header.ContentLength > 0) {
				byte[] body = new byte[header.ContentLength];
				int bodyOffset = 0;
				if (bodyStart < stream.Available) {
					if (body.Length < stream.Available - bodyStart) {
						throw new IOException("Unexpected data in stream");
					}
					Buffer.BlockCopy(stream.Data, bodyStart, body, 0, stream.Available - bodyStart);
					bodyOffset += stream.Available - bodyStart;
					stream.Consume(stream.Available);
				}
				while (bodyOffset < body.Length) {
					int read = await stream.ReadAsync(body, bodyOffset, body.Length - bodyOffset);
					if (read == 0) {
						throw new IOException("Connection reset by peer.");
					}
					bodyOffset += read;
				}
				response.SetBody(body);
			}
			else if (header.TransferEncoding == "chunked") {
				int readCount = 0;
				stream.Consume(bodyStart);
				List<int[]> chunks = new List<int[]>();
				int bodyLength = 0;
				int readOffset = 0;
				while (true) {
					int index = IndexOfChunkHeaderEnd(stream.Data, readOffset, stream.Available - readOffset);
					if (index > 0) {
						int chunkLength = int.Parse(Encoding.ASCII.GetString(stream.Data, readOffset, index - readOffset), System.Globalization.NumberStyles.HexNumber);
						bodyLength += chunkLength;
						readOffset = index + 2;
						if (chunkLength > 0) {
							chunks.Add(new int[] { readOffset, chunkLength });
							while (readOffset + chunkLength + 2 > stream.Available) {
								if (stream.Capacity < readOffset + chunkLength + 2) {
									stream.Capacity = readOffset + chunkLength + 2 + 4096;
								}
								await stream.ReadIntoBuffer();
							}
							readOffset += chunkLength + 2;
						}
						else {
							int endIndex = IndexOfHeaderEnd(stream.Data, readOffset - 2, stream.Available);
							while (endIndex < 0) {
								if (stream.RemainingCapacity < 100) {
									stream.Capacity += 100;
								}
								await stream.ReadIntoBuffer();
								endIndex = IndexOfHeaderEnd(stream.Data, readOffset - 2, stream.Available);
							}
							if (endIndex > readOffset) {
								header.Append(stream.Data, readOffset, endIndex - readOffset);
							}
							byte[] body = new byte[bodyLength];
							int bodyIndex = 0;
							foreach (var chunk in chunks) {
								Buffer.BlockCopy(stream.Data, chunk[0], body, bodyIndex, chunk[1]);
								bodyIndex += chunk[1];
							}
							response.SetBody(body);
							stream.Consume(endIndex + 4);
							if (stream.Available > 0) {
								throw new IOException("Unexpected data in stream.");
							}
							break;
						}
					}
					else {
						if (stream.RemainingCapacity < 100) {
							stream.Capacity += stream.Capacity < 102400 ? stream.Capacity : 102400;
						}
						await stream.ReadIntoBuffer();
					}
				}
			}
			return response;
		}

		private void SaveStream(string path, HttpStream stream) {
			byte[] data = new byte[stream.Available];
			Buffer.BlockCopy(stream.Data, 0, data, 0, stream.Available);
			for (int i = 0; i < data.Length; i++) {
				if (data[i] > 127) {
					data[i] = (byte)'a';
				}
				if (data[i] < 32 && data[i] != 10 && data[i] != 13) {
					data[i] = (byte)'b';
				}
			}
			File.WriteAllBytes(path, data);
		}

		private int IndexOfHeaderEnd(byte[] buffer, int offset, int count) {
			for (int i = offset; i < count + offset - 3; i++) {
				if (buffer[i] == '\r' && buffer[i + 1] == '\n' && buffer[i + 2] == '\r' && buffer[i + 3] == '\n') {
					return i;
				}
			}
			return -1;
		}

		private int IndexOfChunkHeaderEnd(byte[] buffer, int offset, int count) {
			for (int i = offset; i < count + offset - 1; i++) {
				if (buffer[i] == '\r' && buffer[i + 1] == '\n') {
					return i;
				}
			}
			return -1;
		}


	}
}
