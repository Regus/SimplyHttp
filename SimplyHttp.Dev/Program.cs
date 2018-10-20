using SimplyHttp.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyHttp.Dev {
	class Program {
		static void Main(string[] args) {
			HttpClient client = new HttpClient();

			client.Get("https://www.google.com/").ContinueWith(response => {
				Console.WriteLine(response.Result.BodyText);

			}).Wait();


		}
	}
}
