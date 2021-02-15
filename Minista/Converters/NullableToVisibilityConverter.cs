using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Collections.ObjectModel;

namespace Minista.Converters
{
    class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is string str)
            {
                if (!string.IsNullOrEmpty(str))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else if (value is int ix && ix > 0)
                return Visibility.Visible;
            else if (value is object obj && obj != null)
            {
                if (int.TryParse(obj.ToString(), out int r))
                {
                    if (r == 0)
                        return Visibility.Collapsed;
                }
                else
                    return Visibility.Visible;
            }
            else if (value is List<object> list && list?.Count > 0)
                return Visibility.Visible;
            else if (value is ObservableCollection<object> observ && observ?.Count > 0)
                return Visibility.Visible;
            else if (value is ObservableCollection<InstaRelatedHashtag> htag && htag?.Count > 0)
                return Visibility.Visible;
            else if (value is List<InstaRelatedHashtag> ltag && ltag?.Count > 0)
                return Visibility.Visible;
            else if (value is InstaBroadcast broadcast && broadcast != null)
                return Visibility.Visible;
            else if (value is InstaChannel channel && channel != null)
                return Visibility.Visible;
            else if (value is InstaDirectInboxItem data && data != null && data.ItemType == InstaDirectThreadItemType.ReelShare)
            {
                if (data.ReelShareMedia.ReelType != "reaction")
                {
                    if (!string.IsNullOrEmpty(data.ReelShareMedia.Text))
                        return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class NotNullableToVisibilityConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) 
                return Visibility.Visible;
            else
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
