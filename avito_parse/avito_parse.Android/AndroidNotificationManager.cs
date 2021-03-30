using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Plugin.Vibrate;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;

[assembly: Dependency(typeof(avito_parse.Droid.AndroidNotificationManager))]
namespace avito_parse.Droid
{
    public class AndroidNotificationManager : INotificationManager
    {
        string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string Url = "";

        int messageId = 0;
        NotificationManager manager;

        public event EventHandler NotificationReceived;

        public void Initialize()
        {

        }

        public int ScheduleNotification(string title, string message, string message2, int mode, string url, string ring)
        {
            Task.Delay(1000).Wait();

            bool isReopened = false;
            if (ring == "reopened")
                isReopened = true;

            messageId++;
            channelId += mode.ToString();
            if (mode != 0 || isReopened)
                ring = null;

            CreateNotificationChannel(mode, ring);

            Intent intent = new Intent(AndroidApp.Context, typeof(ClickActivity));
            intent.PutExtra("Title", title);
            intent.PutExtra("Message", message);
            intent.PutExtra("Message2", message2);
            intent.PutExtra("Id", messageId);
            intent.PutExtra("Url", url);
            
            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, messageId, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetContentTitle(title)
                .SetSubText(message2)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
                .SetSmallIcon(Resource.Drawable.new_ad);

            builder.SetPriority(NotificationCompat.PriorityMax);

            builder.SetSound(Android.Net.Uri.Parse("android.resource://" + Android.App.Application.Context.PackageName + "/raw/" + ring));

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                if (mode == 0)
                {
                    builder.SetPriority(NotificationCompat.PriorityMax);
                    builder.SetSound(Android.Net.Uri.Parse("android.resource://" + Android.App.Application.Context.PackageName + "/raw/" + ring));
                    var v = CrossVibrate.Current;
                    v.Vibration(TimeSpan.FromSeconds(0.5));
                    builder.SetLights(unchecked((int)0xFFFFFFF0), 500, 1000);
                }
                if (mode == 1)
                {
                    builder.SetPriority(NotificationCompat.PriorityMax);
                    builder.SetSound(null);
                    var v = CrossVibrate.Current;
                    v.Vibration(TimeSpan.FromSeconds(0.5));
                    builder.SetLights(unchecked((int)0xFFFFFFF0), 500, 1000);
                }
                if (mode == 2)
                {
                    builder.SetLights(unchecked((int)0xFFFFFFF0), 500, 1000);
                }
            }
            
            var notification = builder.Build();
            manager.Notify(messageId, notification);

            return messageId;
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new EventArgs();
            NotificationReceived?.Invoke(null, args);
        }

        void CreateNotificationChannel(int mode, string ring)
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };

                var alarmAttributes = new AudioAttributes.Builder()
                        .SetContentType(AudioContentType.Sonification)
                        .SetUsage(AudioUsageKind.Notification).Build();
                if (mode == 0)
                {
                    channel.Importance = NotificationImportance.Max;
                    channel.SetSound(Android.Net.Uri.Parse("android.resource://" + Android.App.Application.Context.PackageName + "/raw/" + ring), alarmAttributes);
                    var v = CrossVibrate.Current;
                    v.Vibration(TimeSpan.FromSeconds(0.5));
                    channel.EnableLights(true);
                }
                if (mode == 1)
                {
                    channel.Importance = NotificationImportance.Max;
                    channel.SetSound(null, null);
                    var v = CrossVibrate.Current;
                    v.Vibration(TimeSpan.FromSeconds(0.5));
                    channel.EnableLights(true);
                }
                if (mode == 2)
                {
                    channel.SetSound(null, null);
                    channel.SetVibrationPattern(null);
                }

                manager.CreateNotificationChannel(channel);
            }
        }

    }
}