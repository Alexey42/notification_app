using System;
using System.Collections.Generic;
using System.Text;

namespace avito_parse
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;

        void Initialize();

        int ScheduleNotification(string title, string message, int mode, string url, string ring);

        void ReceiveNotification(string title, string message);
    }
}
