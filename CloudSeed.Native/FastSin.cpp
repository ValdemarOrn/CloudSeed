
#define _USE_MATH_DEFINES
#include <cmath>
#include "Default.h"
#include "FastSin.h"

namespace CloudSeed
{
	double FastSin::data[FastSin::DataSize];

	void FastSin::Init()
	{
		for (int i = 0; i < DataSize; i++)
		{
			data[i] = std::sin(2 * M_PI * i / (double)DataSize);
		}
	}

	double FastSin::Get(double index)
	{
		return data[(int)(index * 32767.99999)];
	}
}
