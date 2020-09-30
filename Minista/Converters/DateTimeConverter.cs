// Copyright ali noshahi
// kesh rafte shode az Winsta App

using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Minista.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = DateTime.UtcNow.Subtract(System.Convert.ToDateTime(value));
            if (v.TotalHours < 1)
            {
                if (v.TotalSeconds >= 1)
                {
                    if (v.TotalSeconds > 59)
                        return $"{System.Convert.ToInt32(v.TotalMinutes)}min";
                    else
                    {
                        if (v.Seconds > 10)
                            return $"{System.Convert.ToInt32(v.Seconds)}s";
                        else
                            return "Just now";
                    }
                }
                else
                    return "Just now";
            }
            else if (v.TotalHours < 24)
                return $"{System.Convert.ToInt32(v.TotalHours)}h";
            else if (v.TotalDays <= 7)
                return $"{System.Convert.ToInt32(v.TotalDays)}d";
            else if (System.Convert.ToInt32(v.TotalDays / 7) < 4)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)}w";
            else if (System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) < 12)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)}w";
            //return $"{System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4)}m";
            else
                return $"{System.Convert.ToInt32(System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) / 12)}y";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class DirectItemDateTimeConverter : IValueConverter
    { 
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var time = System.Convert.ToDateTime(value);

            return time.ToLocalTime().ToString("HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeFullConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = DateTime.UtcNow.Subtract(System.Convert.ToDateTime(value));
            if (v.TotalHours < 1)
            {
                if (v.TotalSeconds >= 30)
                {
                    if (v.TotalSeconds > 59)
                        return $"{System.Convert.ToInt32(v.TotalMinutes)} minutes ago";
                    else
                    {
                        if (v.TotalSeconds > 10)
                            return $"{System.Convert.ToInt32(v.Seconds)} seconds ago";
                        else
                            return "Just now";
                    }
                }
                else
                    return "Just now";
            }
            else if (v.TotalHours < 24)
                return $"{System.Convert.ToInt32(v.TotalHours)} hours ago";
            else if (v.TotalDays <= 7)
                return $"{System.Convert.ToInt32(v.TotalDays)} days ago";
            else if (System.Convert.ToInt32(v.TotalDays / 7) < 4)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)} weeks ago";
            else if (System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) < 12)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)} weeks ago";
            //return $"{System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4)}m";
            else
                return $"{System.Convert.ToInt32(System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) / 12)} years ago";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeFullWithoutSomeDatesConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = DateTime.UtcNow.Subtract(System.Convert.ToDateTime(value));
            if (v.TotalHours < 1)
            {
                if (v.TotalSeconds >= 30)
                {
                    if (v.TotalSeconds > 59)
                        return $"{System.Convert.ToInt32(v.TotalMinutes)} minutes ago";
                    else
                    {
                        if (v.TotalSeconds > 10)
                            return $"{System.Convert.ToInt32(v.Seconds)} seconds ago";
                        else
                            return "Just now";
                    }
                }
                else
                    return "Just now";
            }
            else if (v.TotalHours < 24)
                return $"{System.Convert.ToInt32(v.TotalHours)} hours ago";
            else if (v.TotalDays <= 7)
                return $"{System.Convert.ToInt32(v.TotalDays)} days ago";
            else 
                return System.Convert.ToDateTime(value).ToString("MMMM dd", new CultureInfo("en-US"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
