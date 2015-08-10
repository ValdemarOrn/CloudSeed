#ifndef PARAMETER
#define PARAMETER

enum class Parameter
{
	// Input

	CrossMix = 0,
	PreDelay,

	HighPass,
	LowPass,

	// Early

	TapCount,
	TapLength,
	TapGain,
	TapDecay,

	DiffusionEnabled,
	DiffusionStages,
	DiffusionDelay,
	DiffusionFeedback,

	// Late

	LineCount,
	LineDelay,
	LineFeedback,

	PostDiffusionEnabled,
	PostDiffusionStages,
	PostDiffusionDelay,
	PostDiffusionFeedback,

	// Frequency Response

	PostLowShelfGain,
	PostLowShelfFrequency,
	PostHighShelfGain,
	PostHighShelfFrequency,
	PostCutoffFrequency,

	// Modulation

	DiffusionModAmount,
	DiffusionModRate,

	LineModAmount,
	LineModRate,

	// Seeds

	TapSeed,
	DiffusionSeed,
	CombSeed,
	PostDiffusionSeed,

	// Output

	StereoWidth,

	DryOut,
	PredelayOut,
	EarlyOut,
	MainOut,

	// Switches
	HiPassEnabled,
	LowPassEnabled,
	LowShelfEnabled,
	HighShelfEnabled,
	CutoffEnabled,

	Count,

	Unused = 999
};

#endif
