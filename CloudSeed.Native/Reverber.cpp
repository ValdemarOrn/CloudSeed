
#include "Reverber.h"

Reverber::Reverber()
{
	Samplerate = 48000;

	for (int i = 0; i < PARAM_COUNT; i++)
		Parameters[i] = 0.0;
	
	for (int i = 0; i < MAX_TAP_COUNT; i++)
	{
		TapsIndexes[i] = 0.0;
		TapAmplitudes[i] = 0.0;
	}

	for (int i = 0; i < BUF_LEN; i++)
	{
		EarlyBuffer[i] = 0.0;
		OutBuffer[i] = 0.0;
	}

	TapCount = 0;
	EarlyI = 0;
	OutI = 0;
	SampleCounter = 0;
}

double* Reverber::GetParameters()
{
	return &(this->Parameters[0]);
}

void Reverber::SetSamplerate(double value)
{
	Samplerate = value;
	for (int i = 0; i < ALLPASS_COUNT; i++)
	{
		AllpassModules[i].Samplerate = Samplerate;
		AllpassModules[i].SetHiCut(AllpassModules[i].HiCut);
	}

	RecalculateTapIndexes();
}

void Reverber::SetTaps(double* indexOffsets, double* amplitudes, int count)
{
	if(count > MAX_TAP_COUNT)
		count = MAX_TAP_COUNT;

	TapCount = count;
	for (int i = 0; i < TapCount; i++)
	{
		TapIndexOffsets[i] = indexOffsets[i];
		TapAmplitudes[i] = amplitudes[i];
	}

	RecalculateTapIndexes();
}

void Reverber::SetLate(double* feedback, int* delaySamples)
{
	for (int i = 0; i < ALLPASS_COUNT; i++)
	{
		AllpassModules[i].Feedback = feedback[i];
		AllpassModules[i].DelaySamples = delaySamples[i];
	}
}

void Reverber::SetHiCut(double* fc, double* amount)
{
	for (int i = 0; i < ALLPASS_COUNT; i++)
	{
		AllpassModules[i].SetHiCut(fc[i]);
		AllpassModules[i].HiCutAmount = amount[i];
	}
}

void Reverber::SetAllpassMod(double* freq, double* amount)
{
	for (int i = 0; i < ALLPASS_COUNT; i++)
	{
		AllpassModules[i].SetModFreq(freq[i]);
		AllpassModules[i].ModAmount = amount[i];
	}
}

void Reverber::Process(double* input, double* output, int len)
{
	double globalFeedback = Parameters[Parameter::GlobalFeedback];
	int globalDelay = (int)Parameters[Parameter::GlobalDelay];
	double stageCount = Parameters[Parameter::StageCount];
	double dry = Parameters[Parameter::Dry];
	double wet = Parameters[Parameter::Wet];

	for (int i = 0; i < len; i++)
	{
		SampleCounter++;

		if (SampleCounter % 16 == 0)
		{
			for (int i = 0; i < ALLPASS_COUNT; i++)
				AllpassModules[i].UpdateMod(16);

			SampleCounter = 0;
		}

		EarlyBuffer[EarlyI] = input[i];
		
		double taps[MAX_TAP_COUNT];
		for (int i = 0; i < TapCount; i++)
		{
			int idx = (EarlyI + TapsIndexes[i]) & MODULO;
			taps[i] = EarlyBuffer[idx];
		}

		double outputEarly = 0.0;
		for (int i = 0; i < TapCount; i++)
		{
			outputEarly += taps[i] * TapAmplitudes[i];
		}

		double d = outputEarly + globalFeedback * OutBuffer[(OutI + globalDelay) & MODULO];

		for (int i = 0; i < stageCount; i++)
			d = AllpassModules[i].Process(d);
			
		OutBuffer[OutI] = d;

		EarlyI--;
		if (EarlyI < 0)
			EarlyI += BUF_LEN;

		OutI--;
		if (OutI < 0)
			OutI += BUF_LEN;

		output[i] = dry * input[i] + wet * d;

		// Early only output
		//output[i] = dry * input[i] + wet * outputEarly;
	}
}

void Reverber::RecalculateTapIndexes()
{
	int predelay = (int)(Parameters[Parameter::Predelay] / 1000.0 * Samplerate);
	int earlySizeSamples = Parameters[Parameter::EarlySize] / 1000.0 * Samplerate;

	for (int i = 0; i < TapCount; i++)
		TapsIndexes[i] = predelay + TapIndexOffsets[i] * earlySizeSamples;
}