#ifndef LP1
#define LP1

namespace AudioLib
{
	class Lp1
	{
	public:
		double Output;

	public:
		Lp1(double fs);
		double GetSamplerate();
		void SetSamplerate(double samplerate);
		double GetCutoffHz();
		void SetCutoffHz(double hz);
		void Update();
		double Process(double input);
		void Process(double* input, double* output, int len);

	private:
		double fs;
		double b0, a1;
		double cutoffHz;
	};
}

#endif
