using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Minista.Converters
{
    class RepliedToMessageImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            if (value is InstaDirectInboxItem item)
            {
                var type = item.ItemType;
                if (type == InstaDirectThreadItemType.FelixShare && item.FelixShareMedia != null)
                    return item.FelixShareMedia.Images[0].Uri.ToUri();
                else if (type == InstaDirectThreadItemType.MediaShare && item.MediaShare != null)
                {
                    switch (item.MediaShare.MediaType)
                    {
                        case InstaMediaType.Carousel:
                            {
                                if (!string.IsNullOrEmpty(item.MediaShare.CarouselShareChildMediaId))
                                {
                                    var defaultMedia = item.MediaShare.Carousel.FirstOrDefault(m => m.InstaIdentifier == item.MediaShare.CarouselShareChildMediaId);
                                    if (defaultMedia != null)
                                        return defaultMedia.Images.FirstOrDefault().Uri.ToUri();
                                }
                                else
                                    return item.MediaShare.Carousel.FirstOrDefault().Images.FirstOrDefault().Uri.ToUri();
                            }
                            break;
                        default:
                            return item.MediaShare.Images.FirstOrDefault().Uri.ToUri();
                    }
                }
                else if (type == InstaDirectThreadItemType.Media && item.Media != null)
                    return item.Media.Images[0].Uri.ToUri();
                else if (type == InstaDirectThreadItemType.Profile && item.ProfileMedia != null)
                    return item.ProfileMedia.ProfilePicture.ToUri();
                else if (type == InstaDirectThreadItemType.ReelShare && item.ReelShareMedia != null &&
                    item.ReelShareMedia?.Media?.Images?.Count != 0 && item.ReelShareMedia?.Media?.Videos?.Count != 0)
                    return item.ReelShareMedia.Media.Images[0].Uri.ToUri();
                else if (type == InstaDirectThreadItemType.StoryShare && item.StoryShare != null)
                    return item.StoryShare?.Media?.Images?[0].Uri.ToUri();
                
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    class RepliedToMessageImageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            Visibility Visibile() => Visibility.Visible;

            if (value is InstaDirectInboxItem item)
            {
                var type = item.ItemType;
                if ((type == InstaDirectThreadItemType.FelixShare && item.FelixShareMedia != null) ||
                    (type == InstaDirectThreadItemType.MediaShare && item.MediaShare != null) ||
                    (type == InstaDirectThreadItemType.Media && item.Media != null) ||
                    (type == InstaDirectThreadItemType.Profile && item.ProfileMedia != null) ||
                    (type == InstaDirectThreadItemType.StoryShare && item.StoryShare != null) ||
                    (type == InstaDirectThreadItemType.ReelShare && item.ReelShareMedia != null &&
                    item.ReelShareMedia?.Media?.Images?.Count != 0 && item.ReelShareMedia?.Media?.Videos?.Count != 0))
                    return Visibile();

            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
