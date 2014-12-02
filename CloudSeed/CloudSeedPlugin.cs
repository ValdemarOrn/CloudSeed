using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
		private readonly CloudSeedViewModel viewModel;
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
			viewModel = new CloudSeedViewModel(controller);
		}

		public void InitializeDevice()
		{
			devInfo.DeviceID = "Low Profile - Cloud Seed";
			devInfo.Developer = "Valdemar Erlingsson";
			devInfo.EditorWidth = 950;
			devInfo.EditorHeight = 500;
			devInfo.HasEditor = true;
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
		}

		public void DisposeDevice() { }

		public void Start()
		{
			controller.ClearBuffers();
		}

		public void Stop() { }

		//private static Stopwatch sw = new Stopwatch();

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			PTimer.Begin("Main");

			/*
			 * for (int i = 0; i < bufferSize; i++)
			{
				output[0][i] = input[0][i];
				output[1][i] = input[1][i];
			}
			sw.Restart();
			var ticks = TimeSpan.TicksPerMillisecond * 0.3;
			while (sw.ElapsedTicks < ticks)
			{
				
			}
			sw.Stop();
			 */

			controller.Process(input, output);

			PTimer.End("Main");
		}

		
		public void OpenEditor(IntPtr parentWindow) 
		{
			view = new CloudSeedView(viewModel);
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
					SetParameter((Parameter)i, value);
				}
			}
		}

		public void SetParameter(Parameter param, double value, bool updateHost = false)
		{
			controller.SetParameter(param, value);
			ParameterInfo[param.Value()].Value = value;
			ParameterInfo[param.Value()].Display = GetDisplay(param);

			System.Windows.Forms.Cursor.Show();
			//VM.Update(parameter);
			if (updateHost)
			{
				HostInfo.SendEvent(this, new Event
				{
					Data = value,
					EventIndex = param.Value(),
					Type = EventType.Parameter
				});
			}
		}

		private string GetDisplay(Parameter param)
		{
			var propVal = controller.GetScaledParameter(param);
			return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", propVal);
		}
	
		public void SetProgramData(Program program, int index)
		{
			try
			{
				var dict = ReverbController.ParseJsonProgram(program.Data);
				foreach (var kvp in dict)
				{
					Parameter param;
					var ok = Enum.TryParse<Parameter>(kvp.Key, out param);
					if (ok)
					{
						SendEvent(new Event { Data = kvp.Value, EventIndex = param.Value(), Type = EventType.Parameter });
					}
				}
			}
			catch (Exception)
			{

			}
		}

		public Program GetProgramData(int index)
		{
			var output = new Program();
			output.Data = controller.GetJsonProgram();
			output.Name = "Default Program";
			return output;
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

