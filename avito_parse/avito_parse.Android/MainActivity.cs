using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android;
using Android.Gms.Ads;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using Android.Content;

namespace avito_parse.Droid
{
    // MainLauncher - splash
    [Activity(Label = "Уведомления объявлений", Icon = "@mipmap/logo", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            TabLayoutResource = Resource.Layout.Tabbar;
            //ToolbarResource = Resource.Layout.Toolbar;
            
            MobileAds.Initialize(ApplicationContext, "ca-app-pub-2209506349221532~9084833433");
            //CrossMTAdmob.Current.TestDevices.Add("CE800CFC9C8560368BA93B55926C0E14");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);            
     
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, 0);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 0);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReceiveBootCompleted) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReceiveBootCompleted }, 0);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ForegroundService) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ForegroundService }, 0);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BindNotificationListenerService) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.BindNotificationListenerService }, 0);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Intent alarmIntent = new Intent(Application.Context, typeof(AlarmReceiver));
                var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
                var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
                alarmManager.SetRepeating(AlarmType.RtcWakeup, SystemClock.ElapsedRealtime() + 5 * 1000, AlarmManager.IntervalFifteenMinutes, pending);
            }
            
            LoadApplication(new App(false));
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}