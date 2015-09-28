#ifndef MODULATEDALLPASS
#define MODULATEDALLPASS

namespace CloudSeed
{
	class ModulatedAllpass
	{
	public:
		const int DelayBufferSamples = 9600; // 50ms at 192Khz
		static const int ModulationUpdateRate = 8;

	private:
		int Id;
		double* delayBuffer;
		double* output;
		int bufferSize;
		int index;
		unsigned int samplesProcessed;

		double modPhase;
		int delayA;
		int delayB;
		double gainA;
		double gainB;

	public:

		int SampleDelay;
		double Feedback;
		double ModAmount;
		double ModRate;

		bool InterpolationEnabled;
		bool ModulationEnabled;

		ModulatedAllpass(int bufferSize, int sampleDelay);
		~ModulatedAllpass();

		double* GetOutput();
		void ClearBuffers();
		void Process(double* input, int sampleCount);

	private:
		void ProcessNoMod(double* input, int sampleCount);
		void ProcessWithMod(double* input, int sampleCount);
		double Get(int delay);
		void Update();		
	};
}

#endif