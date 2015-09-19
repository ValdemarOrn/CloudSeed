using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioLib;

namespace CloudSeed
{
	public class ModulatedAllpass
	{
		private const int ModulationUpdateRate = 8;

		private readonly double[] buffer;
		private readonly double[] output;
		private readonly int bufferSize;
		private int index;
		private int samplesProcessed;

		private double modPhase;
		private int delayA;
		private int delayB;
		private double gainA;
		private double gainB;

		public int SampleDelay;
		public double Feedback;

		public double ModAmount;
		public double ModRate;
		
		public bool ModulationEnabled;

		public ModulatedAllpass(int bufferSize, int sampleDelay)
		{
			this.bufferSize = bufferSize;
			this.buffer = new double[bufferSize];
			this.output = new double[bufferSize];
			this.SampleDelay = sampleDelay;
			index = bufferSize - 1;
		}

		public double[] Output { get { return output; } }

		internal void Clear()
		{
			buffer.Zero();
			output.Zero();
		}

		public void Process(double[] input, int sampleCount)
		{
			if (ModulationEnabled)
				ProcessWithMod(input, sampleCount);
			else
				ProcessNoMod(input, sampleCount);
		}

		private void ProcessNoMod(double[] input, int sampleCount)
		{
			var delayedIndex = index - SampleDelay;
			if (delayedIndex < 0) delayedIndex += bufferSize;

			for (int i = 0; i < sampleCount; i++)
			{
				var bufOut = buffer[delayedIndex];
				var inVal = input[i] + bufOut * Feedback;
				buffer[index] = inVal;
				output[i] = bufOut - inVal * Feedback;

				index++;
				delayedIndex++;
				if (index >= bufferSize) index -= bufferSize;
				if (delayedIndex >= bufferSize) delayedIndex -= bufferSize;
				samplesProcessed++;
			}
		}

		private void ProcessWithMod(double[] input, int sampleCount)
		{
			for (int i = 0; i < sampleCount; i++)
			{
				if (samplesProcessed == ModulationUpdateRate)
					Update();

				var bufOut = Get(delayA) * gainA + Get(delayB) * gainB;
				var inVal = input[i] + bufOut * Feedback;
				buffer[index] = inVal;
				output[i] = bufOut - inVal * Feedback;

				index++;
				if (index >= bufferSize) index -= bufferSize;
				samplesProcessed++;
			}
		}

		private double Get(int delay)
		{
			var idx = index - delay;
			if (idx < 0)
				idx += bufferSize;

			return buffer[idx];
		}

		private void Update()
		{
			modPhase += ModRate * ModulationUpdateRate;
			if (modPhase > 1) modPhase -= 1;

			var mod = FastSin.Get(modPhase);
			var totalDelay = SampleDelay + ModAmount * mod;
			
			delayA = (int)totalDelay;
			delayB = (int)totalDelay + 1;

			var partial = totalDelay - delayA;

			gainA = 1 - partial;
			gainB = partial;

			samplesProcessed = 0;
		}
	}
}
