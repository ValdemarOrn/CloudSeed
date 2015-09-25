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
	public partial class RenameProgramDialog : Window
	{
		public RenameProgramDialog()
		{
			InitializeComponent();
			this.IsVisibleChanged += (s, x) => this.Center();
        }

		public bool Cancelled { get; private set; }
		
		public string ShowDialog(string title)
		{
			TitleLabel.Content = title;
			this.ShowDialog();
			return Cancelled ? null : MainTextBox.Text;
        }

		private void Save(object sender, RoutedEventArgs e)
		{
			Cancelled = false;
			Close();
		}

		private void Cancel(object sender, RoutedEventArgs e)
		{
			Cancelled = true;
			Close();
		}
	}
}
