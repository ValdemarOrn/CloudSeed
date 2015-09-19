using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudSeed.Tests
{
	[TestClass]
	public class UnsafeReverbTests
	{
		[TestMethod]
		public void TestCreate()
		{
			var rev = new UnsafeReverbController(48000);
			rev.Dispose();
		}
	}
}
