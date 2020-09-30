using Minista.Models.Main;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minista.Selectors
{
    public class MainStoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BroadcastTemplate { get; set; }
        public DataTemplate PostLiveTemplate { get; set; } 
        public DataTemplate StoryTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var Item = item as StoryWithLiveSupportModel;

            if (Item.Type == StoryType.Story)
                return StoryTemplate;
            else if (Item.Type == StoryType.Broadcast)
                return BroadcastTemplate;
            else
                return PostLiveTemplate;
        }
    }
}
