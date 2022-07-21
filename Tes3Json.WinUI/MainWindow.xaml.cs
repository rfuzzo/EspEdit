using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tes3Json.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tes3Json.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(480, 800);

            //this.DataContext = App.Current.Services.GetService<MainPageViewModel>();
            ViewModel = App.Current.Services.GetService<MainPageViewModel>();
        }

        public MainPageViewModel ViewModel { get; set; }

        private void MenuExit_Clicked(object sender, RoutedEventArgs e) => Application.Current.Exit();

        private void MenuChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            //switch (Application.Current.UserAppTheme)
            //{
            //    case AppTheme.Unspecified:
            //        Application.Current.UserAppTheme = AppTheme.Light;
            //        break;
            //    case AppTheme.Light:
            //        Application.Current.UserAppTheme = AppTheme.Dark;
            //        break;
            //    case AppTheme.Dark:
            //        Application.Current.UserAppTheme = AppTheme.Light;
            //        break;
            //    default:
            //        break;
            //}
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => ViewModel.PerformSearchCommand.Execute(args.QueryText);

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => ViewModel.PerformSearchCommand.Execute(findRecordsTextBox.Text);
    }
}
