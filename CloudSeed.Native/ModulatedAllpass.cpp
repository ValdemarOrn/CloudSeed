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
		buffer = new double[bufferSize];
		output = new double[bufferSize];
		SampleDelay = sampleDelay;
		index = bufferSize - 1;
		modPhase = 0.01 + 0.98 * std::rand() / (double)RAND_MAX;
		ModRate = 0.0;
		ModAmount = 0.0;
		Update();
	}

	ModulatedAllpass::~ModulatedAllpass()
	{
		delete buffer;
		delete output;
	}

	double* ModulatedAllpass::GetOutput()
	{
		return output;
	}

	void ModulatedAllpass::ClearBuffers()
	{
		Utils::ZeroBuffer(buffer, bufferSize);
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
		if (delayedIndex < 0) delayedIndex += bufferSize;

		for (int i = 0; i < sampleCount; i++)
		{			
			auto bufOut = buffer[delayedIndex];
			auto inVal = input[i] + bufOut * Feedback;

			buffer[index] = inVal;
			output[i] = bufOut - inVal * Feedback;

			index++;
			delayedIndex++;
			if (index >= bufferSize) index -= bufferSize;
			if (delayedIndex >= bufferSize) delayedIndex -= bufferSize;
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
				idxA += bufferSize * (idxA < 0); // modulo
				idxB += bufferSize * (idxB < 0); // modulo

				bufOut = buffer[idxA] * gainA + buffer[idxB] * gainB;
			}
			else
			{
				int idxA = index - delayA;
				idxA += bufferSize * (idxA < 0); // modulo
				bufOut = buffer[idxA];
			}

			auto inVal = input[i] + bufOut * Feedback;
			buffer[index] = inVal;
			output[i] = bufOut - inVal * Feedback;

			index++;
			if (index >= bufferSize) index -= bufferSize;
			samplesProcessed++;
		}
	}

	double inline ModulatedAllpass::Get(int delay)
	{
		int idx = index - delay;
		if (idx < 0)
			idx += bufferSize;

		return buffer[idx];
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