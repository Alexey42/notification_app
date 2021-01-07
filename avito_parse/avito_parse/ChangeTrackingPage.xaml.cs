using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avito_parse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangeTrackingPage : ContentPage
    {
        private Dictionary<string, int> pickerKeys = new Dictionary<string, int> {
            { "Звук и вибрация", 0 },
            { "Вибрация", 1 },
            { "Только на экране", 2 }
        };

        public ChangeTrackingPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            var cur = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());

            Url.Text = cur.Url;
            Name.Text = cur.Name;
            picker.SelectedIndex = cur.Notices;
            picker2.SelectedIndex = picker2.Items.IndexOf(cur.Ring);
        }

        public async void UrlOnFocus(object sender, System.EventArgs e)
        {
            Url.BackgroundColor = Color.AliceBlue;
        }

        public async void Change(object sender, System.EventArgs e)
        {
            add_button.IsVisible = false;
            loading.IsVisible = true;

            var cur = Trackings.list.Find(x => x.Name == App.Current.Properties["current_tracking_touched"].ToString());          

            string url = Url.Text;
            if (url == null || url.Length < 5)
            {
                Url.BackgroundColor = Color.FromHex("#FA9081");
                add_button.IsVisible = true;
                loading.IsVisible = false;
                return;
            }

            List<string> _ads = cur.Ads;
            List<string> _history = cur.History;
            var _site = cur.Site;
            var _name = Name.Text;
            if (cur.Url != Url.Text)
            {
                _history = new List<string>();
                var client = new HttpClient();
                HttpResponseMessage request = new HttpResponseMessage();
                try { request = await client.GetAsync(url); }
                catch
                {
                    Url.BackgroundColor = Color.FromHex("#FA9081");
                    add_button.IsVisible = true;
                    loading.IsVisible = false;
                    return;
                }
                request = await client.GetAsync(url);
                var parser = new HtmlParser();
                var response = await request.Content.ReadAsStringAsync();
                var doc = parser.ParseDocument(response);
                
                _ads = new List<string>();

                if (url.Contains("avito"))
                {
                    if (url.Substring(url.Length - 6) != "?s=104") // Сортировать по дате
                    {
                        url += "?s=104";
                    }

                    try
                    {
                        if (doc.QuerySelector("div.b-404").InnerHtml.Length > 0)
                        {
                            Url.BackgroundColor = Color.FromHex("#FA9081");
                            add_button.IsVisible = true;
                            loading.IsVisible = false;
                            return;
                        }
                    }
                    catch
                    {
                        _site = "avito";

                        var temp = doc.QuerySelectorAll("div[itemtype='http://schema.org/Product']").ToList();
                        foreach (var x in temp) _ads.Add(x.GetAttribute("href"));

                        if (Name.Text.Length == 0)
                        {
                            try
                            {
                                if (doc.QuerySelector("input[data-marker='search/input']").GetAttribute("value").Length > 0) _name = doc.QuerySelector("input[data-marker='search/input']").GetAttribute("value");
                                else _name = doc.QuerySelector("h1[data-marker='search-title']").InnerHtml.Replace("&nbsp;", " ");
                            }
                            catch { _name = "Новый поиск"; }
                        }
                        else _name = Name.Text;
                    }

                }
                else if (url.Contains("auto.youla.ru"))
                {
                    try
                    {
                        if (doc.QuerySelector("h1.title").InnerHtml == "404")
                        {
                            Url.BackgroundColor = Color.FromHex("#FA9081");
                            add_button.IsVisible = true;
                            loading.IsVisible = false;
                            return;
                        }
                    }
                    catch
                    {
                        _site = "auto.youla";

                        var temp = doc.QuerySelectorAll(".SerpSnippet_colLeft__1qO8G").ToList();
                        foreach (var x in temp) _ads.Add(x.GetAttribute("href"));

                        if (Name.Text.Length == 0)
                        {
                            try
                            {
                                if (doc.QuerySelector(".sc-hwwEjo input").GetAttribute("value").Length > 1) _name = doc.QuerySelector(".sc-hwwEjo input").GetAttribute("value");
                                else _name = doc.QuerySelector(".Search_pageHeader__1HztN").InnerHtml;
                            }
                            catch { _name = "Новый поиск"; }
                        }
                        else _name = Name.Text;
                    }
                }
                else if (url.Contains("youla.ru"))
                {
                    try
                    {
                        if (doc.QuerySelector("h1.error_page__title").InnerHtml.Length > 0)
                        {
                            Url.BackgroundColor = Color.FromHex("#FA9081");
                            add_button.IsVisible = true;
                            loading.IsVisible = false;
                            return;
                        }
                    }
                    catch
                    {
                        _site = "youla";

                        if (Name.Text.Length == 0)
                        {
                            if (doc.QuerySelector("._search_fixed_input").GetAttribute("value").Length > 1) _name = doc.QuerySelector("._search_fixed_input").GetAttribute("value");
                            else _name = "Новый поиск";
                        }
                        else _name = Name.Text;
                    }
                }
                else if (url.Contains("cian.ru"))
                {
                    try
                    {
                        if (doc.QuerySelector("h1.title").InnerHtml == "Страница не найдена")
                        {
                            Url.BackgroundColor = Color.FromHex("#FA9081");
                            add_button.IsVisible = true;
                            loading.IsVisible = false;
                            return;
                        }
                    }
                    catch
                    {
                        _site = "cian";

                        var temp = doc.QuerySelectorAll(".c29edcec40--link--xSR68").ToList();
                        foreach (var x in temp) _ads.Add(x.GetAttribute("href"));

                        if (Name.Text.Length == 0) _name = "Новый поиск";
                        else _name = Name.Text;
                    }

                }
                else if (url.Contains("drom.ru"))
                {
                    if (doc.QuerySelector(".b-title_type_h1").InnerHtml.Replace("&nbsp;", " ") == "Запрошенная вами страница не существует!")
                    {
                        Url.BackgroundColor = Color.FromHex("#FA9081");
                        add_button.IsVisible = true;
                        loading.IsVisible = false;
                        return;
                    }
                    _site = "drom";

                    var temp = doc.QuerySelectorAll("a.b-advItem").ToList();
                    foreach (var x in temp) _ads.Add(x.GetAttribute("href"));

                    if (Name.Text.Length == 0)
                    {
                        try { _name = doc.QuerySelector(".b-title_type_h1").InnerHtml.Replace("&nbsp;", " "); }
                        catch { _name = "Новый поиск"; }
                    }
                    else _name = Name.Text;

                }
                else
                {
                    Url.BackgroundColor = Color.FromHex("#FA9081");
                    add_button.IsVisible = true;
                    loading.IsVisible = false;
                    return;
                }
            }

            Trackings.list.Remove(cur);

            if (Trackings.list.Where(x => x.Name == _name).Any()) _name += "1";
            for (int i = 1; Trackings.list.Where(x => x.Name == _name).Any(); i++)
            {
                _name = _name.Remove(_name.Length - 1);
                _name += i.ToString();
            }

            var _notices = picker.Items[picker.SelectedIndex];

            add_button.IsVisible = true;
            loading.IsVisible = false;

            Tracking result = new Tracking
            {
                Name = _name,
                Ads = _ads,
                NewAdsCount = 0,
                Site = _site,
                Url = url,
                Notices = pickerKeys[_notices],
                Ring = picker2.Items[picker2.SelectedIndex],
                isActive = true,
                justAdded = false,
                Color = Color.FromHex("F0F0F0"),
                Date = DateTime.Now,
                History = _history
            };

            Trackings.list.Add(result);
            await Navigation.PopModalAsync();
        }

        public async void GoBack(object sender, System.EventArgs e)
        {
            try
            {
                await Navigation.PopModalAsync();
            }
            catch { }
        }
    }
}