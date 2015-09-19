#ifndef UTILS
#define UTILS

#include <cstring>
#include <algorithm>

namespace CloudSeed
{
	class Utils
	{
	public:

		static inline void ZeroBuffer(double* buffer, int len)
		{
			for (int i = 0; i < len; i++)
				buffer[i] = 0.0;
		}

		static inline void Copy(double* source, double* dest, int len)
		{
			std::memcpy(dest, source, len * sizeof(double));
		}

		static inline void Gain(double* buffer, double gain, int len)
		{
			for (int i = 0; i < len; i++)
			{
				buffer[i] *= gain;
			}
		}
	};
}

#endif
