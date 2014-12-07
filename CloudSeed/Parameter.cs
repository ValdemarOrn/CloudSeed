using System;
using System.Collections.Generic;
using System.Globalization;
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
		MainOut,

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

		public static string NameWithSpaces(this Parameter para)
		{
			var name = Enum.GetName(typeof(Parameter), para);
			var chars = new List<char>();
			foreach (var ch in name)
			{
				if (Char.IsUpper(ch))
					chars.Add(' ');

				chars.Add(ch);
			}

			return new string(chars.ToArray()).Trim();
		}

		public static bool In(this Parameter para, params Parameter[] values)
		{
			return values.Contains(para);
		}

		public static Func<double, string> Formatter(this Parameter para)
		{
			return Formatters[para];
		}

		private static readonly Func<double, string> DecimalFormatter = x => x.ToString("0.00", CultureInfo.InvariantCulture);
		private static readonly Func<double, string> IntFormatter = x => x.ToString("0", CultureInfo.InvariantCulture);
		private static readonly Func<double, string> MillisFormatter = x => x.ToString("0", CultureInfo.InvariantCulture) + " ms";
		private static readonly Func<double, string> FrequencyFormatter = x => x.ToString("0", CultureInfo.InvariantCulture) + " Hz";
		private static readonly Func<double, string> FrequencyDecimalFormatter = x => x.ToString("0.00", CultureInfo.InvariantCulture) + " Hz";
		private static readonly Func<double, string> OnOffFormatter = x => x >= 0.5 ? "On" : "Off";
		private static readonly Func<double, string> DbFormatter = x =>
		{
			var val = AudioLib.Utils.Gain2DB(x);
			if (double.IsInfinity(val))
				return "None";
			else
				return val.ToString("0.00", CultureInfo.InvariantCulture) + " dB";
		};

		private static readonly Dictionary<Parameter, Func<double, string>> Formatters = new Dictionary<Parameter, Func<double, string>>
		{
			{ Parameter.CrossMix, DecimalFormatter },
			{ Parameter.PreDelay, MillisFormatter },

			{ Parameter.HighPass,  FrequencyFormatter },
			{ Parameter.LowPass, FrequencyFormatter },

			{ Parameter.TapCount, IntFormatter },
			{ Parameter.TapLength, MillisFormatter },
			{ Parameter.TapGain, DbFormatter },
			{ Parameter.TapDecay, DecimalFormatter },

			{ Parameter.DiffusionEnabled, OnOffFormatter },
			{ Parameter.DiffusionStages, IntFormatter },
			{ Parameter.DiffusionDelay, MillisFormatter },
			{ Parameter.DiffusionFeedback, DecimalFormatter },

			{ Parameter.LineCount, IntFormatter },
			{ Parameter.LineDelay, MillisFormatter },
			{ Parameter.LineFeedback, DecimalFormatter },

			{ Parameter.PostDiffusionEnabled, OnOffFormatter },
			{ Parameter.PostDiffusionStages, IntFormatter },
			{ Parameter.PostDiffusionDelay, MillisFormatter },
			{ Parameter.PostDiffusionFeedback, DecimalFormatter },

			{ Parameter.PostLowShelfGain, DbFormatter },
			{ Parameter.PostLowShelfFrequency, FrequencyFormatter },
			{ Parameter.PostHighShelfGain, DbFormatter },
			{ Parameter.PostHighShelfFrequency, FrequencyFormatter },
			{ Parameter.PostCutoffFrequency, FrequencyFormatter },

			{ Parameter.DiffusionModAmount, DecimalFormatter },
			{ Parameter.DiffusionModRate, FrequencyDecimalFormatter },
			{ Parameter.LineModAmount, DecimalFormatter },
			{ Parameter.LineModRate, FrequencyDecimalFormatter },

			{ Parameter.TapSeed, IntFormatter },
			{ Parameter.DiffusionSeed, IntFormatter },
			{ Parameter.CombSeed, IntFormatter },
			{ Parameter.PostDiffusionSeed, IntFormatter },

			{ Parameter.StereoWidth, DecimalFormatter },

			{ Parameter.DryOut, DbFormatter },
			{ Parameter.PredelayOut, DbFormatter },
			{ Parameter.EarlyOut, DbFormatter },
			{ Parameter.MainOut, DbFormatter },

			{ Parameter.HiPassEnabled, OnOffFormatter },
			{ Parameter.LowPassEnabled, OnOffFormatter },
			{ Parameter.LowShelfEnabled, OnOffFormatter },
			{ Parameter.HighShelfEnabled, OnOffFormatter },
			{ Parameter.CutoffEnabled, OnOffFormatter }
		};
	}
}
