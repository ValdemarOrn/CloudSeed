using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class SimpleDelay
	{
		private readonly double[] buffer;
		private readonly double[] output;
		private readonly int len;

		private int sampleDelay;
		private int index;
		
		public SimpleDelay(int bufferSize, int sampleDelay)
		{
			this.len = bufferSize;
			this.buffer = new double[bufferSize];
			this.output = new double[bufferSize];
			this.sampleDelay = sampleDelay;
			index = bufferSize - 1;
		}

		public double[] Output { get { return output; } }

		public void SetDelay(int delaySamples)
		{
			if (delaySamples <= 0)
				delaySamples = 1;

			sampleDelay = delaySamples;
		}

		public void Process(double[] input, int sampleCount)
		{
			int indexread = (index + sampleDelay) % len;

			for (int i = 0; i < sampleCount; i++)
			{
				if (index < 0) index += len;
				buffer[index--] = input[i];
			}

			for (int i = 0; i < sampleCount; i++)
			{
				if (indexread < 0) indexread += len;
				output[i] = buffer[indexread--];
			}
		}
	}
}
