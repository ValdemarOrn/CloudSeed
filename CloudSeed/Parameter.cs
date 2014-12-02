using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public enum Parameter
	{
		// Input

		CrossMix = 0,     
		PreDelay,

		HighPass, 
		LowPass,

		// Early

		TapCount,
		TapLength,
		TapGain,
		TapDecay,

		DiffusionEnabled,
		DiffusionStages,
		DiffusionDelay,
		DiffusionFeedback,

		// Late

		LineCount,
		LineDelay,
		LineFeedback,

		PostDiffusionEnabled,
		PostDiffusionStages,
		PostDiffusionDelay,
		PostDiffusionFeedback,

		// Frequency Response

		PostLowShelfGain,
		PostLowShelfFrequency,
		PostHighShelfGain,
		PostHighShelfFrequency,
		PostCutoffFrequency,
		
		// Modulation
		
		DiffusionModAmount,
		DiffusionModRate,

		LineModAmount,
		LineModRate,

		// Seeds

		TapSeed,
		DiffusionSeed,
		CombSeed,
		PostDiffusionSeed,
		
		// Output

		StereoWidth,

		DryOut,
		PredelayOut,
		EarlyOut,
		LineOut,

		// Switches
		HiPassEnabled,
		LowPassEnabled,
		LowShelfEnabled,
		HighShelfEnabled,
		CutoffEnabled,

		Count,

		Unused = 999
	}

	public static class ParameterEnumExtensions
	{
		public static int Value(this Parameter para)
		{
			return (int)para;
		}

		public static string Name(this Parameter para)
		{
			return Enum.GetName(typeof(Parameter), para);
		}

		public static bool In(this Parameter para, params Parameter[] values)
		{
			return values.Contains(para);
		}
	}
}
