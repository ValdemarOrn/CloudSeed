#ifndef AUDIOLIB_VALUETABLES
#define AUDIOLIB_VALUETABLES

namespace AudioLib
{
	class ValueTables
	{
	public:
		static const int TableSize = 4001;

		static double Sqrt[TableSize];
		static double Sqrt3[TableSize];
		static double Pow1_5[TableSize];
		static double Pow2[TableSize];
		static double Pow3[TableSize];
		static double Pow4[TableSize];
		static double x2Pow3[TableSize];

		// octave response. value double every step (2,3,4,5 or 6 steps)
		static double Response2Oct[TableSize];
		static double Response3Oct[TableSize];
		static double Response4Oct[TableSize];
		static double Response5Oct[TableSize];
		static double Response6Oct[TableSize];

		// decade response, value multiplies by 10 every step
		static double Response2Dec[TableSize];
		static double Response3Dec[TableSize];
		static double Response4Dec[TableSize];

		static void Init();
		static double Get(double index, double* table);
	};
}

#endif
