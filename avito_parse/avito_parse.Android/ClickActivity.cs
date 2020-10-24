using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace avito_parse.Droid
{
    [Activity(Label = "ClickActivity", NoHistory = true)]
    public class ClickActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string notifyUrl = Intent.GetStringExtra("Url");
            Browser.OpenAsync(notifyUrl, BrowserLaunchMode.SystemPreferred);
           
            INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.ScheduleNotification(Intent.GetStringExtra("Title"),
                Intent.GetStringExtra("Message"), 2, notifyUrl, null);

            Finish();
        }
    }
}