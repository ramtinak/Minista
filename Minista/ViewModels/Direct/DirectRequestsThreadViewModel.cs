using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Helpers;
using InstagramApiSharp.Enums;
using static Helper;
using System.Collections.ObjectModel;
using Minista.ItemsGenerators;
using Windows.UI.Core;
using Minista.UserControls.Direct;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;


namespace Minista.ViewModels.Direct
{
    public class DirectRequestsThreadViewModel
    {
        public ObservableCollection<InstaDirectInboxItem> Items { get; set; } = new ObservableCollection<InstaDirectInboxItem>();
        public InstaDirectInboxThread CurrentThread;
        public async void SetThread(InstaDirectInboxThread directInboxThread)
        {
            CurrentThread = directInboxThread;

            try
            {
                Items.Clear();
                if (directInboxThread.Items.Count == 0)
                {
                    var result = await InstaApi.MessagingProcessor
                       .GetDirectInboxThreadAsync(directInboxThread.ThreadId,
                       PaginationParameters.MaxPagesToLoad(1), Views.Direct.InboxView.Current?.InboxVM?.SeqId ?? 0);
                    if (result.Succeeded)
                    {
                        directInboxThread.Items.AddRange(result.Value.Items);
                    }
                }

                var items = directInboxThread.Items;
                items.Reverse();
                items.ForEach(x => Items.Insert(0, x));
            }
            catch { }
            //if (CurrentThread.Items?.Count > 0)
            //{
            //    CurrentThread.Items.Reverse();
            //    CurrentThread.Items.ForEach(x => Items.Add(x));
            //    if (!any)
            //        ListView.ScrollIntoView(Items[Items.Count - 1]);
            //}
        }



    }
}
