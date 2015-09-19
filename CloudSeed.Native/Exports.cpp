
#include "Default.h"
#include "ReverbController.h"
#include "FastSin.h"
#include "AudioLib\ValueTables.h"

using namespace CloudSeed;
bool isInitialized = false;

extern "C"
{
	__dllexport ReverbController* Create(int samplerate)
	{
		if (!isInitialized)
		{
			AudioLib::ValueTables::Init();
			FastSin::Init();
			isInitialized = true;
		}

		return new ReverbController(samplerate);
	}

	__dllexport void Delete(ReverbController* item)
	{
		delete item;
	}

	__dllexport int GetSamplerate(ReverbController* item)
	{
		return item->GetSamplerate();
	}

	__dllexport void SetSamplerate(ReverbController* item, int samplerate)
	{
		return item->SetSamplerate(samplerate);
	}

	__dllexport int GetParameterCount(ReverbController* item)
	{
		return item->GetParameterCount();
	}

	__dllexport double* GetAllParameters(ReverbController* item)
	{
		return item->GetAllParameters();
	}

	__dllexport double GetScaledParameter(ReverbController* item, Parameter param)
	{
		return item->GetScaledParameter(param);
	}

	__dllexport void SetParameter(ReverbController* item, Parameter param, double value)
	{
		item->SetParameter(param, value);
	}

	__dllexport void ClearBuffers(ReverbController* item)
	{
		item->ClearBuffers();
	}

	__dllexport void Process(ReverbController* item, double** input, double** output, int bufferSize)
	{
		item->Process(input, output, bufferSize);
	}
}
