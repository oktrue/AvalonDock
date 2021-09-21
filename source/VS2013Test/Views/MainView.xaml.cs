using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;
using AvalonDock.VS2013Test.ViewModels;

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

			//var chrome = new WindowChrome
			//{
			//	CornerRadius = new CornerRadius(),
			//	GlassFrameThickness = new Thickness(0, 0, 0, 0),
			//	ResizeBorderThickness = new Thickness(0, 0, 0, 0),
			//	UseAeroCaptionButtons = false
			//};
			//BindingOperations.SetBinding(chrome, WindowChrome.CaptionHeightProperty, new Binding(NonClientAreaHeightProperty.Name) { Source = this });
			//WindowChrome.SetWindowChrome(this, chrome);

			DataContext = Workspace.This;
		}
	}
}
