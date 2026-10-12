
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace ScreenDimmer
{
	public partial class DimOverlayWindow : Window
	{
		private const int GWL_EXSTYLE = -20;

		private const long WS_EX_TRANSPARENT = 0x20;
		private const long WS_EX_LAYERED = 0x80000;
		private const long WS_EX_NOACTIVATE = 0x08000000;

		private readonly Forms.Screen screen;

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW",
			SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr64(
			IntPtr hwnd, int index);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongW",
			SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr32(
			IntPtr hwnd, int index);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW",
			SetLastError = true)]
		private static extern IntPtr SetWindowLongPtr64(
			IntPtr hwnd, int index, IntPtr value);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongW",
			SetLastError = true)]
		private static extern IntPtr SetWindowLongPtr32(
			IntPtr hwnd, int index, IntPtr value);

		private static IntPtr GetStyle(IntPtr hwnd)
		{
			return IntPtr.Size == 8
				? GetWindowLongPtr64(hwnd, GWL_EXSTYLE)
				: GetWindowLongPtr32(hwnd, GWL_EXSTYLE);
		}

		private static void SetStyle(IntPtr hwnd, IntPtr value)
		{
			if (IntPtr.Size == 8)
				SetWindowLongPtr64(hwnd, GWL_EXSTYLE, value);
			else
				SetWindowLongPtr32(hwnd, GWL_EXSTYLE, value);
		}

		public DimOverlayWindow(Forms.Screen targetScreen)
		{
			InitializeComponent();

			screen = targetScreen;

			SourceInitialized += OnSourceInitialized;
		}

		private void OnSourceInitialized(
			object? sender, EventArgs e)
		{
			IntPtr hwnd = new WindowInteropHelper(this).Handle;

			long style = GetStyle(hwnd).ToInt64();

			style |= WS_EX_TRANSPARENT;
			style |= WS_EX_LAYERED;
			style |= WS_EX_NOACTIVATE;

			SetStyle(hwnd, new IntPtr(style));

			var source = PresentationSource.FromVisual(this);

			if (source?.CompositionTarget != null)
			{
				var transform =
					source.CompositionTarget.TransformFromDevice;

				var bounds = screen.Bounds;

				var topLeft = transform.Transform(
					new System.Windows.Point(bounds.Left, bounds.Top));

				var bottomRight = transform.Transform(
					new System.Windows.Point(bounds.Right, bounds.Bottom));

				Left = topLeft.X;
				Top = topLeft.Y;
				Width = bottomRight.X - topLeft.X;
				Height = bottomRight.Y - topLeft.Y;
			}
		}

		public void SetDarkness(double percent)
		{
			percent = Math.Clamp(percent, 0, 90);

			OverlayGrid.Opacity = percent / 100.0;
		}
	}
}
