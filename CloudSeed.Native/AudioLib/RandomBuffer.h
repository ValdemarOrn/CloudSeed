#pragma once

#include <vector>

namespace AudioLib
{
	class RandomBuffer
	{
	public:
		static std::vector<double> Generate(long long seed, int count);
		static std::vector<double> Generate(long long seed, int count, double crossSeed);
	};
}

