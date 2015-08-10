
#include <cmath>
#include "ReverbChannel.h"

namespace CloudSeed
{
	ReverbChannel::ReverbChannel(int bufferSize, int samplerate)
		: preDelay(bufferSize, 10000)
		, multitap(bufferSize)
		, highPass(samplerate)
		, lowPass(samplerate)
	{
		for (auto value = 0; value < (int)Parameter::Count; value++)
			this->parameters[static_cast<Parameter>(value)] = 0.0;

//		multitap = new MultitapDiffuser(bufferSize);
//		diffuser = new AllpassDiffuser(bufferSize, samplerate){ ModulationEnabled = true };

		for (int i = 0; i < TotalLineCount; i++)
		{
			unique_ptr<DelayLine> ptr(new DelayLine(bufferSize, samplerate));
			this->lines.push_back(std::move(ptr));
		}
		
		lineCount = 8;
		perLineGain = GetPerLineGain();

		highPass.SetCutoffHz(20);
		lowPass.SetCutoffHz(20000);

		tempBuffer = new double[bufferSize];
		outBuffer = new double[bufferSize];
//		delayLineSeeds = rand.Generate(12345, lines.Length * 3);

		this->samplerate = samplerate;
	}

	ReverbChannel::~ReverbChannel()
	{
		delete this->tempBuffer;
		delete this->outBuffer;
		delete this->delayLineSeeds;
	}

	int ReverbChannel::GetSamplerate()
	{
		return samplerate;
	}

	void ReverbChannel::SetSamplerate(int samplerate)
	{
		this->samplerate = samplerate;
		highPass.SetSamplerate(samplerate);
		lowPass.SetSamplerate(samplerate);

/*		for (int i = 0; i < lines.Length; i++)
		{
			lines[i].Samplerate = samplerate;
		}
*/
		auto update = [&](Parameter p) { SetParameter(p, parameters[p]); };
		update(Parameter::PreDelay);
		update(Parameter::TapLength);
		update(Parameter::DiffusionDelay);
		update(Parameter::LineDelay);
		update(Parameter::PostDiffusionDelay);
		update(Parameter::DiffusionModAmount);
		update(Parameter::LineModAmount);
		UpdateLines();
	}

	double* ReverbChannel::GetOutput()
	{
		return outBuffer;
	}

	void ReverbChannel::SetParameter(Parameter para, double value)
	{
		parameters[para] = value;

		switch (para)
		{
		case Parameter::PreDelay:
			preDelay.SampleDelay = (int)Ms2Samples(value);
			break;
		case Parameter::HighPass:
			highPass.SetCutoffHz(value);
			break;
		case Parameter::LowPass:
			lowPass.SetCutoffHz(value);
			break;

		case Parameter::TapCount:
//			multitap.SetTapCount((int)value);
			break;
		case Parameter::TapLength:
//			multitap.SetTapLength((int)Ms2Samples(value));
			break;
		case Parameter::TapGain:
//			multitap.SetTapGain(value);
			break;
		case Parameter::TapDecay:
//			multitap.SetTapDecay(value);
			break;

		case Parameter::DiffusionEnabled:
//			diffuserEnabled = value >= 0.5;
			break;
		case Parameter::DiffusionStages:
//			diffuser.Stages = (int)value;
			break;
		case Parameter::DiffusionDelay:
//			diffuser.SetDelay((int)Ms2Samples(value));
			break;
		case Parameter::DiffusionFeedback:
//			diffuser.SetFeedback(value);
			break;

		case Parameter::LineCount:
			lineCount = (int)value;
			perLineGain = GetPerLineGain();
			break;
		case Parameter::LineDelay:
			UpdateLines();
			break;
		case Parameter::LineFeedback:
			UpdateLines();
			break;

		case Parameter::PostDiffusionEnabled:
//			foreach(var line in lines)
//				line.DiffuserEnabled = value >= 0.5;
			break;
		case Parameter::PostDiffusionStages:
//			foreach(var line in lines)
//				line.SetDiffuserStages((int)value);
			break;
		case Parameter::PostDiffusionDelay:
//			foreach(var line in lines)
//				line.SetDiffuserDelay((int)Ms2Samples(value));
			break;
		case Parameter::PostDiffusionFeedback:
//			foreach(var line in lines)
//				line.SetDiffuserFeedback(value);
			break;

		case Parameter::PostLowShelfGain:
//			foreach(var line in lines)
//				line.SetLowShelfGain(value);
			break;
		case Parameter::PostLowShelfFrequency:
//			foreach(var line in lines)
//				line.SetLowShelfFrequency(value);
			break;
		case Parameter::PostHighShelfGain:
//			foreach(var line in lines)
//				line.SetHighShelfGain(value);
			break;
		case Parameter::PostHighShelfFrequency:
//			foreach(var line in lines)
//				line.SetHighShelfFrequency(value);
			break;
		case Parameter::PostCutoffFrequency:
//			foreach(var line in lines)
//				line.SetCutoffFrequency(value);
			break;

		case Parameter::DiffusionModAmount:
//			diffuser.SetModAmount(Ms2Samples(value));
			break;
		case Parameter::DiffusionModRate:
//			diffuser.SetModRate(value);
			break;
		case Parameter::LineModAmount:
			UpdateLines();
			break;
		case Parameter::LineModRate:
			UpdateLines();
			break;

		case Parameter::TapSeed:
//			multitap.Seeds = rand.Generate((int)value, 100).ToArray();
			break;
		case Parameter::DiffusionSeed:
//			diffuser.Seeds = rand.Generate((int)value, 12).ToArray();
			break;
		case Parameter::CombSeed:
//			delayLineSeeds = rand.Generate((int)value, lines.Length * 3).ToArray();
			UpdateLines();
			break;
		case Parameter::PostDiffusionSeed:
//			for (int i = 0; i < lines.Length; i++)
//				lines[i].DiffuserSeeds = rand.Generate(((int)value) + i, 12).ToArray();
			break;

		case Parameter::DryOut:
			dryOut = value;
			break;
		case Parameter::PredelayOut:
			predelayOut = value;
			break;
		case Parameter::EarlyOut:
			earlyOut = value;
			break;
		case Parameter::MainOut:
			lineOut = value;
			break;

		case Parameter::HiPassEnabled:
			highPassEnabled = value >= 0.5;
			break;
		case Parameter::LowPassEnabled:
			lowPassEnabled = value >= 0.5;
			break;
		case Parameter::LowShelfEnabled:
//			foreach(var line in lines)
//				line.LowShelfEnabled = value >= 0.5;
			break;
		case Parameter::HighShelfEnabled:
//			foreach(var line in lines)
//				line.HighShelfEnabled = value >= 0.5;
			break;
		case Parameter::CutoffEnabled:
//			foreach(var line in lines)
//				line.CutoffEnabled = value >= 0.5;
			break;
		}
	}

	void ReverbChannel::Process(double* input, int sampleCount)
	{
		int len = sampleCount;
		auto predelayOutput = preDelay.GetOutput();
		auto lowPassInput = highPassEnabled ? tempBuffer : input;

		if (highPassEnabled)
			highPass.Process(input, tempBuffer, len);
		if (lowPassEnabled)
			lowPass.Process(lowPassInput, tempBuffer, len);
//		if (!lowPassEnabled && !highPassEnabled)
//			input.Copy(tempBuffer, len);

		// completely zero if no input present
		// Previously, the very small values were causing some really strange CPU spikes
		for (int i = 0; i < len; i++)
		{
			auto n = tempBuffer[i];
			if (n * n < 0.000000001)
				tempBuffer[i] = 0;
		}

		preDelay.Process(tempBuffer, len);
		multitap.Process(preDelay.GetOutput(), len);

		/*auto earlyOutStage = diffuserEnabled ? diffuser.GetOutput() : multitap.GetOutput();

		if (diffuserEnabled)
		{
			diffuser.Process(multitap.Output, len);
			diffuser.Output.Copy(tempBuffer, len);
		}
		else
		{
			multitap.Output.Copy(tempBuffer, len);
		}

		for (int i = 0; i < lineCount; i++)
			lines[i].Process(tempBuffer, len);

		for (int i = 0; i < lineCount; i++)
		{
			var buf = lines[i].Output;

			if (i == 0)
			{
				for (int j = 0; j < len; j++)
					tempBuffer[j] = buf[j];
			}
			else
			{
				for (int j = 0; j < len; j++)
					tempBuffer[j] += buf[j];
			}
		}

		tempBuffer.Gain(perLineGain, len);

		for (int i = 0; i < len; i++)
		{
			outBuffer[i] =
				dryOut           * input[i] +
				predelayOut      * predelayOutput[i] +
				earlyOut         * earlyOutStage[i] +
				lineOut          * tempBuffer[i];
		}*/
	}

	void ReverbChannel::ClearBuffers()
	{
/*		tempBuffer.Zero();
		outBuffer.Zero();*/
		lowPass.Output = 0;
		highPass.Output = 0;

		preDelay.ClearBuffers();
		/*multitap.ClearBuffers();
		diffuser.ClearBuffers();
		foreach(var line in lines)
			line.ClearBuffers();*/
	}
	
	double ReverbChannel::GetPerLineGain()
	{
		return 1 / std::sqrt(lineCount);
	}
	
	void ReverbChannel::UpdateLines()
	{
		auto lineModRate = parameters[Parameter::LineModRate];
		auto lineModAmount = Ms2Samples(parameters[Parameter::LineModAmount]);
		auto lineFeedback = parameters[Parameter::LineFeedback];
		auto lineDelay = (int)Ms2Samples(parameters[Parameter::LineDelay]);
		if (lineDelay < 50) lineDelay = 50;

		int count = 0;// lines.Length;
		for (int i = 0; i < count; i++)
		{
			auto delay = (0.1 + 0.9 * delayLineSeeds[i]) * lineDelay;
			auto ratio = delay / lineDelay;
			auto adjustedFeedback = std::pow(lineFeedback, ratio);

			auto modAmount = lineModAmount * (0.8 + 0.2 * delayLineSeeds[i + count]);
			auto modRate = lineModRate * (0.8 + 0.2 * delayLineSeeds[i + 2 * count]) / samplerate;

/*			lines[i].SetDelay((int)delay);
			lines[i].SetFeedback(adjustedFeedback);
			lines[i].SetModAmount(modAmount);
			lines[i].SetModRate(modRate);*/
		}
	}

	double ReverbChannel::Ms2Samples(double value)
	{
		return value / 1000.0 * samplerate;
	}

}
