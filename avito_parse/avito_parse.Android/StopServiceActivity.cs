using Android.App;
using Android.Content;
using Android.OS;

namespace avito_parse.Droid
{
    [Activity(Label = "StopServiceActivity", NoHistory = true)]
    public class StopServiceActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StopService(new Intent(this, typeof(PeriodicService)));

            Finish();
        }
    }
}