using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CloudSeed.Tests
{
	public static class OxyPlotHelper
	{
		public class LineStyle
		{
			public OxyColor Color { get; set; }
			public double Thickness { get; set; }
			public OxyPlot.LineStyle Style { get; set; }
		}

		public static IEnumerable<ScatterPoint> ToScatterPoints(this IEnumerable<double> values)
		{
			int x = 0;
			foreach (var val in values)
				yield return new ScatterPoint(x++, val);
		}

		public static IEnumerable<DataPoint> ToDataPoints(this IEnumerable<double> values)
		{
			int x = 0;
			foreach (var val in values)
				yield return new DataPoint(x++, val);
		}

		public static void Show(this PlotModel model, double width = 800, double height = 500, bool asDialog = true)
		{
			var w = new Window() { Title = model.Title ?? "", Width = width, Height = height };
			var plotView = new PlotView();
			plotView.Model = model;
			w.Content = plotView;
			if (asDialog)
				w.ShowDialog();
			else
				w.Show();
		}

		public static OxyPlot.Axes.Axis AddDateTimeAxis(this PlotModel model, string title, DateTimeIntervalType interval = DateTimeIntervalType.Auto)
		{
			var axis = new OxyPlot.Axes.DateTimeAxis
			{
				IntervalType = interval,
				Position = AxisPosition.Bottom,
				Title = title,
				MajorTickSize = 3.0,
				MinorTickSize = 1.0
			};
			model.Axes.Add(axis);
			return axis;
		}

		public static void SetLimitsX(this PlotModel model, double min, double max)
		{
			var axes = model.Axes.Where(x =>
					x.GetType() != typeof(OxyPlot.Axes.DateTimeAxis) &&
					x.IsHorizontal())
				.ToArray();

			foreach (var axis in axes)
			{
				axis.Minimum = min;
				axis.Maximum = max;
			}
		}

		public static void SetLimitsX(this PlotModel model, DateTime min, DateTime max)
		{
			var axes = model.Axes.Where(x =>
					x.GetType() == typeof(OxyPlot.Axes.DateTimeAxis) &&
					x.IsHorizontal())
				.ToArray();

			foreach (var axis in axes)
			{
				axis.Minimum = OxyPlot.Axes.DateTimeAxis.ToDouble(min);
				axis.Maximum = OxyPlot.Axes.DateTimeAxis.ToDouble(max);
			}
		}

		public static void SetLimitsY(this PlotModel model, double min, double max)
		{
			var axes = model.Axes.Where(x => x.IsVertical()).ToArray();

			foreach (var axis in axes)
			{
				axis.Minimum = min;
				axis.Maximum = max;
			}
		}

		public static void AddLine(
			this PlotModel model,
			IEnumerable<double> data,
			LineStyle lineStyle = null)
		{
			var data2 = data.Select((x, i) => new { Value = x, Index = i }).ToArray();
			AddLine(model, data2, x => x.Value, x => x.Index, lineStyle);
		}

		public static void AddLine<T>(
			this PlotModel model,
			IEnumerable<T> data,
			Func<T, double> ySelector,
			Func<T, DateTime> xSelector,
			LineStyle lineStyle = null)
		{
			model.AddLine(data, ySelector, x => OxyPlot.Axes.DateTimeAxis.ToDouble(xSelector(x)), lineStyle);
		}

		public static void AddLine<T>(
			this PlotModel model,
			IEnumerable<T> data,
			Func<T, double> ySelector,
			Func<T, double> xSelector = null,
			LineStyle lineStyle = null)
		{
			var series = new OxyPlot.Series.LineSeries();

			var dataList = data.ToList();
			for (int i = 0; i < dataList.Count; i++)
			{
				var item = dataList[i];
				var x = xSelector != null ? xSelector(item) : (double)i;
				var y = ySelector(item);
				series.Points.Add(new DataPoint(x, y));
			}

			if (lineStyle != null)
			{
				series.Color = lineStyle.Color;
				series.StrokeThickness = lineStyle.Thickness;
				series.LineStyle = lineStyle.Style;
			}

			model.Series.Add(series);
		}

		public static void ToPng(this PlotModel model, string filename, int width, int height, OxyColor? backgound = null)
		{
			using (var stream = System.IO.File.Create(filename))
			{
				PngExporter.Export(model, stream, width, height, backgound ?? OxyColors.White);
			}
		}
	}
}
