using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications.Messages;

namespace FIVStandard.Modules
{
    public class UpdateCheck
    {
        private readonly MainWindow mainWindow;

        public Version currentVersion;
        public Version downloadVersion;

        private Task _task;
        private readonly object _lock = new object();

        public UpdateCheck(MainWindow mw)
        {
            mainWindow = mw;

            currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Task CheckForUpdates()
        {
            lock (_lock)//prevent more than one checks being started in new threads
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
                                downloadVersion = new Version(reader.ReadLine());
                                reader.ReadLine();

                                if (HasLaterVersion())
                                {

                                }
                                else
                                {
                                    DownloadNewAppVersion();
                                }

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    //UI thread stuff
                                    mainWindow.notifier.ShowInformation(downloadVersion.ToString());
                                });
                            }
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

        private void DownloadNewAppVersion()
        {

        }

        public bool HasLaterVersion()
        {
            return currentVersion >= downloadVersion;
        }
    }
}
