using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CloudSeed.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class CloudSeedView : UserControl
	{
		private CloudSeedViewModel viewModel;

		public CloudSeedView(CloudSeedViewModel viewModel)
		{
			this.viewModel = viewModel;
			DataContext = viewModel;
			InitializeComponent();
		}

		public CloudSeedView()
		{
			InitializeComponent();
			Setup();
		}

		void Setup()
		{
			var linkedControls = ParameterControl.GetChildrenWithValue(this);


			/*nouncer.RegisterAnnouncer(this, (caption, val, timeout) =>
			{
				VM.AnnouncerCaption = caption;
				VM.AnnouncerValue = val;
				Timer.Stop();

				if(timeout)
					Timer.Start();
			});*/
		}
	
	}
}
