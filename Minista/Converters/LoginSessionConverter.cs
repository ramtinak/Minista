using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Minista.Converters
{
    public class LoginSessionTextConverter : IValueConverter
    { 
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is InstaLoginSession loginSession && loginSession != null)
            {
                if (loginSession.IsCurrent)
                    return "Active now";
                else
                    return new DateTimeFullWithoutSomeDatesConverter().Convert(loginSession.Timestamp.FromUnixTimeSeconds());
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class LoginSessionColorConverter : IValueConverter 
    {
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is InstaLoginSession loginSession && loginSession != null)
            {
                if (loginSession.IsCurrent)
                    return "#FF187CF5".GetColorBrush();
            }

            return "#FF838383".GetColorBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
