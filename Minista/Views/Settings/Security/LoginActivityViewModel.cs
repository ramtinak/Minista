using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Minista.Views.Settings.Security
{
    public class LoginActivityViewModel : BaseModel
    {
        public ObservableCollection<InstaLoginSession> SessionItems { get; set; } = new ObservableCollection<InstaLoginSession>();
        public ObservableCollection<InstaLoginSessionSuspiciousLogin> SuspiciousLoginItems { get; set; } = new ObservableCollection<InstaLoginSessionSuspiciousLogin>();

        public async void RunLoadMore()
        {
            await RunLoadMoreAsync();
        }
        async Task RunLoadMoreAsync()
        {
            await Helper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await LoadMoreItemsAsync();
            });
        }

        private async Task LoadMoreItemsAsync()
        {
            try
            {
                // show loadings
                // get notifications settings!
                var result = await Helper.InstaApi.AccountProcessor.GetLoginSessionsAsync();
                if (result.Succeeded)
                {
                    SessionItems.Clear();
                    SuspiciousLoginItems.Clear();
                    SessionItems.AddRange(result.Value.Sessions);
                    SuspiciousLoginItems.AddRange(result.Value.SuspiciousLogins);
                }
            }
            catch { }
        }
    }
}
