using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSeed.UI
{
	public enum ReductionEffect
	{
		ResolutionBit8 = 0,
		ResolutionBit12 = 1,
		ResolutionFull = 2,

		UndersamplingOff = 3,
		Undersampling2x = 4,
		Undersampling4x = 5,
		Undersampling8x = 6,
		
		InterpolationDisabled = 7,
        InterpolationEnabled = 8,
	}
}
