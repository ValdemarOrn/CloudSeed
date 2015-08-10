#ifndef DELAYLINE
#define DELAYLINE

#include "AudioLib\Lp1.h"

namespace CloudSeed
{
	class DelayLine
	{
	private:
		//ModulatedDelay delay;
		//AllpassDiffuser diffuser;
		//Biquad lowShelf;
		//Biquad highShelf;
		AudioLib::Lp1 lowPass;
		double* tempBuffer;
		double* filterOutputBuffer;
		
		double feedback;
		int samplerate;
		
	public:

		bool DiffuserEnabled;
		bool LowShelfEnabled;
		bool HighShelfEnabled;
		bool CutoffEnabled;

		DelayLine(int bufferSize, int samplerate);
		~DelayLine();

		int GetSamplerate();
		void SetSamplerate(int samplerate);
		double* GetDiffuserSeeds();
		void SetDiffuserSeeds(double* seeds);
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
		void SetModAmount(double amount);
		void SetModRate(double rate);
		double* GetOutput();
		void Process(double* input, int sampleCount);
		void ClearBuffers();
	};
}

#endif