using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace avito_parse
{
    public partial class App : Application
    {      
        public App()
        {
            InitializeComponent(); 
            Trackings.Init();
            bool firstLaunch = false;
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
                try { Trackings.list = (List<Tracking>) formatter.Deserialize(fs); }
                catch {
                    firstLaunch = true;
                }
            }
            path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedInterval.xml");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(double));
                try { Trackings.interval = (int) formatter.Deserialize(fs); }
                catch {
                    firstLaunch = true;
                }
            }

            if (firstLaunch) MainPage = new NavigationPage(new Tutorial());
            else MainPage = new NavigationPage(new MainPage());

        }

        protected override void OnStart()
        {
            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
                try { Trackings.list = (List<Tracking>)formatter.Deserialize(fs); }
                catch { }
            }
            path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedInterval.xml");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(double));
                try { Trackings.interval = (int)formatter.Deserialize(fs); }
                catch { }
            }

            foreach (var item in Trackings.list)
            {
                item.NewAdsCount = 0;
                if (item.isActive) item.Color = Color.FromHex("F0F0F0");
            }
        }

        protected override void OnSleep()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(List<Tracking>));
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedData.xml");
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Trackings.list);
            }
            formatter = new XmlSerializer(typeof(double));
            path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "SavedInterval.xml");
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Trackings.interval);
            }
        }

        protected override void OnResume()
        {

        }

    }
}
