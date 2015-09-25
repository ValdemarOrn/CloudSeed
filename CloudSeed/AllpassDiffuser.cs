using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLib;

namespace CloudSeed
{
	public class AllpassDiffuser
	{
		public const int MaxStageCount = 8;

		private readonly ModulatedAllpass[] filters;
		private double[] output;
		private int delay;
		private double modRate;
		private int samplerate;
		private double[] seeds;

		public int Stages;
		
		public AllpassDiffuser(int bufferSize, int samplerate)
		{
			filters = Enumerable.Range(0, MaxStageCount).Select(x => new ModulatedAllpass(bufferSize, 100)).ToArray();
			output = new double[bufferSize];
			Seeds = new ShaRandom().Generate(23456, MaxStageCount * 3).ToArray();
			Stages = 1;

			Samplerate = samplerate;
		}

		public int Samplerate
		{
			get { return samplerate; }
			set
			{
				samplerate = value;
				SetModRate(modRate);
			}
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

		public bool ModulationEnabled
		{
			get { return filters[0].ModulationEnabled; }
			set
			{
				foreach (var filter in filters)
					filter.ModulationEnabled = value;
			}
		}

		public double[] Output { get { return output; } }

		

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

		public void SetModAmount(double amount)
		{
			for (int i = 0; i < filters.Length; i++)
				filters[i].ModAmount = amount * (0.8 + 0.2 * Seeds[MaxStageCount + i]);
		}

		public void SetModRate(double rate)
		{
			modRate = rate;

			for (int i = 0; i < filters.Length; i++)
				filters[i].ModRate = rate * (0.5 + 0.5 * Seeds[MaxStageCount * 2 + i]) / samplerate;
		}

		private void Update()
		{
			for (int i = 0; i < filters.Length; i++)
				filters[i].SampleDelay = (int)(delay * (0.5 + 1.0 * Seeds[i]));
		}

		public void Process(double[] input, int sampleCount)
		{
			filters[0].Process(input, sampleCount);

			for (int i = 1; i < Stages; i++)
			{
				filters[i].Process(filters[i - 1].Output, sampleCount);
			}
			
			output = filters[Stages - 1].Output;
		}

		public void ClearBuffers()
		{
			for (int i = 0; i < filters.Length; i++)
				filters[i].Clear();
		}
	}
}
