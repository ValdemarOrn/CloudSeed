#ifndef MULTITAPDIFFUSER
#define MULTITAPDIFFUSER

#include <vector>

namespace CloudSeed
{
	using namespace std;

	class MultitapDiffuser
	{
	public:
		static const int MaxTaps = 50;

	private:
		double* buffer;
		double* output;
		int len;

		int index;
		vector<double> tapGains;
		vector<int> tapPosition;
		vector<double> seedValues;
		int seed;
		double crossSeed;
		int count;
		double length;
		double gain;
		double decay;

		bool isDirty;
		vector<double> tapGainsTemp;
		vector<int> tapPositionTemp;
		int countTemp;

	public:
		MultitapDiffuser(int bufferSize);
		~MultitapDiffuser();

		void SetSeed(int seed);
		void SetCrossSeed(double crossSeed);
		double* GetOutput();
		void SetTapCount(int tapCount);
		void SetTapLength(int tapLength);
		void SetTapDecay(double tapDecay);
		void SetTapGain(double tapGain);
		void Process(double* input, int sampleCount);
		void ClearBuffers();

	private:
		void Update();
		void UpdateSeeds();
	};
}

#endif
