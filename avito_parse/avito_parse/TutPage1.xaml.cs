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
    public partial class TutPage1 : ContentPage
    {
        public TutPage1()
        {
            InitializeComponent();
            image1.Source = "@drawable/Slide1_xhdpi";
        }
       
    }
}