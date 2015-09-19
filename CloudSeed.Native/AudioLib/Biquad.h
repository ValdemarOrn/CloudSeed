#ifndef BIQUAD
#define BIQUAD

#include <vector>
using namespace std;

namespace AudioLib
{
	class Biquad
	{
	public:
		enum class FilterType
		{
			LowPass = 0,
			HighPass,
			BandPass,
			Notch,
			Peak,
			LowShelf,
			HighShelf
		};

	private:
		double samplerate;
		double _gainDb;
		double _q;
		double a0, a1, a2, b0, b1, b2;
		double x1, x2, y, y1, y2;
		double gain;

	public:
		FilterType Type;
		double Output;
		double Frequency;
		double Slope;

		Biquad();
		Biquad(FilterType filterType, double samplerate);
		~Biquad();

		double GetSamplerate();
		void SetSamplerate(double samplerate);
		double GetGainDb();
		void SetGainDb(double value);
		double GetGain();
		void SetGain(double value);
		double GetQ();
		void SetQ(double value);
		vector<double> GetA();
		vector<double> GetB();

		void Update();
		double GetResponse(double freq);
		
		double inline Process(double x)
		{
			y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
			x2 = x1;
			y2 = y1;
			x1 = x;
			y1 = y;

			Output = y;
			return Output;
		}

		void inline Process(double* input, double* output, int len)
		{
			for (int i = 0; i < len; i++)
				output[i] = Process(input[i]);
		}

		void ClearBuffers();
	};
}

#endif