
#ifndef DEFAULT_INCLUDE
#define DEFAULT_INCLUDE

namespace Parameter
{
	enum ParameterEnum
	{
		PredelayLeft = 0,     
		PredelayRight,
		EarlySizeLeft, 
		EarlySizeRight,
		GrainCountLeft,
		GrainCountRight,

		EarlySeedLeft,
		EarlySeedRight,
		LateSeedLeft,
		LateSeedRight,
		StageCount,
		Parallel,


		AllpassDelay,
		AllpassFeedback,
		AllpassModRate,
		AllpassModAmount,

		HiCut,
		HiCutAmount,
		LowCut,
		LowCutAmount,


		Delay1,
		Delay2,
		Delay3,
		Feedback1,
		Feedback2,
		Feedback3,
		ModRate1,
		ModRate2,
		ModRate3,
		ModAmt1,
		ModAmt2,
		ModAmt3,


		FreqLow,
		FreqHi,
		FreqMid,
		GainLow,
		GainHi,
		GainMid,
		QMid,


		EarlyOut,
		WetOut,
		DryOut,

		COUNT
	};
}

#define PI 3.141592653589793238462643383

#define __dllexport __declspec(dllexport)

#ifdef DEBUG
	#define __inline_always inline
#else
	#define __inline_always __forceinline
#endif

const int PARAM_COUNT = Parameter::COUNT;
const int BUF_LEN = 65536;
const int ALLPASS_COUNT = 8;
const int MAX_TAP_COUNT = 200;
const int MODULO = 65535;
const int LATE_COUNT = 16;

#endif
