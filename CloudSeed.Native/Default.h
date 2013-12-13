
#ifndef DEFAULT_INCLUDE
#define DEFAULT_INCLUDE

namespace Parameter
{
	enum ParameterEnum
	{
		Predelay = 0,
		EarlySize,
		Density,

		APDelay,
		APFeedback,

		HiCut,
		HiCutAmt,
		LowCut,
		LowCutAmt,

		ModRate,
		ModAmount,
		StageCount,

		GlobalFeedback,
		GlobalDelay,

		Dry,
		Wet
	};
}

#define PI 3.141592653589793238462643383

#define __dllexport __declspec(dllexport)

#ifdef DEBUG
	#define __inline_always inline
#else
	#define __inline_always __forceinline
#endif

const int PARAM_COUNT = Parameter::Wet + 1;
const int BUF_LEN = 65536;
const int ALLPASS_COUNT = 8;
const int MAX_TAP_COUNT = 200;
const int MODULO = 65535;

#endif
