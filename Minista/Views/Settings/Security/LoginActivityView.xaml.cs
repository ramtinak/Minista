using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Minista.Views.Settings.Security
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginActivityView : Page
    {
        public LoginActivityViewModel LoginActivityVM { get; set; } = new LoginActivityViewModel();
        public LoginActivityView()
        {
            this.InitializeComponent();
            DataContext = LoginActivityVM;
            Loaded += LoginActivityViewLoaded;
        }

        private void LoginActivityViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginActivityVM.RunLoadMore();
            }
            catch { }
        }

        public void ShowTopLoading() => TopLoading.Start();
        public void HideTopLoading() => TopLoading.Stop();

        private void ToggleMenuToggled(object sender, RoutedEventArgs e)
        {

        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
