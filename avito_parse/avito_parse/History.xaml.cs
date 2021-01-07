using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avito_parse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class History : ContentPage
    {
        public History(Tracking item)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            trackings_view.ItemsSource = item.History.ToList();
        }

        public void Browse(object sender, System.EventArgs e)
        {
            Button button = sender as Button;
            string text = button.Text;
            Browser.OpenAsync(text.Replace(" ", "/"), BrowserLaunchMode.SystemPreferred);
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