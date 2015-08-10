#include "DelayLine.h"

namespace CloudSeed
{
	
	DelayLine::DelayLine(int bufferSize, int samplerate)
		: lowPass(samplerate)
	{
		//delay = new ModulatedDelay(bufferSize, 10000);
		//diffuser = new AllpassDiffuser(bufferSize, samplerate){ ModulationEnabled = false };
		
		tempBuffer = new double[bufferSize];
		filterOutputBuffer = new double[bufferSize];
		
		//lowShelf = new Biquad(Biquad.FilterType.LowShelf, samplerate){ Slope = 1.0, GainDB = -20, Frequency = 20 };
		//highShelf = new Biquad(Biquad.FilterType.HighShelf, samplerate){ Slope = 1.0, GainDB = -20, Frequency = 19000 };
		lowPass.SetCutoffHz(1000);
		//lowShelf.Update();
		//highShelf.Update();
		SetSamplerate(samplerate);
	}

	DelayLine::~DelayLine()
	{
		delete tempBuffer;
		delete filterOutputBuffer;
	}

	int DelayLine::GetSamplerate()
	{
		return samplerate;
	}

	void DelayLine::SetSamplerate(int samplerate)
	{
		this->samplerate = samplerate;
		//diffuser.Samplerate = samplerate;
		//lowPass.Samplerate = samplerate;
		//lowShelf.Samplerate = samplerate;
		//highShelf.Samplerate = samplerate;
	}

	double* DelayLine::GetDiffuserSeeds()
	{
		//return diffuser.Seeds;
		return 0;
	}

	void DelayLine::SetDiffuserSeeds(double* seeds)
	{
		//diffuser.Seeds = value;
	}

	void DelayLine::SetDelay(int delaySamples)
	{
		//delay.SampleDelay = delaySamples;
	}

	void DelayLine::SetFeedback(double feedb)
	{
		feedback = feedb;
	}

	void DelayLine::SetDiffuserDelay(int delaySamples)
	{
		//diffuser.SetDelay(delaySamples);
	}

	void DelayLine::SetDiffuserFeedback(double feedb)
	{
		//diffuser.SetFeedback(feedb);
	}

	void DelayLine::SetDiffuserStages(int stages)
	{
		//diffuser.Stages = stages;
	}

	void DelayLine::SetLowShelfGain(double gain)
	{
		//lowShelf.Gain = gain;
		//lowShelf.Update();
	}

	void DelayLine::SetLowShelfFrequency(double frequency)
	{
		//lowShelf.Frequency = frequency;
		//lowShelf.Update();
	}

	void DelayLine::SetHighShelfGain(double gain)
	{
		//highShelf.Gain = gain;
		//highShelf.Update();
	}

	void DelayLine::SetHighShelfFrequency(double frequency)
	{
		//highShelf.Frequency = frequency;
		//highShelf.Update();
	}

	void DelayLine::SetCutoffFrequency(double frequency)
	{
		//lowPass.CutoffHz = frequency;
	}

	void DelayLine::SetModAmount(double amount)
	{
		//delay.ModAmount = amount;
	}

	void DelayLine::SetModRate(double rate)
	{
		//delay.ModRate = rate;
	}

	double* DelayLine::GetOutput()
	{ 
		//return delay.Output;
		return 0;
	}

	void DelayLine::Process(double* input, int sampleCount)
	{
		/*var feedbackBuffer = DiffuserEnabled ? diffuser.Output : filterOutputBuffer;

		for (int i = 0; i < sampleCount; i++)
			tempBuffer[i] = input[i] + feedbackBuffer[i] * feedback;

		delay.Process(tempBuffer, sampleCount);
		delay.Output.Copy(tempBuffer, sampleCount);

		if (LowShelfEnabled)
			lowShelf.Process(tempBuffer, tempBuffer, sampleCount);
		if (HighShelfEnabled)
			highShelf.Process(tempBuffer, tempBuffer, sampleCount);
		if (CutoffEnabled)
			lowPass.Process(tempBuffer, tempBuffer, sampleCount);

		tempBuffer.Copy(filterOutputBuffer, sampleCount);

		if (DiffuserEnabled)
		{
			diffuser.Process(filterOutputBuffer, sampleCount);
		}*/
	}

	void DelayLine::ClearBuffers()
	{
		/*delay.ClearBuffers();
		diffuser.ClearBuffers();
		lowShelf.ClearBuffers();
		highShelf.ClearBuffers();
		lowPass.Output = 0;
		tempBuffer.Zero();
		filterOutputBuffer.Zero();*/
	}


}