#ifndef MODULATEDDELAY
#define MODULATEDDELAY

#include "ModulatedDelay.h"
#include "Utils.h"
#include "FastSin.h"

namespace CloudSeed
{
	class ModulatedDelay
	{
	private:

		const int DelayBufferSamples = 96000; // 500ms at 192Khz
		const int ModulationUpdateRate = 8;

		double* delayBuffer;
		double* output;
		int bufferSize;
		int writeIndex;
		int readIndexA;
		int readIndexB;
		int samplesProcessed;

		double modPhase;
		double gainA;
		double gainB;

	public:
		int SampleDelay;

		double ModAmount;
		double ModRate;

		ModulatedDelay(int bufferSize, int sampleDelay)
		{
			this->bufferSize = bufferSize;
			this->delayBuffer = new double[DelayBufferSamples];
			this->output = new double[bufferSize];
			this->SampleDelay = sampleDelay;
			writeIndex = 0;
			modPhase = 0.01 + 0.98 * (std::rand() / (double)RAND_MAX);
			ModRate = 0.0;
			ModAmount = 0.0;
			Update();
		}

		~ModulatedDelay()
		{
			delete delayBuffer;
			delete output;
		}

		double* GetOutput()
		{
			return output;
		}

		void Process(double* input, int sampleCount)
		{
			for (int i = 0; i < sampleCount; i++)
			{
				if (samplesProcessed == ModulationUpdateRate)
					Update();

				delayBuffer[writeIndex] = input[i];
				output[i] = delayBuffer[readIndexA] * gainA + delayBuffer[readIndexB] * gainB;

				writeIndex++;
				readIndexA++;
				readIndexB++;
				if (writeIndex >= DelayBufferSamples) writeIndex -= DelayBufferSamples;
				if (readIndexA >= DelayBufferSamples) readIndexA -= DelayBufferSamples;
				if (readIndexB >= DelayBufferSamples) readIndexB -= DelayBufferSamples;
				samplesProcessed++;
			}
		}

		void ClearBuffers()
		{
			Utils::ZeroBuffer(delayBuffer, DelayBufferSamples);
			Utils::ZeroBuffer(output, bufferSize);
		}


	private:
		void Update()
		{
			modPhase += ModRate * ModulationUpdateRate;
			if (modPhase > 1)
				modPhase = std::fmod(modPhase, 1.0);

			auto mod = FastSin::Get(modPhase);
			auto totalDelay = SampleDelay + ModAmount * mod;

			auto delayA = (int)totalDelay;
			auto delayB = (int)totalDelay + 1;

			auto partial = totalDelay - delayA;

			gainA = 1 - partial;
			gainB = partial;

			readIndexA = writeIndex - delayA;
			readIndexB = writeIndex - delayB;
			if (readIndexA < 0) readIndexA += DelayBufferSamples;
			if (readIndexB < 0) readIndexB += DelayBufferSamples;

			samplesProcessed = 0;
		}
	};
}

#endif