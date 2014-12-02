//#define PTIMER_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSeed
{
	public static class PTimer
	{
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		private static Dictionary<string, Stopwatch> stopwatches;
		private static Dictionary<string, Queue<long>> processingTimes;

		static PTimer()
		{
			#if PTIMER_ENABLED
			stopwatches = new Dictionary<string, Stopwatch>();
			processingTimes = new Dictionary<string, Queue<long>>();
			AllocConsole();
			new Thread(Loop).Start();
			#endif
		}

		public static void Begin(string section)
		{
			#if PTIMER_ENABLED
			Stopwatch val;
			if (!stopwatches.TryGetValue(section, out val))
			{
				stopwatches[section] = Stopwatch.StartNew();
				processingTimes[section] = new Queue<long>();
			}
			else
			{
				val.Restart();
			}
			#endif
		}

		public static void End(string section)
		{
			#if PTIMER_ENABLED
			Stopwatch stopwatch;
			if (!stopwatches.TryGetValue(section, out stopwatch))
			{
				// Stopping a non-existing section
			}
			else
			{
				stopwatch.Stop();
				var queue = processingTimes[section];
				lock (queue)
				{
					queue.Enqueue(stopwatch.ElapsedTicks);
					if (queue.Count > 1000)
						queue.Dequeue();
				}
			}
			#endif
		}

		private static void Loop()
		{
			while (true)
			{
				Thread.Sleep(500);
				PrintTimes();
			}
		}

		private static void PrintTimes()
		{
			var sb = new StringBuilder();

			foreach (var kvp in processingTimes)
			{
				var key = kvp.Key;
				var queue = kvp.Value;
				lock (queue)
				{
					var averageTicks = queue.Sum() / (double)queue.Count;
					var averageMicros = averageTicks / TimeSpan.TicksPerMillisecond * 1000;
					sb.Append(string.Format("{0}: {1:0.0}   ", key, averageMicros));
				}
			}

			Console.WriteLine(sb.ToString());
		}

	}
}
