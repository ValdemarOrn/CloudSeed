using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloudSeed.UI
{
	/// <summary>
	/// Interaction logic for RenameProgramDialog.xaml
	/// </summary>
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();
			this.IsVisibleChanged += (s, x) =>
			{
				this.Center();
			};

			VersionLabel.Content = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

		private void Close(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void GoToWebsite(object sender, MouseButtonEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/ValdemarOrn/CloudSeed");
		}
	}
}
