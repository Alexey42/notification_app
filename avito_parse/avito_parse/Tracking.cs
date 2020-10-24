using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace avito_parse
{
    [Serializable]
    public static class Trackings
    {
        public static List<Tracking> list;

        public static int interval;

        public static void Init()
        {
            list = new List<Tracking>();
            interval = 60;
        }
    }

    [Serializable]
    public class Tracking
    {
        public Tracking() {
            
        }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Site { get; set; }
        public int Notices { get; set; }
        public string Ring { get; set; }
        public int NewAdsCount { get; set; }
        public List<string> Ads { get; set; }
        public string UrlToNew { get; set; }       
        public Color Color { get; set; }
        public DateTime Date { get; set; }
        public List<string> History { get; set; }
        public CancellationToken token { get; set; }
        public CancellationTokenSource cts { get; set; }
        public bool isActive { get; set; }
        public bool justAdded { get; set; }

        public void ResetToken()
        {
            if (token.CanBeCanceled) {
                cts.Cancel();
                cts.Dispose();
            }
            cts = new CancellationTokenSource();
            token = new CancellationToken();
            token = cts.Token;
        }

        public void CancelToken()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
