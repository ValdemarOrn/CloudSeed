#ifndef MODULATEDALLPASS
#define MODULATEDALLPASS

namespace CloudSeed
{
	class ModulatedAllpass
	{
	public:
		static const int ModulationUpdateRate = 8;

	private:
		int Id;
		double* buffer;
		double* output;
		int bufferSize;
		int index;
		int samplesProcessed;

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