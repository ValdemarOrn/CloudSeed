using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using SharpSoundDevice;
using AudioLib;
using System.Globalization;

namespace CloudSeed
{
	public class CloudSeedPlugin : IAudioDevice
	{
		private static string[] ParameterNames = new[] 
		{
			"Predelay", 
			"Early Size", 
			"Density", 

			"AP-Delay", 
			"AP-Feedback", 

			"Hi Cut", 
			"Hi Cut Amt", 
			"Low Cut", 
			"Low Cut Amt", 

			"Mod Rate",
			"Mod Amount",
			"Stage Count",

			"Feedback", 
			"Delay", 
			
			"Dry", 
			"Wet"
		};

		// --------------- IAudioDevice Properties ---------------

		DeviceInfo DevInfo;

		public double Samplerate;

		public DeviceInfo DeviceInfo { get { return this.DevInfo; } }
		public Parameter[] ParameterInfo { get; private set; }
		public Port[] PortInfo { get; private set; }
		public int CurrentProgram { get; private set; }

		// --------------- Necessary Parameters ---------------

		private ReverberWrapper RevLeft, RevRight;
		private double[] Parameters;

		public CloudSeedPlugin()
		{
			RevLeft = new ReverberWrapper();
			RevRight = new ReverberWrapper();
			RevLeft.SetSamplerate(48000);
			RevRight.SetSamplerate(48000);
			Samplerate = 48000;
			DevInfo = new DeviceInfo();
			ParameterInfo = new Parameter[ParameterNames.Length];
			Parameters = new double[ParameterNames.Length];
			PortInfo = new Port[2];
		}

		public void InitializeDevice()
		{
			DevInfo.DeviceID = "Low Profile - Cloud Seed";
			DevInfo.Developer = "Valdemar Erlingsson";
			DevInfo.EditorHeight = 0;
			DevInfo.EditorWidth = 0;
			DevInfo.HasEditor = false;
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

			for (int i = 0; i < ParameterInfo.Length; i++)
			{
				var p = new Parameter();
				p.Display = GetDisplay(i);
				p.Index = (uint)i;
				p.Name = ParameterNames[i];
				p.Steps = 0;
				p.Value = Parameters[i];
				ParameterInfo[i] = p;
			}
		}

		public double Predelay { get { return Parameters[(int)ParameterEnum.Predelay] * 100; } }
		public double EarlySize { get { return Parameters[(int)ParameterEnum.EarlySize] * 1000; } }
		public int Density { get { return (int)(Parameters[(int)ParameterEnum.Density] * 200); } }
		public double GlobalFeedback { get { return Parameters[(int)ParameterEnum.GlobalFeedback]; } }
		public double GlobalDelay { get { return Parameters[(int)ParameterEnum.GlobalDelay] * 100; } }

		public double HiCut { get { return ValueTables.Get(Parameters[(int)ParameterEnum.HiCut], ValueTables.Response4Oct) * 20000; } }
		public double HiCutAmt { get { return Parameters[(int)ParameterEnum.HiCutAmt]; } }

		public double APDelay { get { return Parameters[(int)ParameterEnum.APDelay] * 100; } }
		public double APFeedback { get { return Parameters[(int)ParameterEnum.APFeedback]; } }

		public double ModRate { get { return Parameters[(int)ParameterEnum.ModRate] * 2; } }
		public double ModAmount { get { return Parameters[(int)ParameterEnum.ModAmount] * 2; } }

		public int StageCount { get { return 1 + (int)(Parameters[(int)ParameterEnum.StageCount] * 7.999); } }

		public double Dry { get { return Parameters[(int)ParameterEnum.Dry]; } }
		public double Wet { get { return Parameters[(int)ParameterEnum.Wet]; } }

		public string GetDisplay(int i)
		{
			switch((ParameterEnum)i)
			{
				case ParameterEnum.Predelay:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", Predelay);
				case ParameterEnum.EarlySize:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", EarlySize);
				case ParameterEnum.Density:
					return String.Format(CultureInfo.InvariantCulture, "{0:0}x", Density);
				case ParameterEnum.GlobalFeedback:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", GlobalFeedback);
				case ParameterEnum.GlobalDelay:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", GlobalDelay);

				case ParameterEnum.HiCut:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", HiCut);
				case ParameterEnum.HiCutAmt:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", HiCutAmt);

				case ParameterEnum.APDelay:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", APDelay);
				case ParameterEnum.APFeedback:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", APFeedback);

				case ParameterEnum.ModRate:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", ModRate);
				case ParameterEnum.ModAmount:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", ModAmount);

				case ParameterEnum.StageCount:
					return String.Format(CultureInfo.InvariantCulture, "{0:0}", StageCount);

				case ParameterEnum.Dry:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Dry);
				case ParameterEnum.Wet:
					return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Wet);
			}
			

			return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Parameters[i]);
		}

		public void DisposeDevice() { }

		public void Start() { }

		public void Stop() { }

		public void ProcessSample(double[][] input, double[][] output, uint bufferSize)
		{
			RevLeft.Process(input[0], output[0]);
			RevRight.Process(input[1], output[1]);
		}

		public void OpenEditor(IntPtr parentWindow) { }

		public void CloseEditor() { }

		public void SendEvent(Event ev)
		{
			if (ev.Type == EventType.Parameter)
			{
				SetParameter(ev.EventIndex, (double)ev.Data);
			}
		}

		private void SetParameter(int index, double value)
		{
			if (index >= 0 && index < ParameterInfo.Length)
			{
				Parameters[index] = value;
				ParameterInfo[index].Value = value;
				ParameterInfo[index].Display = GetDisplay(index);

				RevLeft.SetParameter(ParameterEnum.EarlySize, EarlySize);
				RevLeft.SetParameter(ParameterEnum.Predelay, Predelay);
				RevLeft.SetParameter(ParameterEnum.Dry, Dry);
				RevLeft.SetParameter(ParameterEnum.Wet, Wet);
				RevLeft.SetParameter(ParameterEnum.StageCount, StageCount);

				RevLeft.SetTaps(EarlySize, Density);
				RevLeft.SetLate(APFeedback, (int)(APDelay / 1000.0 * Samplerate));
				RevLeft.SetParameter(ParameterEnum.GlobalFeedback, GlobalFeedback);
				RevLeft.SetParameter(ParameterEnum.GlobalDelay, (int)(GlobalDelay / 1000.0 * Samplerate));
				RevLeft.SetHiCut(HiCut, HiCutAmt);
				RevLeft.SetAllpassMod(ModRate, ModAmount);
				

				RevRight.SetParameter(ParameterEnum.EarlySize, EarlySize);
				RevRight.SetParameter(ParameterEnum.Predelay, Predelay);
				RevRight.SetParameter(ParameterEnum.Dry, Dry);
				RevRight.SetParameter(ParameterEnum.Wet, Wet);
				RevRight.SetParameter(ParameterEnum.StageCount, StageCount);

				RevRight.SetTaps(EarlySize, Density);
				RevRight.SetLate(APFeedback, (int)(APDelay / 1000.0 * Samplerate));
				RevRight.SetParameter(ParameterEnum.GlobalFeedback, GlobalFeedback);
				RevRight.SetParameter(ParameterEnum.GlobalDelay, (int)(GlobalDelay / 1000.0 * Samplerate));
				RevRight.SetHiCut(HiCut, HiCutAmt);
				RevRight.SetAllpassMod(ModRate, ModAmount);
				
			}

			System.Windows.Forms.Cursor.Show();
		}

		public void SetProgramData(Program program, int index)
		{
			try
			{
				DeviceUtilities.DeserializeParameters(ParameterInfo, program.Data);
				for (int i = 0; i < ParameterInfo.Length; i++)
					SetParameter(i, ParameterInfo[i].Value);
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

	}
}

