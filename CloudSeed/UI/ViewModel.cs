using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CloudSeed.UI
{
	public class ViewModel : INotifyPropertyChanged
	{
		public CloudSeedPlugin Plugin { get; private set; }

		public ViewModel(CloudSeedPlugin plugin)
		{
			Plugin = plugin;
		}

		public double PredelayLeft
		{
			get { return Plugin.GetParameterBase(ParameterEnum.PredelayLeft); }
			set { Plugin.SetParameterBase(ParameterEnum.PredelayLeft, value); NotifyChanged(() => PredelayLeft); }
		}

		public double EarlySizeLeft
		{
			get { return Plugin.GetParameterBase(ParameterEnum.EarlySizeLeft); }
			set { Plugin.SetParameterBase(ParameterEnum.EarlySizeLeft, value); NotifyChanged(() => EarlySizeLeft); }
		}

		public double GrainCountLeft
		{
			get { return Plugin.GetParameterBase(ParameterEnum.GrainCountLeft); }
			set { Plugin.SetParameterBase(ParameterEnum.GrainCountLeft, value); NotifyChanged(() => GrainCountLeft); }
		}

		public double PredelayRight
		{
			get { return Plugin.GetParameterBase(ParameterEnum.PredelayRight); }
			set { Plugin.SetParameterBase(ParameterEnum.PredelayRight, value); NotifyChanged(() => PredelayRight); }
		}

		public double EarlySizeRight
		{
			get { return Plugin.GetParameterBase(ParameterEnum.EarlySizeRight); }
			set { Plugin.SetParameterBase(ParameterEnum.EarlySizeRight, value); NotifyChanged(() => EarlySizeRight); }
		}

		public double GrainCountRight
		{
			get { return Plugin.GetParameterBase(ParameterEnum.GrainCountRight); }
			set { Plugin.SetParameterBase(ParameterEnum.GrainCountRight, value); NotifyChanged(() => GrainCountRight); }
		}



		public double EarlySeedLeft
		{
			get { return Plugin.GetParameter(ParameterEnum.EarlySeedLeft); }
			set { Plugin.SetParameterBase(ParameterEnum.EarlySeedLeft, value / 1023.999); NotifyChanged(() => EarlySeedLeft); }
		}

		public double LateSeedLeft
		{
			get { return Plugin.GetParameter(ParameterEnum.LateSeedLeft); }
			set { Plugin.SetParameterBase(ParameterEnum.LateSeedLeft, value / 1023.999); NotifyChanged(() => LateSeedLeft); }
		}

		public double EarlySeedRight
		{
			get { return Plugin.GetParameter(ParameterEnum.EarlySeedRight); }
			set { Plugin.SetParameterBase(ParameterEnum.EarlySeedRight, value / 1023.999); NotifyChanged(() => EarlySeedRight); }
		}

		public double LateSeedRight
		{
			get { return Plugin.GetParameter(ParameterEnum.LateSeedRight); }
			set { Plugin.SetParameterBase(ParameterEnum.LateSeedRight, value / 1023.999); NotifyChanged(() => LateSeedRight); }
		}



		public double StageCount
		{
			get { return 1 + Plugin.GetParameterBase(ParameterEnum.StageCount) * 7.9999; }
			set { Plugin.SetParameterBase(ParameterEnum.StageCount, (value - 1) / 7.999); NotifyChanged(() => StageCount); }
		}

		public double Parallel
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Parallel); }
			set { Plugin.SetParameterBase(ParameterEnum.Parallel, value); NotifyChanged(() => Parallel); }
		}

		

		public double Delay1
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Delay1); }
			set { Plugin.SetParameterBase(ParameterEnum.Delay1, value); NotifyChanged(() => Delay1); }
		}

		public double Delay2
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Delay2); }
			set { Plugin.SetParameterBase(ParameterEnum.Delay2, value); NotifyChanged(() => Delay2); }
		}

		public double Delay3
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Delay3); }
			set { Plugin.SetParameterBase(ParameterEnum.Delay3, value); NotifyChanged(() => Delay3); }
		}

		public double Feedback1
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Feedback1); }
			set { Plugin.SetParameterBase(ParameterEnum.Feedback1, value); NotifyChanged(() => Feedback1); }
		}

		public double Feedback2
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Feedback2); }
			set { Plugin.SetParameterBase(ParameterEnum.Feedback2, value); NotifyChanged(() => Feedback2); }
		}

		public double Feedback3
		{
			get { return Plugin.GetParameterBase(ParameterEnum.Feedback3); }
			set { Plugin.SetParameterBase(ParameterEnum.Feedback3, value); NotifyChanged(() => Feedback3); }
		}

		public double ModRate1
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModRate1); }
			set { Plugin.SetParameterBase(ParameterEnum.ModRate1, value); NotifyChanged(() => ModRate1); }
		}

		public double ModRate2
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModRate2); }
			set { Plugin.SetParameterBase(ParameterEnum.ModRate2, value); NotifyChanged(() => ModRate2); }
		}

		public double ModRate3
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModRate3); }
			set { Plugin.SetParameterBase(ParameterEnum.ModRate3, value); NotifyChanged(() => ModRate3); }
		}

		public double ModAmt1
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModAmt1); }
			set { Plugin.SetParameterBase(ParameterEnum.ModAmt1, value); NotifyChanged(() => ModAmt1); }
		}

		public double ModAmt2
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModAmt2); }
			set { Plugin.SetParameterBase(ParameterEnum.ModAmt2, value); NotifyChanged(() => ModAmt2); }
		}

		public double ModAmt3
		{
			get { return Plugin.GetParameterBase(ParameterEnum.ModAmt3); }
			set { Plugin.SetParameterBase(ParameterEnum.ModAmt3, value); NotifyChanged(() => ModAmt3); }
		}

		public double FreqMid
		{
			get { return Plugin.GetParameterBase(ParameterEnum.FreqMid); }
			set { Plugin.SetParameterBase(ParameterEnum.FreqMid, value); NotifyChanged(() => FreqMid); }
		}

		public double GainMid
		{
			get { return Plugin.GetParameterBase(ParameterEnum.GainMid); }
			set { Plugin.SetParameterBase(ParameterEnum.GainMid, value); NotifyChanged(() => GainMid); }
		}

		public double QMid
		{
			get { return Plugin.GetParameterBase(ParameterEnum.QMid); }
			set { Plugin.SetParameterBase(ParameterEnum.QMid, value); NotifyChanged(() => QMid); }
		}

		public double FreqLow
		{
			get { return Plugin.GetParameterBase(ParameterEnum.FreqLow); }
			set { Plugin.SetParameterBase(ParameterEnum.FreqLow, value); NotifyChanged(() => FreqLow); }
		}

		public double GainLow
		{
			get { return Plugin.GetParameterBase(ParameterEnum.GainLow); }
			set { Plugin.SetParameterBase(ParameterEnum.GainLow, value); NotifyChanged(() => GainLow); }
		}

		public double FreqHi
		{
			get { return Plugin.GetParameterBase(ParameterEnum.FreqHi); }
			set { Plugin.SetParameterBase(ParameterEnum.FreqHi, value); NotifyChanged(() => FreqHi); }
		}

		public double GainHi
		{
			get { return Plugin.GetParameterBase(ParameterEnum.GainHi); }
			set { Plugin.SetParameterBase(ParameterEnum.GainHi, value); NotifyChanged(() => GainHi); }
		}

		public double EarlyOut
		{
			get { return Plugin.GetParameterBase(ParameterEnum.EarlyOut); }
			set { Plugin.SetParameterBase(ParameterEnum.EarlyOut, value); NotifyChanged(() => EarlyOut); }
		}

		public double WetOut
		{
			get { return Plugin.GetParameterBase(ParameterEnum.WetOut); }
			set { Plugin.SetParameterBase(ParameterEnum.WetOut, value); NotifyChanged(() => WetOut); }
		}

		public double DryOut
		{
			get { return Plugin.GetParameterBase(ParameterEnum.DryOut); }
			set { Plugin.SetParameterBase(ParameterEnum.DryOut, value); NotifyChanged(() => DryOut); }
		}

		public double AllpassDelay
		{
			get { return Plugin.GetParameterBase(ParameterEnum.AllpassDelay); }
			set { Plugin.SetParameterBase(ParameterEnum.AllpassDelay, value); NotifyChanged(() => AllpassDelay); }
		}

		public double AllpassFeedback
		{
			get { return Plugin.GetParameterBase(ParameterEnum.AllpassFeedback); }
			set { Plugin.SetParameterBase(ParameterEnum.AllpassFeedback, value); NotifyChanged(() => AllpassFeedback); }
		}

		public double AllpassModAmount
		{
			get { return Plugin.GetParameterBase(ParameterEnum.AllpassModAmount); }
			set { Plugin.SetParameterBase(ParameterEnum.AllpassModAmount, value); NotifyChanged(() => AllpassModAmount); }
		}

		public double AllpassModRate
		{
			get { return Plugin.GetParameterBase(ParameterEnum.AllpassModRate); }
			set { Plugin.SetParameterBase(ParameterEnum.AllpassModRate, value); NotifyChanged(() => AllpassModRate); }
		}

		public double HiCut
		{
			get { return Plugin.GetParameterBase(ParameterEnum.HiCut); }
			set { Plugin.SetParameterBase(ParameterEnum.HiCut, value); NotifyChanged(() => HiCut); }
		}

		public double LowCut
		{
			get { return Plugin.GetParameterBase(ParameterEnum.LowCut); }
			set { Plugin.SetParameterBase(ParameterEnum.LowCut, value); NotifyChanged(() => LowCut); }
		}

		public double HiCutAmount
		{
			get { return Plugin.GetParameterBase(ParameterEnum.HiCutAmount); }
			set { Plugin.SetParameterBase(ParameterEnum.HiCutAmount, value); NotifyChanged(() => HiCutAmount); }
		}

		public double LowCutAmount
		{
			get { return Plugin.GetParameterBase(ParameterEnum.LowCutAmount); }
			set { Plugin.SetParameterBase(ParameterEnum.LowCutAmount, value); NotifyChanged(() => LowCutAmount); }
		}


		public void Update(ParameterEnum parameter)
		{
			NotifyChanged(parameter.ToString());
		}

		#region Notify Change

		public event PropertyChangedEventHandler PropertyChanged;

		// I used this and GetPropertyName to avoid having to hard-code property names
		// into the NotifyChange events. This makes the application much easier to refactor
		// leter on, if needed.
		private void NotifyChanged<T>(System.Linq.Expressions.Expression<Func<T>> exp)
		{
			var name = GetPropertyName(exp);
			NotifyChanged(name);
		}

		private void NotifyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
		}

		private static string GetPropertyName<T>(System.Linq.Expressions.Expression<Func<T>> exp)
		{
			return (((System.Linq.Expressions.MemberExpression)(exp.Body)).Member).Name;
		}

		#endregion
	}
}
