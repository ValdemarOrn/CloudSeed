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

		// perform bit crushing and undersampling
		// undersampling: if set to 1, perfroms no effect, if set to 2, will undersample to 1/2 samplerate, etc...
		// sampleResolution: if set to 32, will use 2^32 steps, if set to 8, will resude to 2^8 = 256 steps
		// Currently Unused
		static inline void BitcrushAndReduce(double* bufferIn, double* bufferOut, int len, int undersampling, int sampleResolution)
		{
			double sampleSteps = std::pow(2, sampleResolution);
			double inverseSteps = 1.0 / sampleSteps;

			double sample = 0.0;

			for (int i = 0; i < len; i++)
			{
				if (i % undersampling == 0)
					sample = ((long)(bufferIn[i] * sampleSteps)) * inverseSteps;

				bufferOut[i] = sample;
			}
		}

		template<typename T>
		static double DB2gain(T input)
		{
			return std::pow(10, input / 20.0);
		}

		template<typename T>
		static double Gain2DB(T input)
		{
			if (input < 0.0000001)
				return -100000;

			return 20.0f * std::log10(input);
		}
	};
}

#endif
