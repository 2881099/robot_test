using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2 {
	class Program {
		
		static void Main(string[] args) {
			string aaa = null;
			Console.WriteLine(aaa.IsNullOrEmpty());

			var r = new NJob();

			r.Run += (a, b) => {
				string logObj = b.Name;
				System.Net.WebClient wc = new System.Net.WebClient();
				wc.Encoding = Encoding.UTF8;
				string ret = wc.DownloadString(b.RunParam);
				ret = string.Format("{0} {1} 第{3}次执行结果：{2}", Now, logObj, ret, b.RunTimes);

				Console.WriteLine(ret);
				Console.Write(DateTime.Now);
			};


			r.Error += (a, b) => {
				//b.Def.RunParam
				Console.WriteLine("{0} {1} 发生错误：", Now, b.Def, b.Exception.Message);
			};

			r.Start();

			Console.WriteLine("...");
			Console.ReadKey();
			r.Stop();
		}

		public static string Now {
			get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
		}
	}
}

public static class 扩展方法 {
	public static bool IsNullOrEmpty(this string str) {
		return string.IsNullOrEmpty(str);
	}
}
