using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudSeed.Tests
{
	[TestClass]
	public class TestAllpass
	{
		[TestMethod]
		public void TestAllpass1()
		{
			var ap = new ModulatedAllpass(200, 5);
			ap.Feedback = 0.9;
			var input = new double[200];
			input[0] = 1.0;
			ap.Process(input, input.Length);
			var output = ap.Output;
		}
	}
}
