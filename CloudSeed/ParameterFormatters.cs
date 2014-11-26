using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class ParameterFormatters
	{
		/*public static Dictionary<ParameterEnum, Func<double, string>> Formatters;

		static ParameterFormatters()
		{
			Formatters = new Dictionary<ParameterEnum, Func<double, string>>();

			Formatters[ParameterEnum.PredelayLeft] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.EarlySizeLeft] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.GrainCountLeft] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.PredelayRight] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.EarlySizeRight] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.GrainCountRight] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);

			Formatters[ParameterEnum.EarlySeedLeft] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.LateSeedLeft] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.EarlySeedRight] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.LateSeedRight] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.StageCount] = x => String.Format(CultureInfo.InvariantCulture, "{0:0}", x);
			Formatters[ParameterEnum.Parallel] = x => String.Format(CultureInfo.InvariantCulture, "{0}", x > 0.5 ? "On" : "Off");


			Formatters[ParameterEnum.AllpassDelay] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.AllpassFeedback] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.AllpassModRate] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.AllpassModAmount] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);

			Formatters[ParameterEnum.HiCut] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.HiCutAmount] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.LowCut] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.LowCutAmount] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));

			Formatters[ParameterEnum.Delay1] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.Delay2] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.Delay3] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} ms", x);
			Formatters[ParameterEnum.Feedback1] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.Feedback2] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.Feedback3] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.ModRate1] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.ModRate2] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.ModRate3] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.ModAmt1] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.ModAmt2] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			Formatters[ParameterEnum.ModAmt3] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);


			Formatters[ParameterEnum.FreqLow] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.FreqHi] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.FreqMid] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} Hz", x);
			Formatters[ParameterEnum.GainLow] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.GainHi] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.GainMid] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.QMid] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);


			Formatters[ParameterEnum.EarlyOut] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.WetOut] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
			Formatters[ParameterEnum.DryOut] = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", AudioLib.Utils.Gain2DB(x));
		}*/
	}
}
