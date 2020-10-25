using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Serialization;
using Android.Content;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Xamarin.Forms;

[assembly: Dependency(typeof(avito_parse.Droid.AndroidNotificationManager))]
namespace avito_parse.Droid
{
    [BroadcastReceiver]
    public partial class AlarmReceiver : BroadcastReceiver
    {
        AndroidNotificationManager notificationManager = DependencyService.Get<AndroidNotificationManager>();

        public override async void OnReceive(Context context, Intent intent)
        {
            var client = new HttpClient();
            HttpResponseMessage request = new HttpResponseMessage();
            
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
                try { Trackings.list = (List<Tracking>)formatter.Deserialize(fs); }
                catch { }
            }

            foreach (var x in Trackings.list)
            {
                    if (!x.isActive) continue;
                    var url = x.Url;
                    try { request = await client.GetAsync(url); } catch { continue; }
                    var parser = new HtmlParser();
                    var response = await request.Content.ReadAsStringAsync();
                    var doc = parser.ParseDocument(response);
                    string temp_head = ""; string temp_text = ""; string temp_UrlToNew = ""; int prev_NewAdsCount = x.NewAdsCount;

                    if (x.Site == "avito")
                    {
                        var temp = doc.QuerySelectorAll("div._2Gp1j").ToList();
                        foreach (var ad in temp)
                        {
                            var item = ad.GetElementsByClassName("_27a-N")[0];
                            if (!x.Ads.Contains(item.GetAttribute("href")))
                            {
                                x.Ads.Add(item.GetAttribute("href"));
                                x.NewAdsCount += 1;
                                x.UrlToNew = "https://m.avito.ru" + item.GetAttribute("href");
                                temp_UrlToNew = x.UrlToNew;
                                temp_head = item.Text();
                                temp_text = ad.GetElementsByClassName("_2B-6G")[0].Text();
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
                        var temp = doc.QuerySelectorAll(".c29edcec40--container--kceaZ").ToList();
                        foreach (var ad in temp)
                        {
                            var item = ad.GetElementsByClassName("c29edcec40--container--3jF-D")[0];
                            if (!x.Ads.Contains(item.GetAttribute("href")))
                            {
                                x.Ads.Add(item.GetAttribute("href"));
                                x.NewAdsCount += 1;
                                x.UrlToNew = "https://cian.ru" + item.GetAttribute("href");
                                temp_UrlToNew = x.UrlToNew;
                                temp_head = item.Text().Replace("&nbsp;", " ");
                                temp_text = ad.GetElementsByClassName("c29edcec40--price--2nonH")[0].Text();
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

                    if (x.NewAdsCount > 0 && temp_head.Length > 0 && temp_text.Length > 0)
                    {
                        string title = $"Новое объявление";
                        string message = $"{ temp_head } за { temp_text }";
                        notificationManager.ScheduleNotification(title, message, x.Notices, temp_UrlToNew, x.Ring);
                        WriteToHistory(temp_UrlToNew, x);
                    }

                if (x.Ads.Count() > 300) x.Ads.RemoveRange(0, 150);
            }

            path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
                formatter.Serialize(fs, Trackings.list);
            }
            
        }

        public void WriteToHistory(string url, Tracking x)
        {
            x.History.Insert(0, url);
            if (x.History.Count > 30) x.History.RemoveRange(x.History.Count - 3, 3);
            
        }
    }
}