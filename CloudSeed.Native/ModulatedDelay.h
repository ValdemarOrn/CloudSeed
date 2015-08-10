#ifndef MODULATEDDELAY
#define MODULATEDDELAY

namespace CloudSeed
{
	class ModulatedDelay
	{
	private:

		const int ModulationUpdateRate = 8;

		double* buffer;
		double* output;
		int bufferSize;
		int writeIndex;
		int readIndexA;
		int readIndexB;
		int samplesProcessed;

		double modPhase;
		double gainA;
		double gainB;

	public:
		int SampleDelay;

		double ModAmount;
		double ModRate;

		ModulatedDelay(int bufferSize, int sampleDelay);
		~ModulatedDelay();
		double* GetOutput();
		void Process(double* input, int sampleCount);
		void ClearBuffers();

	private:
		void Update();
	};
}

#endif