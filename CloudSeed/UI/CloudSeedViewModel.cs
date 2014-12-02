using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CloudSeed.UI
{
	public class CloudSeedViewModel : INotifyPropertyChanged
	{
		private readonly ReverbController instance;

		public event PropertyChangedEventHandler PropertyChanged;

		public CloudSeedViewModel(ReverbController instance)
		{
			this.instance = instance;
		}

		public double Parameter0
		{
			get { return instance.Parameters[0]; }
			set { instance.Parameters[0] = value; NotifyChanged(); }
		}
		
		public void Update(int parameter)
		{
			NotifyChanged(parameter.ToString());
		}

		#region Notify Change

		// I used this and GetPropertyName to avoid having to hard-code property names
		// into the NotifyChange events. This makes the application much easier to refactor
		// leter on, if needed.
		private void NotifyChanged<T>(System.Linq.Expressions.Expression<Func<T>> exp)
		{
			var name = GetPropertyName(exp);
			NotifyChanged(name);
		}

		private void NotifyChanged([CallerMemberName]string property = null)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
		}

		private static string GetPropertyName<T>(System.Linq.Expressions.Expression<Func<T>> exp)
		{
			return (((System.Linq.Expressions.MemberExpression)(exp.Body)).Member).Name;
		}

		#endregion
	}
}
