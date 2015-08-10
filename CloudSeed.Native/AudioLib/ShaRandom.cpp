
#include <climits>
#include "ShaRandom.h"
#include "../Utils/Sha256.h"

namespace AudioLib
{
	using namespace std;

	vector<double> ShaRandom::Generate(long long seed, int count)
	{
		vector<unsigned char> byteList;
		auto iterations = count * sizeof(unsigned int) / (256 / 8) + 1;
		auto byteArr = (unsigned char*)&seed;
		vector<unsigned char> bytes(byteArr, byteArr + 8);

		for (int i = 0; i < iterations; i++)
		{
			bytes = sha256(&bytes[0], 8);
			for (auto b : bytes)
				byteList.push_back(b);
		}

		auto intArray = (int*)(&byteList[0]);
		vector<double> output;

		for (int i = 0; i < count; i++)
		{
			auto val = intArray[i];
			auto doubleVal = val / (double)UINT_MAX;
			output.push_back(doubleVal);
		}

		return output;
	}
}