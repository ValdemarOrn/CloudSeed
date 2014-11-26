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

		LineGain,
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
		
		DiffuserModAmount,
		DiffuserModRate,

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
		PostDiffusionOut,

		Count
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
