#ifndef SHARANDOM
#define SHARANDOM

#include <vector>

namespace AudioLib
{
	class ShaRandom
	{
	public:
		static std::vector<double> Generate(long long seed, int count);
		static std::vector<double> Generate(long long seed, int count, double crossSeed);
	};
}

#endif
