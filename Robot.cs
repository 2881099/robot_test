//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Timers;
//using System.Xml;

///// <summary>
///// 机器人，自动运行库
///// </summary>
//public class Robot : IDisposable {

//	private string _def_path;
//	private List<RobotDef> _robots;
//	private object _robots_lock = new object();
//	private FileSystemWatcher _defWatcher;
//	public event RobotErrorHandler Error;

//	public Robot()
//		: this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"robot.txt")) {
//	}
//	public Robot(string path) {
//		_def_path = path;
//	}

//	public void Start() {
//		Stop();
//		if (!File.Exists(_def_path)) return;
//		lock (_robots_lock) {
//			_robots = LoadDef();
//			foreach (RobotDef bot in _robots) {
//				bot.RunNow();
//			}
//		}
//		if (_defWatcher == null) {
//			_defWatcher = new FileSystemWatcher(Path.GetDirectoryName(_def_path), Path.GetFileName(_def_path));
//			_defWatcher.Changed += delegate(object sender, FileSystemEventArgs e) {
//				_defWatcher.EnableRaisingEvents = false;
//				if (_robots.Count > 0) {
//					Start();
//				}
//				_defWatcher.EnableRaisingEvents = true;
//			};
//			_defWatcher.EnableRaisingEvents = true;
//		}
//	}
//	public void Stop() {
//		lock (_robots_lock) {
//			if (_robots != null) {
//				for (int a = 0; a < _robots.Count; a++) {
//					_robots[a].Dispose();
//				}
//				_robots.Clear();
//			}
//		}
//	}

//	#region IDisposable 成员

//	public void Dispose() {
//		if (_defWatcher != null) {
//			_defWatcher.Dispose();
//		}
//		Stop();
//	}

//	#endregion

//	public List<RobotDef> LoadDef() {
//		string defDoc = Encoding.UTF8.GetString(readFile(_def_path));
//		return LoadDef(defDoc);
//	}
//	public List<RobotDef> LoadDef(string defDoc) {
//		Dictionary<string, RobotDef> dic = new Dictionary<string, RobotDef>();
//		string[] defs = defDoc.Split(new string[] { "\r\n" }, StringSplitOptions.None);
//		int row = 1;
//		foreach (string def in defs) {
//			string loc1 = def.Trim();
//			if (string.IsNullOrEmpty(loc1) || loc1[0] == ';') continue;
//			string pattern = @"([^\s]+)\s+(NONE|SEC|MIN|HOUR|DAY|RunOnDay|RunOnWeek)\s+([^\s]+)\s+([^\s]+)";
//			Match m = Regex.Match(loc1, pattern, RegexOptions.IgnoreCase);
//			if (!m.Success) {
//				onError(new Exception("Robot配置错误“" + loc1 + "”, 第" + row + "行"));
//				continue;
//			}
//			RobotDef rd = new RobotDef(this);
//			rd._name = m.Groups[1].Value;
//			rd._mode = getMode(m.Groups[2].Value);
//			rd._parm = m.Groups[3].Value;
//			rd._url = m.Groups[4].Value;
//			if (rd.Mode == RobotRunMode.NONE) continue;
//			if (dic.ContainsKey(rd._name)) {
//				onError(new Exception("Robot配置存在重复的名字“" + rd._name + "”, 第" + row + "行"));
//				continue;
//			}
//			if (rd.Interval < 0) {
//				onError(new Exception("Robot配置参数错误“" + def + "”, 第" + row + "行"));
//				continue;
//			}
//			dic.Add(rd.Name, rd);
//			row++;
//		}
//		List<RobotDef> rds = new List<RobotDef>();
//		foreach (RobotDef rd in dic.Values) {
//			rds.Add(rd);
//		}
//		return rds;
//	}

//	private void onError(Exception ex) {
//		onError(ex, null);
//	}
//	internal void onError(Exception ex, RobotDef def) {
//		if (Error != null) {
//			RobotErrorEventArgs e = new RobotErrorEventArgs(ex, def);
//			Error(this, e);
//		}
//	}
//	private byte[] readFile(string path) {
//		if (File.Exists(path)) {
//			string destFileName = Path.GetTempFileName();
//			File.Copy(path, destFileName, true);
//			int read = 0;
//			byte[] data = new byte[1024];
//			MemoryStream ms = new MemoryStream();
//			using (FileStream fs = new FileStream(destFileName, FileMode.OpenOrCreate, FileAccess.Read)) {
//				do {
//					read = fs.Read(data, 0, data.Length);
//					if (read <= 0) break;
//					ms.Write(data, 0, read);
//				} while (true);
//				fs.Close();
//			}
//			File.Delete(destFileName);
//			data = ms.ToArray();
//			ms.Close();
//			return data;
//		}
//		return new byte[] { };
//	}
//	private RobotRunMode getMode(string mode) {
//		mode = string.Concat(mode).ToUpper().Trim();
//		switch (mode) {
//			case "SEC": return RobotRunMode.SEC;
//			case "MIN": return RobotRunMode.MIN;
//			case "HOUR": return RobotRunMode.HOUR;
//			case "DAY": return RobotRunMode.DAY;
//			case "RUNONDAY": return RobotRunMode.RunOnDay;
//			case "RUNONWEEK": return RobotRunMode.RunOnWeek;
//			default: return RobotRunMode.NONE;
//		}
//	}
//}

//public class RobotDef : IDisposable {
//	internal string _name;
//	internal RobotRunMode _mode = RobotRunMode.NONE;
//	internal string _parm;
//	internal string _url;

//	private Robot _onwer;
//	private Timer _timer;

//	public RobotDef(Robot onwer) {
//		_onwer = onwer;
//	}

//	public void RunNow() {
//		if (_timer == null) {
//			_timer = new Timer();
//			_timer.AutoReset = false;
//			_timer.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e) {
//				string logObj = this.ToString();
//				try {
//					Console.WriteLine("{0} {1} 准备执行...", Now, logObj);
//					System.Net.WebClient wc = new System.Net.WebClient();
//					wc.Encoding = Encoding.UTF8;
//					string ret = wc.DownloadString(_url);
//					Console.WriteLine("{0} {1} 执行结果：{2}", Now, logObj, ret);
//				} catch (Exception ex) {
//					Console.WriteLine("{0} {1} 发生错误：{2}", Now, logObj, ex.Message);
//					_onwer.onError(ex, this);
//				}
//				RunNow();
//			};
//		}
//		if (_timer != null) {
//			_timer.Stop();
//			_timer.Interval = Interval;
//			_timer.Start();
//		}
//	}

//	public override string ToString() {
//		return Name + ", " + Mode + ", " + Parm + ", " + Url;
//	}
//	public string Now {
//		get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
//	}

//	#region IDisposable 成员

//	public void Dispose() {
//		if (_timer != null) {
//			_timer.Stop();
//			_timer.Close();
//			_timer.Dispose();
//		}
//	}

//	#endregion

//	public string Name {
//		get { return _name; }
//	}
//	public RobotRunMode Mode {
//		get { return _mode; }
//	}
//	public string Parm {
//		get { return _parm; }
//	}
//	public string Url {
//		get { return _url; }
//	}
//	public double Interval {
//		get {
//			DateTime now = DateTime.Now;
//			double interval = -1;
//			switch (_mode) {
//				case RobotRunMode.SEC:
//					double.TryParse(_parm, out interval);
//					interval *= 1000;
//					break;
//				case RobotRunMode.MIN:
//					double.TryParse(_parm, out interval);
//					interval *= 60 * 1000;
//					break;
//				case RobotRunMode.HOUR:
//					double.TryParse(_parm, out interval);
//					interval *= 60 * 60 * 1000;
//					break;
//				case RobotRunMode.DAY:
//					double.TryParse(_parm, out interval);
//					interval *= 24 * 60 * 60 * 1000;
//					break;
//				case RobotRunMode.RunOnDay:
//					string[] hhmmss = string.Concat(_parm).Split(':');
//					if (hhmmss.Length == 3) {
//						int hh, mm, ss;
//						if (int.TryParse(hhmmss[0], out hh) && hh >= 0 && hh < 24 &&
//							int.TryParse(hhmmss[1], out mm) && mm >= 0 && mm < 60 &&
//							int.TryParse(hhmmss[2], out ss) && ss >= 0 && ss < 60) {
//							DateTime curt = now.Date.AddHours(hh).AddMinutes(mm).AddSeconds(ss);
//							TimeSpan ts = curt - now;
//							if (ts.TotalSeconds > 0) {
//								interval = ts.TotalSeconds * 1000;
//							} else {
//								curt = curt.AddDays(1);
//								ts = curt - now;
//								interval = ts.TotalSeconds * 1000;
//							}
//						}
//					}
//					break;
//				case RobotRunMode.RunOnWeek:
//					string[] wwhhmmss = string.Concat(_parm).Split(':');
//					if (wwhhmmss.Length == 4) {
//						int ww, hh, mm, ss;
//						if (int.TryParse(wwhhmmss[0], out ww) && ww >= 0 && ww < 7 &&
//							int.TryParse(wwhhmmss[1], out hh) && hh >= 0 && hh < 24 &&
//							int.TryParse(wwhhmmss[2], out mm) && mm >= 0 && mm < 60 &&
//							int.TryParse(wwhhmmss[3], out ss) && ss >= 0 && ss < 60) {
//							DateTime curt = now.Date.AddHours(hh).AddMinutes(mm).AddSeconds(ss);
//							TimeSpan ts = curt - now;
//							if (ts.TotalSeconds > 0) {
//								interval = ts.TotalSeconds * 1000;
//							} else {
//								do {
//									curt = curt.AddDays(1);
//								} while ((int)curt.DayOfWeek != ww);
//								ts = curt - now;
//								interval = ts.TotalSeconds * 1000;
//							}
//						}
//					}
//					break;
//			}
//			if (interval == 0) interval = 1;
//			return interval;
//		}
//	}
//}
///*
//;SEC：按秒运行
//;MIN：按分运行
//;HOUR：按小时运行
//;DAY：按天运行
//;RunOnDay：按每天什么时候运行
//;RUNONWEEK：按星期几及时间运行
//Name RunOnWeek 5:15:55:59 Run10Hour Asian,Server.Common
//;星期五15点55分59秒运行 method 方法
//*/
//public enum RobotRunMode {
//	/// <summary>
//	/// 无
//	/// </summary>
//	NONE = 0,
//	/// <summary>
//	/// 按秒运行 
//	/// </summary>
//	SEC = 1,
//	/// <summary>
//	/// 按分运行
//	/// </summary>
//	MIN = 2,
//	/// <summary>
//	/// 按小时运行
//	/// </summary>
//	HOUR = 3,
//	/// <summary>
//	/// 按天运行
//	/// </summary>
//	DAY = 4,
//	/// <summary>
//	/// 按每天什么时候运行
//	/// </summary>
//	RunOnDay = 10,
//	/// <summary>
//	/// 按星期几及时间运行
//	/// </summary>
//	RunOnWeek = 11
//}

//public delegate void RobotErrorHandler(object sender, RobotErrorEventArgs e);
//public class RobotErrorEventArgs : EventArgs {

//	private Exception _exception;
//	private RobotDef _def;

//	public RobotErrorEventArgs(Exception exception, RobotDef def) {
//		_exception = exception;
//		_def = def;
//	}

//	public Exception Exception {
//		get { return _exception; }
//	}
//	public RobotDef Def {
//		get { return _def; }
//	}
//}