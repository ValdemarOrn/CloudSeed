using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using AudioLib;
using System.Linq;

namespace CloudSeed.Tests
{
	[TestClass]
	public class BlockTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var diff = new MultitapDiffuser(3000);
			diff.SetTapCount(50);
			diff.SetTapDecay(0.9);
			diff.SetTapGain(1.0);
			diff.SetTapLength((int)(2000));
			var inp = new double[3000];
			inp[0] = 1.0;
			
			diff.Process(inp, inp.Length);
			var outp = diff.Output;

			var pm = new PlotModel();
			var series = new StemSeries();
			series.MarkerSize = 1.0;
			series.Color = OxyColors.Black;
			series.Points.AddRange(outp.ToDataPoints());
			pm.Series.Add(series);
			pm.ToPng(@"e:\multitap.png", 800, 600);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var diff = new AllpassDiffuser(3000, 48000);
			diff.Seeds = new ShaRandom().Generate(652, 10000).ToArray();
			diff.SetDelay(100);
			diff.Stages = 3;
			diff.SetFeedback(0.7);
			diff.SetModAmount(0.0);
			var inp = new double[3000];
			inp[0] = 1.0;

			diff.Process(inp, inp.Length);
			var outp = diff.Output;

			var pm = new PlotModel();
			var series = new StemSeries();
			series.MarkerSize = 1.0;
			series.Color = OxyColors.Black;
			series.Points.AddRange(outp.ToDataPoints());
			pm.Series.Add(series);
			pm.ToPng(@"e:\allpass3.png", 800, 600);
		}

	}
}
