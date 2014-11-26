using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public class AllpassDiffuser
	{
		private readonly Allpass[] filters;
		private double[] output;
		private int delay;
		private double[] seeds;

		public AllpassDiffuser(int bufferSize)
		{
			filters = Enumerable.Range(0, 4).Select(x => new Allpass(bufferSize, 100)).ToArray();
			output = new double[bufferSize];
			Seeds = new[] { 0.17124124, 0.3392345612, 0.62312624, 1.0 };
			Stages = 1;
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

		public int Stages;

		public void SetDelay(int delaySamples)
		{
			delay = delaySamples;
			Update();
		}

		public void SetFeedback(double feedback)
		{
			for (int i = 0; i < filters.Length; i++)
				filters[i].Feedback = feedback;
		}

		public void Update()
		{
			for (int i = 0; i < filters.Length; i++)
				filters[i].SampleDelay = (int)(delay * (0.5 + 0.5 * Seeds[i]));
		}

		public void Process(double[] input, int sampleCount)
		{
			filters[0].Process(input, sampleCount);
			filters[1].Process(filters[0].Output, sampleCount);
			filters[2].Process(filters[1].Output, sampleCount);
			filters[3].Process(filters[2].Output, sampleCount);

			output = filters[Stages - 1].Output;
		}
	}
}
