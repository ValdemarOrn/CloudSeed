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
using System.Threading;

namespace CloudSeed
{
	public class CloudSeedPlugin : IAudioDevice
	{
		// --------------- IAudioDevice Properties ---------------

		private DeviceInfo devInfo;

		private readonly IReverbController controller;
		private CloudSeedView view;
		private System.Windows.Window window;
		private volatile bool isDisposing;

		public double Samplerate;
		public DeviceInfo DeviceInfo { get { return devInfo; } }
		public SharpSoundDevice.Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }
		public int DeviceId { get; set; }
		public IHostInfo HostInfo { get; set; }

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int AllocConsole();

		public CloudSeedPlugin()
		{
			//AllocConsole();
			controller = new UnsafeReverbController(48000);
			controller.Samplerate = 48000;

			Samplerate = 48000;
			devInfo = new DeviceInfo();
			ParameterInfo = new SharpSoundDevice.Parameter[controller.GetParameterCount()];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			devInfo.DeviceID = "Low Profile - CloudSeed";
			devInfo.Developer = "Valdemar Erlingsson";
			devInfo.EditorWidth = 995;
			devInfo.EditorHeight = 386;
			devInfo.HasEditor = true;
			devInfo.Name = "CloudSeed Algorithmic Reverb";
			devInfo.ProgramCount = 1;
			devInfo.Type = DeviceType.Effect;
			devInfo.Version = 1000;
			devInfo.UnsafeProcessing = controller is IUnsafeReverbController;
			devInfo.VstId = DeviceUtilities.GenerateIntegerId(devInfo.DeviceID);
			
			PortInfo[0].Direction = PortDirection.Input;
			PortInfo[0].Name = "Stereo Input";
			PortInfo[0].NumberOfChannels = 2;

			PortInfo[1].Direction = PortDirection.Output;
			PortInfo[1].Name = "Stereo Output";
			PortInfo[1].NumberOfChannels = 2;

			for (int i = 0; i < controller.GetParameterCount(); i++)
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
			controller.ClearBuffers();
		}

		public CloudSeedViewModel ViewModel { get; private set; }

		public void DisposeDevice()
		{
			/*isDisposing = true;
			Thread.Sleep(100);

			var disposable = controller as IDisposable;
			if (disposable != null)
				disposable.Dispose();*/
		}

		public void Start()
		{
			controller.ClearBuffers();
		}

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			if (isDisposing)
				return;

			((IManagedReverbController)controller).Process(input, output);
		}

		public void ProcessSample(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
			if (inChannelCount != 2)
				throw new Exception("InChannelCount for CloudSeed must be 2");
			if (outChannelCount != 2)
				throw new Exception("OutChannelCount for CloudSeed must be 2");

			((IUnsafeReverbController)controller).Process(input, output, (int)bufferSize);
		}
		
		public void OpenEditor(IntPtr parentWindow) 
		{
			System.Threading.Tasks.Task.Run(() => ProgramBanks.Bank.ReloadPrograms());

			view = new CloudSeedView(ViewModel);
			devInfo.EditorWidth = (int)view.Width;
			devInfo.EditorHeight = (int)view.Height;
			HostInfo.SendEvent(DeviceId, new Event { Data = null, EventIndex = 0, Type = EventType.WindowSize });
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

		public bool SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				if (ev.EventIndex >= 0 && ev.EventIndex < controller.GetParameterCount())
				{
					var i = ev.EventIndex;
					var value = (double)ev.Data;
					SetParameter((Parameter)i, value, true, false);
				}
			}

			return false;
		}

		public void SetParameter(Parameter param, double value)
		{
			SetParameter(param, value, false, true);
		}

		private void SetParameter(Parameter param, double value, bool updateUi, bool updateHost)
		{
			controller.SetParameter(param, value);
			ParameterInfo[param.Value()].Value = value;
			ParameterInfo[param.Value()].Display = GetDisplay(param);

			//System.Windows.Forms.Cursor.Show();

			if (updateUi && ViewModel != null)
				ViewModel.UpdateParameterAsync(param, value);

			if (updateHost && HostInfo != null)
			{
				HostInfo.SendEvent(DeviceId, new Event
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
			controller.ClearBuffers();
		}

		public string GetJsonProgram()
		{
			var dict = controller.GetAllParameters()
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
		
	}
}

