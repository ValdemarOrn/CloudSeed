
#ifndef DEFAULT_INCLUDE
#define DEFAULT_INCLUDE

namespace Parameter
{
	enum ParameterEnum
	{
		Predelay = 0, 
		EarlySize, 
		Density, 
		GlobalFeedback, 
		GlobalDelay, 
		HiCut, 
		HiCutAmt, 
		LowCut,
		LowCutAmt,
		APDelay,
		APFeedback,
		ModRate,
		ModAmount,
		StageCount,
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

const int PARAM_COUNT = Parameter::Wet;
const int BUF_LEN = 192000;
const int ALLPASS_COUNT = 8;
const int MAX_TAP_COUNT = 200;

#endif
