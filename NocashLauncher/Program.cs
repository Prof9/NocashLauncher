using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace NocashLauncher {
	class Program {
		const int PROCESS_TERMINATE = 0x0001;
		const int PROCESS_VM_OPERATION = 0x0008;
		const int PROCESS_VM_READ = 0x0010;
		const int PROCESS_VM_WRITE = 0x0020;

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

		static string[] foldersToSearch = new string[] {
			"NO$GBA", "NOCASH", "bin"
		};

		static int Main(string[] args) {
			Console.WriteLine("NocashLauncher v1.0 by Prof. 9");
			Console.WriteLine();

#if DEBUG
			Console.Write("args:");
			foreach (string arg in args) {
				Console.Write(" \"" + arg + "\"");
			}
			Console.WriteLine();
			Console.WriteLine();
			Console.ReadKey();
#endif

			// Start No$gba and get process ID.
			Process nocash = null;
			int pid;
			try {
				nocash = CreateNocashProcess(args);
				nocash.Start();
				pid = nocash.Id;
			} catch {
				try {
					Console.WriteLine("Could not start " + Path.GetFileName(nocash.StartInfo.FileName) + ".");
				} catch {
					Console.WriteLine("Could not start No$gba.");
				}
#if DEBUG
				Console.ReadKey();
#endif
				return 1;
			}
			Console.WriteLine("Started " + Path.GetFileName(nocash.StartInfo.FileName) + " with PID " + pid + ".");

			// Open process.
			int handle = (int)OpenProcess(PROCESS_TERMINATE | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, pid);

			int tries = 20;
			while (tries > 0) {
				Thread.Sleep(10);

				bool applied = false;

				applied |= Patches.PatchDebug28c(handle);
				applied |= Patches.PatchDebug28b(handle);
				applied |= Patches.PatchDebug28a(handle);
				applied |= Patches.PatchDebug28(handle);
				applied |= Patches.PatchDebug27d(handle);
				applied |= Patches.PatchDebug27c(handle);
				applied |= Patches.PatchDebug26a(handle);

				if (applied) {
					break;
				}

				// don't look pls
				if (--tries == 0) {
					while (true) {
						Console.WriteLine("Could not apply any patches. Press Y to retry or N to exit. Y/N");
						char key = Console.ReadKey().KeyChar;
						Console.SetCursorPosition(0, Console.CursorTop);
						if (key == 'Y' || key == 'y') {
							tries += 1;
							break;
						}
						if (key == 'N' || key == 'n') {
							while (true) {
								Console.WriteLine("Kill No$gba as well? Y/N");
								char key2 = Console.ReadKey().KeyChar;
								Console.SetCursorPosition(0, Console.CursorTop);
								if (key2 == 'Y' || key2 == 'y') {
									if (!nocash.HasExited) {
										try {
											nocash.Kill();
										} catch { }
									}
#if DEBUG
									Console.WriteLine("Exiting...");
									Console.ReadKey();
#endif
									return 1;
								}
								if (key2 == 'N' || key2 == 'n') {
#if DEBUG
									Console.WriteLine("Exiting...");
									Console.ReadKey();
#endif
									return 1;
								}
							}
						}
					}
				}
			}

#if DEBUG
			Console.WriteLine("Exiting...");
			Console.ReadKey();
#endif
			return 0;
		}

		static Process CreateNocashProcess(string[] args) {
			string argsFileName = GetArgsFileName(args);
			string embeddedFileName = GetEmbeddedFileName();
			string fileName = null;
			int argsIndex = 0;

			if (fileName == null && argsFileName != null) {
				if (File.Exists(argsFileName)) {
					fileName = argsFileName;
					argsIndex = 1;
				} else {
					Console.WriteLine("Could not find " + argsFileName + ".");
				}
			}
			if (fileName == null && embeddedFileName != null) {
				if (File.Exists(embeddedFileName)) {
					fileName = embeddedFileName;
				} else {
					Console.WriteLine("Could not find " + embeddedFileName + ".");
				}
			}
			if (fileName == null) {
				fileName = SearchFolders("NO$GBA.EXE");
			}

			fileName = Path.GetFullPath(fileName);

			// Get No$gba args.
			string nocashArgs = "";
			for (int i = argsIndex; i < args.Length; i++) {
				if (i > argsIndex) {
					nocashArgs += " ";
				}
				nocashArgs += "\"" + args[i] + "\"";
			}

			// Create No$gba process.
			Process nocash = new Process();
			nocash.StartInfo.FileName = fileName;
			nocash.StartInfo.Arguments = nocashArgs;
			nocash.StartInfo.WorkingDirectory = Path.GetDirectoryName(fileName);

			return nocash;
		}

		static string SearchFolders(string fileName) {
			foreach (string folder in foldersToSearch) {
				string temp = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar + fileName;
				if (fileName == null) {
					try {
						if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar + "NO$GBA.EXE")) {
							fileName = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar + "NO$GBA.EXE";
						}
					} catch { }
				}
			}

			return fileName;
		}

		static string GetArgsFileName(string[] args) {
			if (args.Length <= 0) {
				return null;
			}

			try {
				if (Path.GetExtension(args[0]).ToLowerInvariant() == ".exe") {
					return args[0];
				}
			} catch { }

			return null;
		}

		static string GetEmbeddedFileName() {
			// Is the No$gba executable name embedded in the filename of the Nocash Launcher executable?
			string filename = Path.GetFileName(AppDomain.CurrentDomain.FriendlyName).Trim();
			Match regexMatch = Regex.Match(filename,
				@"^((?!\s*[-_@\.]?\s*launch(?:er)?\s*(\.[a-z_]+)*\.exe$).)+(?=\s*[-_@\.]?\s*launch(?:er)?\s*(\.[a-z_]+)*\.exe$)",
				RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
			);
			if (regexMatch.Success) {
				return SearchFolders(regexMatch.Value + ".exe");
			} else {
				return null;
			}
		}


		public static bool CheckAndWriteBytes(int handle, int address, IList<byte> before, IList<byte> after) {
			return CheckBytes(handle, address, before) && WriteBytes(handle, address, after);
		}

		public static bool CheckBytes(int handle, int address, IList<byte> bytes) {
			byte[] read = ReadBytes(handle, address, bytes.Count);

			if (read.Length != bytes.Count) {
				return false;
			}

			for (int i = 0; i < bytes.Count; i++) {
				if (bytes[i] != read[i]) {
					return false;
				}
			}

			return true;
		}

		public static byte[] ReadBytes(int handle, int address, int count) {
			byte[] buffer = new byte[count];
			int read = -1;

			ReadProcessMemory(handle, address, buffer, count, ref read);

			//Console.Write("Read bytes: ");
			//for (int i = 0; i < read; i++) {
			//	Console.Write(" " + buffer[i].ToString("X2"));
			//}
			//Console.WriteLine();

			byte[] result = new byte[read];
			for (int i = 0; i < read; i++) {
				result[i] = buffer[i];
			}

			return result;
		}

		public static bool WriteBytes(int handle, int address, IList<byte> bytes) {
			byte[] buffer = new byte[bytes.Count];
			for (int i = 0; i < bytes.Count; i++) {
				buffer[i] = bytes[i];
			}

			int written = -1;
			WriteProcessMemory(handle, address, buffer, bytes.Count, ref written);

			//Console.Write("Wrote bytes:");
			//for (int i = 0; i < written; i++) {
			//	Console.Write(" " + buffer[i].ToString("X2"));
			//}
			//Console.WriteLine();

			return written == bytes.Count;
		}
	}
}
