
#ifndef REVERBCONTROLLER
#define REVERBCONTROLLER

#include <vector>
#include "Default.h"
#include "Parameter.h"
#include "ReverbChannel.h"

namespace CloudSeed
{
	#pragma pack(push, 4)
	class ReverbController
	{
	private:
		static const int bufferSize = 96000; // just make it huge by default...
		int samplerate;

		ReverbChannel channelL;
		ReverbChannel channelR;
		double leftChannelIn[bufferSize];
		double rightChannelIn[bufferSize];

		double parameters[(int)Parameter::Count];

	public:
		ReverbController(int samplerate);
		int GetSamplerate();
		void SetSamplerate(int samplerate);
		int GetParameterCount();
		double* GetAllParameters();
		double GetScaledParameter(Parameter param);
		void SetParameter(Parameter param, double value);
		void ClearBuffers();
		void Process(double** input, double** output, int bufferSize);

	private:
		double P(Parameter para);
	};
	#pragma pack(pop)
}
#endif
