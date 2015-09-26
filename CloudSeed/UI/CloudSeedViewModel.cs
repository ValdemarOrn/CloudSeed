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

		public CloudSeedViewModel(CloudSeedPlugin plugin)
		{
			this.plugin = plugin;

            this.parameterUpdates = new Dictionary<Parameter, double>();
			NumberedParameters = new ObservableCollection<double>();
			foreach (var para in Enum.GetValues(typeof(Parameter)).Cast<Parameter>())
				NumberedParameters.Add(0.0);

			SaveProgramCommand = new DelegateCommand(name => SaveProgram(name.ToString()));
			LoadProgramCommand = new DelegateCommand(program => LoadProgram((ProgramBanks.PluginProgram?)program));
			DeleteProgramCommand = new DelegateCommand(_ => DeleteProgram());

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
			updateThread.IsBackground = true;
			updateThread.Priority = ThreadPriority.Lowest;
			updateThread.Start();
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
		public ICommand LoadProgramCommand { get; private set; }
		public ICommand DeleteProgramCommand { get; private set; }

		public ObservableCollection<double> NumberedParameters
		{
			get;
			private set;
		}

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

		private void LoadProgram(ProgramBanks.PluginProgram? programData)
		{
			if (programData == null)
				return;

			plugin.SetPluginProgram(programData.Value);
		}

		private void SaveProgram(string name)
		{
			var jsonData = plugin.GetJsonProgram();
			var newProg = ProgramBanks.Bank.SaveProgram(name, jsonData, true);

			if (newProg.HasValue)
			{
				NotifyChanged(() => UserPrograms);
				LoadProgram(newProg.Value);
			}
		}

		private void DeleteProgram()
		{
			ProgramBanks.Bank.DeleteProgram(SelectedProgram);
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
