#include "ReverbController.h"
#include "AudioLib/ValueTables.h"
#include <iostream>

using namespace AudioLib;

namespace CloudSeed
{
	ReverbController::ReverbController(int samplerate)
		: channelL(bufferSize, samplerate),	channelR(bufferSize, samplerate)
	{
		this->samplerate = samplerate;
	}

	int ReverbController::GetSamplerate()
	{
		return samplerate;
	}

	void ReverbController::SetSamplerate(int samplerate)
	{
		this->samplerate = samplerate;

		channelL.SetSamplerate(samplerate);
		channelR.SetSamplerate(samplerate);
	}

	int ReverbController::GetParameterCount()
	{
		return (int)Parameter::Count;
	}

	double* ReverbController::GetAllParameters()
	{
		return parameters;
	}

	double ReverbController::GetScaledParameter(Parameter param)
	{
			switch (param)
			{
			// Input
			case Parameter::CrossMix:                  return P(Parameter::CrossMix);
			case Parameter::PreDelay:                  return (int)(P(Parameter::PreDelay) * 500);

			case Parameter::HighPass:                  return 20 + ValueTables::Get(P(Parameter::HighPass), ValueTables::Response4Oct) * 980;
			case Parameter::LowPass:                   return 400 + ValueTables::Get(P(Parameter::LowPass), ValueTables::Response4Oct) * 19600;

			// Early
			case Parameter::TapCount:                  return 1 + (int)(P(Parameter::TapCount) * 49.0);
			case Parameter::TapLength:                 return (int)(P(Parameter::TapLength) * 500);
			case Parameter::TapGain:                   return ValueTables::Get(P(Parameter::TapGain), ValueTables::Response2Dec);
			case Parameter::TapDecay:                  return P(Parameter::TapDecay);

			case Parameter::DiffusionEnabled:          return P(Parameter::DiffusionEnabled);
			case Parameter::DiffusionStages:           return 1 + (int)(P(Parameter::DiffusionStages) * 3.999);
			case Parameter::DiffusionDelay:            return (int)(P(Parameter::DiffusionDelay) * 50);
			case Parameter::DiffusionFeedback:         return P(Parameter::DiffusionFeedback);

			// Late
			case Parameter::LineCount:                 return 1 + (int)(P(Parameter::LineCount) * 11.999);
			case Parameter::LineDelay:                 return (int)(P(Parameter::LineDelay) * 500);
			case Parameter::LineFeedback:              return P(Parameter::LineFeedback);

			case Parameter::PostDiffusionEnabled:      return P(Parameter::PostDiffusionEnabled);
			case Parameter::PostDiffusionStages:       return 1 + (int)(P(Parameter::PostDiffusionStages) * 3.999);
			case Parameter::PostDiffusionDelay:        return (int)(P(Parameter::PostDiffusionDelay) * 50);
			case Parameter::PostDiffusionFeedback:     return P(Parameter::PostDiffusionFeedback);

			// Frequency Response
			case Parameter::PostLowShelfGain:          return ValueTables::Get(P(Parameter::PostLowShelfGain), ValueTables::Response2Dec);
			case Parameter::PostLowShelfFrequency:     return 20 + ValueTables::Get(P(Parameter::PostLowShelfFrequency), ValueTables::Response4Oct) * 980;
			case Parameter::PostHighShelfGain:         return ValueTables::Get(P(Parameter::PostHighShelfGain), ValueTables::Response2Dec);
			case Parameter::PostHighShelfFrequency:    return 400 + ValueTables::Get(P(Parameter::PostHighShelfFrequency), ValueTables::Response4Oct) * 19600;
			case Parameter::PostCutoffFrequency:       return 400 + ValueTables::Get(P(Parameter::PostCutoffFrequency), ValueTables::Response4Oct) * 19600;

			// Modulation
			case Parameter::DiffusionModAmount:        return P(Parameter::DiffusionModAmount) * 2.5;
			case Parameter::DiffusionModRate:          return ValueTables::Get(P(Parameter::DiffusionModRate), ValueTables::Response2Dec) * 5;
			case Parameter::LineModAmount:             return P(Parameter::LineModAmount) * 2.5;
			case Parameter::LineModRate:               return ValueTables::Get(P(Parameter::LineModRate), ValueTables::Response2Dec) * 5;

			// Seeds
			case Parameter::TapSeed:                   return (int)(P(Parameter::TapSeed) * 1000000);
			case Parameter::DiffusionSeed:             return (int)(P(Parameter::DiffusionSeed) * 1000000);
			case Parameter::CombSeed:                  return (int)(P(Parameter::CombSeed) * 1000000);
			case Parameter::PostDiffusionSeed:         return (int)(P(Parameter::PostDiffusionSeed) * 1000000);

			// Output
			case Parameter::StereoWidth:               return P(Parameter::StereoWidth);

			case Parameter::DryOut:                    return ValueTables::Get(P(Parameter::DryOut), ValueTables::Response2Dec);
			case Parameter::PredelayOut:               return ValueTables::Get(P(Parameter::PredelayOut), ValueTables::Response2Dec);
			case Parameter::EarlyOut:                  return ValueTables::Get(P(Parameter::EarlyOut), ValueTables::Response2Dec);
			case Parameter::MainOut:                   return ValueTables::Get(P(Parameter::MainOut), ValueTables::Response2Dec);

			// Switches
			case Parameter::HiPassEnabled:             return P(Parameter::HiPassEnabled);
			case Parameter::LowPassEnabled:            return P(Parameter::LowPassEnabled);
			case Parameter::LowShelfEnabled:           return P(Parameter::LowShelfEnabled);
			case Parameter::HighShelfEnabled:          return P(Parameter::HighShelfEnabled);
			case Parameter::CutoffEnabled:             return P(Parameter::CutoffEnabled);

			default: return 0.0;
			}

		return 0.0;
	}

	void ReverbController::SetParameter(Parameter param, double value)
	{
		parameters[(int)param] = value;
		auto scaled = GetScaledParameter(param);

		channelL.SetParameter(param, scaled);

		if ((int)param >= (int)Parameter::TapSeed && (int)param <= (int)Parameter::PostDiffusionSeed)
			scaled = (int)scaled + 1000000; // different seeds for right channel

		channelR.SetParameter(param, scaled);
	}

	void ReverbController::ClearBuffers()
	{
		channelL.ClearBuffers();
		channelR.ClearBuffers();
	}

	void ReverbController::Process(double** input, double** output, int bufferSize)
	{
		auto len = bufferSize;

		auto cm = GetScaledParameter(Parameter::CrossMix) * 0.5;
		auto cmi = (1 - cm);
		auto st = 0.5 + 0.5 * GetScaledParameter(Parameter::StereoWidth);
		auto sti = (1 - st);

		for (uint i = 0; i < len; i++)
		{
			leftChannelIn[i] = input[0][i] * cmi + input[1][i] * cm;
			rightChannelIn[i] = input[1][i] * cmi + input[0][i] * cm;
		}

		channelL.Process(leftChannelIn, len);
		channelR.Process(rightChannelIn, len);
		auto leftOut = channelL.GetOutput();
		auto rightOut = channelR.GetOutput();

		for (uint i = 0; i < len; i++)
		{
			output[0][i] = leftOut[i] * st + rightOut[i] * sti;
			output[1][i] = rightOut[i] * st + leftOut[i] * sti;
		}

		std::cout << "Sample 0: " << output[0][0] << std::endl;
	}

	double ReverbController::P(Parameter para)
	{
		auto idx = (int)para;
		return idx >= 0 && idx < (int)Parameter::Count ? parameters[idx] : 0.0;
	}
}
