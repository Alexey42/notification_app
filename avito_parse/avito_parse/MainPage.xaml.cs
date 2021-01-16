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
        static ReaderWriterLockSlim rwl1 = new ReaderWriterLockSlim();
        static ReaderWriterLockSlim rwl2 = new ReaderWriterLockSlim();

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


