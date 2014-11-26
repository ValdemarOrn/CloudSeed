using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public class CombLine
	{
		private readonly SimpleDelay delay;
		private readonly AllpassDiffuser diffuser;
		private readonly double[] sumBuffer;

		private double feedback;
		private bool diffuserEnabled;

		public CombLine(int bufferSize)
		{
			delay = new SimpleDelay(bufferSize, 10000);
			diffuser = new AllpassDiffuser(bufferSize);
			sumBuffer = new double[bufferSize];
		}

		public double[] Seeds
		{
			get { return diffuser.Seeds; }
			set { diffuser.Seeds = value; }
		}

		public void SetDelay(int delaySamples)
		{
			delay.SetDelay(delaySamples);
		}

		public void SetFeedback(double feedb)
		{
			feedback = feedb;
		}
		
		public void SetDiffuserDelay(int delaySamples)
		{
			diffuser.SetDelay(delaySamples);
		}

		public void SetDiffuserFeedback(double feedb)
		{
			diffuser.SetFeedback(feedb);
		}

		public void SetDiffuserStages(int stages)
		{
			diffuser.Stages = stages;
		}

		public void SetDiffuserEnabled(bool isEnabled)
		{
			diffuserEnabled = isEnabled;
		}

		public double[] OutputDelay { get { return delay.Output; } }
		public double[] Output { get { return diffuserEnabled ? diffuser.Output : delay.Output; } }

		public void Process(double[] input, int sampleCount)
		{
			var delayOut = delay.Output;
			var feedbackBuffer = diffuserEnabled ? diffuser.Output : delay.Output;

			for (int i = 0; i < sampleCount; i++)
				sumBuffer[i] = input[i] + feedbackBuffer[i] * feedback;

			delay.Process(sumBuffer, sampleCount);

			if (diffuserEnabled)
				diffuser.Process(delayOut, sampleCount);
		}
	}
}
