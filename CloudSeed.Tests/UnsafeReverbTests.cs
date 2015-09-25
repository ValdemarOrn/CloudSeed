using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudSeed.Tests
{
	[TestClass]
	public class UnsafeReverbTests
	{
		private const string program = @"
{
  ""InputMix"": 0.0,
  ""PreDelay"": 0.0,
  ""HighPass"": 0.0,
  ""LowPass"": 1.0,
  ""TapCount"": 1.0,
  ""TapLength"": 1.0,
  ""TapGain"": 0.0,
  ""TapDecay"": 1.0,
  ""DiffusionEnabled"": 0.0,
  ""DiffusionStages"": 0.0,
  ""DiffusionDelay"": 0.0,
  ""DiffusionFeedback"": 0.49999982118606567,
  ""LineCount"": 0.0,
  ""LineDelay"": 0.82999998331069946,
  ""LineFeedback"": 0.81999993324279785,
  ""LateDiffusionEnabled"": 0.0,
  ""LateDiffusionStages"": 1.0,
  ""LateDiffusionDelay"": 0.20999975502490997,
  ""LateDiffusionFeedback"": 0.5499998927116394,
  ""PostLowShelfGain"": 1.0,
  ""PostLowShelfFrequency"": 0.0,
  ""PostHighShelfGain"": 1.0,
  ""PostHighShelfFrequency"": 1.0,
  ""PostCutoffFrequency"": 1.0,
  ""EarlyDiffusionModAmount"": 0.0,
  ""EarlyDiffusionModRate"": 0.0,
  ""LineModAmount"": 0.0,
  ""LineModRate"": 0.0,
  ""LateDiffusionModAmount"": 0.0,
  ""LateDiffusionModRate"": 0.0,
  ""TapSeed"": 0.0003009999927598983,
  ""DiffusionSeed"": 0.00012099999730708078,
  ""CombSeed"": 0.00010299999848939478,
  ""PostDiffusionSeed"": 0.00023499999952036887,
  ""CrossFeed"": 1.0,
  ""DryOut"": 0.0,
  ""PredelayOut"": 0.0,
  ""EarlyOut"": 0.0,
  ""MainOut"": 1.0,
  ""HiPassEnabled"": 0.0,
  ""LowPassEnabled"": 0.0,
  ""LowShelfEnabled"": 0.0,
  ""HighShelfEnabled"": 0.0,
  ""CutoffEnabled"": 0.0,
  ""LateStageTap"": 1.0
}";

		[TestMethod]
		public void TestCreate()
		{
			var rev = new UnsafeReverbController(48000);
			rev.Dispose();
		}

		[TestMethod]
		public void TestSetProgram()
		{
			var controller = new UnsafeReverbController(48000);
			
			var dict = JsonConvert.DeserializeObject<Dictionary<string, double>>(program);
			foreach (var kvp in dict)
			{
				Parameter param;
				var ok = Enum.TryParse(kvp.Key, out param);
				if (ok)
				{
					controller.SetParameter(param, kvp.Value);
				}
			}
			controller.ClearBuffers();
			var inputLArr = new double[64];
			var inputRArr = new double[64];
			var outputLArr = new double[64];
			var outputRArr = new double[64];
			var input = new IntPtr[2];
			var output = new IntPtr[2];

			unsafe
			{
				fixed (double* inL = inputLArr)
				fixed (double* inR = inputLArr)
				fixed (double* outL = outputLArr)
				fixed (double* outR = outputRArr)
				fixed (IntPtr* ins = input)
				fixed (IntPtr* outs = output)
				{
					inL[0] = 1.0;
					inR[0] = 1.0;

					ins[0] = (IntPtr)inL;
					ins[1] = (IntPtr)inR;

					outs[0] = (IntPtr)outL;
					outs[1] = (IntPtr)outR;

					for (int i = 0; i < 10000; i++)
					{
						controller.Process((IntPtr)ins, (IntPtr)outs, 64);
						inL[0] = 0.0;
						inR[0] = 0.0;
					}
				}
            }
			controller.Dispose();
		}
	}
}
