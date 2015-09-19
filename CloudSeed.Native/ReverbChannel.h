
#ifndef REVERBCHANNEL
#define REVERBCHANNEL

#include <map>
#include <memory>
#include "Parameter.h"
#include "ModulatedDelay.h"
#include "MultitapDiffuser.h"
#include "AudioLib/ShaRandom.h"
#include "AudioLib/Lp1.h"
#include "AudioLib/Hp1.h"
#include "DelayLine.h"
#include "AllpassDiffuser.h"

using namespace std;

namespace CloudSeed
{
	class ReverbChannel
	{
	private:
		static const int TotalLineCount = 12;

		map<Parameter, double> parameters;
		int samplerate;
		int bufferSize;

		ModulatedDelay preDelay;
		MultitapDiffuser multitap;
		AllpassDiffuser diffuser;
		vector<DelayLine*> lines;
		AudioLib::ShaRandom rand;
		AudioLib::Hp1 highPass;
		AudioLib::Lp1 lowPass;
		double* tempBuffer;
		double* outBuffer;
		vector<double> delayLineSeeds;

		// Used the the main process loop
		int lineCount;
		double perLineGain;

		bool highPassEnabled;
		bool lowPassEnabled;
		bool diffuserEnabled;
		double dryOut;
		double predelayOut;
		double earlyOut;
		double lineOut;

	public:
		ReverbChannel(int bufferSize, int samplerate);
		~ReverbChannel();
		int GetSamplerate();
		void SetSamplerate(int samplerate);
		double* GetOutput();
		void SetParameter(Parameter para, double value);
		void Process(double* input, int sampleCount);
		void ClearBuffers();

	private:
		double GetPerLineGain();
		void UpdateLines();
		double Ms2Samples(double value);
	};

}

#endif
