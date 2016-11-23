
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
		buffer = new double[bufferSize];
		output = new double[bufferSize];
		index = 0;
		count = 1;
		length = 1;
		gain = 1.0;
		decay = 0.0;
		crossSeed = 0.0;
		UpdateSeeds();
	}

	MultitapDiffuser::~MultitapDiffuser()
	{
		delete buffer;
		delete output;
	}

	void MultitapDiffuser::SetSeed(int seed)
	{
		this->seed = seed;
		UpdateSeeds();
	}

	void MultitapDiffuser::SetCrossSeed(double crossSeed)
	{
		this->crossSeed = crossSeed;
		UpdateSeeds();
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

	void MultitapDiffuser::Process(double* input, int sampleCount)
	{
		// prevents race condition when parameters are updated from Gui
		if (isDirty)
		{
			tapGainsTemp = tapGains;
			tapPositionTemp = tapPosition;
			countTemp = count;
			isDirty = false;
		}

		int* const tapPos = &tapPositionTemp[0];
		double* const tapGain = &tapGainsTemp[0];
		const int cnt = countTemp;

		for (int i = 0; i < sampleCount; i++)
		{
			if (index < 0) index += len;
			buffer[index] = input[i];
			output[i] = 0.0;

			for (int j = 0; j < cnt; j++)
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
	
	void MultitapDiffuser::Update()
	{
		vector<double> newTapGains;
		vector<int> newTapPosition;

		int s = 0;
		auto rand = [&]() {return seedValues[s++]; };

		if (count < 1)
			count = 1;

		if (length < count)
			length = count;

		// used to adjust the volume of the overall output as it grows when we add more taps
		double tapCountFactor = 1.0 / (1 + std::sqrt(count / MaxTaps));

		newTapGains.resize(count);
		newTapPosition.resize(count);

		vector<double> tapData(count, 0.0);

		auto sumLengths = 0.0;
		for (size_t i = 0; i < count; i++)
		{
			auto val = 0.1 + rand();
			tapData[i] = val;
			sumLengths += val;
		}

		auto scaleLength = length / sumLengths;
		newTapPosition[0] = 0;

		for (int i = 1; i < count; i++)
		{
			newTapPosition[i] = newTapPosition[i - 1] + (int)(tapData[i] * scaleLength);
		}

		double sumGains = 0.0;
		double lastTapPos = newTapPosition[count - 1];
		for (int i = 0; i < count; i++)
		{
			// when decay set to 0, there is no decay, when set to 1, the gain at the last sample is 0.01 = -40dB
			auto g = std::pow(10, -decay * 2 * newTapPosition[i] / (double)(lastTapPos + 1));
			
			auto tap = (2 * rand() - 1) * tapCountFactor;
			newTapGains[i] = tap * g * gain;
		}

		// Set the tap vs. clean mix
		newTapGains[0] = (1 - gain);

		this->tapGains = newTapGains;
		this->tapPosition = newTapPosition;
		isDirty = true;
	}
	
	void MultitapDiffuser::UpdateSeeds()
	{
		this->seedValues = AudioLib::ShaRandom::Generate(seed, 100, crossSeed);
		Update();
	}
}
