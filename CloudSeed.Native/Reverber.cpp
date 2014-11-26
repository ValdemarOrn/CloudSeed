
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
		for (int j = 0; j < LATE_COUNT; j++)
			LateBuffers[i * LATE_COUNT + j] = 0.0;
	}

	TapCount = 0;
	EarlyI = 0;
	LateI = 0;
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

void Reverber::SetEarly(double* feedback, int* delaySamples)
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
	double globalFeedback = Parameters[Parameter::Feedback1];
	double globalDelay = Parameters[Parameter::Delay1] / 1000.0 * Samplerate;
	int lateDelays[LATE_COUNT];
	lateDelays[0] = (int)(globalDelay * 1.0);
	lateDelays[1] = (int)(globalDelay * 1.383453);
	lateDelays[2] = (int)(globalDelay * 1.26234);
	lateDelays[3] = (int)(globalDelay * 1.68834512);
	lateDelays[4] = (int)(globalDelay * 1.81246);
	lateDelays[5] = (int)(globalDelay * 1.377345);
	lateDelays[6] = (int)(globalDelay * 1.236458);
	lateDelays[7] = (int)(globalDelay * 1.57345);
	lateDelays[8] = (int)(globalDelay * 1.4264344);
	lateDelays[9] = (int)(globalDelay * 1.07126324);
	lateDelays[10] = (int)(globalDelay * 1.1426745);
	lateDelays[11] = (int)(globalDelay * 1.5123476);
	lateDelays[12] = (int)(globalDelay * 1.89344);
	lateDelays[13] = (int)(globalDelay * 1.6234);
	lateDelays[14] = (int)(globalDelay * 1.95);
	lateDelays[15] = (int)(globalDelay * 1.146234);
	
	double stageCount = Parameters[Parameter::StageCount];
	double dry = Parameters[Parameter::DryOut];
	double wet = Parameters[Parameter::WetOut];
	double earlyAmount = Parameters[Parameter::EarlyOut];

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

		double tapSum = 0.0;
		for (int i = 0; i < TapCount; i++)
		{
			tapSum += taps[i] * TapAmplitudes[i];
		}

		if (tapSum > 5.0)
			tapSum = 5.0;
		else if (tapSum < -5.0)
			tapSum = -5.0;

		//double early = input[i];
		double early = tapSum;
		
		for (int i = 0; i < stageCount; i++)
			early = AllpassModules[i].Process(early);
		
		double total = 0.0;
		for (int i = 0; i < LATE_COUNT; i++)
		{
			int idx = (LateI + lateDelays[i]) & MODULO;
			double bufOut = LateBuffers[i * BUF_LEN + idx];
			LateBuffers[i * BUF_LEN + LateI] = early + bufOut * globalFeedback;
			total += bufOut;
		}

		EarlyI--;
		if (EarlyI < 0)
			EarlyI += BUF_LEN;

		LateI--;
		if (LateI < 0)
			LateI += BUF_LEN;

		output[i] = dry * input[i] + wet * total + earlyAmount * early;
	}
}

void Reverber::RecalculateTapIndexes()
{
	int predelay = (int)(Parameters[Parameter::PredelayLeft] / 1000.0 * Samplerate);
	double earlySizeSamples = Parameters[Parameter::EarlySizeLeft] / 1000.0 * Samplerate;
	if(TapCount == 1)
		earlySizeSamples = 1.001;

	for (int i = 0; i < TapCount; i++)
		TapsIndexes[i] = predelay + (int)(TapIndexOffsets[i] * earlySizeSamples);
}