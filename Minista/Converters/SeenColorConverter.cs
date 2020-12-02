using System;
using InstagramApiSharp.Classes.Models;
using Minista.Models.Main;
using Windows.UI.Xaml.Data;
using InstagramApiSharp.Helpers;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Minista.Converters
{
    class SeenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var color = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                //StartPoint = new Point(0.7, .2),
                //EndPoint = new Point(0.5, 1.2),
                GradientStops =
                        {    new GradientStop() { Color = "#FF187CF5".GetColorFromHex() },
                            new GradientStop() { Color = "#FF90CDFB".GetColorFromHex(), Offset = 1 }
                            //new GradientStop() { Color = "#FFEE1262".GetColorFromHex() },
                            //new GradientStop() { Color = "#FFFF7F35".GetColorFromHex(), Offset = 1 }
                        }
            }; 
            if (value == null) return color;
            //if (value is long data)
            //{
            //    if (data > 0)
            //        return Helper.GetColorBrush("#DF959595"); // white
            //}
            if (value is StoryModel model)
            {
                //if (model.Items?.Count > 0)
                {
                    //if (model.Seen > 1 /*&& model.Items.LastOrDefault()?.TakenAt.Year > 2009*/)
                    {
                        //var last = (int)model.Items.LastOrDefault()?.TakenAt.ToUnixTime();
                        if (model.Seen == model.LatestReelMedia)
                            return Helper.GetColorBrush("#DF959595");
                        //if (model.User.UserName.Contains("najme"))
                        //{

                        //    model.IsSeen = model.Seen != -1;
                        //}
                    }
                }
            }
            else if(value is long l)
            {
                if(l != 0)
                    return Helper.GetColorBrush("#DF959595");
            }
            //if(value != null) return Helper.GetColorBrush("#DF959595");
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
