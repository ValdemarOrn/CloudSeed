using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioLib;

namespace CloudSeed
{
	public class ModulatedDelay
	{
		private const int ModulationUpdateRate = 8;

		private readonly double[] buffer;
		private readonly double[] output;
		private readonly int bufferSize;
		private int writeIndex;
		private int readIndexA;
		private int readIndexB;
		private int samplesProcessed;

		private double modPhase;
		private double gainA;
		private double gainB;

		public int SampleDelay;

		public double ModAmount;
		public double ModRate;
		
		public ModulatedDelay(int bufferSize, int sampleDelay)
		{
			this.bufferSize = bufferSize;
			this.buffer = new double[bufferSize];
			this.output = new double[bufferSize];
			this.SampleDelay = sampleDelay;
			writeIndex = 0;
			Update();
		}

		public double[] Output { get { return output; } }

		public void Process(double[] input, int sampleCount)
		{
			for (int i = 0; i < sampleCount; i++)
			{
				if (samplesProcessed == ModulationUpdateRate)
					Update();

				buffer[writeIndex] = input[i];
				output[i] = buffer[readIndexA] * gainA + buffer[readIndexB] * gainB;

				writeIndex++;
				readIndexA++;
				readIndexB++;
				if (writeIndex >= bufferSize) writeIndex -= bufferSize;
				if (readIndexA >= bufferSize) readIndexA -= bufferSize;
				if (readIndexB >= bufferSize) readIndexB -= bufferSize;
				samplesProcessed++;
			}
		}

		public void ClearBuffers()
		{
			buffer.Zero();
		}

		private void Update()
		{
			modPhase += ModRate * ModulationUpdateRate;
			if (modPhase > 1) modPhase -= 1;

			var mod = FastSin.Get(modPhase);
			var totalDelay = SampleDelay + ModAmount * mod;

			var delayA = (int)totalDelay;
			var delayB = (int)totalDelay + 1;

			var partial = totalDelay - delayA;

			gainA = 1 - partial;
			gainB = partial;
			
			readIndexA = writeIndex - delayA;
			readIndexB = writeIndex - delayB;
			if (readIndexA < 0) readIndexA += bufferSize;
			if (readIndexB < 0) readIndexB += bufferSize;

			samplesProcessed = 0;
		}
	}
}
