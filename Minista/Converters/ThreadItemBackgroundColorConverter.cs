using System;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Xaml.Data;

namespace Minista.Converters
{
    class ThreadItemBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //#FF1D1C1C   #FF252525
            if (value == null) return "DirectItemPeopleBackgroundColor".GetColorFromResource();
            if (value is InstaDirectInboxItem data && data != null)
            {
                if(data.UserId == Helper.CurrentUser.Pk)
                    return "DirectItemSelfBackgroundColor".GetColorFromResource();
            }
            return "DirectItemPeopleBackgroundColor".GetColorFromResource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
