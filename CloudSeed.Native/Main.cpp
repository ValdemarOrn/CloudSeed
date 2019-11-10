#include <iostream>
#include "Exports.h"
#include "AudioFile.h"
#include "Main.h"
#include "Parameter.h"

using namespace std;

void SetProgram(ReverbController* plugin)
{
	SetParameter(plugin, Parameter::InputMix, 0.0);
	SetParameter(plugin, Parameter::PreDelay, 0.0);
	SetParameter(plugin, Parameter::HighPass, 0.0);
	SetParameter(plugin, Parameter::LowPass, 0.63999992609024048);
	SetParameter(plugin, Parameter::TapCount, 0.51999980211257935);
	SetParameter(plugin, Parameter::TapLength, 0.26499992609024048);
	SetParameter(plugin, Parameter::TapGain, 0.69499999284744263);
	SetParameter(plugin, Parameter::TapDecay, 1.0);
	SetParameter(plugin, Parameter::DiffusionEnabled, 1.0);
	SetParameter(plugin, Parameter::DiffusionStages, 0.8571428656578064);
	SetParameter(plugin, Parameter::DiffusionDelay, 0.5700000524520874);
	SetParameter(plugin, Parameter::DiffusionFeedback, 0.76000010967254639);
	SetParameter(plugin, Parameter::LineCount, 0.18181818723678589);
	SetParameter(plugin, Parameter::LineDelay, 0.585000216960907);
	SetParameter(plugin, Parameter::LineDecay, 0.29499980807304382);
	SetParameter(plugin, Parameter::LateDiffusionEnabled, 1.0);
	SetParameter(plugin, Parameter::LateDiffusionStages, 0.57142859697341919);
	SetParameter(plugin, Parameter::LateDiffusionDelay, 0.69499951601028442);
	SetParameter(plugin, Parameter::LateDiffusionFeedback, 0.71499985456466675);
	SetParameter(plugin, Parameter::PostLowShelfGain, 0.87999987602233887);
	SetParameter(plugin, Parameter::PostLowShelfFrequency, 0.19499993324279785);
	SetParameter(plugin, Parameter::PostHighShelfGain, 0.72000008821487427);
	SetParameter(plugin, Parameter::PostHighShelfFrequency, 0.520000159740448);
	SetParameter(plugin, Parameter::PostCutoffFrequency, 0.79999983310699463);
	SetParameter(plugin, Parameter::EarlyDiffusionModAmount, 0.13499999046325684);
	SetParameter(plugin, Parameter::EarlyDiffusionModRate, 0.26000010967254639);
	SetParameter(plugin, Parameter::LineModAmount, 0.054999928921461105);
	SetParameter(plugin, Parameter::LineModRate, 0.21499986946582794);
	SetParameter(plugin, Parameter::LateDiffusionModAmount, 0.17999963462352753);
	SetParameter(plugin, Parameter::LateDiffusionModRate, 0.38000011444091797);
	SetParameter(plugin, Parameter::TapSeed, 0.0003009999927598983);
	SetParameter(plugin, Parameter::DiffusionSeed, 0.00018899999849963933);
	SetParameter(plugin, Parameter::DelaySeed, 0.0001610000035725534);
	SetParameter(plugin, Parameter::PostDiffusionSeed, 0.00050099997315555811);
	SetParameter(plugin, Parameter::CrossSeed, 0.7850000262260437);
	SetParameter(plugin, Parameter::DryOut, 1.0);
	SetParameter(plugin, Parameter::PredelayOut, 0.0);
	SetParameter(plugin, Parameter::EarlyOut, 0.699999988079071);
	SetParameter(plugin, Parameter::MainOut, 0.84499984979629517);
	SetParameter(plugin, Parameter::HiPassEnabled, 0.0);
	SetParameter(plugin, Parameter::LowPassEnabled, 1.0);
	SetParameter(plugin, Parameter::LowShelfEnabled, 1.0);
	SetParameter(plugin, Parameter::HighShelfEnabled, 0.0);
	SetParameter(plugin, Parameter::CutoffEnabled, 1.0);
	SetParameter(plugin, Parameter::LateStageTap, 1.0);
	SetParameter(plugin, Parameter::Interpolation, 1.0);
}

int main()
{
	cout << "Running CloudSeed Test...";
	const int BUFSIZE = 64;

	// --------- Reading WAV File ---------
	AudioFile<double> audioFile;
	audioFile.load("c:\\dev\\sound.wav");

	int sampleRate = audioFile.getSampleRate();
	int bitDepth = audioFile.getBitDepth();

	int numSamples = audioFile.getNumSamplesPerChannel();
	double lengthInSeconds = audioFile.getLengthInSeconds();

	int numChannels = audioFile.getNumChannels();
	bool isMono = audioFile.isMono();
	bool isStereo = audioFile.isStereo();

	// allocate output buffers with 5 extra seconds for the reverb tail
	int audioLengthSamples = numSamples + sampleRate * 5;
	double* left = new double[audioLengthSamples]();
	double* right = new double[audioLengthSamples]();
	double* leftOut = new double[audioLengthSamples]();
	double* rightOut = new double[audioLengthSamples]();

	// fill the input buffers
	for (int i = 0; i < numSamples; i++)
	{
		int chLeft = 0;
		int chRight = isStereo ? 1 : 0;
		left[i] = audioFile.samples[chLeft][i];
		right[i] = audioFile.samples[chRight][i];
	}

	// --------- Initializing plugin ---------
	auto plugin = Create(sampleRate);
	SetProgram(plugin);
	ClearBuffers(plugin);

	// --------- Processing loop ---------
	int idx = 0;
	while (idx < audioLengthSamples - BUFSIZE)
	{
		double* ins[2] = { &left[idx], &right[idx] };
		double* outs[2] = { &leftOut[idx], &rightOut[idx] };
		Process(plugin, ins, outs, BUFSIZE);
		idx += BUFSIZE;
	}

	// --------- Write output to file ---------
	AudioFile<double>::AudioBuffer buffer;
	buffer.resize(2);
	buffer[0].resize(audioLengthSamples);
	buffer[1].resize(audioLengthSamples);
	for (int i = 0; i < audioLengthSamples; i++)
	{
		buffer[0][i] = leftOut[i];
		buffer[1][i] = rightOut[i];
	}
	bool ok = audioFile.setAudioBuffer(buffer);
	audioFile.save("c:\\dev\\sound-processed.wav");

	int p;
	cin >> p;
	return 0;
}
