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
    public partial class TutPage2 : ContentPage
    {
        public TutPage2()
        {
            InitializeComponent();
            image2.Source = "@drawable/Slide2_xhdpi";
        }
    }
}