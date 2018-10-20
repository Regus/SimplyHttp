using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyHttp.Client {
	public class HttpRequest {
		private HttpHeader _header;
		private byte[] _bodyData;
		private Uri _url;

		public HttpRequest() {
			_header = HttpHeader.CreateRequestHeader();
		}

		public HttpRequest(Uri url) {
			_header = HttpHeader.CreateRequestHeader();
			_url = url;
			_header.Url = _url.PathAndQuery;
			_header.Host = _url.Host;
		}

		public HttpHeader Header {
			get {
				return _header;
			}
		}

		public Uri Url {
			get {
				return _url;
			}
			set {
				_url = value;
				_header.Url = _url.PathAndQuery;
				_header.Host = _url.Host;
			}
		}

		public byte[] BodyData {
			get {
				return _bodyData;
			}
			set {
				_bodyData = value;
				_header.ContentLength = _bodyData?.Length ?? 0;
			}
		}

		public string Body {
			get {
				return _header.CharsetEncoding.GetString(_bodyData);
			}
			set {
				if (_header.Charset == "") {
					_header.Charset = "utf-8";
				}
				_bodyData = _header.CharsetEncoding.GetBytes(value);
				_header.ContentLength = _bodyData?.Length ?? 0;
			}
		}

		public byte[] ToByteArray() {
			byte[] header = _header.ToByteArray();
			if (_bodyData != null) {
				byte[] result = new byte[header.Length + _bodyData.Length];
				Buffer.BlockCopy(header, 0, result, 0, header.Length);
				Buffer.BlockCopy(header, 0, _bodyData, header.Length, _bodyData.Length);
				return result;
			}
			return header;
		}

	}
}
