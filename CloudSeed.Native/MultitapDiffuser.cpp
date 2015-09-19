
#include <memory>
#include <array>
#include "MultitapDiffuser.h"
#include "Utils.h"
#include "AudioLib/ShaRandom.h"
#include <iostream>

namespace CloudSeed
{
	MultitapDiffuser::MultitapDiffuser(int bufferSize)
	{
		len = bufferSize;
		buffer = new double[bufferSize];
		output = new double[bufferSize];
		index = 0;
		seeds = AudioLib::ShaRandom::Generate(1, 100);
	}

	MultitapDiffuser::~MultitapDiffuser()
	{
		std::cout << "Deleting MultitapDiffuser " << std::endl;
		delete buffer;
		delete output;
	}

	vector<double> MultitapDiffuser::GetSeeds()
	{
		return seeds;
	}

	void MultitapDiffuser::SetSeeds(vector<double> seeds)
	{
		this->seeds = seeds;
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
		auto rand = [&](){return seeds[s++]; };

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
		int* const tapPos = &tapPosition[0];
		double* const tapGain = &tapGains[0];

		for (int i = 0; i < sampleCount; i++)
		{
			if (index < 0) index += len;
			buffer[index] = input[i];
			output[i] = 0.0;

			for (int j = 0; j < count; j++)
			{
				auto idx = (index + tapPos[j]) % len;
				output[i] += buffer[idx] * tapGain[j];
			}

			index--;
		}
	}

	void MultitapDiffuser::ClearBuffers()
	{
		Utils::ZeroBuffer(buffer, len);
		Utils::ZeroBuffer(output, len);
	}
}
