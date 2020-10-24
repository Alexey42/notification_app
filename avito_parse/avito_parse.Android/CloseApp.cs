using Android.App;
using Xamarin.Forms;

[assembly: Dependency(typeof(avito_parse.Droid.CloseApp))]
namespace avito_parse.Droid
{
    public class CloseApp:ICloseApp
    {
        public void CloseApplication()
        {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();
        }
    }
}