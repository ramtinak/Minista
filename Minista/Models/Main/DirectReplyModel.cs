using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minista.Helpers;

namespace Minista.Models.Main
{
    public class DirectReplyModel : InstaDirectInboxItem
    {
        private Uri _userProfile, _image;
        string _username, _textToShow;
        public string Username { get => _username; set { _username = value; OnPropertyChanged("Username"); } }

        //public string UserProfile { get => _userProfile; set { _userProfile = value; OnPropertyChanged("UserProfile"); } }

        public string TextToShow { get => _textToShow; set { _textToShow = value; OnPropertyChanged("TextToShow"); } }

        public Uri UserProfile { get => _userProfile; set { _userProfile = value; OnPropertyChanged("UserProfile"); } }
        public Uri Image { get => _image; set { _image = value; OnPropertyChanged("Image"); } }

        internal static DirectReplyModel GetReplyModel(InstaDirectInboxItem item, InstaDirectInboxThread thread)
        {
            var type = item.ItemType;
            var reply = new DirectReplyModel
            {
                TextToShow = item.Text,
            };

            try
            {
                PropertyCopy.Copy(item, reply);
                if (type == InstaDirectThreadItemType.VoiceMedia)
                    reply.TextToShow = "Voice message";

                if (type == InstaDirectThreadItemType.FelixShare && item.FelixShareMedia != null)
                {
                    reply.TextToShow = (item.FelixShareMedia.Caption?.Text);
                    reply.Image = item.FelixShareMedia.Images[0].Uri.ToUri();
                }
                else if (type == InstaDirectThreadItemType.Hashtag && item.HashtagMedia != null)
                    reply.TextToShow = (item.HashtagMedia.Name);
                else if (type == InstaDirectThreadItemType.Link && item.LinkMedia != null)
                    reply.TextToShow = (item.LinkMedia.LinkContext.LinkUrl);
                else if (type == InstaDirectThreadItemType.Location && item.LocationMedia != null)
                    reply.TextToShow = (item.LocationMedia.Name);
                else if (type == InstaDirectThreadItemType.MediaShare && item.MediaShare != null)
                {
                    reply.TextToShow = (item.MediaShare.Caption?.Text);
                    switch (item.MediaShare.MediaType)
                    {
                        case InstaMediaType.Carousel:
                            {
                                bool flag = false;
                                if (!string.IsNullOrEmpty(item.MediaShare.CarouselShareChildMediaId))
                                {
                                    var defaultMedia = item.MediaShare.Carousel.FirstOrDefault(m => m.InstaIdentifier == item.MediaShare.CarouselShareChildMediaId);
                                    if (defaultMedia != null)
                                    {
                                        reply.Image = defaultMedia.Images.FirstOrDefault().Uri.ToUri();
                                        flag = true;
                                    }
                                }
                                if(!flag)
                                    reply.Image = item.MediaShare.Carousel.FirstOrDefault().Images.FirstOrDefault().Uri.ToUri();
                            }
                            break;
                        default:
                            reply.Image = item.MediaShare.Images.FirstOrDefault().Uri.ToUri();
                            break;
                    }
                }
                else if (type == InstaDirectThreadItemType.Media && item.Media != null)
                {
                    reply.TextToShow = null;
                    reply.Image = item.Media.Images.FirstOrDefault().Uri.ToUri();
                }
                else if (type == InstaDirectThreadItemType.Profile && item.ProfileMedia != null)
                {
                    reply.TextToShow = (item.ProfileMedia?.UserName);
                    reply.Image = item.ProfileMedia.ProfilePicture.ToUri();
                }
                else if (type == InstaDirectThreadItemType.ReelShare && item.ReelShareMedia != null)
                {
                    reply.TextToShow = (item.ReelShareMedia?.Text);
                    try
                    {
                        if (item.ReelShareMedia.Media.Images.Count != 0 && item.ReelShareMedia.Media.Videos.Count != 0)
                            reply.Image = item.ReelShareMedia.Media.Images[0].Uri.ToUri();
                    }
                    catch { }
                }
                else if (type == InstaDirectThreadItemType.StoryShare && item.StoryShare != null)
                {
                    reply.TextToShow = (item.StoryShare?.Text);
                    try
                    {
                        reply.Image = item.StoryShare.Media.Images[0].Uri.ToUri();
                    }
                    catch { }
                }
                else
                    reply.TextToShow = (item.Text);
                if (Helper.CurrentUser.Pk != item.UserId)
                {
                    var findUser = thread.Users.FirstOrDefault(x => x.Pk == item.UserId);
                    if (findUser != null)
                    {
                        reply.UserProfile = findUser.ProfilePicture.ToUri();
                        reply.Username = findUser.FullName;
                    }
                    if (!thread.IsGroup)
                        reply.UserProfile = null;

                }
                else
                {
                    reply.Username = Helper.CurrentUser.FullName;
                    reply.UserProfile = null;
                }

                return reply;
            }
            catch { }
            return item as DirectReplyModel;
        }
    }
}
