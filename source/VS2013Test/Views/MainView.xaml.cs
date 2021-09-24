using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using AvalonDock.VS2013Test.ViewModels;
using ControlzEx.Native;
using ControlzEx.Standard;

namespace AvalonDock.VS2013Test.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainView
	{
		public MainView()
		{
			InitializeComponent();
			DataContext = Workspace.This;
		}

		//TODO: Move code below to a new base window class and inherit GlowWindow from it
		#region CustomWindowChrome

		private static readonly PropertyInfo criticalHandlePropertyInfo = typeof(Window).GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance);
#if NET452
		private static readonly object[] emptyObjectArray = new object[0];
#else
		private static readonly object[] emptyObjectArray = Array.Empty<object>();
#endif

#pragma warning disable 618

		private const SWP _SwpFlags = SWP.FRAMECHANGED | SWP.NOSIZE | SWP.NOMOVE | SWP.NOZORDER | SWP.NOOWNERZORDER | SWP.NOACTIVATE;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			HwndSource hwnd = PresentationSource.FromDependencyObject(this) as HwndSource;
			hwnd.AddHook(WndProc);
			hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
			MARGINS dwmMargin = new MARGINS();
			NativeMethods.DwmExtendFrameIntoClientArea(hwnd.Handle, ref dwmMargin);
			NativeMethods.SetWindowPos(hwnd.Handle, IntPtr.Zero, 0, 0, 0, 0, _SwpFlags);
		}

		protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case (int)WM.NCCALCSIZE:
					if (wParam != IntPtr.Zero)
					{
						handled = true;
					}
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}

		private void Header_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				e.Handled = true;

				VerifyAccess();
				_ = UnsafeNativeMethods.ReleaseCapture();
				IntPtr criticalHandle = (IntPtr)criticalHandlePropertyInfo.GetValue(this, emptyObjectArray);
				Point wpfPoint = PointToScreen(Mouse.GetPosition(this));
				int x = (int)wpfPoint.X;
				int y = (int)wpfPoint.Y;
				_ = NativeMethods.SendMessage(criticalHandle, WM.NCLBUTTONDOWN, (IntPtr)HT.CAPTION, new IntPtr(x | (y << 16)));
			}
			else if (e.ClickCount == 2 && ResizeMode != ResizeMode.NoResize)
			{
				e.Handled = true;

				if (WindowState == WindowState.Normal && ResizeMode != ResizeMode.NoResize && ResizeMode != ResizeMode.CanMinimize)
				{
					SystemCommands.MaximizeWindow(this);
				}
				else
				{
					SystemCommands.RestoreWindow(this);
				}
			}
		}

		private void Header_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			ControlzEx.Windows.Shell.SystemCommands.ShowSystemMenu(this, e);
		}

		private void Icon_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Border icon = sender as Border;
			Point p = icon.TransformToAncestor(this).Transform(new Point(0, icon.ActualHeight));
			ControlzEx.Windows.Shell.SystemCommands.ShowSystemMenu(this, p);
		}

#pragma warning restore 618

		#endregion

		#region SystemCommands

		protected override void OnInitialized(EventArgs e)
		{
			_ = CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
			_ = CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
			_ = CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
			_ = CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));

			base.OnInitialized(e);
		}

		private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode != ResizeMode.NoResize;
		}

		private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		private void OnCloseWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void OnMaximizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}

		private void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}

		#endregion
	}
}
