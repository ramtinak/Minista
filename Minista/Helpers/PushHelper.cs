using InstagramApiSharp.API;
//using InstagramApiSharp.API.Push;
using MinistaHelper.Push;
using NotifySharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Minista.Helpers
{
   internal static partial class PushHelperX
    {
        private const string ENTRY_POINT = "MinistaBH.NotifyQuickReplyTask";


        //static void Unegister()
        //{
        //    foreach (var task in BackgroundTaskRegistration.AllTasks)
        //    {
        //        Debug.WriteLine(task.Value.Name);
        //        switch (task.Value.Name)
        //        {
        //            case ENTRY_POINT:
        //                task.Value.Unregister(true);
        //                return;
        //        }
        //    }
        //}
        //public static async void RegisterX()
        //{
        //    //try
        //    //{
        //    //    Unegister();
        //    //    await Task.Delay(250);
        //    //    var builder = new BackgroundTaskBuilder
        //    //    {
        //    //        Name = ENTRY_POINT,
        //    //        TaskEntryPoint = ENTRY_POINT
        //    //    };
        //    //    builder.SetTrigger(new ToastNotificationActionTrigger());
        //    //    builder.Register();
        //    //}
        //    //catch { }
        //}
        public async static void Register()
        {
            try
            {
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(ENTRY_POINT)))
                {
                    ($"{ENTRY_POINT} background task already registered.").PrintDebug();
                    return;
                }

                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder
                {
                    Name = ENTRY_POINT,
                    TaskEntryPoint = ENTRY_POINT
                };

                builder.SetTrigger(new ToastNotificationActionTrigger());

                builder.Register();

                ($"{ENTRY_POINT} background task registered.").PrintDebug();
            }
            catch { }
        }
    }
}
