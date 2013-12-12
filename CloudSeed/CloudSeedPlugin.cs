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
			"Size", 
			"Density", 
			"Decay", 
			"Delay", 
			"Hi Cut", 
			"Hi Cut Amt", 
			"AP-Delay", 
			"AP-Feedback", 
			"Mod Rate",
			"Mod Amount",
			"Late Stages",
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

		public double Predelay { get { return Parameters[0] * 100; } }
		public double Size { get { return Parameters[1] * 400; } }
		public int Density { get { return (int)(Parameters[2] * 100); } }
		public double Decay { get { return Parameters[3]; } }
		public double Delay { get { return Parameters[4] * 100; } }

		public double HiCut { get { return ValueTables.Get(Parameters[5], ValueTables.Response4Oct) * 20000; } }
		public double HiCutAmt { get { return Parameters[6]; } }

		public double APDelay { get { return Parameters[7] * 100; } }
		public double APFeedback { get { return Parameters[8]; } }

		public double ModRate { get { return Parameters[9] * 2; } }
		public double ModAmount { get { return Parameters[10]; } }

		public int LateStages { get { return 1 + (int)(Parameters[11] * 7.999); } }

		public double Dry { get { return Parameters[12]; } }
		public double Wet { get { return Parameters[13]; } }

		public string GetDisplay(int i)
		{
			if (i == 0) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", Predelay);
			if (i == 1) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", Size);
			if (i == 2) return String.Format(CultureInfo.InvariantCulture, "{0:0}x", Density);
			if (i == 3) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Decay);
			if (i == 4) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Delay);

			if (i == 5) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", HiCut);
			if (i == 6) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", HiCutAmt);

			if (i == 7) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}ms", APDelay);
			if (i == 8) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", APFeedback);

			if (i == 9) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}Hz", ModRate);
			if (i == 10) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", ModAmount);

			if (i == 11) return String.Format(CultureInfo.InvariantCulture, "{0:0}", LateStages);

			if (i == 12) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Dry);
			if (i == 13) return String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Wet);

			return Parameters[i].ToString();
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

				RevLeft.SetTaps(Predelay, Size, Density);
				RevLeft.SetLate(APFeedback, (int)(APDelay / 1000.0 * Samplerate));
				RevLeft.SetParameter(ParameterEnum.GlobalFeedback, Decay);
				RevLeft.SetParameter(ParameterEnum.GlobalDelay, (int)(Delay / 1000.0 * Samplerate));
				RevLeft.SetHiCut(HiCut, HiCutAmt);
				RevLeft.SetMod(ModRate, ModAmount);
				RevLeft.SetParameter(ParameterEnum.StageCount, LateStages);
				RevLeft.SetParameter(ParameterEnum.Dry, Dry);
				RevLeft.SetParameter(ParameterEnum.Wet, Wet);

				RevRight.SetTaps(Predelay, Size, Density);
				RevRight.SetLate(APFeedback, (int)(APDelay / 1000.0 * Samplerate));
				RevRight.SetParameter(ParameterEnum.GlobalFeedback, Decay);
				RevRight.SetParameter(ParameterEnum.GlobalDelay, (int)(Delay / 1000.0 * Samplerate));
				RevRight.SetHiCut(HiCut, HiCutAmt);
				RevRight.SetMod(ModRate, ModAmount);
				RevRight.SetParameter(ParameterEnum.StageCount, LateStages);
				RevRight.SetParameter(ParameterEnum.Dry, Dry);
				RevRight.SetParameter(ParameterEnum.Wet, Wet);
			}
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
				RevLeft.SetTaps(Predelay, Size, Density);
				RevRight.SetTaps(Predelay, Size, Density);
			}
		}

		public IHostInfo HostInfo { get; set; }

	}
}

