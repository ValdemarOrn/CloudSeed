using System;
using System.Linq;
using AudioLib;
using AudioLib.Modules;

namespace CloudSeed
{
	public class ReverbChannel
	{
		private readonly SimpleDelay preDelay;
		private readonly MultitapDiffuser multitap;
		private readonly AllpassDiffuser diffuser;
		private readonly CombLine[] lines;
		private readonly double[] tempBuffer;
		private readonly double[] outBuffer;
		private readonly ShaRandom rand;

		private readonly Biquad highPass;
		private readonly Biquad lowPass;
		
		private double[] delayLineSeeds;
		private int lineCount;
		private double perLineGain;
		
		public ReverbChannel(int bufferSize, int samplerate)
		{
			preDelay = new SimpleDelay(bufferSize, 10000);
			multitap = new MultitapDiffuser(bufferSize);
			diffuser = new AllpassDiffuser(bufferSize);
			lines = Enumerable.Range(0, 20).Select(x => new CombLine(bufferSize, samplerate)).ToArray();
			lineCount = 8;
			perLineGain = GetPerLineGain();

			highPass = new Biquad(Biquad.FilterType.HighPass, samplerate) { Q = 1.0, Frequency = 20 };
			lowPass = new Biquad(Biquad.FilterType.LowPass, samplerate) { Q = 1.0, Frequency = 20000 };
			highPass.Update();
			lowPass.Update();
			
			rand = new ShaRandom();
			tempBuffer = new double[bufferSize];
			outBuffer = new double[bufferSize];
			delayLineSeeds = rand.Generate(12345, lines.Length).ToArray();
		}

		public double[] Output { get { return outBuffer; } }

		private bool diffuserEnabled;
		private double lineGain;

		private double dryOut;
		private double predelayOut;
		private double earlyOut;
		private double lineOut;
		private int lineDelay;
		private double lineFeedback;

		private double GetPerLineGain()
		{
			return 1 / Math.Sqrt(lineCount);
		}

		public void SetParameter(Parameter para, object value)
		{
			if (para == Parameter.PreDelay)
			{
				preDelay.SetDelay((int)value);
			}


			else if (para == Parameter.HighPass)
			{
				highPass.Frequency = (double)value;
				highPass.Update();
			}
			else if (para == Parameter.LowPass)
			{
				lowPass.Frequency = (double)value;
				lowPass.Update();
			}


			else if (para == Parameter.TapCount)
			{
				multitap.SetTapCount((int)value);
			}
			else if (para == Parameter.TapLength)
			{
				multitap.SetTapLength((int)value);
			}
			else if (para == Parameter.TapGain)
			{
				multitap.SetTapGain((double)value);
			}
			else if (para == Parameter.TapDecay)
			{
				multitap.SetTapDecay((double)value);
			}


			else if (para == Parameter.DiffusionEnabled)
			{
				diffuserEnabled = (bool)value;
			}
			else if (para == Parameter.DiffusionStages)
			{
				diffuser.Stages = (int)value;
			}
			else if (para == Parameter.DiffusionDelay)
			{
				diffuser.SetDelay((int)value);
			}
			else if (para == Parameter.DiffusionFeedback)
			{
				diffuser.SetFeedback((double)value);
			}

			else if (para == Parameter.LineCount)
			{
				lineCount = (int)value;
				perLineGain = GetPerLineGain();
			}
			else if (para == Parameter.LineGain)
			{
				lineGain = (double)value;
			}
			else if (para == Parameter.LineDelay)
			{
				var val = (int)value;
				if (val < 50) val = 50;
				lineDelay = val;
				UpdateLines();
			}
			else if (para == Parameter.LineFeedback)
			{
				lineFeedback = (double)value;
				UpdateLines();
			}


			else if (para == Parameter.PostDiffusionEnabled)
			{
				for (int i = 0; i < lines.Length; i++)
					lines[i].SetDiffuserEnabled((bool)value);
			}
			else if (para == Parameter.PostDiffusionStages)
			{
				for (int i = 0; i < lines.Length; i++)
					lines[i].SetDiffuserStages((int)value);
			}
			else if (para == Parameter.PostDiffusionDelay)
			{
				for (int i = 0; i < lines.Length; i++)
					lines[i].SetDiffuserDelay((int)value);
			}
			else if (para == Parameter.PostDiffusionFeedback)
			{
				for (int i = 0; i < lines.Length; i++)
					lines[i].SetDiffuserFeedback((double)value);
			}


			else if (para == Parameter.TapSeed)
			{
				multitap.Seeds = rand.Generate((int)value, 400).ToArray();
			}
			else if (para == Parameter.DiffusionSeed)
			{
				diffuser.Seeds = rand.Generate((int)value, 4).ToArray();
			}
			else if (para == Parameter.CombSeed)
			{
				delayLineSeeds = rand.Generate((int)value, lines.Length).ToArray();
				UpdateLines();
			}
			else if (para == Parameter.PostDiffusionSeed)
			{
				for (int i = 0; i < lines.Length; i++)
				{
					lines[i].Seeds = rand.Generate(((int)value) + i, 4).ToArray();
				}
			}


			else if (para == Parameter.DryOut)
			{
				dryOut = (double)value;
			}
			else if (para == Parameter.PredelayOut)
			{
				predelayOut = (double)value;
			}
			else if (para == Parameter.EarlyOut)
			{
				earlyOut = (double)value;
			}
			else if (para == Parameter.LineOut)
			{
				lineOut = (double)value;
			}
		}

		private void UpdateLines()
		{
			for (int i = 0; i < lines.Length; i++)
			{
				var delay = (0.1 + 0.9 * delayLineSeeds[i]) * lineDelay;
				var ratio = delay / lineDelay;
				var adjustedFeedback = Math.Pow(lineFeedback, ratio);
				lines[i].SetDelay((int)delay);
				lines[i].SetFeedback(adjustedFeedback);
			}
		}

		public void Process(double[] input, int sampleCount)
		{
			int len = sampleCount;
			var predelayOutput = preDelay.Output;
			
			preDelay.Process(input, len);

			highPass.Process(preDelay.Output, tempBuffer, len);
			lowPass.Process(tempBuffer, tempBuffer, len);

			// completely zero if no input present
			for (int i = 0; i < len; i++)
			{
				var n = tempBuffer[i];
				if (n * n < 0.000000001)
					tempBuffer[i] = 0;
			}

			multitap.Process(tempBuffer, len);

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

			tempBuffer.Gain(lineGain, len);

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

	}
}
