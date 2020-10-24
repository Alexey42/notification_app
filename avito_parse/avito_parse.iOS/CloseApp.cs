using System.Threading;

namespace avito_parse.iOS
{
    class CloseApp:ICloseApp
    {
        public void CloseApplication()
        {
            Thread.CurrentThread.Abort();
        }
    }
}