using AudioLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class Reverber
	{
		double _samplerate;
		public double Samplerate
		{
			get { return _samplerate; }
			set
			{
				_samplerate = value;
				foreach (var allpass in AllpassModules)
				{
					allpass.Samplerate = _samplerate;
					allpass.HiCut = allpass.HiCut;
				}
			}
		}

		public double GlobalFeedback { get; set; }
		public int GlobalDelay { get; set; }
		public int LateStages { get; set; }

		double[] Amplitudes;
		int[] Taps;
		AllpassM[] AllpassModules;

		double[] EarlyBuffer;
		int EarlyI;

		double[] OutBuffer;
		int OutI;

		int SampleCounter;

		public Reverber()
		{
			AllpassModules = new AllpassM[8];
			for (int i = 0; i < AllpassModules.Length; i++)
			{
				AllpassModules[i] = new AllpassM();
			}
			
			OutBuffer = new double[48000];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="predelay">Predelay ms</param>
		/// <param name="size">Early Reflection size ms</param>
		public void SetTaps(double predelay, double size, int density)
		{
			if (size < 5)
				size = 5;
			if (density < 5)
				density = 5;

			var rand = new Random();
			var min = (int)(predelay / 1000.0 * Samplerate);
			var max = min + (int)(size / 1000.0 * Samplerate);
			Taps = Enumerable.Range(0, density).Select(x => rand.Next(min, max)).OrderBy(x => x).ToArray();
			Amplitudes = Taps.Select(x => Math.Exp(-x / (double)max * 3) * 2 * (0.5 - rand.NextDouble())).ToArray();
			//var gain = 1.0 / Amplitudes.Sum();
			//Amplitudes = Amplitudes.Select(x => x * gain).ToArray();
			EarlyBuffer = new double[max * 2];
			EarlyI = 0;
		}

		public void SetLate(double feedback, int allpassDelaySamples)
		{
			var rand = new Random();

			for (int i = 0; i < AllpassModules.Length; i++)
			{
				var ap = AllpassModules[i];
			
				ap.Feedback = feedback * (0.9 + 0.1 * rand.NextDouble());
				ap.Feedback = ap.Feedback > 0.98 ? 0.98 : ap.Feedback;
				ap.DelaySamples = (int)(allpassDelaySamples * (1 + 0.5 * i * (0.3 + 0.7 * rand.NextDouble())));
			}
		}

		public void SetHiCut(double fc, double amount)
		{
			var rand = new Random();
			foreach (var allpass in AllpassModules)
			{
				allpass.HiCut = fc * (0.5 + rand.NextDouble());
				allpass.HiCutAmount = amount;
			}
		}

		public void SetMod(double freq, double amount)
		{
			var rand = new Random();

			foreach (var allpass in AllpassModules)
			{
				allpass.ModFreq = freq * (1 + 0.8 * rand.NextDouble());
				allpass.ModAmount = amount;
			}

		}

		public double Process(double x)
		{
			SampleCounter++;

			if (SampleCounter % 16 == 0)
			{
				for (int i = 0; i < AllpassModules.Length; i++)
				{
					AllpassModules[i].UpdateMod(16);
				}
				SampleCounter = 0;
			}
			
			var len = EarlyBuffer.Length;

			EarlyBuffer[EarlyI] = x;
			
			double outputEarly = 0.0;

			for (int i = 0; i < Taps.Length; i++)
			{
				var idx = (EarlyI + Taps[i]) % len;
				outputEarly += EarlyBuffer[idx] * Amplitudes[i];
			}

			var d = outputEarly + GlobalFeedback * OutBuffer[(OutI + GlobalDelay) % OutBuffer.Length];

			var stageCount = LateStages;
			for (int i = 0; i < stageCount; i++)
			{
				d = AllpassModules[i].Process(d);
			}
			
			OutBuffer[OutI] = d;

			EarlyI--;
			if (EarlyI < 0)
				EarlyI += len;

			OutI--;
			if (OutI < 0)
				OutI += OutBuffer.Length;

			return d;
		}
	}
}
