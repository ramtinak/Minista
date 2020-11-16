using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Minista.UserControls.Main
{
    public sealed partial class MediaTagUc : UserControl
    {
        public InstaUserTag UserTag;
        public InstaUserTagUpload UserTagUpload;
        public bool IsVideo { get; set; } = false;
        public MediaTagUc()
        {
            this.InitializeComponent();
        }
        public void SetUserTag(InstaUserTag tag)
        {
            UserTag = tag;
            SetText("@" + tag.User.UserName.ToLower());
            //Width = txtUsername.ActualWidth + 4;
            //Height = txtUsername.ActualHeight + 4;
        }
        public void SetUserTag(InstaUserTagUpload tagUpload, bool isVideo)
        {
            IsVideo = isVideo;
            UserTagUpload = tagUpload;
            if (!string.IsNullOrEmpty(tagUpload.Username))
                SetText("@" + tagUpload.Username.ToLower());
            else
                SetText("Who's this?");
        }
        public void SetText(string text) => txtUsername.Text = text;
        public MediaTagUc TrashItem = null;
    }
}
