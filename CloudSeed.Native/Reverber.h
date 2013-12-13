
#ifndef REVERBER
#define REVERBER

#include "Default.h"
#include "Allpass.h"

#pragma pack(4)
struct Reverber
{
	double Samplerate;
	double Parameters[PARAM_COUNT];

	double EarlySize;

	int TapsIndexes[MAX_TAP_COUNT];
	double TapIndexOffsets[MAX_TAP_COUNT];
	double TapAmplitudes[MAX_TAP_COUNT];
	int TapCount;

	Allpass AllpassModules[ALLPASS_COUNT];

	double EarlyBuffer[BUF_LEN];
	int EarlyI;

	double OutBuffer[BUF_LEN];
	int OutI;

	int SampleCounter;

	Reverber();
	double* GetParameters();
	void SetSamplerate(double samplerate);
	void SetTaps(double* indexOffsets, double* amplitudes, int count);
	void SetLate(double* feedback, int* delaySamples);
	void SetHiCut(double* fc, double* amount);
	void SetAllpassMod(double* freq, double* amount);
	void Process(double* input, double* output, int len);

private:
	void RecalculateTapIndexes();
};

extern "C"
{
	__dllexport Reverber* Create()
	{
		return new Reverber();
	}

	__dllexport double* GetParameters(Reverber* item)
	{
		return item->GetParameters();
	}

	__dllexport void Delete(Reverber* item)
	{
		delete item;
	}

	__dllexport void SetSamplerate(Reverber* item, double samplerate)
	{
		item->SetSamplerate(samplerate);
	}

	__dllexport void SetTaps(Reverber* item, double* indexOffsets, double* amplitudes, int count)
	{
		item->SetTaps(indexOffsets, amplitudes, count);
	}

	__dllexport void SetLate(Reverber* item, double* feedback, int* delaySamples)
	{
		item->SetLate(feedback, delaySamples);
	}

	__dllexport void SetHiCut(Reverber* item, double* fc, double* amount)
	{
		item->SetHiCut(fc, amount);
	}

	__dllexport void SetAllpassMod(Reverber* item, double* freq, double* amount)
	{
		item->SetAllpassMod(freq, amount);
	}

	__dllexport void Process(Reverber* item, double* input, double* output, int len)
	{
		item->Process(input, output, len);
	}
}

#endif
