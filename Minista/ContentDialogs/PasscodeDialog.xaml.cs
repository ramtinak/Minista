using Minista.Views.Security;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Minista.ContentDialogs
{
    public sealed partial class PasscodeDialog : ContentPopup
    {
        private readonly bool ForAccounts = false;
        public Action CallMeAnAction;
        public PasscodeDialog(bool forAcc = false) 
        {
            ForAccounts = forAcc;
            this.InitializeComponent();
            Loaded += PasscodeDialogLoaded;
        }
        private void BackButtonClick(object sender, RoutedEventArgs e) => Hide();

        private void PasscodeDialogLoaded(object sender, RoutedEventArgs e)
        { 
            try { Passcode.OnSuccessPass -= PasscodeOnSuccessPass; }
            catch { }
            try { Passcode.OnSuccessPass += PasscodeOnSuccessPass; }
            catch { }
        }

        private async void PasscodeOnSuccessPass()
        {
            try
            {
                Hide();
                if (ForAccounts)
                    CallMeAnAction?.Invoke();
                else
                    await new PasscodeSettingDialog().ShowAsync();
            }
            catch { }
        }
    }
}
