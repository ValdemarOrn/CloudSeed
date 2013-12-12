
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

	double Alpha;
	double Buffer[BUF_LEN];
	double BufferOut[BUF_LEN];
	int I;

	double A;
	double AOut;

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
	}
	
	__inline_always double Allpass::Process(double x)
	{
		int k = (int)(I + DelaySamples * (1 + 0.01 * ModAmount * ModValue)) % BUF_LEN;

		double bufOut = Buffer[k];
		double bufIn = x - Feedback * bufOut;
		Buffer[I] = bufIn;
		double y = Feedback * bufIn + bufOut;

		I--;
		if (I < 0)
			I += BUF_LEN;

		A = (1 - Alpha) * y + Alpha * A;
		AOut = A * HiCutAmount + y * (1 - HiCutAmount);
		return AOut;	
	}
};

#endif
