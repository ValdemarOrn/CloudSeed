using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace CloudSeed.UI
{
	public class CloudSeedViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private readonly object updateLock = new object();
		private readonly CloudSeedPlugin plugin;
		private readonly Thread updateThread;
		private readonly Dictionary<Parameter, double> parameterUpdates;

		private volatile bool suppressUpdates;
		private Parameter? activeControl;
		private ProgramBanks.PluginProgram selectedProgram;
		private string newProgramName;
		private System.Windows.FontWeight[] fontWeights;

		private ReductionEffect reductionResolution;
		private ReductionEffect reductionUndersampling;
		private ReductionEffect reductionInterpolation;

		public CloudSeedViewModel(CloudSeedPlugin plugin)
		{
			this.plugin = plugin;

			fontWeights = new System.Windows.FontWeight[9];
			SetReductionEffect(ReductionEffect.ResolutionFull.ToString());
			SetReductionEffect(ReductionEffect.UndersamplingOff.ToString());
			SetReductionEffect(ReductionEffect.InterpolationEnabled.ToString());
			
            this.parameterUpdates = new Dictionary<Parameter, double>();
			NumberedParameters = new ObservableCollection<double>();
			foreach (var para in Enum.GetValues(typeof(Parameter)).Cast<Parameter>())
				NumberedParameters.Add(0.0);

			SaveProgramCommand = new DelegateCommand(x => SaveProgram());
			RenameProgramCommand = new DelegateCommand(x => RenameProgram());
			LoadProgramCommand = new DelegateCommand(LoadProgram);
			SetReductionEffectCommand = new DelegateCommand(x => SetReductionEffect(x.ToString()));

			NumberedParameters.CollectionChanged += (s, e) =>
			{
				if (suppressUpdates)
					return;

				lock (updateLock)
				{
					var para = (Parameter)e.NewStartingIndex;
					var val = (double)e.NewItems[0];
					plugin.SetParameter(para, val);
					NotifyChanged(() => ActiveControlDisplay);
				}
			};

			updateThread = new Thread(UpdateParameters);
			updateThread.Priority = ThreadPriority.Lowest;
			updateThread.Start();
			
			LoadProgram(ProgramBanks.Bank.UserPrograms.FirstOrDefault() ?? new ProgramBanks.PluginProgram { Name = "Default Program" });
		}

		private void SetReductionEffect(string value)
		{
			var type = (ReductionEffect)Enum.Parse(typeof(ReductionEffect), value);

			if (value.Contains("Resolution"))
			{
				fontWeights[(int)ReductionEffect.ResolutionBit8] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.ResolutionBit12] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.ResolutionFull] = System.Windows.FontWeights.Normal;

				reductionResolution = type;
				var val = (int)type - (int)ReductionEffect.ResolutionBit8;
				plugin.SetParameter(Parameter.SampleResolution, val / 2.0);
			}
			else if (value.Contains("Undersampling"))
			{
				fontWeights[(int)ReductionEffect.Undersampling8x] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.Undersampling4x] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.Undersampling2x] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.UndersamplingOff] = System.Windows.FontWeights.Normal;
				
				reductionUndersampling = type;
				var val = (int)type - (int)ReductionEffect.UndersamplingOff;
				plugin.SetParameter(Parameter.Undersampling, val / 3.0);

			}
			else if (value.Contains("Interpolation"))
			{
				fontWeights[(int)ReductionEffect.InterpolationDisabled] = System.Windows.FontWeights.Normal;
				fontWeights[(int)ReductionEffect.InterpolationEnabled] = System.Windows.FontWeights.Normal;

				reductionInterpolation = type;
				var val = (int)type - (int)ReductionEffect.InterpolationDisabled;
				plugin.SetParameter(Parameter.Interpolation, val);
			}

			fontWeights[(int)type] = System.Windows.FontWeights.Bold;
			FontWeights = fontWeights.Select(x => x).ToArray();
			NotifyChanged(() => ReductionEffectsString);
		}

		private void UpdateParameters()
		{
			var toProcess = new List<KeyValuePair<Parameter, double>>();

			while (true)
			{
				Thread.Sleep(50);
				toProcess.Clear();

				lock (parameterUpdates)
				{
					toProcess.AddRange(parameterUpdates);
					parameterUpdates.Clear();
				}

				if (toProcess.Count == 0)
					continue;

				lock (updateLock)
				{
					foreach (var tuple in toProcess)
						UpdateParameter(tuple.Key, tuple.Value);
				}
			}
		}

		public ICommand SaveProgramCommand { get; private set; }
		public ICommand RenameProgramCommand { get; private set; }
		public ICommand LoadProgramCommand { get; private set; }
		public ICommand SetReductionEffectCommand { get; private set; }

        public System.Windows.FontWeight[] FontWeights
		{
			get { return fontWeights; }
			set { fontWeights = value; NotifyChanged(); }
		}

		public string ReductionEffectsString
		{
			get
			{
				var parts = new List<string>();

				if (reductionResolution == ReductionEffect.ResolutionBit12)
					parts.Add("12Bit");
				if (reductionResolution == ReductionEffect.ResolutionBit8)
					parts.Add("8Bit");

				if (reductionUndersampling == ReductionEffect.Undersampling2x)
					parts.Add("2x");
				if (reductionUndersampling == ReductionEffect.Undersampling4x)
					parts.Add("4x");
				if (reductionUndersampling == ReductionEffect.Undersampling8x)
					parts.Add("8x");

				if (reductionInterpolation == ReductionEffect.InterpolationDisabled)
					parts.Add("No Interp.");

				if (parts.Any())
					return string.Join(", ", parts);
				else
					return "No Effects";
			}
		}

		public ObservableCollection<double> NumberedParameters { get; private set; }

		public Parameter? ActiveControl
		{
			get { return activeControl; }
			set
			{
				activeControl = value; 
				NotifyChanged(() => ActiveControlName);
				NotifyChanged(() => ActiveControlDisplay);
			}
		}

		public string ActiveControlName
		{
			get { return ActiveControl.HasValue ? ActiveControl.Value.NameWithSpaces() : ""; }
		}

		public string ActiveControlDisplay
		{
			get { return ActiveControl.HasValue ? plugin.GetDisplay(ActiveControl.Value) : ""; }
		}

		public ProgramBanks.PluginProgram[] FactoryPrograms
		{
			get { return ProgramBanks.Bank.FactoryPrograms.ToArray(); }
		}

		public ProgramBanks.PluginProgram[] UserPrograms
		{
			get { return ProgramBanks.Bank.UserPrograms.ToArray(); }
		}

		public ProgramBanks.PluginProgram SelectedProgram
		{
			get { return selectedProgram; }
			set { selectedProgram = value; NotifyChanged(); }
		}

		public string NewProgramName
		{
			get { return newProgramName; }
			set { newProgramName = value; NotifyChanged(); }
		}

		public void UpdateParameterAsync(Parameter param, double newValue)
		{
			lock (parameterUpdates)
			{
				parameterUpdates[param] = newValue;
			}
		}

		private void UpdateParameter(Parameter param, double newValue)
		{
			lock (updateLock)
			{
				suppressUpdates = true;
				NumberedParameters[param.Value()] = newValue;

				suppressUpdates = false;
				NotifyChanged(() => NumberedParameters);
			}
		}

		private void LoadProgram(object obj)
		{
			var programData = obj as ProgramBanks.PluginProgram;
			if (programData == null)
				return;

			if (programData.Data != null)
				plugin.SetJsonProgram(programData.Data);

			SelectedProgram = programData;
		}

		private void SaveProgram()
		{
			var jsonData = plugin.GetJsonProgram();
			ProgramBanks.Bank.SaveProgram(NewProgramName, jsonData, true);
			NotifyChanged(() => UserPrograms);
		}

		private void RenameProgram()
		{
			NotifyChanged(() => UserPrograms);
		}

		#region Notify Change

		// I used this and GetPropertyName to avoid having to hard-code property names
		// into the NotifyChange events. This makes the application much easier to refactor
		// leter on, if needed.
		private void NotifyChanged<T>(System.Linq.Expressions.Expression<Func<T>> exp)
		{
			var name = GetPropertyName(exp);
			NotifyChanged(name);
		}

		private void NotifyChanged([CallerMemberName]string property = null)
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
