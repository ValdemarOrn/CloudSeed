#ifndef DELAYLINE
#define DELAYLINE

#include "AudioLib\Lp1.h"
#include "ModulatedDelay.h"
#include "AllpassDiffuser.h"
#include "AudioLib\Biquad.h"

using namespace AudioLib;

namespace CloudSeed
{
	class DelayLine
	{
	private:
		ModulatedDelay delay;
		AllpassDiffuser diffuser;
		Biquad lowShelf;
		Biquad highShelf;
		AudioLib::Lp1 lowPass;
		double* tempBuffer;
		double* mixedBuffer;
		double* filterOutputBuffer;
		int bufferSize;
		double feedback;
		int samplerate;

	public:

		bool DiffuserEnabled;
		bool LowShelfEnabled;
		bool HighShelfEnabled;
		bool CutoffEnabled;
		bool LateStageTap;

		DelayLine(int bufferSize, int samplerate);
		~DelayLine();

		int GetSamplerate();
		void SetSamplerate(int samplerate);
		void SetDiffuserSeed(int seed, double crossMix);
		void SetDelay(int delaySamples);
		void SetFeedback(double feedb);
		void SetDiffuserDelay(int delaySamples);
		void SetDiffuserFeedback(double feedb);
		void SetDiffuserStages(int stages);
		void SetLowShelfGain(double gain);
		void SetLowShelfFrequency(double frequency);
		void SetHighShelfGain(double gain);
		void SetHighShelfFrequency(double frequency);
		void SetCutoffFrequency(double frequency);
		void SetLineModAmount(double amount);
		void SetLineModRate(double rate);
		void SetDiffuserModAmount(double amount);
		void SetDiffuserModRate(double rate);
		void SetInterpolationEnabled(bool value);

		double* GetOutput();
		void Process(double* input, int sampleCount);
		void ClearDiffuserBuffer();
		void ClearBuffers();
	};
}

#endif