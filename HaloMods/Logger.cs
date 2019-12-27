using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloMods
{
	public delegate void LogDelegate(string message);
	public delegate void LogLineDelegate(string message);

	public static class Logger
	{
		public static event LogDelegate LogHandle;
		public static event LogLineDelegate LogLineHandle;

		public static void Log(string message)
		{
			Console.Write(message);
			if (LogHandle != null)
				LogHandle(message);
		}

		public static void LogLine(string message)
		{
			Console.WriteLine(message);
			if (LogLineHandle != null)
				LogLineHandle(message);
		}

	}
}
