using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avito_parse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Tutorial : TabbedPage
    {
        public Tutorial()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            App.Current.MainPage = new NavigationPage(new MainPage());
        }
        public void p1App(object sender, System.EventArgs e)
        {
            p1.IconImageSource = "@drawable/swipe";
            p2.IconImageSource = null;
            p3.IconImageSource = null;
            p4.IconImageSource = null;
        }
        public void p2App(object sender, System.EventArgs e)
        {
            p1.IconImageSource = null;
            p2.IconImageSource = "@drawable/swipe";
            p3.IconImageSource = null;
            p4.IconImageSource = null;
        }
        public void p3App(object sender, System.EventArgs e)
        {
            p1.IconImageSource = null;
            p2.IconImageSource = null;
            p3.IconImageSource = "@drawable/swipe";
            p4.IconImageSource = null;
        }
        public void p4App(object sender, System.EventArgs e)
        {
            p1.IconImageSource = null;
            p2.IconImageSource = null;
            p3.IconImageSource = null;
            p4.IconImageSource = "@drawable/swipe";
        }
    }
}