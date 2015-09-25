#include "DelayLine.h"
#include "Utils.h"
#include <iostream>

namespace CloudSeed
{
	DelayLine::DelayLine(int bufferSize, int samplerate)
		: lowPass(samplerate)
		, delay(bufferSize, 10000)
		, diffuser(bufferSize, samplerate)
		, lowShelf(AudioLib::Biquad::FilterType::LowShelf, samplerate)
		, highShelf(AudioLib::Biquad::FilterType::HighShelf, samplerate)
	{
		this->SampleResolution = 32;
		this->Undersampling = 1;

		this->bufferSize = bufferSize;
		tempBuffer = new double[bufferSize];
		mixedBuffer = new double[bufferSize];
		filterOutputBuffer = new double[bufferSize];
		
		lowShelf.Slope = 1.0;
		lowShelf.SetGainDb(-20);
		lowShelf.Frequency = 20;

		highShelf.Slope = 1.0;
		highShelf.SetGainDb(-20);
		highShelf.Frequency = 19000;

		lowPass.SetCutoffHz(1000);
		lowShelf.Update();
		highShelf.Update();
		SetSamplerate(samplerate);
	}

	DelayLine::~DelayLine()
	{
		std::cout << "Deleting DelayLine " << std::endl;
		delete tempBuffer;
		delete mixedBuffer;
		delete filterOutputBuffer;
	}

	int DelayLine::GetSamplerate()
	{
		return samplerate;
	}

	void DelayLine::SetSamplerate(int samplerate)
	{
		this->samplerate = samplerate;
		diffuser.SetSamplerate(samplerate);
		lowPass.SetSamplerate(samplerate);
		lowShelf.SetSamplerate(samplerate);
		highShelf.SetSamplerate(samplerate);
	}

	vector<double> DelayLine::GetDiffuserSeeds()
	{
		return diffuser.GetSeeds();
	}

	void DelayLine::SetDiffuserSeeds(vector<double> seeds)
	{
		diffuser.SetSeeds(seeds);
	}

	void DelayLine::SetDelay(int delaySamples)
	{
		delay.SampleDelay = delaySamples;
	}

	void DelayLine::SetFeedback(double feedb)
	{
		feedback = feedb;
	}

	void DelayLine::SetDiffuserDelay(int delaySamples)
	{
		diffuser.SetDelay(delaySamples);
	}

	void DelayLine::SetDiffuserFeedback(double feedb)
	{
		diffuser.SetFeedback(feedb);
	}

	void DelayLine::SetDiffuserStages(int stages)
	{
		diffuser.Stages = stages;
	}

	void DelayLine::SetLowShelfGain(double gain)
	{
		lowShelf.SetGain(gain);
		lowShelf.Update();
	}

	void DelayLine::SetLowShelfFrequency(double frequency)
	{
		lowShelf.Frequency = frequency;
		lowShelf.Update();
	}

	void DelayLine::SetHighShelfGain(double gain)
	{
		highShelf.SetGain(gain);
		highShelf.Update();
	}

	void DelayLine::SetHighShelfFrequency(double frequency)
	{
		highShelf.Frequency = frequency;
		highShelf.Update();
	}

	void DelayLine::SetCutoffFrequency(double frequency)
	{
		lowPass.SetCutoffHz(frequency);
	}

	void DelayLine::SetLineModAmount(double amount)
	{
		delay.ModAmount = amount;
	}

	void DelayLine::SetLineModRate(double rate)
	{
		delay.ModRate = rate;
	}

	void DelayLine::SetDiffuserModAmount(double amount)
	{
		diffuser.SetModulationEnabled(amount > 0.0);
		diffuser.SetModAmount(amount);
	}

	void DelayLine::SetDiffuserModRate(double rate)
	{
		diffuser.SetModRate(rate);
	}

	void DelayLine::SetInterpolationEnabled(bool value)
	{
		diffuser.SetInterpolationEnabled(value);
	}

	double* DelayLine::GetOutput()
	{
		if (LateStageTap)
		{
			if (DiffuserEnabled)
				return diffuser.GetOutput();
			else
				return mixedBuffer;
		}
		else
		{
			return delay.GetOutput();
		}
	}

	void DelayLine::Process(double* input, int sampleCount)
	{
		for (int i = 0; i < sampleCount; i++)
			mixedBuffer[i] = input[i] + filterOutputBuffer[i] * feedback;

		if (LateStageTap)
		{
			if (DiffuserEnabled)
			{
				diffuser.Process(mixedBuffer, sampleCount);
				Utils::BitcrushAndReduce(diffuser.GetOutput(), mixedBuffer, sampleCount, Undersampling, SampleResolution);
				delay.Process(mixedBuffer, sampleCount);
			}
			else
			{
				Utils::BitcrushAndReduce(mixedBuffer, mixedBuffer, sampleCount, Undersampling, SampleResolution);
				delay.Process(mixedBuffer, sampleCount);
			}

			Utils::Copy(delay.GetOutput(), tempBuffer, sampleCount);
		}
		else
		{
			
			if (DiffuserEnabled)
			{
				Utils::BitcrushAndReduce(mixedBuffer, mixedBuffer, sampleCount, Undersampling, SampleResolution);
				delay.Process(mixedBuffer, sampleCount);
				diffuser.Process(delay.GetOutput(), sampleCount);
				Utils::Copy(diffuser.GetOutput(), tempBuffer, sampleCount);
			}
			else
			{
				Utils::BitcrushAndReduce(mixedBuffer, mixedBuffer, sampleCount, Undersampling, SampleResolution);
				delay.Process(mixedBuffer, sampleCount);
				Utils::Copy(delay.GetOutput(), tempBuffer, sampleCount);
			}
		}

		if (LowShelfEnabled)
			lowShelf.Process(tempBuffer, tempBuffer, sampleCount);
		if (HighShelfEnabled)
			highShelf.Process(tempBuffer, tempBuffer, sampleCount);
		if (CutoffEnabled)
			lowPass.Process(tempBuffer, tempBuffer, sampleCount);

		Utils::Copy(tempBuffer, filterOutputBuffer, sampleCount);
	}

	void DelayLine::ClearDiffuserBuffer()
	{
		diffuser.ClearBuffers();
	}

	void DelayLine::ClearBuffers()
	{
		delay.ClearBuffers();
		diffuser.ClearBuffers();
		lowShelf.ClearBuffers();
		highShelf.ClearBuffers();
		lowPass.Output = 0;

		for (int i = 0; i < bufferSize; i++)
		{
			tempBuffer[i] = 0.0;
			filterOutputBuffer[i] = 0.0;
		}
	}
}
