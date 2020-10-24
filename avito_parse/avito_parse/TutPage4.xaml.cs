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
    public partial class TutPage4 : ContentPage
    {
        public TutPage4()
        {
            InitializeComponent();
            image4.Source = "@drawable/Slide4_xhdpi";
        }

        public async void GoBack(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage());
        }
    }
}