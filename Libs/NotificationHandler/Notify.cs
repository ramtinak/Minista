using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace NotifySharp
{
    public static class Notify
    {
        public static void SendMessageNotify(string name, string message, string image, string action, string heroImage = null)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = name ?? "",
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = message ?? ""
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = image,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },
                        HeroImage = heroImage != null ? new ToastGenericHeroImage { Source = heroImage } : null
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastTextBox("textBox")
                        {
                            PlaceholderContent = "reply"
                        }
                    },
                    Buttons =
                    {
                        new ToastButton("Send", action)
                        {
                            ActivationType = ToastActivationType.Background,
                            ImageUri = "Assets/Icons/send.png",
                            TextBoxId = "textBox"
                        }
                    }
                },
                Launch = action
            };

            var toastNotif = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        public static void SendMessageWithoutTextNotify(string name, string message, string image, string action, string heroImage = null)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = name,
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text =message
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = image,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },
                        HeroImage = heroImage != null ? new ToastGenericHeroImage { Source = heroImage } : null
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Open", action)
                        {
                            ActivationType = ToastActivationType.Background,
                        },
                        new ToastButton("Dismiss", "dismiss=True")
                        {
                            ActivationType = ToastActivationType.Background,
                        }
                    }
                },
                Launch = action
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        public static void SendLikeNotify(string name, string message, string image, string action, string heroImage = null)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = name,
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text =message
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = image,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },
                        HeroImage = heroImage != null ? new ToastGenericHeroImage { Source = heroImage } : null
                    }
                },
                Launch = action
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

    }
}
