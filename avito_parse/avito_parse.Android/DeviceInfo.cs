using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.App;
using Android.Content;
using Xamarin.Forms;

[assembly: Dependency(typeof(avito_parse.Droid.DeviceInfo))]
namespace avito_parse.Droid
{
    public class DeviceInfo : IDeviceInfo
    {
        public string GetInfo()
        {
            
            return $"Android {Build.VERSION.Release.ToString()}";
        }

    }
}