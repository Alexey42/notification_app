using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace avito_parse.Droid
{
    [Activity(Label = "ClickActivity", NoHistory = true)]
    public class ClickActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            string notifyUrl = Intent.GetStringExtra("Url");
            Browser.OpenAsync(notifyUrl, BrowserLaunchMode.SystemPreferred);
           
            INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.ScheduleNotification(Intent.GetStringExtra("Title"),
                Intent.GetStringExtra("Message"), 2, notifyUrl, null);

            Finish();
        }
    }
}