using AudioLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	/*public class ParameterTransform
	{
		public static Dictionary<ParameterEnum, Func<double, double>> Transforms;

		static ParameterTransform()
		{
			Transforms = new Dictionary<ParameterEnum, Func<double, double>>();

			Transforms[ParameterEnum.PredelayLeft] = x => x * 100;
			Transforms[ParameterEnum.EarlySizeLeft] = x => x * 500;
			Transforms[ParameterEnum.GrainCountLeft] = x => x * 200;
			Transforms[ParameterEnum.PredelayRight] = x => x * 100;
			Transforms[ParameterEnum.EarlySizeRight] = x => x * 500;
			Transforms[ParameterEnum.GrainCountRight] = x => x * 200;

			Transforms[ParameterEnum.EarlySeedLeft] = x => Math.Floor(x * 1023.999);
			Transforms[ParameterEnum.LateSeedLeft] = x => Math.Floor(x * 1023.999);
			Transforms[ParameterEnum.EarlySeedRight] = x => Math.Floor(x * 1023.999);
			Transforms[ParameterEnum.LateSeedRight] = x => Math.Floor(x * 1023.999);
			Transforms[ParameterEnum.StageCount] = x => Math.Floor(1 + x * 7.999);

			Transforms[ParameterEnum.AllpassDelay] = x => x * 100;
			Transforms[ParameterEnum.AllpassModRate] = x => x * 2;
			Transforms[ParameterEnum.AllpassModAmount] = x => x * 2;

			Transforms[ParameterEnum.HiCut] = x => ValueTables.Get(x, ValueTables.Response3Oct) * 20000;
			Transforms[ParameterEnum.LowCut] = x => ValueTables.Get(x, ValueTables.Response3Oct) * 2000;

			Transforms[ParameterEnum.Delay1] = x => x * 500;
			Transforms[ParameterEnum.Delay2] = x => x * 500;
			Transforms[ParameterEnum.Delay3] = x => x * 500;
			Transforms[ParameterEnum.ModRate1] = x => x * 2;
			Transforms[ParameterEnum.ModRate2] = x => x * 2;
			Transforms[ParameterEnum.ModRate3] = x => x * 2;

			Transforms[ParameterEnum.FreqHi] = x => ValueTables.Get(x, ValueTables.Response3Oct) * 20000;
			Transforms[ParameterEnum.FreqMid] = x => ValueTables.Get(x, ValueTables.Response3Oct) * 20000;
			Transforms[ParameterEnum.FreqLow] = x => ValueTables.Get(x, ValueTables.Response3Oct) * 2000;
			
		}
	}*/
}
