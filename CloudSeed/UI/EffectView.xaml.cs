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
	public partial class EffectView : Window
	{
		private ViewModel VM;
		private DispatcherTimer Timer;

		public EffectView(ViewModel vm)
		{
			VM = vm;
			DataContext = VM;
			InitializeComponent();
			//SetupAnnouncer();
		}

		/*public SynthView(SynthController ctrl)
		{
			VM = new ViewModel(ctrl);
			DataContext = VM;
			InitializeComponent();
			SetupAnnouncer();
		}*/

		public EffectView()
		{
			InitializeComponent();
			//SetupAnnouncer();
		}

		/*void SetupAnnouncer()
		{
			Timer = new DispatcherTimer();
			Timer.Interval = TimeSpan.FromSeconds(1);
			Timer.Tick += (s, e) =>
			{
				VM.AnnouncerCaption = "";
				VM.AnnouncerValue = "";	
			};

			Announcer.RegisterAnnouncer(this, (caption, val, timeout) =>
			{
				VM.AnnouncerCaption = caption;
				VM.AnnouncerValue = val;
				Timer.Stop();

				if(timeout)
					Timer.Start();
			});
		}*/
	
	}
}
