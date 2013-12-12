

#include "Allpass.h"
#include <math.h>

Allpass::Allpass()
{
	Samplerate = 48000;
	Feedback = 0.7;
	DelaySamples = 0;
	ModAmount = 0;
	HiCutAmount = 0;
	ModFreq = 0;
	HiCut = 0;
	ModPhase = 0;
	ModValue = 0;
	ModIncrement = 0;
	Alpha = 0;
	I = 0;
	A = 0;
	AOut = 0;

	for (int i = 0; i < BUF_LEN; i++)
	{
		Buffer[i] = 0.0;
		BufferOut[i] = 0.0;
	}
}

void Allpass::SetModFreq(double value)
{
	ModFreq = value;
	ModIncrement = 1.0 / Samplerate * ModFreq;
}

void Allpass::SetHiCut(double value)
{
	HiCut = value;
	Alpha = exp(-2 * PI * HiCut / Samplerate);
}