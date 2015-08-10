
#include "Utils.h"

namespace CloudSeed
{
	void Utils::ZeroBuffer(double* buffer, int len)
	{
		for (int i = 0; i < len; i++)
			buffer[i] = 0;
	}
}
