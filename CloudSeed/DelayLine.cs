using System;
using System.Collections.Generic;
using System.Linq;
using AudioLib;
using AudioLib.Modules;
using AudioLib.TF;

namespace CloudSeed
{
	public class DelayLine
	{
		private readonly ModulatedDelay delay;
		private readonly AllpassDiffuser diffuser;
		private readonly Biquad lowShelf;
		private readonly Biquad highShelf;
		private readonly Lp1 lowPass;
		private readonly double[] tempBuffer;
		private readonly double[] filterOutputBuffer;

		private double feedback;
		private int samplerate;

		public bool DiffuserEnabled;
		public bool LowShelfEnabled;
		public bool HighShelfEnabled;
		public bool CutoffEnabled;

		public DelayLine(int bufferSize, int samplerate)
		{
			delay = new ModulatedDelay(bufferSize, 10000);
			diffuser = new AllpassDiffuser(bufferSize, samplerate) { ModulationEnabled = false };
			tempBuffer = new double[bufferSize];
			filterOutputBuffer = new double[bufferSize];

			lowPass = new Lp1(samplerate);
			lowShelf = new Biquad(Biquad.FilterType.LowShelf, samplerate) { Slope = 1.0, GainDB = -20, Frequency = 20 };
			highShelf = new Biquad(Biquad.FilterType.HighShelf, samplerate) { Slope = 1.0, GainDB = -20, Frequency = 19000 };
			lowPass.CutoffHz = 1000;
			lowShelf.Update();
			highShelf.Update();
			Samplerate = samplerate;
		}

		public int Samplerate
		{
			get { return samplerate; }
			set
			{
				samplerate = value;
				diffuser.Samplerate = samplerate;
				lowPass.Samplerate = samplerate;
				lowShelf.Samplerate = samplerate;
				highShelf.Samplerate = samplerate;
			}
		}

		public double[] DiffuserSeeds
		{
			get { return diffuser.Seeds; }
			set { diffuser.Seeds = value; }
		}

		public void SetDelay(int delaySamples)
		{
			delay.SampleDelay = delaySamples;
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

		public void SetCutoffFrequency(double frequency)
		{
			lowPass.CutoffHz = frequency;
		}

		public void SetModAmount(double amount)
		{
			delay.ModAmount = amount;
		}

		public void SetModRate(double rate)
		{
			delay.ModRate = rate;
		}

		public double[] Output { get { return delay.Output; } }

		public void Process(double[] input, int sampleCount)
		{
			var feedbackBuffer = DiffuserEnabled ? diffuser.Output : filterOutputBuffer;
			
			for (int i = 0; i < sampleCount; i++)
				tempBuffer[i] = input[i] + feedbackBuffer[i] * feedback;

			delay.Process(tempBuffer, sampleCount);
			delay.Output.Copy(tempBuffer, sampleCount);

			if (LowShelfEnabled)
				lowShelf.Process(tempBuffer, tempBuffer, sampleCount);
			if (HighShelfEnabled)
				highShelf.Process(tempBuffer, tempBuffer, sampleCount);
			if (CutoffEnabled)
				lowPass.Process(tempBuffer, tempBuffer, sampleCount);

			tempBuffer.Copy(filterOutputBuffer, sampleCount);

			if (DiffuserEnabled)
			{
				diffuser.Process(filterOutputBuffer, sampleCount);
			}
		}

		public void ClearBuffers()
		{
			delay.ClearBuffers();
			diffuser.ClearBuffers();
			lowShelf.ClearBuffers();
			highShelf.ClearBuffers();
			lowPass.Output = 0;
			tempBuffer.Zero();
			filterOutputBuffer.Zero();
		}
	}
}
