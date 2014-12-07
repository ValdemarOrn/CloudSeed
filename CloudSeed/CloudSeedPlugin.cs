using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using SharpSoundDevice;
using System.Globalization;
using CloudSeed.UI;

namespace CloudSeed
{
	public class CloudSeedPlugin : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		private DeviceInfo devInfo;

		private readonly ReverbController controller;
		private CloudSeedView view;
		private System.Windows.Window window; 
		
		public double Samplerate;
		public DeviceInfo DeviceInfo { get { return devInfo; } }
		public SharpSoundDevice.Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		public CloudSeedPlugin()
		{
			controller = new ReverbController(48000);
			controller.Samplerate = 48000;
			Samplerate = 48000;
			devInfo = new DeviceInfo();
			ParameterInfo = new SharpSoundDevice.Parameter[controller.Parameters.Length];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			devInfo.DeviceID = "Low Profile - CloudSeed";
			devInfo.Developer = "Valdemar Erlingsson";
			devInfo.EditorWidth = 936;
			devInfo.EditorHeight = 346;
			devInfo.HasEditor = true;
			devInfo.Name = "CloudSeed Algorithmic Reverb";
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

			for (int i = 0; i < controller.Parameters.Length; i++)
			{
				var p = new SharpSoundDevice.Parameter();
				p.Display = GetDisplay(0.0);
				p.Index = (uint)i;
				p.Name = i < Parameter.Count.Value() ? ((Parameter)i).Name() : "Parameter " + i;
				p.Steps = 0;
				p.Value = 0.0;
				ParameterInfo[i] = p;
			}

			ViewModel = new CloudSeedViewModel(this);
		}

		public CloudSeedViewModel ViewModel { get; private set; }

		public void DisposeDevice() { }

		public void Start()
		{
			controller.ClearBuffers();
		}

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			PTimer.Begin("Main");
			controller.Process(input, output);
			PTimer.End("Main");
		}
		
		public void OpenEditor(IntPtr parentWindow) 
		{
			view = new CloudSeedView(ViewModel);
			devInfo.EditorWidth = (int)view.Width;
			devInfo.EditorHeight = (int)view.Height;
			HostInfo.SendEvent(this, new Event { Data = null, EventIndex = 0, Type = EventType.WindowSize });
			window = new System.Windows.Window() { Content = view };
			window.Width = view.Width;
			window.Height = view.Height;
			DeviceUtilities.DockWpfWindow(window, parentWindow);
			window.Show();
		}

		public void CloseEditor() 
		{
			window.Close();
		}

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				if (ev.EventIndex >= 0 && ev.EventIndex < controller.Parameters.Length)
				{
					var i = ev.EventIndex;
					var value = (double)ev.Data;
					SetParameter((Parameter)i, value, true, false);
				}
			}
		}

		public void SetParameter(Parameter param, double value, bool updateUi, bool updateHost)
		{
			controller.SetParameter(param, value);
			ParameterInfo[param.Value()].Value = value;
			ParameterInfo[param.Value()].Display = GetDisplay(param);

			//System.Windows.Forms.Cursor.Show();

			if (updateUi && ViewModel != null)
				ViewModel.UpdateParameter(param, value);

			if (updateHost && HostInfo != null)
			{
				HostInfo.SendEvent(this, new Event
				{
					Data = value,
					EventIndex = param.Value(),
					Type = EventType.Parameter
				});
			}
		}

		public string GetDisplay(Parameter param)
		{
			var propVal = controller.GetScaledParameter(param);
			return param.Formatter()(propVal);
		}
	
		public void SetProgramData(Program program, int index)
		{
			try
			{
				var jsonData = Encoding.UTF8.GetString(program.Data);
				SetJsonProgram(jsonData);
			}
			catch
			{
				// Nothing we can do except crash or ignore
			}
		}

		public Program GetProgramData(int index)
		{
			var output = new Program();
			var jsonData = GetJsonProgram();
			output.Data = Encoding.UTF8.GetBytes(jsonData);
			output.Name = "Default Program";
			return output;
		}

		public void SetJsonProgram(string jsonData)
		{
			var dict = JsonConvert.DeserializeObject<Dictionary<string, double>>(jsonData);
			foreach (var kvp in dict)
			{
				Parameter param;
				var ok = Enum.TryParse(kvp.Key, out param);
				if (ok)
				{
					SetParameter(param, kvp.Value, true, true);
				}
			}
		}

		public string GetJsonProgram()
		{
			var dict = controller.Parameters
				.Select((x, i) => new { Name = ((Parameter)i).Name(), Value = x })
				.ToDictionary(x => x.Name, x => x.Value);

			var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
			return json;
		}

		public void HostChanged()
		{
			var samplerate = HostInfo.SampleRate;
			if (samplerate != Samplerate)
			{
				Samplerate = samplerate;
				controller.Samplerate = (int)samplerate;
			}
		}

		public IHostInfo HostInfo { get; set; }

	}
}

