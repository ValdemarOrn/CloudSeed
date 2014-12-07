using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSeed.AudioPipe
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			//System.Diagnostics.Debugger.Launch();

			using (var istream = Console.OpenStandardInput(1024 * 64))
			using (var ostream = Console.OpenStandardOutput(1024 * 64))
			{
				var buf = new byte[1024 * 64];
				while (true)
				{
					var count = istream.Read(buf, 0, buf.Length);

					for (int i = 0; i < count; i++)
					{
						buf[i] = (byte)(buf[i] * 2 % 256);
					}

					ostream.Write(buf, 0, count);
					ostream.Flush();
				}
			}
		}
	}
}
