using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace CloudSeed.UI
{
	public static class CenterHelper
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);

		[StructLayout(LayoutKind.Sequential)]
		private struct Rect
		{
			public int Left, Top, Right, Bottom;
		}

		public static void Center(this Window dialog)
		{
			var owner = dialog.Owner;

			Rect rect;
			var ownerHandle = new WindowInteropHelper(owner).Handle;
			GetWindowRect(ownerHandle, out rect);

			var w = rect.Right - rect.Left;
			var h = rect.Bottom - rect.Top;

			var x = (int)(rect.Left + 0.5 * w - 0.5 * dialog.Width);
			var y = (int)(rect.Top + 0.5 * h - 0.5 * dialog.Height);

			dialog.Left = x;
			dialog.Top = y;
		}

	}
}
