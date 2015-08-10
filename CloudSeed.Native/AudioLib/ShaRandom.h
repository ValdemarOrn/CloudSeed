#ifndef SHARANDOM
#define SHARANDOM

#include <vector>

namespace AudioLib
{
	class ShaRandom
	{
	public:
		std::vector<double> Generate(long long seed, int count);
	};
}

#endif
