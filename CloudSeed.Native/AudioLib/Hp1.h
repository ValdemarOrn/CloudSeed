#ifndef HP1
#define HP1

namespace AudioLib
{
	class Hp1
	{
	public:
		double Output;

	public:
		Hp1(double fs);
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
		double lpOut;
		double cutoffHz;
	};
}

#endif
