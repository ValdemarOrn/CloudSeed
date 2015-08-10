#ifndef MULTITAPDIFFUSER
#define MULTITAPDIFFUSER

#include <vector>

namespace CloudSeed
{
	using namespace std;

	class MultitapDiffuser
	{
	private:
		static const int SeedValueCount = 100;

		double* buffer;
		double* output;
		int len;

		int index;
		vector<double> tapGains;
		vector<int> tapPosition;
		double seeds[SeedValueCount];

		int count;
		double length;
		double gain;
		double decay;

	public:
		MultitapDiffuser(int bufferSize);

		double* GetSeeds();
		void SetSeeds(double* values);
		double* GetOutput();
		void SetTapCount(int tapCount);
		void SetTapLength(int tapLength);
		void SetTapDecay(double tapDecay);
		void SetTapGain(double tapGain);
		void Update();
		void Process(double* input, int sampleCount);
		void ClearBuffers();
	};
}

#endif
