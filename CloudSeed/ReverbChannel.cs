using System;
using System.Collections.Generic;
using System.Linq;
using AudioLib;
using AudioLib.TF;

namespace CloudSeed
{
	public class ReverbChannel
	{
		private readonly Dictionary<Parameter, double> parameters;
		private int samplerate;

		private readonly ModulatedDelay preDelay;
		private readonly MultitapDiffuser multitap;
		private readonly AllpassDiffuser diffuser;
		private readonly DelayLine[] lines;
		private readonly ShaRandom rand;
		private readonly Hp1 highPass;
		private readonly Lp1 lowPass;
		private readonly double[] tempBuffer;
		private readonly double[] outBuffer;
		private double[] delayLineSeeds;

		// Used the the main process loop
		private int lineCount;
		private double perLineGain;

		private bool highPassEnabled;
		private bool lowPassEnabled;
		private bool diffuserEnabled;
		private double dryOut;
		private double predelayOut;
		private double earlyOut;
		private double lineOut;

		public ReverbChannel(int bufferSize, int samplerate)
		{
			parameters = new Dictionary<Parameter, double>();
			foreach (var value in Enum.GetValues(typeof(Parameter)).Cast<Parameter>())
				parameters[value] = 0.0;

			preDelay = new ModulatedDelay(bufferSize, 10000);
			
			multitap = new MultitapDiffuser(bufferSize);
			diffuser = new AllpassDiffuser(bufferSize, samplerate) { ModulationEnabled = true };
			lines = Enumerable.Range(0, 12).Select(x => new DelayLine(bufferSize, samplerate)).ToArray();
			lineCount = 8;
			perLineGain = GetPerLineGain();

			highPass = new Hp1(samplerate) { CutoffHz = 20 };
			lowPass = new Lp1(samplerate) { CutoffHz = 20000 };
			
			rand = new ShaRandom();
			tempBuffer = new double[bufferSize];
			outBuffer = new double[bufferSize];
			delayLineSeeds = rand.Generate(12345, lines.Length * 3).ToArray();

			this.samplerate = samplerate;
		}

		public int Samplerate
		{
			get { return samplerate; }
			set
			{
				samplerate = value;
				highPass.Samplerate = samplerate;
				lowPass.Samplerate = samplerate;

				for (int i = 0; i < lines.Length; i++)
				{
					lines[i].Samplerate = samplerate;	
				}
				
				Action<Parameter> update = p => SetParameter(p, parameters[p]);
				update(Parameter.PreDelay);
				update(Parameter.TapLength);
				update(Parameter.DiffusionDelay);
				update(Parameter.LineDelay);
				update(Parameter.LateDiffusionDelay);
				update(Parameter.EarlyDiffusionModRate);
				update(Parameter.LineModRate);
				update(Parameter.LateDiffusionModRate);
				update(Parameter.LineModAmount);
				UpdateLines();
			}
		}

		public double[] Output { get { return outBuffer; } }

		public void SetParameter(Parameter para, double value)
		{
			parameters[para] = value;

			switch (para)
			{
				case Parameter.PreDelay:
					preDelay.SampleDelay = (int)Ms2Samples(value);
					break;
				case Parameter.HighPass:
					highPass.CutoffHz = value;
					break;
				case Parameter.LowPass:
					lowPass.CutoffHz = value;
					break;

				case Parameter.TapCount:
					multitap.SetTapCount((int)value);
					break;
				case Parameter.TapLength:
					multitap.SetTapLength((int)Ms2Samples(value));
					break;
				case Parameter.TapGain:
					multitap.SetTapGain(value);
					break;
				case Parameter.TapDecay:
					multitap.SetTapDecay(value);
					break;

				case Parameter.DiffusionEnabled:
					diffuserEnabled = value >= 0.5;
					break;
				case Parameter.DiffusionStages:
					diffuser.Stages = (int)value;
					break;
				case Parameter.DiffusionDelay:
					diffuser.SetDelay((int)Ms2Samples(value));
					break;
				case Parameter.DiffusionFeedback:
					diffuser.SetFeedback(value);
					break;

				case Parameter.LineCount:
					lineCount = (int)value;
					perLineGain = GetPerLineGain();
					break;
				case Parameter.LineDelay:
					UpdateLines();
					break;
				case Parameter.LineDecay:
					UpdateLines();
					break;

				case Parameter.LateDiffusionEnabled:
					foreach (var line in lines)
						line.DiffuserEnabled = value >= 0.5;
					break;
				case Parameter.LateDiffusionStages:
					foreach (var line in lines)
						line.SetDiffuserStages((int)value);
					break;
				case Parameter.LateDiffusionDelay:
					foreach (var line in lines)
						line.SetDiffuserDelay((int)Ms2Samples(value));
					break;
				case Parameter.LateDiffusionFeedback:
					foreach (var line in lines)
						line.SetDiffuserFeedback(value);
					break;

				case Parameter.PostLowShelfGain:
					foreach (var line in lines)
						line.SetLowShelfGain(value);
					break;
				case Parameter.PostLowShelfFrequency:
					foreach (var line in lines)
						line.SetLowShelfFrequency(value);
					break;
				case Parameter.PostHighShelfGain:
					foreach (var line in lines)
						line.SetHighShelfGain(value);
					break;
				case Parameter.PostHighShelfFrequency:
					foreach (var line in lines)
						line.SetHighShelfFrequency(value);
					break;
				case Parameter.PostCutoffFrequency:
					foreach (var line in lines)
						line.SetCutoffFrequency(value);
					break;

				case Parameter.EarlyDiffusionModAmount:
					diffuser.SetModAmount(Ms2Samples(value));
					break;
				case Parameter.EarlyDiffusionModRate:
					diffuser.SetModRate(value);
					break;
				case Parameter.LineModAmount:
					UpdateLines();
					break;
				case Parameter.LineModRate:
					UpdateLines();
					break;
				case Parameter.LateDiffusionModAmount:
					UpdateLines();
					break;
				case Parameter.LateDiffusionModRate:
					UpdateLines();
					break;

				case Parameter.TapSeed:
					multitap.Seeds = rand.Generate((int)value, 100).ToArray();
					break;
				case Parameter.DiffusionSeed:
					diffuser.Seeds = rand.Generate((int)value, AllpassDiffuser.MaxStageCount * 3).ToArray();
					break;
				case Parameter.DelaySeed:
					delayLineSeeds = rand.Generate((int)value, lines.Length * 3).ToArray();
					UpdateLines();
					break;
				case Parameter.PostDiffusionSeed:
					for (int i = 0; i < lines.Length; i++)
						lines[i].DiffuserSeeds = rand.Generate(((long)value) * (i + 1), AllpassDiffuser.MaxStageCount * 3).ToArray();
					break;

				case Parameter.DryOut:
					dryOut = value;
					break;
				case Parameter.PredelayOut:
					predelayOut = value;
					break;
				case Parameter.EarlyOut:
					earlyOut = value;
					break;
				case Parameter.MainOut:
					lineOut = value;
					break;

				case Parameter.HiPassEnabled:
					highPassEnabled = value >= 0.5;
					break;
				case Parameter.LowPassEnabled:
					lowPassEnabled = value >= 0.5;
					break;
				case Parameter.LowShelfEnabled:
					foreach (var line in lines)
						line.LowShelfEnabled = value >= 0.5;
					break;
				case Parameter.HighShelfEnabled:
					foreach (var line in lines)
						line.HighShelfEnabled = value >= 0.5;
					break;
				case Parameter.CutoffEnabled:
					foreach (var line in lines)
						line.CutoffEnabled = value >= 0.5;
					break;
			}
		}

		private double GetPerLineGain()
		{
			return 1 / Math.Sqrt(lineCount);
		}

		private void UpdateLines()
		{
			var lineModRate = parameters[Parameter.LineModRate];
			var lineModAmount = Ms2Samples(parameters[Parameter.LineModAmount]);
			var lineFeedback = parameters[Parameter.LineDecay];
			var lineDelay = (int)Ms2Samples(parameters[Parameter.LineDelay]);
			if (lineDelay < 50) lineDelay = 50;		

			var count = lines.Length;
			for (int i = 0; i < count; i++)
			{
				var delay = (0.1 + 0.9 * delayLineSeeds[i]) * lineDelay;
				var ratio = delay / lineDelay;
				var adjustedFeedback = Math.Pow(lineFeedback, ratio);

				var modAmount = lineModAmount * (0.8 + 0.2 * delayLineSeeds[i + count]);
				var modRate = lineModRate * (0.8 + 0.2 * delayLineSeeds[i + 2 * count]) / samplerate;

				lines[i].SetDelay((int)delay);
				lines[i].SetFeedback(adjustedFeedback);
				lines[i].SetModAmount(modAmount);
				lines[i].SetModRate(modRate);
			}
		}

		public void Process(double[] input, int sampleCount)
		{
			int len = sampleCount;
			var predelayOutput = preDelay.Output;
			var lowPassInput = highPassEnabled ? tempBuffer : input;

			if (highPassEnabled)
				highPass.Process(input, tempBuffer, len);
			if (lowPassEnabled)
				lowPass.Process(lowPassInput, tempBuffer, len);
			if (!lowPassEnabled && !highPassEnabled)
				input.Copy(tempBuffer, len);

			// completely zero if no input present
			// Previously, the very small values were causing some really strange CPU spikes
			for (int i = 0; i < len; i++)
			{
				var n = tempBuffer[i];
				if (n * n < 0.000000001)
					tempBuffer[i] = 0;
			}

			preDelay.Process(tempBuffer, len);
			multitap.Process(preDelay.Output, len);
			
			var earlyOutStage = diffuserEnabled ? diffuser.Output : multitap.Output;

			if (diffuserEnabled)
			{
				diffuser.Process(multitap.Output, len);
				diffuser.Output.Copy(tempBuffer, len);
			}
			else
			{
				multitap.Output.Copy(tempBuffer, len);
			}

			for (int i = 0; i < lineCount; i++)
				lines[i].Process(tempBuffer, len);

			for (int i = 0; i < lineCount; i++)
			{
				var buf = lines[i].Output;

				if (i == 0)
				{	
					for (int j = 0; j < len; j++)
						tempBuffer[j] = buf[j];
				}
				else 
				{
					for (int j = 0; j < len; j++)
						tempBuffer[j] += buf[j];
				}
			}

			tempBuffer.Gain(perLineGain, len);
			
			for (int i = 0; i < len; i++)
			{
				outBuffer[i] = 
					dryOut           * input[i] +
					predelayOut      * predelayOutput[i] +
					earlyOut         * earlyOutStage[i] +
					lineOut          * tempBuffer[i];
			}
		}

		public void ClearBuffers()
		{
			tempBuffer.Zero();
			outBuffer.Zero();
			lowPass.Output = 0;
			highPass.Output = 0;

			preDelay.ClearBuffers();
			multitap.ClearBuffers();
			diffuser.ClearBuffers();
			foreach (var line in lines)
				line.ClearBuffers();
		}

		private double Ms2Samples(double value)
		{
			return value / 1000.0 * samplerate;
		}
	}
}
