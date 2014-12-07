using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloudSeed.UI
{
	/// <summary>
	/// Interaction logic for BorderlessDialog.xaml
	/// </summary>
	public partial class BorderlessDialog : Window
	{
		public BorderlessDialog()
		{
			InitializeComponent();
		}

		protected void CloseDialog(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
