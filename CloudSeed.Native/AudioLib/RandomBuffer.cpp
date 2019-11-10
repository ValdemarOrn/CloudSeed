
#include <climits>
#include "RandomBuffer.h"
#include "LcgRandom.h"

namespace AudioLib
{
	using namespace std;

	vector<double> RandomBuffer::Generate(long long seed, int count)
	{
		LcgRandom rand(seed);
		vector<double> output;

		for (int i = 0; i < count; i++)
		{
			unsigned int val = rand.NextUInt();
			double doubleVal = val / (double)UINT_MAX;
			output.push_back(doubleVal);
		}

		return output;
	}

	vector<double> RandomBuffer::Generate(long long seed, int count, double crossSeed)
	{
		auto seedA = seed;
		auto seedB = ~seed;
		auto seriesA = Generate(seedA, count);
		auto seriesB = Generate(seedB, count);

		vector<double> output;
		for (int i = 0; i < count; i++)
			output.push_back(seriesA[i] * (1 - crossSeed) + seriesB[i] * crossSeed);

		return output;
	}
}