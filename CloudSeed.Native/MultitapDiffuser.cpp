
#include <memory>
#include <array>
#include "MultitapDiffuser.h"
#include "Utils.h"
#include "AudioLib/ShaRandom.h"

namespace CloudSeed
{
	MultitapDiffuser::MultitapDiffuser(int bufferSize)
	{
		len = bufferSize;
		index = 0;
		AudioLib::ShaRandom rand;
		auto seedData = rand.Generate(1, 100);
		for (size_t i = 0; i < seedData.size(); i++)
			seeds[i] = seedData[i];
	}

	double* MultitapDiffuser::GetSeeds()
	{
		return seeds;
	}

	void MultitapDiffuser::SetSeeds(double* seeds)
	{
		for (size_t i = 0; i < SeedValueCount; i++)
			this->seeds[i] = seeds[i];

		Update();
	}
	

	double* MultitapDiffuser::GetOutput()
	{ 
		return output;
	}

	void MultitapDiffuser::SetTapCount(int tapCount)
	{
		count = tapCount;
		Update();
	}

	void MultitapDiffuser::SetTapLength(int tapLength)
	{
		length = tapLength;
		Update();
	}

	void MultitapDiffuser::SetTapDecay(double tapDecay)
	{
		decay = tapDecay;
		Update();
	}

	void MultitapDiffuser::SetTapGain(double tapGain)
	{
		gain = tapGain;
		Update();
	}

	void MultitapDiffuser::Update()
	{
		int s = 0;
		auto rand = [&](){return this->seeds[s++]; };

		if (count < 1)
			count = 1;

		if (length < count)
			length = count;

		tapGains.clear();
		tapGains.resize(count);
		tapPosition.clear();
		tapPosition.resize(count);

		vector<double> tapData(count, 0.0);

		auto sum = 0.0;
		for (size_t i = 0; i < count; i++)
		{
			auto val = 0.1 + rand();
			tapData[i] = val;
			sum += val;
		}
		
		auto scale = length / sum;
		tapPosition[0] = 0;

		for (int i = 1; i < count; i++)
		{
			tapPosition[i] = tapPosition[i - 1] + (int)(tapData[i] * scale);
		}

		for (int i = 0; i < count; i++)
		{
			auto g = gain * (1 - decay * (i / (double)count));
			tapGains[i] = g * (2 * rand() - 1);
		}

		tapGains[0] = (1 - gain) + tapGains[0] * gain;
	}

	void MultitapDiffuser::Process(double* input, int sampleCount)
	{
		for (int i = 0; i < sampleCount; i++)
		{
			if (index < 0) index += len;
			buffer[index] = input[i];
			output[i] = 0.0;

			for (int j = 0; j < count; j++)
			{
				auto idx = (index + tapPosition[j]) % len;
				output[i] += buffer[idx] * tapGains[j];
			}

			index--;
		}
	}

	void MultitapDiffuser::ClearBuffers()
	{
		Utils::ZeroBuffer(buffer, len);
	}
}
