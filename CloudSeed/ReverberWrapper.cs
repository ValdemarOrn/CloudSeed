using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CloudSeed
{
	public unsafe class ReverberWrapper
	{
		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern IntPtr Create();

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void Delete(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern double* GetParameters(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetSamplerate(IntPtr item, double samplerate);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetTaps(IntPtr item, double* indexOffsets, double* amplitudes, int count);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetLate(IntPtr item, double* feedback, int* delaySamples);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetHiCut(IntPtr item, double* fc, double* amount);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetAllpassMod(IntPtr item, double* freq, double* amount);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void Process(IntPtr item, double* input, double* output, int len);

		IntPtr Instance;
		double Samplerate;
		double* ParameterArray;

		public ReverberWrapper()
		{
			Instance = Create();
			ParameterArray = GetParameters(Instance);
		}

		public void SetParameter(ParameterEnum para, double value)
		{
			ParameterArray[(int)para] = value;
		}

		public double GetParameter(ParameterEnum para)
		{
			return ParameterArray[(int)para];
		}

		public void SetSamplerate(double samplerate) 
		{
			Samplerate = samplerate;
			SetSamplerate(Instance, samplerate); 
		}

		public void SetTaps(double size, int density)
		{
			if (size < 5)
				size = 5;
			if (density < 5)
				density = 5;

			var rand = new Random();
			var tapIndexes = Enumerable.Range(0, density).Select(x => rand.NextDouble()).OrderBy(x => x).ToArray();
			var tapAmplitudes = tapIndexes.Select(x => Math.Exp(-x / 1.0 * 3) * 2 * (0.5 - rand.NextDouble())).ToArray();
			
			fixed(double* indexes = tapIndexes)
			fixed(double* amps = tapAmplitudes)
			{
				SetTaps(Instance, indexes, amps, density);
			}
		}

		public void SetLate(double feedback, int delaySamples)
		{
			var rand = new Random();
			double* feedbacks = stackalloc double[Constants.ALLPASS_COUNT];
			int* delays = stackalloc int[Constants.ALLPASS_COUNT];

			for (int i = 0; i < Constants.ALLPASS_COUNT; i++)
			{
				feedbacks[i] = feedback * (0.9 + 0.1 * rand.NextDouble());
				feedbacks[i] = feedbacks[i] > 0.98 ? 0.98 : feedbacks[i];
				delays[i] = (int)(delaySamples * (1 + 0.5 * i * (0.3 + 0.7 * rand.NextDouble())));
			}

			SetLate(Instance, feedbacks, delays);
		}

		public void SetHiCut(double fc, double amount)
		{
			var rand = new Random();
			double* fcs = stackalloc double[Constants.ALLPASS_COUNT];
			double* amounts = stackalloc double[Constants.ALLPASS_COUNT];

			for (int i = 0; i < Constants.ALLPASS_COUNT; i++)
			{
				fcs[i] = fc * (0.5 + rand.NextDouble());
				amounts[i] = amount;
			}

			SetHiCut(Instance, fcs, amounts);
		}

		public void SetAllpassMod(double freq, double amount)
		{
			var rand = new Random();
			double* freqs = stackalloc double[Constants.ALLPASS_COUNT];
			double* amounts = stackalloc double[Constants.ALLPASS_COUNT];

			for (int i = 0; i < Constants.ALLPASS_COUNT; i++)
			{
				freqs[i] = freq * (1 + 0.8 * rand.NextDouble());
				amounts[i] = amount;
			}

			SetAllpassMod(Instance, freqs, amounts);
		}

		public void Process(double[] input, double[] output)
		{
			var len = input.Length;

			fixed (double* ins = input)
			fixed (double* outs = output)
			{
				Process(Instance, ins, outs, len);
			}
		}


		~ReverberWrapper()
		{
			Delete(Instance);
		}
	}
}
