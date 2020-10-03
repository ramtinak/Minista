using Minista.Helpers;
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


namespace Minista.ContentDialogs
{
    public sealed partial class AddPasscodeDialog : ContentPopup
    {
        public AddPasscodeDialog()
        {
            this.InitializeComponent();
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PassText.Password))
            {
                PassText.Focus(FocusState.Keyboard);
                VisualUtilities.ShakeView(PassText);

                return;
            }
            else if (string.IsNullOrEmpty(RePassText.Password))
            {
                RePassText.Focus(FocusState.Keyboard);
                VisualUtilities.ShakeView(RePassText);
                return;
            }
            if (PassText.Password.Length == 4)
            {
                if (PassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(PassText.Password))
                { }
                else
                {
                    PassText.Focus(FocusState.Keyboard);
                    VisualUtilities.ShakeView(PassText);
                    Helper.ShowNotify("Enter 4-digits numbers");
                    return;
                }
            }
            if (RePassText.Password.Length == 4)
            {
                if (RePassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(RePassText.Password))
                { }
                else
                {
                    RePassText.Focus(FocusState.Keyboard);
                    VisualUtilities.ShakeView(RePassText);
                    Helper.ShowNotify("Enter 4-digits numbers");
                    return;
                }
            }
            {
                if (PassText.Password == RePassText.Password)
                {
                    Helper.Passcode.Set(PassText.Password);

                    Hide();
                }
                else
                {
                    VisualUtilities.ShakeView(RePassText);

                    RePassText.Focus(FocusState.Keyboard);
                }
            }
        }
        
        private void CancelButtonClick(object sender, RoutedEventArgs e) => Hide();

        private void PassTextPasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PassText.Password.Length == 4)
                {
                    if (PassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(PassText.Password))
                        RePassText.Focus(FocusState.Keyboard);
                    else
                    {
                        VisualUtilities.ShakeView(PassText);
                        Helper.ShowNotify("Enter 4-digits numbers");
                    }
                }
            }
            catch { }
        }

        private void PassTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (PassText.Password.Length == 4)
                    {
                        if (PassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(PassText.Password))
                            RePassText.Focus(FocusState.Keyboard);
                        else
                        {
                            VisualUtilities.ShakeView(PassText);
                            Helper.ShowNotify("Enter 4-digits numbers");
                        }
                    }
                }
                catch { }
            }
        }

        private void RePassTextPasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RePassText.Password.Length == 4)
                {
                    if (RePassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(PassText.Password))
                        AddButtonClick(null, null);
                    else
                    {
                        VisualUtilities.ShakeView(RePassText);
                        Helper.ShowNotify("Enter 4-digits numbers");
                    }
                }
            }
            catch { }
        }

        private void RePassTextKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    if (RePassText.Password.Length == 4)
                    {
                        if (RePassText.Password.All(x => x >= '0' && x <= '9') && Helper.Passcode.Check(PassText.Password))
                            AddButtonClick(null, null);
                        else
                        {
                            VisualUtilities.ShakeView(RePassText);
                            Helper.ShowNotify("Enter 4-digits numbers");
                        }
                    }
                }
                catch { }
            }
        }
    }
}
