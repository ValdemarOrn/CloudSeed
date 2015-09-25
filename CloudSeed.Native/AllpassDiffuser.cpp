#include "AllpassDiffuser.h"
#include "AudioLib\ShaRandom.h"
#include "Utils.h"
#include <iostream>

namespace CloudSeed
{
	AllpassDiffuser::AllpassDiffuser(int bufferSize, int samplerate)
	{
		this->bufferSize = bufferSize;
		for (int i = 0; i < MaxStageCount; i++)
		{
			filters.push_back(new ModulatedAllpass(bufferSize, 100));
		}
		
		output = new double[bufferSize];
		crossSeed = 0.0;
		seed = 23456;
		UpdateSeeds();
		Stages = 1;

		SetSamplerate(samplerate);
	}

	AllpassDiffuser::~AllpassDiffuser()
	{
		std::cout << "Deleting AllpassDiffuser " << std::endl;
		delete output;

		for (auto filter : filters)
			delete filter;
	}

	int AllpassDiffuser::GetSamplerate()
	{
		return samplerate;
	}

	void AllpassDiffuser::SetSamplerate(int samplerate)
	{
		this->samplerate = samplerate;
		SetModRate(modRate);
	}

	void AllpassDiffuser::SetSeed(int seed)
	{
		this->seed = seed;
		UpdateSeeds();
	}

	void AllpassDiffuser::SetCrossSeed(double crossSeed)
	{
		this->crossSeed = crossSeed;
		UpdateSeeds();
	}

	bool AllpassDiffuser::GetModulationEnabled()
	{
		return filters[0]->ModulationEnabled;
	}

	void AllpassDiffuser::SetModulationEnabled(bool value)
	{
		for(auto filter : filters)
			filter->ModulationEnabled = value;
	}

	void AllpassDiffuser::SetInterpolationEnabled(bool enabled)
	{
		for (auto filter : filters)
			filter->InterpolationEnabled = enabled;
	}

	double* AllpassDiffuser::GetOutput()
	{
		return output;
	}


	void AllpassDiffuser::SetDelay(int delaySamples)
	{
		delay = delaySamples;
		Update();
	}

	void AllpassDiffuser::SetFeedback(double feedback)
	{
		for (auto filter : filters)
			filter->Feedback = feedback;
	}

	void AllpassDiffuser::SetModAmount(double amount)
	{
		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->ModAmount = amount * (0.7 + 0.3 * seedValues[MaxStageCount + i]);
	}

	void AllpassDiffuser::SetModRate(double rate)
	{
		modRate = rate;

		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->ModRate = rate * (0.7 + 0.3 * seedValues[MaxStageCount * 2 + i]) / samplerate;
	}

	void AllpassDiffuser::Update()
	{
		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->SampleDelay = (int)(delay * (0.5 + 1.0 * seedValues[i]));
	}

	void AllpassDiffuser::Process(double* input, int sampleCount)
	{
		ModulatedAllpass** filterPtr = &filters[0];

		filterPtr[0]->Process(input, sampleCount);

		for (int i = 1; i < Stages; i++)
		{
			filterPtr[i]->Process(filterPtr[i - 1]->GetOutput(), sampleCount);
		}
		
		output = filterPtr[Stages - 1]->GetOutput();
	}

	void AllpassDiffuser::ClearBuffers()
	{
		Utils::ZeroBuffer(output, bufferSize);

		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->ClearBuffers();
	}

	void AllpassDiffuser::UpdateSeeds()
	{
		this->seedValues = AudioLib::ShaRandom::Generate(seed, AllpassDiffuser::MaxStageCount * 3, crossSeed);
		Update();
	}

}