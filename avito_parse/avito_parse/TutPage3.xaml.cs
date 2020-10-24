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
    public partial class TutPage3 : ContentPage
    {
        public TutPage3()
        {
            InitializeComponent();
            image3.Source = "@drawable/Slide3_xhdpi";
        }
    }
}