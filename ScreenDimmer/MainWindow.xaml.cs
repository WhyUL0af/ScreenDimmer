
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace ScreenDimmer
{
	public partial class MainWindow : Window
	{
		private readonly List<DimOverlayWindow> overlays = new();

		private Forms.NotifyIcon? trayIcon;

		private bool isExiting = false;

		public MainWindow()
		{
			InitializeComponent();

			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;
			Closed += MainWindow_Closed;
		}

		private void MainWindow_Loaded(
			object sender, RoutedEventArgs e)
		{
			CreateOverlays();
			CreateTrayIcon();

			Topmost = true;
			Activate();
		}

		private void CreateOverlays()
		{
			foreach (var overlay in overlays)
			{
				overlay.Close();
			}

			overlays.Clear();

			foreach (var screen in Forms.Screen.AllScreens)
			{
				var overlay = new DimOverlayWindow(screen);

				overlay.Show();
				overlay.SetDarkness(DarknessSlider.Value);

				overlays.Add(overlay);
			}
		}

		private void CreateTrayIcon()
		{
			var menu = new Forms.ContextMenuStrip();

			menu.Items.Add(
				"Open ScreenDimmer",
				null,
				(s, e) => ShowMainWindow());

			menu.Items.Add(
				"Reset Darkness",
				null,
				(s, e) =>
				{
					Dispatcher.BeginInvoke(() =>
					{
						DarknessSlider.Value = 0;
					});
				});

			menu.Items.Add(new Forms.ToolStripSeparator());

			menu.Items.Add(
				"Exit",
				null,
				(s, e) => ExitApplication());

			trayIcon = new Forms.NotifyIcon
			{
				Icon = SystemIcons.Application,
				Text = "ScreenDimmer",
				ContextMenuStrip = menu,
				Visible = true
			};

			trayIcon.DoubleClick +=
				(s, e) => ShowMainWindow();
		}

		private void ShowMainWindow()
		{
			Dispatcher.BeginInvoke(() =>
			{
				Show();

				WindowState = WindowState.Normal;

				Topmost = true;

				Activate();
			});
		}

		private void DarknessSlider_ValueChanged(
			object sender,
			RoutedPropertyChangedEventArgs<double> e)
		{
			if (DarknessText == null)
				return;

			int darkness = (int)Math.Round(e.NewValue);

			DarknessText.Text = $"{darkness}%";

			foreach (var overlay in overlays)
			{
				overlay.SetDarkness(darkness);
			}
		}

		private void ResetButton_Click(
			object sender,
			RoutedEventArgs e)
		{
			DarknessSlider.Value = 0;
		}

		private void MainWindow_Closing(
			object? sender, CancelEventArgs e)
		{
			if (!isExiting)
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void ExitApplication()
		{
			Dispatcher.BeginInvoke(() =>
			{
				isExiting = true;

				DarknessSlider.Value = 0;

				foreach (var overlay in overlays)
				{
					overlay.Close();
				}

				overlays.Clear();

				Close();

				System.Windows.Application.Current.Shutdown();
			});
		}

		private void MainWindow_Closed(
			object? sender, EventArgs e)
		{
			if (trayIcon != null)
			{
				trayIcon.Visible = false;
				trayIcon.ContextMenuStrip?.Dispose();
				trayIcon.Dispose();
			}
		}
	}
}
