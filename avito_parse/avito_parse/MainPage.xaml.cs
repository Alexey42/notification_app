using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using System.Threading;
using System.Net.Http;
using Xamarin.Essentials;

//ca-app-pub-2209506349221532~9084833433  MainActivity, MainPage, Manifest
//ca-app-pub-2209506349221532/6907539885

//ca-app-pub-3940256099942544/6300978111 test
//ca-app-pub-3940256099942544~1458002511

namespace avito_parse
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        INotificationManager notificationManager;
        int notificationNumber = 0;
        static ReaderWriterLockSlim rwl1;
        static ReaderWriterLockSlim rwl2;

        public MainPage()
        {
            InitializeComponent();
            var display = DeviceDisplay.MainDisplayInfo;
            if(display.Width < 320 || display.Width > 1000) {
                faq.HeightRequest = 70;
                fastMode.HeightRequest = 70;
            }
            
            NavigationPage.SetHasNavigationBar(this, false);
            trackings_view.ItemsSource = Trackings.list.ToList();
            if (Trackings.list.Count != 0) noTrackings_label.IsVisible = false;
            else noTrackings_label.IsVisible = true;
            if (Trackings.interval != 60)
            {
                fastMode.BackgroundColor = Color.FromHex("#FFCE45");
                fastMode_button.BackgroundColor = Color.FromHex("#FFCE45");
            }

            notificationManager = DependencyService.Get<INotificationManager>();
            rwl1 = new ReaderWriterLockSlim();
            rwl2 = new ReaderWriterLockSlim();

            if (!(bool)App.Current.Properties["app_already_exist"])
            {
                App.Current.Properties["app_already_exist"] = true;
                Parallel.ForEach(Trackings.list.ToArray(), (x) =>
                {
                    x.ResetToken();
                    if (x.isActive) Checker(x, x.token);
                });
            }
        }

        protected override void OnAppearing()
        {
            trackings_view.ItemsSource = null;
            trackings_view.ItemsSource = Trackings.list.ToList();
            if (Trackings.list.Count != 0) noTrackings_label.IsVisible = false;
            else noTrackings_label.IsVisible = true;
            if (Trackings.interval != 60)
            {
                fastMode.BackgroundColor = Color.FromHex("#FFCE45");
                fastMode_button.BackgroundColor = Color.FromHex("#FFCE45");
            }

            SaveTrackingsList();

            for (int i = Trackings.list.Count-1; i >= 0; i--)
            {
                var x = Trackings.list[i];
                if (x.justAdded && x.isActive)
                {
                    x.ResetToken();
                    Checker(x, x.token);
                    x.justAdded = false;
                }
            }
        }

        public void OpenPopup_Tracking(object sender, System.EventArgs e)
#pragma warning restore CA1707 // Идентификаторы не должны содержать символы подчеркивания
        {
            popupView.IsVisible = true;
            Button action = sender as Button;
            var t = Trackings.list.Find(x => x.Name == action.Text);
            countNew_label.Text = "Новых объявлений: ";
            countNew_label.Text += t.NewAdsCount;

            if (t.isActive) pause_button.Text = "Пауза";
            else pause_button.Text = "Возобновить";

            if (App.Current.Properties.ContainsKey("current_tracking_touched")) App.Current.Properties["current_tracking_touched"] = action.Text;
            else App.Current.Properties.Add("current_tracking_touched", action.Text);
          
        }

        public void ClosePopup_Tracking(object sender, System.EventArgs e)
        {
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            if (item != null)
            {
                if (item.isActive) item.Color = Color.FromHex("F0F0F0");
                item.NewAdsCount = 0;
            }
            trackings_view.ItemsSource = null;
            trackings_view.ItemsSource = Trackings.list.ToList();
            popupView.IsVisible = false;          
        }

        public async void OpenTracking(object sender, System.EventArgs e)
        {
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            if (item.isActive) item.Color = Color.FromHex("F0F0F0");
            if (item.NewAdsCount == 1) await Browser.OpenAsync(item.UrlToNew, BrowserLaunchMode.SystemPreferred);
            else await Browser.OpenAsync(item.Url, BrowserLaunchMode.SystemPreferred);
            item.NewAdsCount = 0;
        }

        public async void ChangeTracking(object sender, System.EventArgs e)
        {
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            if (item.isActive) item.Color = Color.FromHex("F0F0F0");
            item.NewAdsCount = 0;
            await Navigation.PushModalAsync(new ChangeTrackingPage());
            trackings_view.ItemsSource = null;
            trackings_view.ItemsSource = Trackings.list.ToList();
        }

        public void DeleteTracking(object sender, System.EventArgs e)
        {
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            Trackings.list.Remove(item);
            popupView.IsVisible = false;
            trackings_view.ItemsSource = null;
            trackings_view.ItemsSource = Trackings.list.ToList();
            
            SaveTrackingsList();
        }

        public void PauseTracking(object sender, System.EventArgs e)
        {
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            if (item.isActive)
            {
                item.Color = Color.FromHex("F0F0F0");
                item.isActive = false;            
                pause_button.Text = "Возобновить";
            }
            else
            {
                item.isActive = true;
                pause_button.Text = "Пауза";
            }
        }
       
        public async void AddNewTracking(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new AddingPage());           
        }

        public async void OpenHistory(object sender, System.EventArgs e)
        {           
            var item = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());
            Page p = new History(item);

            await Navigation.PushModalAsync(p);
        }

        public async void OpenHelp(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new Help());
        }

        public void OpenPopup_FastMode(object sender, System.EventArgs e)
        {
            if (Trackings.interval != 60)
            {
                Trackings.interval = 60;
                fastMode.BackgroundColor = Color.FromHex("#4A8AF4");
                fastMode_button.BackgroundColor = Color.FromHex("#4A8AF4");

                SaveTrackingsInterval();
            }
            else popupFastMode.IsVisible = true;
        }

        public void ClosePopup_FastMode(object sender, System.EventArgs e)
        {
            popupFastMode.IsVisible = false;
        }

        public async void Activate_FastMode(object sender, System.EventArgs e)
        {
            Trackings.interval = 10;
            popupFastMode.IsVisible = false;
            fastMode.BackgroundColor = Color.FromHex("#FFCE45");
            fastMode_button.BackgroundColor = Color.FromHex("#FFCE45");

            SaveTrackingsInterval();
        }

        public async Task Checker(Tracking x, CancellationToken cancellationToken)
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
                try { request = await client.GetAsync(url); } catch { await Task.Delay(Trackings.interval*1000, cancellationToken); continue; }
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
                    Device.BeginInvokeOnMainThread(()=> {
                        x.Color = Color.FromHex("FFCD42");
                        trackings_view.ItemsSource = null;
                        trackings_view.ItemsSource = Trackings.list.ToList();
                    });
                                                                
                    notificationNumber++;
                    string title = $"Новое объявление";
                    string message = $"{ temp_head } за { temp_text }";
                    notificationManager.ScheduleNotification(title, message, x.Notices, temp_UrlToNew, x.Ring);
                    if (temp_UrlToNew.ToLower().Contains("https://")) 
                        temp_UrlToNew = temp_UrlToNew.Substring(8, temp_UrlToNew.Length - 8);
                    x.History.Insert(0, temp_UrlToNew.Replace("/", " "));
                    if (x.History.Count > 30) x.History.RemoveRange(x.History.Count - 3, 3);
                }

                if (x.Ads.Count() > 200)
                {
                    x.Ads.RemoveRange(0, 90);
                    x.Ads.RemoveAll(item => item == null);
                }
                SaveTrackingsList();
                await Task.Delay(Trackings.interval*1000, cancellationToken);
            }

            client.Dispose();
            request.Dispose();          
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

        public static void SaveTrackingsInterval()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(int));
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedInterval.xml");
            if (File.Exists(path)) File.Delete(path);

            rwl2.EnterWriteLock();
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, Trackings.interval);
                    fs.Close();
                }
            }
            finally
            {
                rwl2.ExitWriteLock();
            }
        }
    }   

}


