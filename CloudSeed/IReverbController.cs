using System;

namespace CloudSeed
{
	public interface IReverbController
	{
		int Samplerate { get; set; }

		int GetParameterCount();
		double[] GetAllParameters();
		double GetScaledParameter(Parameter param);
		void SetParameter(Parameter param, double value);
		void ClearBuffers();
	}

	public interface IManagedReverbController : IReverbController
	{
		void Process(double[][] input, double[][] output);
	}

	public interface IUnsafeReverbController : IReverbController
	{
		void Process(IntPtr input, IntPtr output, int bufferSize);
	}
}