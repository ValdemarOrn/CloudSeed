using System;
using System.Linq;
using AudioLib;

namespace CloudSeed
{
	public class ReverbChannel
	{
		private readonly SimpleDelay preDelay;
		private readonly MultitapDiffuser multitap;
		private readonly AllpassDiffuser diffuser;
		private readonly CombLine[] lines;
		
		private readonly ShaRandom rand;
		private double[] delayLineSeeds;
		
		private readonly double[] tempBuffer;
		private readonly double[] tempBuffer2;
		private readonly double[] outBuffer;

		public ReverbChannel(int bufferSize)
		{
			preDelay = new SimpleDelay(bufferSize, 10000);
			multitap = new MultitapDiffuser(bufferSize);
			diffuser = new AllpassDiffuser(bufferSize);
			lines = Enumerable.Range(0, 8).Select(x => new CombLine(bufferSize)).ToArray();
			
			rand = new ShaRandom();
			tempBuffer = new double[bufferSize];
			tempBuffer2 = new double[bufferSize];
			outBuffer = new double[bufferSize];
			delayLineSeeds = rand.Generate(12345, 8).ToArray();
		}

		public double[] Output { get { return outBuffer; } }

		private bool diffuserEnabled;
		private double lineGain;

		private double dryOut;
		private double predelayOut;
		private double earlyOut;
		private double lineOut;
		private double postDiffusionOut;
		private int lineDelay;
		private double lineFeedback;

		public void SetParameter(Parameter para, object value)
		{
			if (para == Parameter.PreDelay)
			{
				preDelay.SetDelay((int)value);
			}


			else if (para == Parameter.HighPass)
			{
				
			}
			else if (para == Parameter.LowPass)
			{
				
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
				delayLineSeeds = rand.Generate((int)value, 8).ToArray();
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
			else if (para == Parameter.PostDiffusionOut)
			{
				postDiffusionOut = (double)value;
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
			var diffuserOut = diffuser.Output;
			
			preDelay.Process(input, len);
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

			tempBuffer.Gain(lineGain, len);

			for (int i = 0; i < lines.Length; i++)
				lines[i].Process(tempBuffer, len);

			tempBuffer.MixInto(len,
				lines[0].Output,
				lines[1].Output,
				lines[2].Output,
				lines[3].Output,
				lines[4].Output,
				lines[5].Output,
				lines[6].Output,
				lines[7].Output);

			tempBuffer2.MixInto(len,
				lines[0].OutputDelay,
				lines[1].OutputDelay,
				lines[2].OutputDelay,
				lines[3].OutputDelay,
				lines[4].OutputDelay,
				lines[5].OutputDelay,
				lines[6].OutputDelay,
				lines[7].OutputDelay);
			
			for (int i = 0; i < len; i++)
			{
				outBuffer[i] = 
					dryOut           * input[i] +
					predelayOut      * predelayOutput[i] +
					earlyOut         * earlyOutStage[i] +
					lineOut          * tempBuffer[i] +
					postDiffusionOut * tempBuffer2[i];
			}
		}

	}
}
