using InstagramApiSharp.Classes.Models;
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

using System.Threading.Tasks;
using UICompositionAnimations.Enums;

namespace Minista.UserControls.Main
{
    public sealed partial class ActivityNotifyUc : UserControl
    {
        public ActivityNotifyUc()
        {
            this.InitializeComponent();
        }

        public async void Show(CompositeTransform transform, InstaActivityCount count,bool isBottom = false)
        {
            try
            {
                if (count == null) return;

                //{
                //  "usertags": 1,
                //  "comments": 0,
                //  "comment_likes": 0,
                //  "relationships": 0,
                //  "likes": 0,
                //  "campaign_notification": 0,
                //  "shopping_notification": 0,
                //  "photos_of_you": 1,
                //  "requests": 0
                //}
                // asli ha ina hasta>
                //  "usertags": 1,
                //  "comments": 0,
                //  "likes": 0,
                //  "requests": 0

                if (count.Usertags == 0 && count.Comments == 0 && count.Likes == 0 && count.Requests == 0) return;
                RenderTransform = transform;
                CommentGrid.Visibility = count.Comments == 0 ? Visibility.Collapsed : Visibility.Visible;
                LikeGrid.Visibility = count.Likes == 0 ? Visibility.Collapsed : Visibility.Visible;
                TagGrid.Visibility = count.Usertags == 0 ? Visibility.Collapsed : Visibility.Visible;
                RequestGrid.Visibility = count.Requests == 0 ? Visibility.Collapsed : Visibility.Visible;

                CommentText.Text = count.Comments.ToString();
                LikeText.Text = count.Likes.ToString();
                TagText.Text = count.Usertags.ToString();
                RequestText.Text = count.Requests.ToString();

                if (isBottom)
                {
                    PathTop.Visibility = Visibility.Collapsed;
                    PathBottom.Visibility = Visibility.Visible;
                }
                else
                {
                    PathTop.Visibility = Visibility.Visible;
                    PathBottom.Visibility = Visibility.Collapsed;
                }

                await this.Animation(FrameworkLayer.Xaml)
                     .Scale(1, 0, Easing.QuadraticEaseInOut)
                     .Duration(0)
                     .StartAsync();
                Visibility = Visibility.Visible;

                await this.Animation(FrameworkLayer.Xaml)
                      .Opacity(0, 1, Easing.CircleEaseOut)
                      .Scale(0, 1.2, Easing.QuadraticEaseInOut)
                      .Duration(250)
                      .StartAsync();


                await this.Animation(FrameworkLayer.Xaml)
                      .Scale(1.2, 1, Easing.QuadraticEaseInOut)
                      .Duration(100)
                      .StartAsync();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async ()=>
                {
                    await Task.Delay(5500);

                    await this.Animation(FrameworkLayer.Xaml)
                         .Scale(1, 1.2, Easing.QuadraticEaseInOut)
                         .Duration(100)
                         .StartAsync();


                    await this.Animation(FrameworkLayer.Xaml)
                         .Opacity(1, 0, Easing.CircleEaseOut)
                         .Scale(1.2, 0, Easing.QuadraticEaseInOut)
                         .Duration(250)
                         .StartAsync();
                    await Task.Delay(300);

                    Visibility = Visibility.Collapsed;
                });
            }
            catch { }
        }
    }
}
