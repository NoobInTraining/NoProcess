using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NoProcessConsole
{
	class Program
	{
		static void Main(string[] args)
		{			
			//get all processes
			var proc = Process.GetProcessesByName("FragenExtractor")[0];
			ResumeProcess(proc.Id);
			SuspendProcess(proc.Id);
			Console.WriteLine("Found potential lethal process");
			Console.WriteLine("Continue working it");
			Console.ReadKey();
			ResumeProcess(proc.Id);
		}

		[Flags]
		public enum ThreadAccess : int
		{
			TERMINATE = (0x0001),
			SUSPEND_RESUME = (0x0002),
			GET_CONTEXT = (0x0008),
			SET_CONTEXT = (0x0010),
			SET_INFORMATION = (0x0020),
			QUERY_INFORMATION = (0x0040),
			SET_THREAD_TOKEN = (0x0080),
			IMPERSONATE = (0x0100),
			DIRECT_IMPERSONATION = (0x0200)
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
		[DllImport("kernel32.dll")]
		static extern uint SuspendThread(IntPtr hThread);
		[DllImport("kernel32.dll")]
		static extern int ResumeThread(IntPtr hThread);
		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool CloseHandle(IntPtr handle);


		private static void SuspendProcess(int pid)
		{
			var process = Process.GetProcessById(pid);

			if (process.ProcessName == string.Empty)
				return;

			foreach (ProcessThread pT in process.Threads)
			{
				IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

				if (pOpenThread == IntPtr.Zero)
				{
					continue;
				}

				SuspendThread(pOpenThread);

				CloseHandle(pOpenThread);
			}
		}

		/// <summary>
		/// https://stackoverflow.com/questions/71257/suspend-process-in-c-sharp/71457#71457
		/// </summary>
		/// <param name="pid"></param>
		public static void ResumeProcess(int pid)
		{
			var process = Process.GetProcessById(pid);

			if (process.ProcessName == string.Empty)
				return;

			foreach (ProcessThread pT in process.Threads)
			{
				IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

				if (pOpenThread == IntPtr.Zero)
				{
					continue;
				}

				var suspendCount = 0;
				do
				{
					suspendCount = ResumeThread(pOpenThread);
				} while (suspendCount > 0);

				CloseHandle(pOpenThread);
			}
		}
	}
}
