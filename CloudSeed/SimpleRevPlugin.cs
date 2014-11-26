using System;
using SharpSoundDevice;
using System.Globalization;
using CloudSeed.UI;

namespace CloudSeed
{
	public class SimpleRevPlugin : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		private DeviceInfo devInfo;

		private SimpleRev rev;
		private SimpleRevView view;
		private SimpleRevViewModel vm;

		public double Samplerate;
		public DeviceInfo DeviceInfo { get { return devInfo; } }
		public SharpSoundDevice.Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		public SimpleRevPlugin()
		{
			rev = new SimpleRev();
			rev.Samplerate = 48000;
			Samplerate = 48000;
			devInfo = new DeviceInfo();
			ParameterInfo = new SharpSoundDevice.Parameter[rev.Parameters.Length];
			PortInfo = new Port[2];
			//vm = new SimpleRevViewModel(rev);
		}

		public void InitializeDevice()
		{
			devInfo.DeviceID = "Low Profile - Cloud Seed";
			devInfo.Developer = "Valdemar Erlingsson";
			devInfo.EditorWidth = 950;
			devInfo.EditorHeight = 500;
			devInfo.HasEditor = false;
			devInfo.Name = "Cloud Seed Algorithmic Reverb";
			devInfo.ProgramCount = 1;
			devInfo.Type = DeviceType.Effect;
			devInfo.Version = 1000;
			devInfo.VstId = DeviceUtilities.GenerateIntegerId(devInfo.DeviceID);

			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Stereo Input";
			PortInfo[0].NumberOfChannels = 2;

			PortInfo[1].Direction = PortDirection.Output;
			PortInfo[1].Name = "Stereo Output";
			PortInfo[1].NumberOfChannels = 2;

			for (int i = 0; i < rev.Parameters.Length; i++)
			{
				var p = new SharpSoundDevice.Parameter();
				p.Display = GetDisplay(0.0);
				p.Index = (uint)i;
				p.Name = i < Parameter.Count.Value() ? ((Parameter)i).Name() : "Parameter " + i;
				p.Steps = 0;
				p.Value = 0.0;
				ParameterInfo[i] = p;
			}
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			rev.Process(input, output);
		}

		public void OpenEditor(IntPtr parentWindow) 
		{
			view = new SimpleRevView(vm);
			devInfo.EditorWidth = (int)view.Width;
			devInfo.EditorHeight = (int)view.Height;
			HostInfo.SendEvent(this, new Event { Data = null, EventIndex = 0, Type = EventType.WindowSize });
			DeviceUtilities.DockWpfWindow(view, parentWindow);
			view.Show();
		}

		public void CloseEditor() 
		{
			view.Close();
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				if (ev.EventIndex >= 0 && ev.EventIndex < rev.Parameters.Length)
				{
					var i = ev.EventIndex;
					var value = (double)ev.Data;
					rev.SetParameter((Parameter)i, value);
					ParameterInfo[i].Value = value;
					ParameterInfo[i].Display = GetDisplay((Parameter)i);
				}
			}
		}

		public string GetDisplay(Parameter param)
		{
			var propVal = rev.GetType().GetProperty(param.ToString()).GetValue(rev);
			return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", propVal);
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
			if (samplerate != Samplerate)
			{
				Samplerate = samplerate;
				rev.Samplerate = samplerate;
			}
		}

		public IHostInfo HostInfo { get; set; }

	}
}

