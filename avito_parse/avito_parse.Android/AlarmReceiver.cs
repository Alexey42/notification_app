using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Xamarin.Forms;
using Android.Runtime;
using Android.OS;
using System.Threading.Tasks;

[assembly: Dependency(typeof(avito_parse.Droid.AndroidNotificationManager))]
namespace avito_parse.Droid
{
    /*[BroadcastReceiver]
    public partial class AlarmReceiver : BroadcastReceiver
    {
        AndroidNotificationManager notificationManager = DependencyService.Get<AndroidNotificationManager>();
        //static ReaderWriterLockSlim rwl1;

        public override async void OnReceive(Context context, Intent intent)
        {
                Parallel.ForEach(Trackings.list.ToArray(), (x) =>
                {
                    x.ResetToken();
                    if (x.isActive) Checker(x, x.token, context);
                });
        }

        public async Task Checker(Tracking x, CancellationToken cancellationToken, Context context)
        {
            var client = new HttpClient();
            HttpResponseMessage request = new HttpResponseMessage();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!Trackings.list.FindAll((y) => x.Name == y.Name).Any() || Trackings.list.Find((y) => x.Name == y.Name).token.IsCancellationRequested) x.CancelToken();
                else x = Trackings.list.Find((y) => x.Name == y.Name);

                if (!x.isActive)
                {
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }
                var url = x.Url;
                try { request = await client.GetAsync(url); } catch { await Task.Delay(Trackings.interval * 1000, cancellationToken); continue; }
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

                if (x.Ads.Count() > 200)
                {
                    x.Ads.RemoveRange(0, 90);
                    x.Ads.RemoveAll(item => item == null);
                }
                SaveTrackingsList();
                await Task.Delay(Trackings.interval * 1000, cancellationToken);
            }

            client.Dispose();
            request.Dispose();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Intent alarmIntent = new Intent(Android.App.Application.Context, typeof(AlarmReceiver));
                var pending = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
                var alarmManager = Android.App.Application.Context.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>();
                alarmManager.SetRepeating(AlarmType.RtcWakeup, SystemClock.ElapsedRealtime() + 5 * 1000, AlarmManager.IntervalFifteenMinutes, pending);
            }

        }
        public static void SaveTrackingsList()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            if (File.Exists(path)) File.Delete(path);

            //rwl1.EnterWriteLock();
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
                //rwl1.ExitWriteLock();
            }
        }

    }*/
}