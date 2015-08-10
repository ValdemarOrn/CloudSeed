using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLib;

namespace CloudSeed
{
	public class MultitapDiffuser
	{
		private readonly double[] buffer;
		private readonly double[] output;
		private readonly int len;

		private int index;
		private double[] tapGains;
		private int[] tapPosition;
		private double[] seeds;

		private int count;
		private double length;
		private double gain;
		private double decay;

		public MultitapDiffuser(int bufferSize)
		{
			this.len = bufferSize;
			this.buffer = new double[bufferSize];
			this.output = new double[bufferSize];
			index = 0;
			Seeds = new ShaRandom().Generate(1, 100).ToArray();
		}

		public double[] Seeds
		{
			get { return seeds; }
			set
			{
				seeds = value;
				Update();
			}
		}

		public double[] Output { get { return output; } }

		public void SetTapCount(int tapCount)
		{
			count = tapCount;
			Update();
		}

		public void SetTapLength(int tapLength)
		{
			length = tapLength;
			Update();
		}

		public void SetTapDecay(double tapDecay)
		{
			decay = tapDecay;
			Update();
		}

		public void SetTapGain(double tapGain)
		{
			gain = tapGain;
			Update();
		}

		public void Update()
		{
			int s = 0;
			Func<double> rand = () => Seeds[s++];

			if (count < 1)
				count = 1;

			if (length < count)
				length = count;

			tapGains = new double[count];
			tapPosition = new int[count];

			var tapData = Enumerable.Range(0, count).Select(x => 0.1 + rand()).ToArray();
			var scale = length / tapData.Sum();
			tapPosition[0] = 0;

			for (int i = 1; i < count; i++)
			{
				tapPosition[i] = tapPosition[i - 1] + (int)(tapData[i] * scale);
			}

			for (int i = 0; i < count; i++)
			{
				var g = gain * (1 - decay * (i / (double)count));
				tapGains[i] = g * (2 * rand() - 1);
			}

			tapGains[0] = (1 - gain) + tapGains[0] * gain;
		}

		public void Process(double[] input, int sampleCount)
		{
			var taps = tapGains.Length;

			for (int i = 0; i < sampleCount; i++)
			{
				if (index < 0) index += len;
				buffer[index] = input[i];
				output[i] = 0.0;

				for (int j = 0; j < taps; j++)
				{
					var idx = (index + tapPosition[j]) % len;
					output[i] += buffer[idx] * tapGains[j];
				}

				index--;
			}
		}

		public void ClearBuffers()
		{
			buffer.Zero();
		}
	}
}
