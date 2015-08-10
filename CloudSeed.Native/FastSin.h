#ifndef FASTSIN
#define FASTSIN

namespace CloudSeed
{
	class FastSin
	{
	private:
		static const int DataSize = 32768;
		static double data[DataSize];

	public:
		static void ZeroBuffer(double* buffer, int len);
		static void Init();
		static double Get(double index);
	};
}

#endif
