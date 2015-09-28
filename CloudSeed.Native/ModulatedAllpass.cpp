#include "ModulatedAllpass.h"
#include "FastSin.h"
#include "Utils.h"

namespace CloudSeed
{
	int id = 1;

	ModulatedAllpass::ModulatedAllpass(int bufferSize, int sampleDelay)
	{
		this->InterpolationEnabled = true;
		this->Id = id++;
		this->bufferSize = bufferSize;
		delayBuffer = new double[DelayBufferSamples];
		output = new double[bufferSize];
		SampleDelay = sampleDelay;
		index = DelayBufferSamples - 1;
		modPhase = 0.01 + 0.98 * std::rand() / (double)RAND_MAX;
		ModRate = 0.0;
		ModAmount = 0.0;
		Update();
	}

	ModulatedAllpass::~ModulatedAllpass()
	{
		delete delayBuffer;
		delete output;
	}

	double* ModulatedAllpass::GetOutput()
	{
		return output;
	}

	void ModulatedAllpass::ClearBuffers()
	{
		Utils::ZeroBuffer(delayBuffer, DelayBufferSamples);
		Utils::ZeroBuffer(output, bufferSize);
	}

	void ModulatedAllpass::Process(double* input, int sampleCount)
	{
		if (ModulationEnabled)
			ProcessWithMod(input, sampleCount);
		else
			ProcessNoMod(input, sampleCount);
	}
	
	void ModulatedAllpass::ProcessNoMod(double* input, int sampleCount)
	{
		auto delayedIndex = index - SampleDelay;
		if (delayedIndex < 0) delayedIndex += DelayBufferSamples;

		for (int i = 0; i < sampleCount; i++)
		{			
			auto bufOut = delayBuffer[delayedIndex];
			auto inVal = input[i] + bufOut * Feedback;

			delayBuffer[index] = inVal;
			output[i] = bufOut - inVal * Feedback;

			index++;
			delayedIndex++;
			if (index >= DelayBufferSamples) index -= DelayBufferSamples;
			if (delayedIndex >= DelayBufferSamples) delayedIndex -= DelayBufferSamples;
			samplesProcessed++;
		}
	}

	void ModulatedAllpass::ProcessWithMod(double* input, int sampleCount)
	{
		for (int i = 0; i < sampleCount; i++)
		{
			if (samplesProcessed >= ModulationUpdateRate)
				Update();

			double bufOut;

			if (InterpolationEnabled)
			{
				int idxA = index - delayA;
				int idxB = index - delayB;
				idxA += DelayBufferSamples * (idxA < 0); // modulo
				idxB += DelayBufferSamples * (idxB < 0); // modulo

				bufOut = delayBuffer[idxA] * gainA + delayBuffer[idxB] * gainB;
			}
			else
			{
				int idxA = index - delayA;
				idxA += DelayBufferSamples * (idxA < 0); // modulo
				bufOut = delayBuffer[idxA];
			}

			auto inVal = input[i] + bufOut * Feedback;
			delayBuffer[index] = inVal;
			output[i] = bufOut - inVal * Feedback;

			index++;
			if (index >= DelayBufferSamples) index -= DelayBufferSamples;
			samplesProcessed++;
		}
	}

	double inline ModulatedAllpass::Get(int delay)
	{
		int idx = index - delay;
		if (idx < 0)
			idx += DelayBufferSamples;

		return delayBuffer[idx];
	}

	void ModulatedAllpass::Update()
	{
		modPhase += ModRate * ModulationUpdateRate;
		if (modPhase > 1)
			modPhase = std::fmod(modPhase, 1.0);

		auto mod = FastSin::Get(modPhase);
		auto totalDelay = SampleDelay + ModAmount * mod;

		delayA = (int)totalDelay;
		delayB = (int)totalDelay + 1;

		auto partial = totalDelay - delayA;

		gainA = 1 - partial;
		gainB = partial;

		samplesProcessed = 0;
	}

}