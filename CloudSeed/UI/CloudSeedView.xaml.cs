using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AudioLib.WpfUi;

namespace CloudSeed.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class CloudSeedView : UserControl
	{
		class DoubleToBoolConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				double dVal = (value is double) ? (double)value : 0.0;
				return dVal >= 0.5;
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				var isTrue = value.Equals(true);
				return isTrue ? 1.0 : 0.0;
			}
		}

		class FreeConverter<TA, TB> : IValueConverter
		{
			private readonly Func<TA, TB> convert;
			private readonly Func<TB, TA> convertBack;

			public FreeConverter(Func<TA, TB> convert, Func<TB, TA> convertBack)
			{
				this.convert = convert;
				this.convertBack = convertBack;
			}

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				if (value is TA)
					return convert((TA)value);

				throw new ArgumentException();
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				if (value is TB)
					return convertBack((TB)value);

				throw new ArgumentException();
			}
		}

		private readonly CloudSeedViewModel viewModel;

		public CloudSeedView(CloudSeedViewModel viewModel)
		{
			this.viewModel = viewModel;
			DataContext = viewModel;
			InitializeComponent();
			Setup();
		}

		public CloudSeedView()
		{
			var plugin = new CloudSeedPlugin();
			plugin.InitializeDevice();

			viewModel = plugin.ViewModel;
			DataContext = viewModel;
			InitializeComponent();
			Setup();
		}

		void Setup()
		{
			var linkedControls = ParameterControl.GetChildrenWithValue(this);

			foreach (var linkedControl in linkedControls)
			{
				var param = linkedControl.Value;
				var control = linkedControl.Key as Control;

				var binding = new Binding("NumberedParameters[" + param.Value() + "]");
				binding.Source = viewModel;
				binding.Mode = BindingMode.TwoWay;

				control.MouseEnter += (s, e) => viewModel.ActiveControl = param;
				control.MouseLeave += (s, e) => { if (viewModel.ActiveControl == param) { viewModel.ActiveControl = null; } };

				if (control is Knob2)
				{
					control.SetBinding(Knob2.ValueProperty, binding);
				}
				else if (control is ToggleButton)
				{
					binding.Converter = new DoubleToBoolConverter();
					control.SetBinding(ToggleButton.IsCheckedProperty, binding);
				}
				else if (control is Spinner)
				{
					var spinner = control as Spinner;
					binding.Converter = new FreeConverter<double, double>(
						x => (int)(spinner.Min + x * (spinner.Max - spinner.Min) + 0.00001),
						x => (x - spinner.Min) / (spinner.Max - spinner.Min));

					control.SetBinding(Spinner.ValueProperty, binding);
				}
			}
		}

		private void ShowSaveDialog(object sender, RoutedEventArgs e)
		{
			var dialog = new RenameProgramDialog();
			dialog.Owner = Parent as Window;
			var newName = dialog.ShowDialog("Save new Program");

			if (newName != null)
			{
				viewModel.SaveProgramCommand.Execute(newName);
			}
		}

		private void DeleteProgram(object sender, RoutedEventArgs e)
		{
			var ok = MessageBox.Show("Are you sure you want to delete program " + viewModel.SelectedProgram.Name + "?", "Delete Program", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ok == MessageBoxResult.Yes)
			{
				viewModel.DeleteProgramCommand.Execute(null);
			}
		}

		private void CloseDialogs(object sender, RoutedEventArgs e)
		{
			SaveProgramDialog.Visibility = Visibility.Collapsed;
			RenameProgramDialog.Visibility = Visibility.Collapsed;
		}

		private void ProgramLabelClick(object sender, MouseButtonEventArgs e)
		{
			ProgramLabel.ContextMenu.DataContext = DataContext;
			ProgramLabel.ContextMenu.IsOpen = true;
		}

		private void ShowAboutDialog(object sender, MouseButtonEventArgs e)
		{
			var dialog = new AboutDialog();
			dialog.Owner = Parent as Window;
			dialog.ShowDialog();
		}
	}
}
