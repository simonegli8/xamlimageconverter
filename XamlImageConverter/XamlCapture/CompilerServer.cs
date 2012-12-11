using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Reflection;

namespace XamlImageConverter {

	public class CompilerServer {

		Mutex messageMutex = null, runMutex = null; 
 		List<Thread> threads = new List<Thread>();
		long id = 0;
		const int NPipes = 8; 
		string binpath;
		public TimeSpan RunTime = new TimeSpan(0, 1, 0);
		public bool Service = false; 
		public bool Debug = false;
		public bool Sleep = false;
		public static CompilerServer Current = null;

		public CompilerServer() {
			try {
				messageMutex = Mutex.OpenExisting("XamlImageConverter.MessageMutex");
			} catch { }
			if (messageMutex == null) messageMutex = new Mutex(false, "XamlImageConverter.MessageMutex");
			try {
				runMutex = Mutex.OpenExisting("XamlImageConverter.RunMutex");
			} catch { }
			if (runMutex == null) runMutex = new Mutex(false, "XamlImageConverter.RunMutex");
		}

		[Serializable]
		class Message {
			public enum Class { Exit, Compile }
			public Class Task;
			public Compiler Compiler;
		}

		long ServerID {
			get {
				long id;
				using (var pipe = new NamedPipeClientStream(".", "XamlImageConverter.ServerID", PipeAccessRights.FullControl, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.None, HandleInheritability.None)) {
					try {
						pipe.Connect(5);
					} catch (TimeoutException) {
						StartServer();
						pipe.Connect();
					}
					pipe.WriteByte((byte)1);
					using (var r = new BinaryReader(pipe)) {
						id = r.ReadInt64();
					}
				}
				return id;
			}
		}
		/*
				var path = Path.Combine(binpath, "XamlImageConverter.runtime");
				try {
					using (var file = new FileStream(path, FileMode.Open, FileAccess.Read)) {
						using (var r = new BinaryReader(file)) {
							id = r.ReadInt32();
						}
					}
				} catch (FileNotFoundException) {
					id = -1;
				}
				if (id < 0) {
					StartServer(id = -id);
					ServerID = id;
				}
				return id;
			}
			set {
				var path = Path.Combine(binpath, "XamlImageConverter.runtime");
				using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write)) {
					using (var w = new BinaryWriter(file)) {
						w.Write(value);
					}
				}
			}
		}
		 */

		void StartServer() {
			ProcessStartInfo pinfo = new ProcessStartInfo();
			pinfo.WorkingDirectory = binpath;
			pinfo.FileName = Path.Combine(pinfo.WorkingDirectory, "XamlImageConverter.exe");
			pinfo.Arguments = "-s -v";
			Process.Start(pinfo);
		}

		void ServerCompile(NamedPipeServerStream pipe, Compiler compiler) {
			try {
				compiler.UseService = false;
				compiler.SeparateAppDomain = true;
				compiler.Loggers.Add(new FileLogger());
				compiler.Compile();
				pipe.WriteByte(compiler.Errors.HasErrors ? (byte)1 : (byte)0);
				pipe.WaitForPipeDrain();
				pipe.Disconnect();
			} catch { }
		}

		NamedPipeServerStream ServerPipe {
			get {
				return new NamedPipeServerStream("XamlImageConverter.Pipes.Server." + id.ToString(), PipeDirection.InOut, NPipes);
			}
		}

		NamedPipeClientStream ClientPipe {
			get {
				return new NamedPipeClientStream("XamlImageConverter.Pipes.Server." + id.ToString());
			}
		}

		int n = 0, free = 0;
		
		void Listen() {
			var ct = Thread.CurrentThread;
			threads.Add(ct);
			BinaryFormatter f = new BinaryFormatter();
			while (true) {
				using (var pipe = ServerPipe) {
					lock (this) { n++; free++; }
					pipe.WaitForConnection();
					var msg = (Message)f.Deserialize(pipe);
					switch (msg.Task) {
					case Message.Class.Exit:
						lock(this) n--;
						threads.Remove(ct);
						pipe.WriteByte((byte)0);
						pipe.WaitForPipeDrain();
						pipe.Disconnect();
						return;
					case Message.Class.Compile:
						int nf;
						lock(this) nf = --free;
						if (nf == 0 && n < NPipes) {
							SpawnListen();
						}
						ServerCompile(pipe, msg.Compiler);
						break;
					}
				}
			}
		}

		int tn = 0;
		void SpawnListen() {
			var t = new Thread(Listen);
			t.Name = "XamlImageConverter Server Thread " + (tn++).ToString();
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
		}

		void ListenID() {
			while (true) {
				using (var pipe = new NamedPipeServerStream("XamlImageConverter.ServerID", PipeDirection.InOut, 1)) {
					pipe.WaitForConnection();
					if (pipe.ReadByte() == 0) {
						return;
					}
					using (var w = new BinaryWriter(pipe)) {
						w.Write(id);
						pipe.WaitForPipeDrain();
						pipe.Disconnect();
					}
				}
			}
		}

		void SpawnListenID() {
			var t = new Thread(ListenID);
			t.Name = "XamlImageConverter Server ID";
			t.Start();
		}

		public void Start() {
			Console.WriteLine("XAML Skin Builder by Chris Cavanagh & johnshope.com v2.4");
			Console.WriteLine("Start XamlImageConverter.exe -? for help.");
			Console.WriteLine("Service started..."); 
			if (runMutex.WaitOne(10)) {
				var a = Assembly.GetExecutingAssembly();
				binpath = Path.GetDirectoryName(a.Location);

				id = DateTime.Now.Ticks;

				SpawnListenID();
				SpawnListen();

				if (!Service) {
					Thread.Sleep(RunTime);
					Stop();
				}
			}
		}

		public void Stop() {
			do {
				runMutex.ReleaseMutex();

				// shutdown server
				try {
					messageMutex.WaitOne();
					using (var pipe = new NamedPipeClientStream(".", "XamlImageConverter.ServerID", PipeDirection.InOut)) {
						pipe.Connect();
						pipe.WriteByte((byte)0);
					}
				} finally {
					messageMutex.ReleaseMutex();
				}

				// stop all listening threads
				try {
					using (var pipe = ClientPipe) {
						pipe.Connect();
						var f = new BinaryFormatter();
						f.Serialize(pipe, new Message { Task = Message.Class.Exit });
						pipe.ReadByte();
					}
				} catch {
					Thread.Sleep(0);
				}
			} while (threads.Count > 0);
		}

		public bool Compile(Compiler compiler) {
			binpath = compiler.LibraryPath;
			bool released = false;
			bool res = false;
			try {
				messageMutex.WaitOne();

				id = ServerID;
				using (var pipe = ClientPipe) {
					pipe.Connect();

					var f = new BinaryFormatter();
					f.Serialize(pipe, new Message { Task = Message.Class.Compile, Compiler = compiler });
					released = true;
					messageMutex.ReleaseMutex();
					res = pipe.ReadByte() != 0;
					pipe.Close();
				}
			} finally {
				if (!released) messageMutex.ReleaseMutex();
			}
			return res;
		}
	}
}
