using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;
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
		private static readonly PropertyInfo criticalHandlePropertyInfo = typeof(Window).GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly object[] emptyObjectArray = new object[0];

		public MainView()
        {
            InitializeComponent();
			DataContext = Workspace.This;

			var chrome = new WindowChrome
			{
				CornerRadius = new CornerRadius(),
				GlassFrameThickness = new Thickness(0, 0, 0, 0),
				ResizeBorderThickness = new Thickness(0, 0, 0, 0),
				CaptionHeight = 0,
				UseAeroCaptionButtons = false
			};
			//WindowChrome.SetWindowChrome(this, chrome);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			var hwnd = PresentationSource.FromDependencyObject(this) as HwndSource;
			hwnd.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
			hwnd.AddHook(FilterMessage);
		}

		protected virtual IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			handled = false;

			switch (msg)
			{
				case 0x0083: // NCCALCSIZE
					if (wParam != IntPtr.Zero) handled = true;
					break;
				case 0x0014: //WM_ERASEBKGND:
							 //hdc = (HDC)wParam;
							 //GetClientRect(hwnd, &rc);
							 //SetMapMode(hdc, MM_ANISOTROPIC);
							 //SetWindowExtEx(hdc, 100, 100, NULL);
							 //SetViewportExtEx(hdc, rc.right, rc.bottom, NULL);
							 //FillRect(hdc, &rc, hbrWhite);
					//handled = true;
					break;
			}
			return IntPtr.Zero;
		}

#pragma warning disable 618
		private void Header_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				e.Handled = true;

				// taken from DragMove internal code
				this.VerifyAccess();

				// for the touch usage
				UnsafeNativeMethods.ReleaseCapture();

				var criticalHandle = (IntPtr)criticalHandlePropertyInfo.GetValue(this, emptyObjectArray);

				// these lines are from DragMove
				// NativeMethods.SendMessage(criticalHandle, WM.SYSCOMMAND, (IntPtr)SC.MOUSEMOVE, IntPtr.Zero);
				// NativeMethods.SendMessage(criticalHandle, WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

				var wpfPoint = this.PointToScreen(Mouse.GetPosition(this));
				var x = (int)wpfPoint.X;
				var y = (int)wpfPoint.Y;
				NativeMethods.SendMessage(criticalHandle, WM.NCLBUTTONDOWN, (IntPtr)HT.CAPTION, new IntPtr(x | (y << 16)));
			}
			else if (e.ClickCount == 2
					 && this.ResizeMode != ResizeMode.NoResize)
			{
				e.Handled = true;

				if (this.WindowState == WindowState.Normal
					&& this.ResizeMode != ResizeMode.NoResize
					&& this.ResizeMode != ResizeMode.CanMinimize)
				{
					SystemCommands.MaximizeWindow(this);
				}
				else
				{
					SystemCommands.RestoreWindow(this);
				}
			}
		}
#pragma warning restore 618

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
