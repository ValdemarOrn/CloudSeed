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
		static extern IntPtr Create(int samplerate);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void Delete(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern int GetSamplerate(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetSamplerate(IntPtr item, int samplerate);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern int GetParameterCount(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern double* GetAllParameters(IntPtr item);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern double GetScaledParameter(IntPtr item, int param);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void SetParameter(IntPtr item, int param, double value);

		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void ClearBuffers(IntPtr item);
		
		[DllImport(@"CloudSeed.Native.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false, ThrowOnUnmappableChar = false)]
		static extern void Process(IntPtr item, double** input, double** output, int bufferSize);

		IntPtr Instance;
		double Samplerate;
		double* ParameterArray;
		object lockObject;

		public ReverberWrapper()
		{
			lockObject = new object();
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

		public void SetTaps(int grainCount)
		{
			lock (lockObject)
			{
				if (grainCount < 1)
					grainCount = 1;

				var rand = new Random();
				var tapIndexes = Enumerable.Range(0, grainCount).Select(x => rand.NextDouble()).OrderBy(x => x).ToArray();
				var tapAmplitudes = tapIndexes.Select(x => Math.Exp(-x / 1.0 * 3) * 2 * (0.5 - rand.NextDouble())).ToArray();

				if (tapAmplitudes.Length == 1)
				{
					tapIndexes[0] = 1;
					tapAmplitudes[0] = 1.0;
				}

				fixed (double* indexes = tapIndexes)
				fixed (double* amps = tapAmplitudes)
				{
					SetTaps(Instance, indexes, amps, grainCount);
				}
			}
		}

		public void SetEarly(double feedback, int delaySamples)
		{
			lock (lockObject)
			{
				var rand = new Random();
				double* feedbacks = stackalloc double[Constants.ALLPASS_COUNT];
				int* delays = stackalloc int[Constants.ALLPASS_COUNT];
				var delayRatios = new[] { 1.0, 1.06654, 1.1273561, 1.2484571, 1.3823743, 1.411276356, 1.527432, 1.5996734 };
				for (int i = 0; i < Constants.ALLPASS_COUNT; i++)
				{
					feedbacks[i] = feedback * (1 + i * 0.02);
					feedbacks[i] = feedbacks[i] > 0.98 ? 0.98 : feedbacks[i];
					var delay = (delaySamples / delayRatios[i]) * (1 + 0.05 * rand.NextDouble());
					delays[i] = (int)delay;
				}

				SetEarly(Instance, feedbacks, delays);
			}
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
			lock (lockObject)
			{
				var len = input.Length;

				fixed (double* ins = input)
				fixed (double* outs = output)
				{
					Process(Instance, ins, outs, len);
				}
			}
		}


		~ReverberWrapper()
		{
			Delete(Instance);
		}
	}
}
