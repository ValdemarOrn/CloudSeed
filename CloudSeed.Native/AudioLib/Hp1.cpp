
#include <cmath>
#include "Hp1.h"

#define PI 3.141592653589793238462643383

namespace AudioLib
{
	using namespace std;

	Hp1::Hp1(double fs)
	{
		this->fs = fs;
	}

	double Hp1::GetSamplerate()
	{
		return fs;
	}

	void Hp1::SetSamplerate(double samplerate)
	{
		fs = samplerate;
	}

	double Hp1::GetCutoffHz()
	{
		return cutoffHz;
	}

	void Hp1::SetCutoffHz(double hz)
	{
		cutoffHz = hz;
		Update();
	}

	void Hp1::Update()
	{
		// Prevent going over the Nyquist frequency
		if (cutoffHz >= fs * 0.5)
			cutoffHz = fs * 0.499;

		auto x = 2 * PI * cutoffHz / fs;
		auto nn = (2 - cos(x));
		auto alpha = nn - sqrt(nn * nn - 1);

		a1 = alpha;
		b0 = 1 - alpha;
	}

	double Hp1::Process(double input)
	{
		if (input == 0 && lpOut < 0.000000000001)
		{
			Output = 0;
		}
		else
		{
			lpOut = b0 * input + a1 * lpOut;
			Output = input - lpOut;
		}

		return Output;
	}

	void Hp1::Process(double* input, double* output, int len)
	{
		for (int i = 0; i < len; i++)
			output[i] = Process(input[i]);
	}

}
