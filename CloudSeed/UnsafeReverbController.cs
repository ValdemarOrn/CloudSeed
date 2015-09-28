using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public unsafe class UnsafeReverbController : IUnsafeReverbController, IDisposable
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

		private static object createLock = new object();

		private IntPtr instance;

		public UnsafeReverbController(int samplerate)
		{
			lock (createLock)
			{
				instance = Create(samplerate);
			}
		}

		~UnsafeReverbController()
		{
			Delete(instance);
		}

		public void Dispose()
		{
			Delete(instance);
			GC.SuppressFinalize(this);
		}

		public int Samplerate
		{
			get { return GetSamplerate(instance); }
			set { SetSamplerate(instance, value); }
		}

		public int GetParameterCount()
		{
			return GetParameterCount(instance);
		}

		public double[] GetAllParameters()
		{
			IntPtr para = (IntPtr)GetAllParameters(instance);
			var count = GetParameterCount(instance);
			var output = new double[count];
			Marshal.Copy(para, output, 0, count);
			return output;
		}

		public double GetScaledParameter(Parameter param)
		{
			return GetScaledParameter(instance, (int)param);
		}

		public void SetParameter(Parameter param, double value)
		{
			SetParameter(instance, (int)param, value);
		}

		public void Process(IntPtr input, IntPtr output, int bufferSize)
		{
			Process(instance, (double**)input, (double**)output, bufferSize);
		}

		public void Process(double[][] input, double[][] output, int bufferSize)
		{
			var inL = input[0];
			var inR = input[1];
			var outL = output[0];
			var outR = output[1];
			var inPtr = new IntPtr[2];
			var outPtr = new IntPtr[2];

			fixed (double* il = inL)
			fixed (double* ir = inR)
			fixed (double* ol = outL)
			fixed (double* or = outR)
			fixed (IntPtr* ins = inPtr)
			fixed (IntPtr* outs = outPtr)
			{
				ins[0] = (IntPtr)il;
				ins[1] = (IntPtr)ir;
				outs[0] = (IntPtr)ol;
				outs[1] = (IntPtr)or;

				Process((IntPtr)ins, (IntPtr)outs, bufferSize);
			}
		}

		public void ClearBuffers()
		{
			ClearBuffers(instance);
		}
	}
}
