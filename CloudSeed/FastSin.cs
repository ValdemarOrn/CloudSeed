using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public static class FastSin
	{
		private static readonly double[] data;

		static FastSin()
		{
			data = Enumerable.Range(0, 32768).Select(x => Math.Sin(2 * Math.PI * x / 32768.0)).ToArray();
		}

		/// <summary>
		/// Index must be 0...1
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static double Get(double index)
		{
			return data[(int)(index * 32767.99999)];
		}
	}
}
