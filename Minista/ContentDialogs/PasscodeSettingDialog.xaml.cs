using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.ContentDialogs
{
    public sealed partial class PasscodeSettingDialog : ContentPopup
    {
        public PasscodeSettingDialog()
        {
            this.InitializeComponent();
            Loaded += PasscodeSettingDialogLoaded;
        }
        private bool CanDoThings = false;

        private void PasscodeSettingDialogLoaded(object sender, RoutedEventArgs e)
        {
            CanDoThings = false;
            try
            {
                ChangePasscodeStack.Visibility = Helper.Passcode.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
                ToggleLock.IsOn = Helper.Passcode.IsEnabled;
            }
            catch { }

            CanDoThings = true;
        }

        private void BackButtonClick(object sender, RoutedEventArgs e) => Hide();

        private async void ToggleLockToggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDoThings) return;
                if (!ToggleLock.IsOn)
                {
                    Helper.Passcode.Reset();
                    await Task.Delay(150);
                    ChangePasscodeStack.Visibility = Helper.Passcode.IsEnabled ? Visibility.Visible : Visibility.Collapsed;

                }
                else
                    ChangePasscodeToggleButtonClick(null, null);
            }
            catch { }
        }

        private async void ChangePasscodeToggleButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
            try
            {
                await new AddPasscodeDialog().ShowAsync();
            }
            catch { }
        }
    }
}
