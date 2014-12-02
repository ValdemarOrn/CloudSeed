using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudSeed
{
	public class ParameterControl : FrameworkElement
	{
		public static Parameter? GetParameter(DependencyObject obj)
		{
			return (Parameter?)obj.GetValue(ParameterProperty);
		}

		public static void SetParameter(DependencyObject obj, Parameter? value)
		{
			obj.SetValue(ParameterProperty, value);
		}

		public static readonly DependencyProperty ParameterProperty = DependencyProperty.RegisterAttached("Parameter",
			typeof(Parameter?), typeof(ParameterControl), new PropertyMetadata(null, PropertyChanged));

		private static void PropertyChanged(DependencyObject item, DependencyPropertyChangedEventArgs e)
		{
			item.SetValue(e.Property, e.NewValue);
		}

		public static Dictionary<DependencyObject, Parameter> GetChildrenWithValue(DependencyObject depObj)
		{
			if (depObj is ContentControl && ((ContentControl)depObj).Content is DependencyObject)
				return GetChildrenWithValue(((ContentControl)depObj).Content as DependencyObject);

			var output = new Dictionary<DependencyObject, Parameter>();

			if (depObj == null)
				return new Dictionary<DependencyObject, Parameter>();

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
				if (child == null)
					continue;

				var para = GetParameter(child);
				if (para != null)
					output[child] = para.Value;
				
				var subChildren = GetChildrenWithValue(child);
				foreach (var subChild in subChildren)
					output[subChild.Key] = subChild.Value;
			}

			return output;
		}
	}
}
