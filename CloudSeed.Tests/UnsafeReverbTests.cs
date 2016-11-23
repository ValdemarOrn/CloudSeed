using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudSeed.Tests
{
	[TestClass]
	public class UnsafeReverbTests
	{
		private const string programFail = @"
{
  ""InputMix"": 0.1549999862909317,
  ""PreDelay"": 0.0,
  ""HighPass"": 0.57999998331069946,
  ""LowPass"": 0.84000009298324585,
  ""TapCount"": 0.41499990224838257,
  ""TapLength"": 0.43999996781349182,
  ""TapGain"": 0.7300000786781311,
  ""TapDecay"": 1.0,
  ""DiffusionEnabled"": 1.0,
  ""DiffusionStages"": 0.4285714328289032,
  ""DiffusionDelay"": 0.27500024437904358,
  ""DiffusionFeedback"": 0.660000205039978,
  ""LineCount"": 0.72727274894714355,
  ""LineDelay"": 0.22500017285346985,
  ""LineDecay"": 0.80499988794326782,
  ""LateDiffusionEnabled"": 1.0,
  ""LateDiffusionStages"": 1.0,
  ""LateDiffusionDelay"": 0.22999951243400574,
  ""LateDiffusionFeedback"": 0.59499990940093994,
  ""PostLowShelfGain"": 0.95999979972839355,
  ""PostLowShelfFrequency"": 0.23999994993209839,
  ""PostHighShelfGain"": 0.97000002861022949,
  ""PostHighShelfFrequency"": 0.72000002861022949,
  ""PostCutoffFrequency"": 0.87999981641769409,
  ""EarlyDiffusionModAmount"": 0.13499999046325684,
  ""EarlyDiffusionModRate"": 0.29000008106231689,
  ""LineModAmount"": 0.53999996185302734,
  ""LineModRate"": 0.44999989867210388,
  ""LateDiffusionModAmount"": 0.17499998211860657,
  ""LateDiffusionModRate"": 0.28500008583068848,
  ""TapSeed"": 0.00048499999684281647,
  ""DiffusionSeed"": 0.00020799999765586108,
  ""CombSeed"": 0.00033499998971819878,
  ""PostDiffusionSeed"": 0.00037200000951997936,
  ""CrossSeed"": 0.800000011920929,
  ""DryOut"": 1.0,
  ""PredelayOut"": 0.0,
  ""EarlyOut"": 0.8200000524520874,
  ""MainOut"": 0.90500003099441528,
  ""HiPassEnabled"": 1.0,
  ""LowPassEnabled"": 1.0,
  ""LowShelfEnabled"": 1.0,
  ""HighShelfEnabled"": 1.0,
  ""CutoffEnabled"": 1.0,
  ""LateStageTap"": 1.0,
  ""Interpolation"": 0.0
}";

		public const string program2 = @"
{
  ""InputMix"": 0.1549999862909317,
  ""PreDelay"": 0.0,
  ""HighPass"": 0.57999998331069946,
  ""LowPass"": 0.84000009298324585,
  ""TapCount"": 0.41499990224838257,
  ""TapLength"": 0.43999996781349182,
  ""TapGain"": 0.7300000786781311,
  ""TapDecay"": 1.0,
  ""DiffusionEnabled"": 1.0,
  ""DiffusionStages"": 0.4285714328289032,
  ""DiffusionDelay"": 0.27500024437904358,
  ""DiffusionFeedback"": 0.660000205039978,
  ""LineCount"": 0.72727274894714355,
  ""LineDelay"": 0.22500017285346985,
  ""LineDecay"": 0.80499988794326782,
  ""LateDiffusionEnabled"": 0.0,
  ""LateDiffusionStages"": 1.0,
  ""LateDiffusionDelay"": 0.22999951243400574,
  ""LateDiffusionFeedback"": 0.59499990940093994,
  ""PostLowShelfGain"": 0.95999979972839355,
  ""PostLowShelfFrequency"": 0.23999994993209839,
  ""PostHighShelfGain"": 0.97000002861022949,
  ""PostHighShelfFrequency"": 0.72000002861022949,
  ""PostCutoffFrequency"": 0.87999981641769409,
  ""EarlyDiffusionModAmount"": 0.13499999046325684,
  ""EarlyDiffusionModRate"": 0.29000008106231689,
  ""LineModAmount"": 0.53999996185302734,
  ""LineModRate"": 0.44999989867210388,
  ""LateDiffusionModAmount"": 0.17499998211860657,
  ""LateDiffusionModRate"": 0.28500008583068848,
  ""TapSeed"": 0.00048499999684281647,
  ""DiffusionSeed"": 0.00020799999765586108,
  ""CombSeed"": 0.00033499998971819878,
  ""PostDiffusionSeed"": 0.00037200000951997936,
  ""CrossSeed"": 0.800000011920929,
  ""DryOut"": 1.0,
  ""PredelayOut"": 0.0,
  ""EarlyOut"": 0.8200000524520874,
  ""MainOut"": 0.90500003099441528,
  ""HiPassEnabled"": 1.0,
  ""LowPassEnabled"": 1.0,
  ""LowShelfEnabled"": 1.0,
  ""HighShelfEnabled"": 1.0,
  ""CutoffEnabled"": 1.0,
  ""LateStageTap"": 1.0,
  ""Interpolation"": 0.0
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
			
			var dict = JsonConvert.DeserializeObject<Dictionary<string, double>>(program2);
			//dict["DiffusionEnabled"] = 0.0;

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

					for (int i = 0; i < 1; i++)
					{
						controller.Process((IntPtr)ins, (IntPtr)outs, 64);
						inL[0] = 0.0;
						inR[0] = 0.0;
					}
				}
            }
			Console.WriteLine("Disposing");
			controller.Dispose();
		}
	}
}
