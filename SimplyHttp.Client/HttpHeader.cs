using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimplyHttp.Client {
	public class HttpHeader {
		private Dictionary<string, List<string>> _items = new Dictionary<string, List<string>>();
		private string _method;
		private string _statusCode;
		private string _statusText;
		private string _url;
		private string _httpVersion;

		public HttpHeader() {

		}

		internal static HttpHeader CreateRequestHeader() {
			var header = new HttpHeader();
			header.Accept = "*/*";
			header.AcceptEncoding = "gzip, deflate";
			header.UserAgent = "SimplyHttp/1.0";
			header._method = "GET";
			header._httpVersion = "HTTP/1.1";
			return header;
		}

		internal HttpHeader(byte[] data, int offset, int count) {
			Append(data, offset, count, true);
		}

		internal void Append(byte[] data, int offset, int count, bool isFirst = false) {
			string headerData = Encoding.ASCII.GetString(data, offset, count);
			string prevHeader = null;
			foreach (string header in Regex.Split(headerData, "\r\n")) {
				Console.WriteLine(header);
				if (isFirst) {
					var items = header.Split(' ');
					_httpVersion = items[0];
					_statusCode = items[1];
					_statusText= items[2];
					isFirst = false;
					continue;
				}
				if (char.IsWhiteSpace(header[0])) {
					var items = _items[prevHeader];
					items[items.Count - 1] += " " + header.Trim();
				}
				else {
					int colon = header.IndexOf(':');
					string name = header.Substring(0, colon).Trim();
					string value = header.Substring(colon + 1).Trim();
					if (!_items.ContainsKey(name)) {
						_items.Add(name, new List<string>());
					}
					_items[name].Add(value);
					prevHeader = name;
				}
			}
		}

		internal string Url {
			get {
				return _url;
			}
			set {
				_url = value;
			}
		}

		internal string Method {
			get {
				return _method;
			}
			set {
				_method = value;
			}
		}

		public string this[string name] {
			get {
				return _items[name][0];
			}
			set {
				if (!_items.ContainsKey(name)) {
					_items.Add(name, new List<string>());
					_items[name].Add(value);
				}
				else {
					_items[name][0] = value;
				}
			}
		}

		public string Host {
			get {
				if (!_items.ContainsKey("Host")) {
					return "";
				}
				return this["Host"];
			}
			set {
				this["Host"] = value;
			}
		}

		public long ContentLength {
			get {
				if (!_items.ContainsKey("Content-Length")) {
					return -1;
				}
				return long.Parse(this["Content-Length"]);
			}
			set {
				this["Content-Length"] = value.ToString();
			}
		}

		public string Accept {
			get {
				if (!_items.ContainsKey("Accept")) {
					return "";
				}
				return this["Accept"];
			}
			set {
				this["Accept"] = value;
			}
		}

		public string AcceptType {
			get {
				if (!_items.ContainsKey("Accept-Charset")) {
					return "";
				}
				return this["Accept-Charset"];
			}
			set {
				this["Accept-Charset"] = value;
			}
		}

		public string AcceptEncoding {
			get {
				if (!_items.ContainsKey("Accept-Encoding")) {
					return "";
				}
				return this["Accept-Encoding"];
			}
			set {
				this["Accept-Encoding"] = value;
			}
		}

		public string AcceptLanguage {
			get {
				if (!_items.ContainsKey("Accept-Language")) {
					return "";
				}
				return this["Accept-Language"];
			}
			set {
				this["Accept-Language"] = value;
			}
		}

		public string UserAgent {
			get {
				if (!_items.ContainsKey("User-Agent")) {
					return "";
				}
				return this["User-Agent"];
			}
			set {
				this["User-Agent"] = value;
			}
		}
		
		public string ContentEncoding {
			get {
				if (!_items.ContainsKey("Content-Encoding")) {
					return "identity";
				}
				return this["Content-Encoding"];
			}
			set {
				this["Content-Encoding"] = value;
			}
		}

		public string TransferEncoding {
			get {
				if (!_items.ContainsKey("Transfer-Encoding")) {
					return "";
				}
				return this["Transfer-Encoding"];
			}
			set {
				this["Transfer-Encoding"] = value;
			}
		}

		public string ContentType {
			get {
				if (!_items.ContainsKey("Content-Type")) {
					return "";
				}
				return this["Content-Type"];
			}
			set {
				this["Content-Type"] = value;
			}
		}

		public string MediaType {
			get {
				if (!_items.ContainsKey("Content-Type")) {
					return "";
				}
				string contentType = ContentType;
				string[] contentTypeItems = contentType.Split(';');
				return contentTypeItems[0];
			}
			set {
				string mediaType = value;
				string charset = Charset;
				string contentType = mediaType;
				if (charset != "") {
					contentType += "; " + charset;
				}
				this["Content-Type"] = contentType;
			}
		}

		public string Charset {
			get {
				if (!_items.ContainsKey("Content-Type")) {
					return "";
				}
				string contentType = ContentType;
				string[] contentTypeItems = contentType.Split(';');
				return contentTypeItems[1].Split('=')[1].Trim();
			}
			set {
				string mediaType = MediaType;
				string charset = value;
				string contentType = mediaType;
				if (charset != "") {
					contentType += "; charset=" + charset;
				}
				this["Content-Type"] = contentType;
			}
		}

		public Encoding CharsetEncoding {
			get {
				return Encoding.GetEncoding(Charset);
			}
		}

		public byte[] ToByteArray() {
			StringBuilder data = new StringBuilder();
			data.AppendLine(_method + " " + _url + " HTTP/1.1");
			foreach (var pair in _items) {
				foreach (var value in pair.Value) {
					data.Append(pair.Key);
					data.Append(": ");
					data.AppendLine(value);
				}
			}
			data.AppendLine();
			Console.WriteLine("header: " + data);
			return Encoding.ASCII.GetBytes(data.ToString());
		}

	}
}
