using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications.Messages;

namespace FIVStandard.Models
{
    public class UpdateCheck
    {
        private readonly MainWindow mainWindow;

        private Task _task;
        private readonly object _lock = new object();

        public UpdateCheck(MainWindow mw)
        {
            mainWindow = mw;
        }

        public Task CheckForUpdates()
        {
            lock (_lock)
            {
                if (_task == null)
                {
                    return _task = Task.Run(() =>
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                //UI thread stuff
                                mainWindow.notifier.ShowInformation("Checking for updates...");
                            });

                            var webRequest = WebRequest.Create(@"https://drive.google.com/uc?export=download&id=1cqCjCSZpo3bSF8G9Wrk0fT-ypQY7RKMn");

                            using (var response = webRequest.GetResponse())
                            using (var content = response.GetResponseStream())
                            using (var reader = new StreamReader(content))
                            {
                                var strContent = reader.ReadToEnd();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    //UI thread stuff
                                    mainWindow.notifier.ShowInformation(strContent);
                                });
                            }

                            //_isRunning.Value = false;
                        }
                        catch (Exception e)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                //UI thread stuff
                                mainWindow.notifier.ShowInformation(e.Message);
                            });
                        }

                        lock (_lock)
                        {
                            _task = null;
                        }
                    });
                }

                return _task;
            }
        }
    }
}
