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
           
            INotificationManager notificationManager = DependencyService.Get<AndroidNotificationManager>();
            notificationManager.ScheduleNotification(Intent.GetStringExtra("Title"),
                Intent.GetStringExtra("Message"), Intent.GetStringExtra("Message2"), 2, notifyUrl, "reopened");

            Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
        }
    }
}