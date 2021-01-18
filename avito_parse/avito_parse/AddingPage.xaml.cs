using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AngleSharp.Html.Parser;

namespace avito_parse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddingPage : ContentPage
    {
        private readonly Dictionary<string, int> pickerKeys = new Dictionary<string, int> { 
            { "Звук и вибрация", 0 },
            { "Вибрация", 1 },
            { "Только на экране", 2 }
        };

        public AddingPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            picker.SelectedIndex = 0;
            picker2.SelectedIndex = 0;
        }

        public async void UrlOnFocus(object sender, System.EventArgs e)
        {
            Url.BackgroundColor = Color.AliceBlue;
        }

        public async void Add(object sender, System.EventArgs e)
        {           
            add_button.IsVisible = false;
            loading.IsVisible = true;

            string url = Url.Text;
            if (url == null || url.Length < 5)
            {
                Url.BackgroundColor = Color.FromHex("#FA9081");
                add_button.IsVisible = true;
                loading.IsVisible = false;
                return;
                //url = @"https://m.avito.ru/rossiya/avtomobili/audi?withImagesOnly=true";
                //url = @"https://youla.ru/nizhniy_novgorod/nedvijimost/prodaja-kvartiri/arenda-chetyrekhkomnatnoj-kvartiry-posutochno?attributes%5Brealty_building_type%5D%5B0%5D=166228";
                //url = @"https://auto.youla.ru/nizhniy-novgorod/cars/used/audi/";
                //url = @"https://www.cian.ru/snyat-1-komnatnuyu-kvartiru/";
                //url = @"https://auto.drom.ru/audi/all/";
                //url = @"https://www.avito.ru/rossiya/avtomobili?cd=1";
            }

            AngleSharp.Html.Dom.IHtmlDocument doc = null;
            using (var client = new HttpClient())
            {
                HttpResponseMessage request = new HttpResponseMessage();
                try { request = await client.GetAsync(url); }
                catch
                {
                    Url.BackgroundColor = Color.FromHex("#FA9081");
                    add_button.IsVisible = true;
                    loading.IsVisible = false;
                    return;
                }
                var parser = new HtmlParser();
                var response = await request.Content.ReadAsStringAsync();
                doc = parser.ParseDocument(response);

                client.Dispose();
                request.Dispose();
            }
            var _site = "";
            var _name = "";
            List<string> _ads = new List<string>();

            if (url.Contains("avito"))
            {
                if (!url.Contains("&sort=date"))
                    url += "&sort=date";

                try {
                    if (doc.QuerySelector("div.b-404").InnerHtml.Length>0)
                    {
                        Url.BackgroundColor = Color.FromHex("#FA9081");
                        add_button.IsVisible = true;
                        loading.IsVisible = false;
                        return;
                    }
                }
                catch {
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
                    if (doc.QuerySelector("h1.error_page__title").InnerHtml.Length>0)
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
                catch {
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
                justAdded = true,
                Color = Color.FromHex("F0F0F0"),
                Date = DateTime.Now,
                History = new List<string>()
            };

            Trackings.list.Add(result);

            XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Trackings.list);
            }

            await Navigation.PopModalAsync();
            await Navigation.PushModalAsync(new MainPage());        
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