using Gu.Localization;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications.Messages;

namespace FIVStandard.Modules
{
    public enum UpdateCheckType
    {
        /// <summary>
        /// Notifies if up to date and new version available; downloads and installs
        /// </summary>
        FullUpdate,
        FullUpdateForced,
        /// <summary>
        /// Notifies only if new version is available
        /// </summary>
        SilentVersionCheck,
    }

    public class UpdateCheck : INotifyPropertyChanged
    {
        private readonly MainWindow mainWindow;

        private Version _currentVersion = new Version("0.0.0.0");

        public bool NotUpdating { get; set; } = true;

        public Version CurrentVersion
        {
            get
            {
                return _currentVersion;
            }
            set
            {
                _currentVersion = value;
                OnPropertyChanged();
            }
        }

        private Version _downloadVersion = new Version("0.0.0.0");

        public Version DownloadVersion
        {
            get
            {
                return _downloadVersion;
            }
            set
            {
                _downloadVersion = value;
                OnPropertyChanged();
                OnPropertyChanged("DownloadVersionString");
            }
        }

        public string DownloadVersionString
        {
            get
            {
                return $"({_downloadVersion.ToString()})";
            }
        }

        private Task _task;
        private readonly object _lock = new object();

        private string _updaterMessage = "";

        public string UpdaterMessage
        {
            get
            {
                return _updaterMessage;
            }
            set
            {
                _updaterMessage = value;
                OnPropertyChanged();
            }
        }

        private string _downloadedChangelog;

        public string DownloadedChangelog
        {
            get
            {
                return _downloadedChangelog;
            }
            set
            {
                _downloadedChangelog = value;
                OnPropertyChanged();
            }
        }

        //https://drive.google.com/uc?export=download&id=FILE_ID
        private const string setupURL = "https://drive.google.com/uc?export=download&id=19Ds5qxy4QVFlFljIgZjHenXOA1qALQ8R";
        private const string changelogURL = "https://drive.google.com/uc?export=download&id=1cqCjCSZpo3bSF8G9Wrk0fT-ypQY7RKMn";

        public UpdateCheck(MainWindow mw)
        {
            mainWindow = mw;

            CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Task CheckForUpdates(UpdateCheckType updateType)
        {
            lock (_lock)//prevent more than one checks being started in new threads
            {
                if (_task == null)
                {
                    return _task = Task.Run(() =>
                    {
                        try
                        {
                            NotUpdating = false;

                            switch(updateType){
                                case UpdateCheckType.FullUpdate:
                                    Download_Full();
                                    break;
                                case UpdateCheckType.FullUpdateForced:
                                    Download_FullForced();
                                    break;
                                case UpdateCheckType.SilentVersionCheck:
                                    Download_VersionInfo();
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                //UI thread stuff
                                mainWindow.notifier.ShowError(e.Message);
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

        private void Download_Full()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //UI thread stuff
                string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CheckingForUpdatesInfo));
                mainWindow.notifier.ShowInformation(cultureTranslated);
            });

            //txt file containing version and update notes
            var webRequest = WebRequest.Create(changelogURL);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                DownloadVersion = new Version(reader.ReadLine());
                reader.ReadLine();//empty line between version and notes
                DownloadedChangelog = reader.ReadToEnd();

                if (HasLaterVersion())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //UI thread stuff
                        string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.AlreadyOnLatestVerInfo));
                        mainWindow.notifier.ShowInformation($"{cultureTranslated}({DownloadVersion.ToString()})");
                    });

                    UpdaterMessage = "";
                    NotUpdating = true;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //UI thread stuff
                        string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NewVerAvailableInfo));
                        string cultureTranslated2 = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo));
                        mainWindow.notifier.ShowInformation($"{cultureTranslated}. {cultureTranslated2} ({DownloadVersion.ToString()})");
                    });

                    DownloadNewAppVersion();
                }
            }
        }

        private void Download_FullForced()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //UI thread stuff
                string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CheckingForUpdatesInfo));
                mainWindow.notifier.ShowInformation(cultureTranslated);
            });

            //txt file containing version and update notes
            var webRequest = WebRequest.Create(changelogURL);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                DownloadVersion = new Version(reader.ReadLine());
                reader.ReadLine();//empty line between version and notes
                DownloadedChangelog = reader.ReadToEnd();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    //UI thread stuff
                    string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo));
                    mainWindow.notifier.ShowInformation($"{cultureTranslated} ({DownloadVersion.ToString()})");
                });

                DownloadNewAppVersion();
            }
        }

        private void Download_VersionInfo()
        {
            //txt file containing version and update notes
            var webRequest = WebRequest.Create(@"https://drive.google.com/uc?export=download&id=1cqCjCSZpo3bSF8G9Wrk0fT-ypQY7RKMn");

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                DownloadVersion = new Version(reader.ReadLine());
                reader.ReadLine();//empty line between version and notes
                DownloadedChangelog = reader.ReadToEnd();


                if (HasLaterVersion())
                {
                    UpdaterMessage = "";
                    NotUpdating = true;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //UI thread stuff
                        string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NewVerAvailableInfo));
                        mainWindow.notifier.ShowInformation($"{cultureTranslated}: {DownloadVersion.ToString()}");
                    });

                    NotUpdating = true;
                }
            }
        }

        private void DownloadNewAppVersion()
        {
            if (CheckForInternetConnection())
            {
                string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo));
                UpdaterMessage = cultureTranslated;

                using (WebClient fileClient = new WebClient())
                {
                    fileClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Exe_DownloadCompleted);
                    fileClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Exe_ProgressChanged);
                    fileClient.DownloadFileAsync(new Uri(setupURL), Path.Combine(mainWindow.StartupPath, "FIV Setup.exe"));
                }
            }
            else
            {
                string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NoInternetInfo));
                UpdaterMessage = $"{cultureTranslated}!";
            }
        }

        private void Exe_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //UpdaterMessage = $"Downloading: {(e.BytesReceived / 1024)}/{(e.TotalBytesToReceive / 1024)}kB ({e.ProgressPercentage}%)";
            string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DownloadingInfo));
            UpdaterMessage = $"{cultureTranslated}: {(e.BytesReceived / 1024)}kB";
            /*if (e.BytesReceived == e.TotalBytesToReceive)
            {
                UpdaterMessage = $"Download Complete: {(e.TotalBytesToReceive / 1024)}kB (100%)";
            }*/
        }

        private void Exe_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string cultureTranslated = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DownloadFinsihedInfo));
            UpdaterMessage = $"{cultureTranslated}!";
            //Thread.Sleep(500);

            if (File.Exists(Path.Combine(mainWindow.StartupPath, "FIV Setup.exe")))
            {
                Process.Start(Path.Combine(mainWindow.StartupPath, "FIV Setup.exe"), "/VERYSILENT");

                /*var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                foreach (Process proc in processes)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(proc.ProcessName);
                    });
                    proc.Kill();
                }*/

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }

            NotUpdating = true;

        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool HasLaterVersion()
        {
            return CurrentVersion >= DownloadVersion;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
