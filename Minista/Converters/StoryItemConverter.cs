using System;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Xaml.Data;
using System.Linq;
using Minista.Models.Main;
using Windows.UI.Xaml;

namespace Minista.Converters
{
    class StoryItemProfilePictureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaReelFeed data)
            {
                try
                {
                    if(data.IsHashtag || data.IsElection)
                        return new Uri(data.Owner.ProfilePicture);
                    else
                        return new Uri(data.User.ProfilePicture);
                }
                catch { }
            }
            else if (value is StoryModel data2)
            {
                try
                {
                    if (data2.IsHashtag || data2.IsElection)
                        return new Uri(data2.Owner.ProfilePicture);
                    else
                        return new Uri(data2.User.ProfilePicture);
                }
                catch { }
            }
            else if (value is InstaBroadcast data3)
            {
                try
                {
                    return new Uri(data3.BroadcastOwner.ProfilePicture);
                }
                catch { }
            }
            else if (value is InstaBroadcastAddToPostLive data4)
            {
                try
                {
                    return new Uri(data4.User.ProfilePicture);
                }
                catch { }
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class StoryItemUsernameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";
            if (value is InstaReelFeed data)
            {
                try
                {
                    if (data.IsHashtag || data.IsElection)
                        return data.Owner.Name;
                    else
                        return data.User.UserName;
                }
                catch { }
            }
            else if (value is StoryModel data2)
            {
                try
                {
                    if (data2.IsHashtag || data2.IsElection)
                        return data2.Owner.Name;
                    else
                        return data2.User.UserName;
                }
                catch { }
            }
            else if (value is InstaBroadcast data3)
            {
                try
                {
                    return data3.BroadcastOwner.UserName;
                }
                catch { }
            }
            else if (value is InstaBroadcastAddToPostLive data4)
            {
                try
                {
                    return data4.User.UserName;
                }
                catch { }
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class StoryItemHashtagVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is InstaReelFeed data)
            {
                try
                {
                    if (data.IsHashtag)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                catch { }
            }
            else if (value is StoryModel data2)
            {
                try
                {
                    if (data2.IsHashtag)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                catch { }
            }

            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class HighlightCoverToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is InstaHighlightCoverMedia data)
            {
                try
                {
                    if (data.CroppedImage != null)
                        return data.CroppedImage.Uri.ToUri();
                }
                catch { }
            }
            return new Uri("ms-appx://Assets/Images/no-profile.jpg");
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TVSelfChannelToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is InstaTVSelfChannel data)
            {
                try
                {
                    if (data.Items.FirstOrDefault() != null)
                        return data.Items.FirstOrDefault().Images.FirstOrDefault().Uri.ToUri();
                }
                catch { }
            }
            return new Uri("ms-appx://Assets/Images/no-profile.jpg");
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
