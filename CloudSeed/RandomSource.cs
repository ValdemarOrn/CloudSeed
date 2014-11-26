using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class RandomSource
	{
		static byte[] Data;

		static RandomSource()
		{
			Data = Resource.random;
		}

		public static uint[] GetRandomUInts(int seed, int count)
		{
			var output = new uint[count];

			for (int i = 0; i < count; i++)
				output[i] = BitConverter.ToUInt32(Data, (seed + i * 4) % (Data.Length - 4));

			return output;
		}

		public static double[] GetRandomDoubles(int seed, int count)
		{
			var ints = GetRandomUInts(seed, count);
			return ints.Select(x => x / (double)UInt32.MaxValue).ToArray();
		}
	}
}
