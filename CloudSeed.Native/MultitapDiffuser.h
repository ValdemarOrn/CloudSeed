#ifndef MULTITAPDIFFUSER
#define MULTITAPDIFFUSER

#include <vector>

namespace CloudSeed
{
	using namespace std;

	class MultitapDiffuser
	{
	private:
		double* buffer;
		double* output;
		int len;

		int index;
		vector<double> tapGains;
		vector<int> tapPosition;
		vector<double> seeds;

		int count;
		double length;
		double gain;
		double decay;

	public:
		MultitapDiffuser(int bufferSize);
		~MultitapDiffuser();

		vector<double> GetSeeds();
		void SetSeeds(vector<double> seeds);
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
