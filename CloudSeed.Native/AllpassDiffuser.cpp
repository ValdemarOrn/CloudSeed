#include "AllpassDiffuser.h"
#include "AudioLib\ShaRandom.h"
#include "Utils.h"
#include <iostream>

namespace CloudSeed
{
	AllpassDiffuser::AllpassDiffuser(int bufferSize, int samplerate)
	{
		this->bufferSize = bufferSize;
		filters.push_back(new ModulatedAllpass(bufferSize, 100));
		filters.push_back(new ModulatedAllpass(bufferSize, 100));
		filters.push_back(new ModulatedAllpass(bufferSize, 100));
		filters.push_back(new ModulatedAllpass(bufferSize, 100));

		output = new double[bufferSize];
		SetSeeds(AudioLib::ShaRandom::Generate(23, 12));
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
		samplerate = samplerate;
		SetModRate(modRate);
	}

	vector<double> AllpassDiffuser::GetSeeds()
	{
		return seeds;
	}

	void AllpassDiffuser::SetSeeds(vector<double> value)
	{
		seeds = value;
		Update();
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
			filters[i]->ModAmount = amount * (0.8 + 0.2 * seeds[i + 4]);
	}

	void AllpassDiffuser::SetModRate(double rate)
	{
		modRate = rate;

		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->ModRate = rate * (0.5 + 0.5 * seeds[i + 8]) / samplerate;
	}

	void AllpassDiffuser::Update()
	{
		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->SampleDelay = (int)(delay * (0.2 + 0.8 * seeds[i]));
	}

	void AllpassDiffuser::Process(double* input, int sampleCount)
	{
		filters[0]->Process(input, sampleCount);
		filters[1]->Process(filters[0]->GetOutput(), sampleCount);
		filters[2]->Process(filters[1]->GetOutput(), sampleCount);
		filters[3]->Process(filters[2]->GetOutput(), sampleCount);

		output = filters[Stages - 1]->GetOutput();
	}

	void AllpassDiffuser::ClearBuffers()
	{
		Utils::ZeroBuffer(output, bufferSize);

		for (size_t i = 0; i < filters.size(); i++)
			filters[i]->ClearBuffers();
	}

}