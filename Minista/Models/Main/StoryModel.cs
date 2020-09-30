using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Minista.Models.Main
{
    public class StoryWithLiveSupportModel : BaseModel
    {
        private StoryModel _story;
        public StoryModel Story { get => _story; set { _story = value; OnPropertyChanged("Story"); } }

        private InstaBroadcast _broadcast;
        public InstaBroadcast Broadcast { get => _broadcast; set { _broadcast = value; OnPropertyChanged("Broadcast"); } }
    
        private InstaBroadcastAddToPostLive _postLives;
        public InstaBroadcastAddToPostLive PostLives { get => _postLives; set { _postLives = value; OnPropertyChanged("PostLives"); } }

        private StoryType _type;
        public StoryType Type { get => _type; set { _type = value; OnPropertyChanged("Type"); } }
    }

    public class StoryModel : InstaReelFeed, INotifyPropertyChanged
    {
        //[AlsoNotifyFor("StrokeColor")]
        public bool IsSeen
        {
            set
            {
                if (value)
                    StrokeColor = Helper.GetColorBrush("#DF959595");
                else
                {
                    var linear = new LinearGradientBrush
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
                    StrokeColor = linear;
                }
            }
        }

        Brush strokeColor_= new LinearGradientBrush
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
        public Brush StrokeColor { get { return strokeColor_; } set { strokeColor_ = value; OnPropertyChangedX("StrokeColor"); } }

        //#FF343434 // khaakestari

        public new event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChangedX(string memberName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }
    public enum StoryType
    {
        Story,
        Broadcast,
        PostLive
    }
}
