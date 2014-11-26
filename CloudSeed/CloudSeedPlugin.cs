using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;
using System.Globalization;
using CloudSeed.UI;

namespace CloudSeed
{/*
	public class CloudSeedPlugin : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public double Samplerate;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		private ReverberWrapper RevLeft, RevRight;
		private Dictionary<ParameterEnum, double> Parameters;
		public EffectView View;
		public ViewModel VM;

		public CloudSeedPlugin()
		{
			RevLeft = new ReverberWrapper();
			RevRight = new ReverberWrapper();
			RevLeft.SetSamplerate(48000);
			RevRight.SetSamplerate(48000);
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[ParameterNames.Names.Count];
			Parameters = new Dictionary<ParameterEnum, double>();
			PortInfo = new Port[2];
			VM = new ViewModel(this);
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Cloud Seed";
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorWidth = 950;
			DevInfo.EditorHeight = 500;
			DevInfo.HasEditor = true;
			DevInfo.Name = "Cloud Seed Algorithmic Reverb";
			DevInfo.ProgramCount = 1;
			DevInfo.Type = DeviceType.Effect;
			DevInfo.Version = 1000;
			DevInfo.VstId = DeviceUtilities.GenerateIntegerId(DevInfo.DeviceID);

			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Stereo Input";
			PortInfo[0].NumberOfChannels = 2;

			PortInfo[1].Direction = PortDirection.Output;
			PortInfo[1].Name = "Stereo Output";
			PortInfo[1].NumberOfChannels = 2;

			for (int i = 0; i < (int)ParameterEnum.COUNT; i++)
			{
				var p = new Parameter();
				p.Display = GetDisplay((ParameterEnum)i);
				p.Index = (uint)i;
				p.Name = ParameterNames.Names[(ParameterEnum)i];
				p.Steps = 0;
				p.Value = GetParameter((ParameterEnum)i);
				ParameterInfo[i] = p;
			}
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			RevLeft.Process(input[0], output[0]);
			RevRight.Process(input[1], output[1]);
		}

		public void OpenEditor(IntPtr parentWindow) 
		{
			View = new EffectView(VM);
			DevInfo.EditorWidth = (int)View.Width;
			DevInfo.EditorHeight = (int)View.Height;
			HostInfo.SendEvent(this, new Event() { Data = null, EventIndex = 0, Type = EventType.WindowSize });
			DeviceUtilities.DockWpfWindow(View, parentWindow);
			View.Show();
		}

		public void CloseEditor() 
		{
			View.Close();
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				if (ev.EventIndex >= 0 && ev.EventIndex < (int)ParameterEnum.COUNT)
				{
					var parameter = (ParameterEnum)ev.EventIndex;
					var value = (double)ev.Data;
					SetParameterBase(parameter, value, false);
				}
			}
		}

		public string GetDisplay(ParameterEnum parameter)
		{
			Func<double, string> formatter = x => String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
			ParameterFormatters.Formatters.TryGetValue(parameter, out formatter);
			var para = GetParameter(parameter);
			return formatter(para);
		}

		public double GetParameter(ParameterEnum parameter)
		{
			double value;
			bool ok = Parameters.TryGetValue(parameter, out value);
			return ok ? value : 0.0;
		}

		public double GetParameterBase(ParameterEnum parameter)
		{
			return ParameterInfo[(int)parameter].Value;
		}

		/// <summary>
		/// Set a parameter in terms of range 0...1
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		public void SetParameterBase(ParameterEnum parameter, double valueInput, bool updateHost = true)
		{
			ParameterInfo[(int)parameter].Value = valueInput;

			double value = valueInput;
			if (ParameterTransform.Transforms.ContainsKey(parameter))
				value = ParameterTransform.Transforms[parameter](valueInput);

			Parameters[parameter] = value;
			ParameterInfo[(int)parameter].Display = GetDisplay(parameter);

			RevLeft.SetParameter(parameter, value);
			RevRight.SetParameter(parameter, value);

			if (new[] { ParameterEnum.EarlySizeLeft, ParameterEnum.EarlySizeRight, 
				ParameterEnum.GrainCountLeft, ParameterEnum.GrainCountRight,
				ParameterEnum.PredelayLeft, ParameterEnum.PredelayRight}.Contains(parameter))
			{
				RevLeft.SetTaps((int)GetParameter(ParameterEnum.GrainCountLeft));
				RevRight.SetTaps((int)GetParameter(ParameterEnum.GrainCountRight));
			}

			if (parameter == ParameterEnum.AllpassFeedback || parameter == ParameterEnum.AllpassDelay)
			{
				var samples = (int)(GetParameter(ParameterEnum.AllpassDelay) / 1000.0 * Samplerate);
				RevLeft.SetEarly(GetParameter(ParameterEnum.AllpassFeedback), samples);
				RevRight.SetEarly(GetParameter(ParameterEnum.AllpassFeedback), samples);
			}

			if (parameter == ParameterEnum.AllpassModRate || parameter == ParameterEnum.AllpassModAmount)
			{
				RevLeft.SetAllpassMod(GetParameter(ParameterEnum.AllpassModRate), GetParameter(ParameterEnum.AllpassModAmount));
				RevRight.SetAllpassMod(GetParameter(ParameterEnum.AllpassModRate), GetParameter(ParameterEnum.AllpassModAmount));
			}

			if (parameter == ParameterEnum.HiCut || parameter == ParameterEnum.HiCutAmount)
			{
				RevLeft.SetHiCut(GetParameter(ParameterEnum.HiCut), GetParameter(ParameterEnum.HiCutAmount));
				RevRight.SetHiCut(GetParameter(ParameterEnum.HiCut), GetParameter(ParameterEnum.HiCutAmount));
			}

			System.Windows.Forms.Cursor.Show();
			VM.Update(parameter);
			if (updateHost)
			{
				HostInfo.SendEvent(this, new Event
				{
					Data = valueInput,
					EventIndex = (int)parameter,
					Type = EventType.Parameter
				});
			}
		}

		public void SetProgramData(Program program, int index)
		{
			try
			{
				DeviceUtilities.DeserializeParameters(ParameterInfo, program.Data);
				for (int i = 0; i < ParameterInfo.Length; i++)
					SendEvent(new Event { EventIndex = i, Type = EventType.Parameter, Data = ParameterInfo[i].Value });
			}
			catch (Exception)
			{

			}
		}

		public Program GetProgramData(int index)
		{
			var output = new Program();
			output.Data = DeviceUtilities.SerializeParameters(ParameterInfo);
			output.Name = "";
			return output;
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != this.Samplerate)
			{
				Samplerate = samplerate;
				RevLeft.SetSamplerate(samplerate);
				RevRight.SetSamplerate(samplerate);
			}
		}

		public IHostInfo HostInfo { get; set; }

	}*/
}

