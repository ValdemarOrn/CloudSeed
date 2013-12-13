
#ifndef ALLPASS
#define ALLPASS

#include "Default.h"
#include <math.h>

#pragma pack(4)
struct Allpass
{
	double Samplerate;
	double Feedback;
	int DelaySamples;

	double ModAmount;
	double HiCutAmount;

	double ModFreq;
	double HiCut;

private:
	double ModPhase;
	double ModValue;
	double ModIncrement;
	int IndexOffset;

	double Alpha;
	double Buffer[BUF_LEN];
	int I;

	double A;

public:

	Allpass();
	void SetModFreq(double value);
	void SetHiCut(double value);

	__inline_always void Allpass::UpdateMod(int sampleCount)
	{
		ModPhase += sampleCount * ModIncrement;
		if (ModPhase > 1.0)
			ModPhase -= 1.0;

		ModValue = sin(ModPhase * 2 * PI);
		IndexOffset = DelaySamples * (1 + 0.01 * ModAmount * ModValue);
	}
	
	__inline_always double Allpass::Process(double x)
	{
		// https://ccrma.stanford.edu/~jos/Reverb/Are_Allpass_Filters_Really.html

		int k = (int)(I + IndexOffset) & MODULO;

		double bufOut = Buffer[k];
		double bufIn = x + Feedback * bufOut;
		Buffer[I] = bufIn;

		I--;
		if (I < 0)
			I += BUF_LEN;

		double y = bufOut - Feedback * bufIn;
		// filter the feedback
		A = (1 - Alpha) * y + Alpha * A;
		A = A * HiCutAmount + y * (1 - HiCutAmount);
		return A;
	}
};

#endif
