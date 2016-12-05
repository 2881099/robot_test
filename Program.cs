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

			int tryint = 0;
			if (args != null && args.Length > 0 && int.TryParse(args[0], out tryint) && tryint > 0) {
				List<ClientSocket> cs = new List<ClientSocket>();
				for (int z = 0; z < tryint; z++) {
					cs.Add(new ClientSocket());
					var handle = new Action<int>(index => {
						var c = cs[index];
						c.Closed += (a, b) => {
							Console.WriteLine("{0} {1}关闭了连接", Now, index);
						};
						c.Error += (a, b) => {
							Console.WriteLine("{0} {1}发生错误({2})：{3}", Now, index, b.Errors, b.Exception.Message);
						};
						c.Receive += (a, b) => {
							Console.WriteLine("{0} {1}接受到了消息({2})：{3}", Now, index, b.Receives, b.Messager);
						};
						c.Connect("127.0.0.1", 19990);
					});
					handle(z);
				}
				
				Console.WriteLine("ClientSocket test ... 按 ESC 键退出 ...");
				while(true) {
					ConsoleKeyInfo input = Console.ReadKey();
					if (input.Key == ConsoleKey.Escape) break;
					if (input.Key == ConsoleKey.Enter) {
						int index = 0;
						cs.ForEach(a => {
							SocketMessager msg = new SocketMessager("test", "ClientServer({0}) SendMessage...");
							a.Write(msg);
							index++;
						});
					}
				}
				cs.ForEach(a => {
					a.Dispose();
				});
				return;
			}

			ServerSocket server = new ServerSocket(19990);
			server.Receive += (a, b) => {
				Console.WriteLine("{0} 接受到了消息{1}：{2}", Now, b.Receives, b.Messager);
			};
			server.Accepted += (a, b) => {
				Console.WriteLine("{0} 新连接：{1}", Now, b.Accepts);
			};
			server.Closed += (a, b) => {
				Console.WriteLine("{0} 关闭了连接：{1}", Now, b.AcceptSocketId);
			};
			server.Error += (a, b) => {
				Console.WriteLine("{0} 发生错误({1})：{2}", Now, b.Errors, b.Exception.Message);
			};
			server.Start();

			Robot r = new Robot();

			r.Run += (a, b) => {
				string logObj = b.Name;
				System.Net.WebClient wc = new System.Net.WebClient();
				wc.Encoding = Encoding.UTF8;
				string ret = wc.DownloadString(b.RunParam);
				ret = string.Format("{0} {1} 第{3}次执行结果：{2}", Now, logObj, ret, b.RunTimes);

				server.Write(new SocketMessager("robot_run", ret));
				Console.WriteLine(ret);
			};

			r.Error += (a, b) => {
				//b.Def.RunParam
				Console.WriteLine("{0} {1} 发生错误：", Now, b.Def, b.Exception.Message);
			};

			r.Start();

			Console.WriteLine("...");
			Console.ReadKey();
			r.Stop();
			server.Stop();
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
