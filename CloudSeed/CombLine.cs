using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLib.Modules;

namespace CloudSeed
{
	public class CombLine
	{
		private readonly SimpleDelay delay;
		private readonly AllpassDiffuser diffuser;
		private readonly Biquad lowShelf;
		private readonly Biquad highShelf;
		private readonly double[] sumBuffer;

		private double feedback;
		private bool diffuserEnabled;

		public CombLine(int bufferSize, int samplerate)
		{
			delay = new SimpleDelay(bufferSize, 10000);
			diffuser = new AllpassDiffuser(bufferSize);
			sumBuffer = new double[bufferSize];

			lowShelf = new Biquad(Biquad.FilterType.LowShelf, samplerate) { Slope = 1.0, GainDB = -20, Frequency = 20 };
			highShelf = new Biquad(Biquad.FilterType.HighShelf, samplerate) { Slope = 1.0, GainDB = -20, Frequency = 19000 };
			lowShelf.Update();
			highShelf.Update();
		}

		public double[] Seeds
		{
			get { return diffuser.Seeds; }
			set { diffuser.Seeds = value; }
		}

		public void SetDelay(int delaySamples)
		{
			delay.SetDelay(delaySamples);
		}

		public void SetFeedback(double feedb)
		{
			feedback = feedb;
		}
		
		public void SetDiffuserDelay(int delaySamples)
		{
			diffuser.SetDelay(delaySamples);
		}

		public void SetDiffuserFeedback(double feedb)
		{
			diffuser.SetFeedback(feedb);
		}

		public void SetDiffuserStages(int stages)
		{
			diffuser.Stages = stages;
		}

		public void SetDiffuserEnabled(bool isEnabled)
		{
			diffuserEnabled = isEnabled;
		}

		public void SetLowShelfGain(double gain)
		{
			lowShelf.Gain = gain;
			lowShelf.Update();
		}

		public void SetLowShelfFrequency(double frequency)
		{
			lowShelf.Frequency = frequency;
			lowShelf.Update();
		}

		public void SetHighShelfGain(double gain)
		{
			highShelf.Gain = gain;
			highShelf.Update();
		}
		
		public void SetHighShelfFrequency(double frequency)
		{
			highShelf.Frequency = frequency;
			highShelf.Update();
		}

		public double[] Output { get { return delay.Output; } }

		public void Process(double[] input, int sampleCount)
		{
			var delayOut = delay.Output;
			var feedbackBuffer = diffuserEnabled ? diffuser.Output : delay.Output;

			for (int i = 0; i < sampleCount; i++)
				sumBuffer[i] = input[i] + feedbackBuffer[i] * feedback;

			delay.Process(sumBuffer, sampleCount);

			if (diffuserEnabled)
				diffuser.Process(delayOut, sampleCount);
		}
	}
}
