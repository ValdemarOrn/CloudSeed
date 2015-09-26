#include "ModulatedDelay.h"
#include "Utils.h"
#include "FastSin.h"

namespace CloudSeed
{
	ModulatedDelay::ModulatedDelay(int bufferSize, int sampleDelay)
	{
		this->bufferSize = bufferSize;
		this->buffer = new double[bufferSize];
		this->output = new double[bufferSize];
		this->SampleDelay = sampleDelay;
		writeIndex = 0;
		modPhase = 0.01 + 0.98 * (std::rand() / (double)RAND_MAX);
		ModRate = 0.0;
		ModAmount = 0.0;
		Update();
	}

	ModulatedDelay::~ModulatedDelay()
	{
		delete buffer;
		delete output;
	}

	double* ModulatedDelay::GetOutput()
	{
		return output;
	}

	void ModulatedDelay::Process(double* input, int sampleCount)
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

	void ModulatedDelay::ClearBuffers()
	{
		Utils::ZeroBuffer(buffer, bufferSize);
		Utils::ZeroBuffer(output, bufferSize);
	}

	void ModulatedDelay::Update()
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
		if (readIndexA < 0) readIndexA += bufferSize;
		if (readIndexB < 0) readIndexB += bufferSize;

		samplesProcessed = 0;
	}
}
