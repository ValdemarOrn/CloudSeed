using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed
{
	public class UnsafeReverbController : IUnsafeReverbController
	{
		public int Samplerate
		{
			get { return 0; }
			set { }
		}

		public int GetParameterCount()
		{
			return 0;
		}

		public double[] GetAllParameters()
		{
			return null;
		}

		public double GetScaledParameter(Parameter param)
		{
			return 0.0;
		}

		public void SetParameter(Parameter param, double value)
		{
		}

		public void Process(IntPtr input, IntPtr output, uint inChannelCount, uint outChannelCount, uint bufferSize)
		{
		}

		public void ClearBuffers()
		{
		}
	}
}
