using AudioLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudSeed
{
	public class ReverbController
	{
		private int samplerate;

		private readonly ReverbChannel channelL;
		private readonly ReverbChannel channelR;
		private readonly double[] leftChannelIn;
		private readonly double[] rightChannelIn;
		
		public readonly double[] Parameters;

		public ReverbController(int samplerate)
		{
			const int bufferSize = 192000 / 2; // just make it huge by default...

			Parameters = new double[Parameter.Count.Value()];
			leftChannelIn = new double[bufferSize];
			rightChannelIn = new double[bufferSize];
			channelL = new ReverbChannel(bufferSize, samplerate);
			channelR = new ReverbChannel(bufferSize, samplerate);
			Samplerate = samplerate;
		}
		
		public int Samplerate
		{
			get { return samplerate; }
			set
			{
				samplerate = value;
				channelL.Samplerate = samplerate;
				channelR.Samplerate = samplerate;
			}
		}
		
		private double P(Parameter para)
		{
			var idx = para.Value();
			return idx >= 0 && idx < Parameters.Length ? Parameters[idx] : 0.0;
		}

		/// <summary>
		/// warps the 0...1 parameter range into a meaningful value
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public double GetScaledParameter(Parameter param)
		{ 
			switch(param)
			{
				// Input
				case Parameter.CrossMix:                  return P(Parameter.CrossMix);
				case Parameter.PreDelay:                  return (int)(P(Parameter.PreDelay) * 500);
		
				case Parameter.HighPass:                  return 20 + ValueTables.Get(P(Parameter.HighPass), ValueTables.Response4Oct) * 980;
				case Parameter.LowPass:                   return 400 + ValueTables.Get(P(Parameter.LowPass), ValueTables.Response4Oct) * 19600;

				// Early
				case Parameter.TapCount:                  return 1 + (int)(P(Parameter.TapCount) * 49.0);
				case Parameter.TapLength:                 return (int)(P(Parameter.TapLength) * 500);
				case Parameter.TapGain:                   return ValueTables.Get(P(Parameter.TapGain), ValueTables.Response2Dec);
				case Parameter.TapDecay:                  return P(Parameter.TapDecay);

				case Parameter.DiffusionEnabled:          return P(Parameter.DiffusionEnabled);
				case Parameter.DiffusionStages:           return 1 + (int)(P(Parameter.DiffusionStages) * 3.999);
				case Parameter.DiffusionDelay:            return (int)(P(Parameter.DiffusionDelay) * 50);
				case Parameter.DiffusionFeedback:         return P(Parameter.DiffusionFeedback);

				// Late
				case Parameter.LineCount:                 return 1 + (int)(P(Parameter.LineCount) * 11.999);
				case Parameter.LineDelay:                 return (int)(P(Parameter.LineDelay) * 500);
				case Parameter.LineFeedback:              return P(Parameter.LineFeedback);

				case Parameter.PostDiffusionEnabled:      return P(Parameter.PostDiffusionEnabled);
				case Parameter.PostDiffusionStages:       return 1 + (int)(P(Parameter.PostDiffusionStages) * 3.999);
				case Parameter.PostDiffusionDelay:        return (int)(P(Parameter.PostDiffusionDelay) * 50);
				case Parameter.PostDiffusionFeedback:     return P(Parameter.PostDiffusionFeedback);

				// Frequency Response
				case Parameter.PostLowShelfGain:          return ValueTables.Get(P(Parameter.PostLowShelfGain), ValueTables.Response2Dec);
				case Parameter.PostLowShelfFrequency:     return 20 + ValueTables.Get(P(Parameter.PostLowShelfFrequency), ValueTables.Response4Oct) * 980;
				case Parameter.PostHighShelfGain:         return ValueTables.Get(P(Parameter.PostHighShelfGain), ValueTables.Response2Dec);
				case Parameter.PostHighShelfFrequency:    return 400 + ValueTables.Get(P(Parameter.PostHighShelfFrequency), ValueTables.Response4Oct) * 19600;
				case Parameter.PostCutoffFrequency:       return 400 + ValueTables.Get(P(Parameter.PostCutoffFrequency), ValueTables.Response4Oct) * 19600;

				// Modulation
				case Parameter.DiffusionModAmount:        return P(Parameter.DiffusionModAmount) * 2.5;
				case Parameter.DiffusionModRate:          return ValueTables.Get(P(Parameter.DiffusionModRate), ValueTables.Response2Dec) * 5;
				case Parameter.LineModAmount:             return P(Parameter.LineModAmount) * 2.5;
				case Parameter.LineModRate:               return ValueTables.Get(P(Parameter.LineModRate), ValueTables.Response2Dec) * 5;

				// Seeds
				case Parameter.TapSeed:                   return (int)(P(Parameter.TapSeed) * 1000000);
				case Parameter.DiffusionSeed:             return (int)(P(Parameter.DiffusionSeed) * 1000000);
				case Parameter.CombSeed:                  return (int)(P(Parameter.CombSeed) * 1000000);
				case Parameter.PostDiffusionSeed:         return (int)(P(Parameter.PostDiffusionSeed) * 1000000);

				// Output
				case Parameter.StereoWidth:               return P(Parameter.StereoWidth);

				case Parameter.DryOut:                    return ValueTables.Get(P(Parameter.DryOut), ValueTables.Response2Dec);
				case Parameter.PredelayOut:               return ValueTables.Get(P(Parameter.PredelayOut), ValueTables.Response2Dec);
				case Parameter.EarlyOut:                  return ValueTables.Get(P(Parameter.EarlyOut), ValueTables.Response2Dec);
				case Parameter.MainOut:                   return ValueTables.Get(P(Parameter.MainOut), ValueTables.Response2Dec);

				// Switches
				case Parameter.HiPassEnabled:             return P(Parameter.HiPassEnabled);
				case Parameter.LowPassEnabled:            return P(Parameter.LowPassEnabled);
				case Parameter.LowShelfEnabled:           return P(Parameter.LowShelfEnabled);
				case Parameter.HighShelfEnabled:          return P(Parameter.HighShelfEnabled);
				case Parameter.CutoffEnabled:             return P(Parameter.CutoffEnabled);

				default: return 0.0;
			}
		}

		/// <summary>
		/// Sets a parameter using a normalized double in the range 0...1
		/// </summary>
		/// <param name="param"></param>
		/// <param name="value"></param>
		public void SetParameter(Parameter param, double value)
		{
			Parameters[param.Value()] = value;
			var scaled = GetScaledParameter(param);

			channelL.SetParameter(param, scaled);

			if (param.Value() >= Parameter.TapSeed.Value() && param.Value() <= Parameter.PostDiffusionSeed.Value())
				scaled = (int)scaled + 1000000; // different seeds for right channel

			channelR.SetParameter(param, scaled);
		}

		public void Process(double[][] input, double[][] output)
		{
			var len = input[0].Length;

			var cm = GetScaledParameter(Parameter.CrossMix) * 0.5;
			var cmi = (1 - cm);
			var st = 0.5 + 0.5 * GetScaledParameter(Parameter.StereoWidth);
			var sti = (1 - st);

			for (int i = 0; i < len; i++)
			{
				leftChannelIn[i] = input[0][i] * cmi + input[1][i] * cm;
				rightChannelIn[i] = input[1][i] * cmi + input[0][i] * cm;
			}

			channelL.Process(leftChannelIn, len);
			channelR.Process(rightChannelIn, len);
			var leftOut = channelL.Output;
			var rightOut = channelR.Output;

			for (int i = 0; i < len; i++)
			{
				output[0][i] = leftOut[i] * st + rightOut[i] * sti;
				output[1][i] = rightOut[i] * st + leftOut[i] * sti;
			}
		}

		public void ClearBuffers()
		{
			channelL.ClearBuffers();
			channelR.ClearBuffers();
		}
	}
}
