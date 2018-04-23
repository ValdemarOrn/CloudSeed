#ifndef DELAYLINE
#define DELAYLINE

#include "AudioLib\Lp1.h"
#include "ModulatedDelay.h"
#include "AllpassDiffuser.h"
#include "AudioLib\Biquad.h"

using namespace AudioLib;

namespace CloudSeed
{
	class DelayLine
	{
	private:
		ModulatedDelay delay;
		AllpassDiffuser diffuser;
		Biquad lowShelf;
		Biquad highShelf;
		AudioLib::Lp1 lowPass;
		double* tempBuffer;
		double* mixedBuffer;
		double* filterOutputBuffer;
		int bufferSize;
		double feedback;
		int samplerate;

	public:

		bool DiffuserEnabled;
		bool LowShelfEnabled;
		bool HighShelfEnabled;
		bool CutoffEnabled;
		bool LateStageTap;

		DelayLine(int bufferSize, int samplerate)
			: lowPass(samplerate)
			, delay(bufferSize, samplerate * 2, 10000) // 2 second buffer, to prevent buffer overflow with modulation and randomness added (Which may increase effective delay)
			, diffuser(samplerate, 150) // 150ms buffer
			, lowShelf(AudioLib::Biquad::FilterType::LowShelf, samplerate)
			, highShelf(AudioLib::Biquad::FilterType::HighShelf, samplerate)
		{
			this->bufferSize = bufferSize;
			tempBuffer = new double[bufferSize];
			mixedBuffer = new double[bufferSize];
			filterOutputBuffer = new double[bufferSize];

			lowShelf.Slope = 1.0;
			lowShelf.SetGainDb(-20);
			lowShelf.Frequency = 20;

			highShelf.Slope = 1.0;
			highShelf.SetGainDb(-20);
			highShelf.Frequency = 19000;

			lowPass.SetCutoffHz(1000);
			lowShelf.Update();
			highShelf.Update();
			SetSamplerate(samplerate);

			SetDiffuserSeed(1, 0.0);
		}

		~DelayLine()
		{
			delete tempBuffer;
			delete mixedBuffer;
			delete filterOutputBuffer;
		}


		int GetSamplerate()
		{
			return samplerate;
		}

		void SetSamplerate(int samplerate)
		{
			this->samplerate = samplerate;
			diffuser.SetSamplerate(samplerate);
			lowPass.SetSamplerate(samplerate);
			lowShelf.SetSamplerate(samplerate);
			highShelf.SetSamplerate(samplerate);
		}

		void SetDiffuserSeed(int seed, double crossSeed)
		{
			diffuser.SetSeed(seed);
			diffuser.SetCrossSeed(crossSeed);
		}

		void SetDelay(int delaySamples)
		{
			delay.SampleDelay = delaySamples;
		}

		void SetFeedback(double feedb)
		{
			feedback = feedb;
		}

		void SetDiffuserDelay(int delaySamples)
		{
			diffuser.SetDelay(delaySamples);
		}

		void SetDiffuserFeedback(double feedb)
		{
			diffuser.SetFeedback(feedb);
		}

		void SetDiffuserStages(int stages)
		{
			diffuser.Stages = stages;
		}

		void SetLowShelfGain(double gain)
		{
			lowShelf.SetGain(gain);
			lowShelf.Update();
		}

		void SetLowShelfFrequency(double frequency)
		{
			lowShelf.Frequency = frequency;
			lowShelf.Update();
		}

		void SetHighShelfGain(double gain)
		{
			highShelf.SetGain(gain);
			highShelf.Update();
		}

		void SetHighShelfFrequency(double frequency)
		{
			highShelf.Frequency = frequency;
			highShelf.Update();
		}

		void SetCutoffFrequency(double frequency)
		{
			lowPass.SetCutoffHz(frequency);
		}

		void SetLineModAmount(double amount)
		{
			delay.ModAmount = amount;
		}

		void SetLineModRate(double rate)
		{
			delay.ModRate = rate;
		}

		void SetDiffuserModAmount(double amount)
		{
			diffuser.SetModulationEnabled(amount > 0.0);
			diffuser.SetModAmount(amount);
		}

		void SetDiffuserModRate(double rate)
		{
			diffuser.SetModRate(rate);
		}

		void SetInterpolationEnabled(bool value)
		{
			diffuser.SetInterpolationEnabled(value);
		}


		double* GetOutput()
		{
			if (LateStageTap)
			{
				if (DiffuserEnabled)
					return diffuser.GetOutput();
				else
					return mixedBuffer;
			}
			else
			{
				return delay.GetOutput();
			}
		}

		void Process(double* input, int sampleCount)
		{
			for (int i = 0; i < sampleCount; i++)
				mixedBuffer[i] = input[i] + filterOutputBuffer[i] * feedback;

			if (LateStageTap)
			{
				if (DiffuserEnabled)
				{
					diffuser.Process(mixedBuffer, sampleCount);
					delay.Process(diffuser.GetOutput(), sampleCount);
				}
				else
				{
					delay.Process(mixedBuffer, sampleCount);
				}

				Utils::Copy(delay.GetOutput(), tempBuffer, sampleCount);
			}
			else
			{

				if (DiffuserEnabled)
				{
					delay.Process(mixedBuffer, sampleCount);
					diffuser.Process(delay.GetOutput(), sampleCount);
					Utils::Copy(diffuser.GetOutput(), tempBuffer, sampleCount);
				}
				else
				{
					delay.Process(mixedBuffer, sampleCount);
					Utils::Copy(delay.GetOutput(), tempBuffer, sampleCount);
				}
			}

			if (LowShelfEnabled)
				lowShelf.Process(tempBuffer, tempBuffer, sampleCount);
			if (HighShelfEnabled)
				highShelf.Process(tempBuffer, tempBuffer, sampleCount);
			if (CutoffEnabled)
				lowPass.Process(tempBuffer, tempBuffer, sampleCount);

			Utils::Copy(tempBuffer, filterOutputBuffer, sampleCount);
		}

		void ClearDiffuserBuffer()
		{
			diffuser.ClearBuffers();
		}

		void ClearBuffers()
		{
			delay.ClearBuffers();
			diffuser.ClearBuffers();
			lowShelf.ClearBuffers();
			highShelf.ClearBuffers();
			lowPass.Output = 0;

			for (int i = 0; i < bufferSize; i++)
			{
				tempBuffer[i] = 0.0;
				filterOutputBuffer[i] = 0.0;
			}
		}
	};
}

#endif