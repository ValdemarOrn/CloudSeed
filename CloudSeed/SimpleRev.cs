using AudioLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class SimpleRev
	{
		private readonly ReverbChannel channelL, channelR;
		private readonly double[] tempBuffer;
		private readonly AllpassDiffuser apDiffuser;

		public readonly double[] Parameters;
		
		public SimpleRev()
		{
			channelL = new ReverbChannel(48000);
			channelR = new ReverbChannel(48000);

			tempBuffer = new double[48000];
			Parameters = new double[Parameter.Count.Value()];
		}
		
		private double P(Parameter para)
		{
			var idx = para.Value();
			return idx >= 0 && idx < Parameters.Length ? Parameters[idx] : 0.0;
		}

		#region Parameters

		// Input

		public double CrossMix               { get { return P(Parameter.CrossMix); } }
		public int PreDelay                  { get { return (int)(P(Parameter.PreDelay) * 0.5 * samplerate); } }
		
		public double HighPass               { get { return ValueTables.Get(P(Parameter.CrossMix), ValueTables.Response4Oct) * 500; } }
		public double LowPass                { get { return ValueTables.Get(P(Parameter.CrossMix), ValueTables.Response4Oct) * 20000; } }

		// Early

		public int TapCount                  { get { return (int)(P(Parameter.TapCount) * 100); } }
		public int TapLength                 { get { return (int)(P(Parameter.TapLength) * 0.5 * samplerate); } }
		public double TapGain                { get { return ValueTables.Get(P(Parameter.TapGain), ValueTables.Response3Dec); } }
		public double TapDecay               { get { return P(Parameter.TapDecay); } }

		public bool DiffusionEnabled         { get { return P(Parameter.DiffusionEnabled) >= 0.5; } }
		public int DiffusionStages           { get { return 1 + (int)(P(Parameter.DiffusionStages) * 3.999); } }
		public int DiffusionDelay            { get { return (int)(P(Parameter.DiffusionDelay) * 0.05 * samplerate); } }
		public double DiffusionFeedback      { get { return P(Parameter.DiffusionFeedback); } }

		// Late

		public double LineGain               { get { return ValueTables.Get(P(Parameter.LineGain), ValueTables.Response3Dec); } }
		public int LineDelay                 { get { return (int)(P(Parameter.LineDelay) * 0.5 * samplerate); } }
		public double LineFeedback           { get { return P(Parameter.LineFeedback); } }

		public bool PostDiffusionEnabled     { get { return P(Parameter.PostDiffusionEnabled) >= 0.5; } }
		public int PostDiffusionStages       { get { return 1 + (int)(P(Parameter.PostDiffusionStages) * 3.999); } }
		public int PostDiffusionDelay        { get { return (int)(P(Parameter.PostDiffusionDelay) * 0.05 * samplerate); } }
		public double PostDiffusionFeedback  { get { return P(Parameter.PostDiffusionFeedback); } }

		// Frequency Response

		public double PostLowShelfGain       { get { return ValueTables.Get(P(Parameter.PostLowShelfGain), ValueTables.Response3Dec); } }
		public double PostLowShelfFrequency  { get { return ValueTables.Get(P(Parameter.PostLowShelfFrequency), ValueTables.Response4Oct) * 1000; } }
		public double PostHighShelfGain      { get { return ValueTables.Get(P(Parameter.PostHighShelfGain), ValueTables.Response3Dec); } }
		public double PostHighShelfFrequency { get { return ValueTables.Get(P(Parameter.PostHighShelfFrequency), ValueTables.Response4Oct) * 20000; } }
		public double PostCutoffFrequency    { get { return ValueTables.Get(P(Parameter.PostHighShelfFrequency), ValueTables.Response4Oct) * 20000; } }

		// Modulation
		
		public double DiffuserModAmount      { get { return P(Parameter.DiffuserModAmount); } }
		public double DiffuserModRate        { get { return ValueTables.Get(P(Parameter.DiffuserModRate), ValueTables.Response3Dec) * 10; } }

		public double LineModAmount          { get { return P(Parameter.LineModAmount); } }
		public double LineModRate            { get { return ValueTables.Get(P(Parameter.LineModRate), ValueTables.Response3Dec) * 10; } }

		// Seeds

		public int TapSeed                   { get { return (int)(P(Parameter.TapSeed) * 1000000); } }
		public int DiffusionSeed             { get { return (int)(P(Parameter.DiffusionSeed) * 1000000); } }
		public int CombSeed                  { get { return (int)(P(Parameter.CombSeed) * 1000000); } }
		public int PostDiffusionSeed         { get { return (int)(P(Parameter.PostDiffusionSeed) * 1000000); } }

		// Output

		public double StereoWidth            { get { return P(Parameter.StereoWidth); } }

		public double DryOut                 { get { return ValueTables.Get(P(Parameter.DryOut), ValueTables.Response3Dec); } }
		public double PredelayOut            { get { return ValueTables.Get(P(Parameter.PredelayOut), ValueTables.Response3Dec); } }
		public double EarlyOut               { get { return ValueTables.Get(P(Parameter.EarlyOut), ValueTables.Response3Dec); } }
		public double LineOut                { get { return ValueTables.Get(P(Parameter.LineOut), ValueTables.Response3Dec); } }
		public double PostDiffusionOut       { get { return ValueTables.Get(P(Parameter.PostDiffusionOut), ValueTables.Response3Dec); } }

		#endregion

		double samplerate;
		public double Samplerate
		{
			get { return samplerate; }
			set
			{
				samplerate = value;
			}
		}

		public void SetParameter(Parameter param, double value)
		{
			Parameters[param.Value()] = value;
			var propVal = GetType().GetProperty(param.ToString()).GetValue(this);
			channelL.SetParameter(param, propVal);
			//channelR.SetParameter(param, propVal);
		}

		public void Process(double[][] input, double[][] output)
		{
			var len = input[0].Length;
			channelL.Process(input[0], len);
			//channelR.Process(input[1], len);
			var leftOut = channelL.Output;
			//var rightOut = channelR.Output;

			for (int i = 0; i < len; i++)
			{
				output[0][i] = leftOut[i];
				output[1][i] = leftOut[i];
				//output[1][i] = rightOut[i];
			}
		}
	}
}
