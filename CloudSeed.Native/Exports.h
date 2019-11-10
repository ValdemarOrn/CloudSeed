#pragma once

#include "Default.h"
#include "ReverbController.h"
#include "FastSin.h"
#include "AudioLib\ValueTables.h"

using namespace CloudSeed;

extern "C"
{
	__dllexport ReverbController* Create(int samplerate);
	__dllexport void Delete(ReverbController* item);
	__dllexport int GetSamplerate(ReverbController* item);
	__dllexport void SetSamplerate(ReverbController* item, int samplerate);
	__dllexport int GetParameterCount(ReverbController* item);
	__dllexport double* GetAllParameters(ReverbController* item);
	__dllexport double GetScaledParameter(ReverbController* item, Parameter param);
	__dllexport void SetParameter(ReverbController* item, Parameter param, double value);
	__dllexport void ClearBuffers(ReverbController* item);
	__dllexport void Process(ReverbController* item, double** input, double** output, int bufferSize);
}