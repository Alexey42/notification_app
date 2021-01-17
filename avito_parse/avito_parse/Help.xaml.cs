using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avito_parse
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Help : ContentPage
    {
        public Help()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public async void OpenPopup(object sender, System.EventArgs e)
        {           
            Button action = sender as Button;
            string instruction = "";
            if (Build.VERSION.SdkInt == BuildVersionCodes.N || Build.VERSION.SdkInt == BuildVersionCodes.NMr1 || Build.VERSION.SdkInt == BuildVersionCodes.M)
            {
                instruction = "Настройки -> Все приложения -> Уведомления объявлений -> Уведомления";
            }
            else
            {
                instruction = "Настройки -> Приложения и уведомления -> Уведомления объявлений -> Разрешите уведомления";
            }

            switch (action.ClassId)
            {
                case "5":
                    text_label.Text = instruction;
                    break;
                case "0":
                    text_label.Text = "Скорее всего в настройках вашего устройства не установлены все разрешения для уведомлений от данного приложения или устройство находится в беззвучном режиме";
                    break;
                case "1":
                    text_label.Text = "В настоящий момент можно отслеживать: avito.ru, cian.ru, youla.ru, auto.youla.ru, drom.ru";
                    break;
                case "2":
                    text_label.Text = "Как правило, приложение ведёт себя некорректно из-за:" + System.Environment.NewLine + 
                        "   1.Проблем с подключением к интернету, " + System.Environment.NewLine +
                        "   2.Слишком большого количества одновременно запущенных поисков, " + System.Environment.NewLine +
                        "   3.Некорректных url-адресов";
                    break;
                case "3":
                    await Navigation.PushModalAsync(new Tutorial());                  
                    break;
                case "4":
                    text_label.Text = "Отзыв в Google Play либо почта: 550002@list.ru";
                    break;
            } 

            popupView.IsVisible = true;
        }

        public void ClosePopup(object sender, System.EventArgs e)
        {
            popupView.IsVisible = false;
        }
        
        public async void GoBack(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}