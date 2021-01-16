using Android.App;
using Android.Content;
using Android.OS;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using avito_parse.Droid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;

[assembly: Dependency(typeof(avito_parse.Droid.AndroidNotificationManager))]
namespace avito_parse
{
    [Service(Exported = true, Name = "com.avito_parse.PeriodicService")]
    public class PeriodicService : Service
    {
        AndroidNotificationManager notificationManager = DependencyService.Get<AndroidNotificationManager>();
        static ReaderWriterLockSlim rwl1 = new ReaderWriterLockSlim();
        static readonly string TAG = "X:" + typeof(PeriodicService).Name;
        Timer _timer;
        PowerManager powerManager;
        PowerManager.WakeLock wakeLock;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel chan = new NotificationChannel("9000", "Search", NotificationImportance.None);
                const int NOTIFICATION_ID = 9000;
                NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager.CreateNotificationChannel(chan);

                PendingIntent pendingIntent = PendingIntent.GetActivity(this, NOTIFICATION_ID, new Intent(this, typeof(StopServiceActivity)), PendingIntentFlags.OneShot);
                Notification.Action action = new Notification.Action(Resource.Drawable.new_ad, "Закрыть", pendingIntent);

                var notification = new Notification.Builder(this)
                    .SetChannelId(NOTIFICATION_ID.ToString())
                    .SetActions(action)
                    .SetOnlyAlertOnce(true)
                    .SetContentTitle("Отслеживаем...")
                    .SetContentText("не закрывайте если хотите получать уведомления")
                    .SetSmallIcon(Resource.Drawable.service)
                    .Build();

                StartForeground(NOTIFICATION_ID, notification);
            }

            try
            {
                _timer = new Timer(o => {
                    Checker();
                }, null, 0, Trackings.interval * 1000);             

                powerManager = (PowerManager)GetSystemService(PowerService);
                wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "MyWakelockTag");
                wakeLock.Acquire();

                return StartCommandResult.RedeliverIntent;
            }
            catch (Exception ex)
            {
                StopService(new Intent(this, typeof(PeriodicService)));
                return StartCommandResult.RedeliverIntent;
            }
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            
        }


        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();

                _timer.Dispose();
                _timer = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void Checker()
        {
            var client = new HttpClient();
            HttpResponseMessage request = new HttpResponseMessage();
            Parallel.ForEach(Trackings.list.ToArray(), async (x) =>
            {
                if (!Trackings.list.FindAll((y) => x.Name == y.Name).Any() || Trackings.list.Find((y) => x.Name == y.Name).token.IsCancellationRequested) x.CancelToken();
                else x = Trackings.list.Find((y) => x.Name == y.Name);
                if (!x.isActive)
                {
                    await Task.Delay(60000);
                    return;
                }
                var url = x.Url;
                try { request = await client.GetAsync(url); } catch { await Task.Delay(Trackings.interval * 1000); return; }
                var parser = new HtmlParser();
                var response = await request.Content.ReadAsStringAsync();
                var doc = parser.ParseDocument(response);
                string temp_head = ""; string temp_text = ""; string temp_UrlToNew = ""; int prev_NewAdsCount = x.NewAdsCount;
                if (x.Site == "avito")
                {
                    var temp = doc.QuerySelectorAll("div[itemtype='http://schema.org/Product']").ToList();
                    temp.Reverse();
                    foreach (var ad in temp)
                    {
                        var item = ad.GetElementsByTagName("a")[0];
                        var href = item.GetAttribute("href");
                        var t = href.Remove(0, 1);
                        var ti = t.IndexOf('/');
                        string city = t.Remove(ti);
                        if (!x.Ads.Contains(href))
                        {
                            x.Ads.Add(href);
                            if (!ad.Text().Contains("Сегодня")) continue;
                            bool checkCity = false;
                            if (
                                city.Contains("oblast") ||
                                city.Contains("respublika") ||
                                city.Contains("kray") ||
                                city.Contains("_ao") ||
                                city.Contains("adygeya") ||
                                city.Contains("dagestan") ||
                                city.Contains("ingushetiya") ||
                                city.Contains("kabardino-balkariya") ||
                                city.Contains("kalmykiya") ||
                                city.Contains("bashkortostan") ||
                                city.Contains("saha") ||
                                city.Contains("buryatiya") ||
                                url.Contains("radius=")) checkCity = true;
                            if (!url.Contains(city) && city != "rossiya" && !checkCity) continue;
                            x.NewAdsCount += 1;
                            x.UrlToNew = "https://m.avito.ru" + href;
                            temp_UrlToNew = x.UrlToNew;
                            temp_head = item.Text();
                            temp_text = ad.QuerySelectorAll("div[itemprop='price']")[0].Text();
                        }
                    }
                }
                if (x.Site == "auto.youla")
                {
                    var temp = doc.QuerySelectorAll(".SerpSnippet_data__3ezjY").ToList();
                    foreach (var ad in temp)
                    {
                        var item = ad.GetElementsByClassName("SerpSnippet_name__3F7Yu SerpSnippet_titleText__1Ex8A")[0];
                        if (!x.Ads.Contains(item.GetAttribute("href")))
                        {
                            x.Ads.Add(item.GetAttribute("href"));
                            x.NewAdsCount += 1;
                            x.UrlToNew = item.GetAttribute("href");
                            temp_UrlToNew = x.UrlToNew;
                            temp_head = item.Text();
                            temp_text = ad.GetElementsByClassName("SerpSnippet_price__1DHTI")[0].Text();
                        }
                    }
                }
                if (x.Site == "youla")
                {
                    var temp = doc.QuerySelectorAll(".product_item a").ToList();
                    foreach (var ad in temp)
                    {
                        if (!x.Ads.Contains(ad.GetAttribute("href")))
                        {
                            x.Ads.Add(ad.GetAttribute("href"));
                            x.NewAdsCount += 1;
                            x.UrlToNew = "https://youla.ru" + ad.GetAttribute("href");
                            temp_UrlToNew = x.UrlToNew;
                            temp_head = ad.GetAttribute("title");
                            temp_text = ad.GetElementsByClassName(".product_item__description")[0].Text();
                        }
                    }
                }
                if (x.Site == "cian")
                {
                    var temp = doc.QuerySelectorAll("div[data-name='CardContainer']").ToList();
                    foreach (var ad in temp)
                    {
                        var href = ad.QuerySelector("div[data-name='LinkArea' a").GetAttribute("href");
                        if (!x.Ads.Contains(href))
                        {
                            x.Ads.Add(href);
                            x.NewAdsCount += 1;
                            x.UrlToNew = "https://cian.ru" + href;
                            temp_UrlToNew = x.UrlToNew;
                            temp_head = ad.QuerySelector("div[data-name='Features']").Text().Replace("&nbsp;", " ");
                            temp_text = ad.QuerySelector("div[data-name='OfferHeader']").Text();
                        }
                    }
                }
                if (x.Site == "drom")
                {
                    var temp = doc.QuerySelectorAll("a.b-advItem").ToList();
                    foreach (var ad in temp)
                    {
                        if (!x.Ads.Contains(ad.GetAttribute("href")))
                        {
                            x.Ads.Add(ad.GetAttribute("href"));
                            x.NewAdsCount += 1;
                            x.UrlToNew = ad.GetAttribute("href");
                            temp_UrlToNew = x.UrlToNew;
                            temp_head = ad.QuerySelector(".b-advItem__header").Text();
                            temp_text = ad.QuerySelector(".b-advItem__price").Text().Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "").Replace("q", "р.");
                        }
                    }
                }
                if (x.NewAdsCount > 0 && (x.NewAdsCount - prev_NewAdsCount < 8) && temp_head.Length > 0 && temp_text.Length > 0)
                {
                    string title = $"Новое объявление";
                    string message = $"{ temp_head } за { temp_text }";
                    notificationManager.ScheduleNotification(title, message, x.Notices, temp_UrlToNew, x.Ring);
                    x.History.Insert(0, temp_UrlToNew);
                    if (x.History.Count > 30) x.History.RemoveRange(x.History.Count - 3, 3);
                }
                if (x.Ads.Count > 200)
                {
                    x.Ads.RemoveRange(0, 90);
                    x.Ads.RemoveAll(item => item == null);
                }
                SaveTrackingsList();
                await Task.Delay(Trackings.interval * 1000);

                client.Dispose();
                request.Dispose();
            });
        }
        
        public static void SaveTrackingsList()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            if (File.Exists(path)) File.Delete(path);
            rwl1.EnterWriteLock();
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, Trackings.list);
                    fs.Close();
                }
            }
            finally
            {
                rwl1.ExitWriteLock();
            }
        }
    }
}
