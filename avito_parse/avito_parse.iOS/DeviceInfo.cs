using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(avito_parse.iOS.DeviceInfo))]
namespace avito_parse.iOS
{
    public class DeviceInfo : IDeviceInfo
    {
        public string GetInfo()
        {
            UIDevice device = new UIDevice();
            return $"{device.SystemName} {device.SystemVersion}";
        }
    }
}