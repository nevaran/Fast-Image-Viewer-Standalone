﻿using Gu.Localization;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ToastNotifications.Messages;

namespace FIVStandard.Core
{
    public enum UpdateCheckType
    {
        /// <summary>
        /// Notifies if up to date and new version available; downloads and installs
        /// </summary>
        FullUpdate,
        /// <summary>
        /// Updates check, download, and installs no matter the version
        /// </summary>
        FullUpdateForced,
        /// <summary>
        /// Downloads version information without any user notifications
        /// </summary>
        SilentVersionCheck,
        /// <summary>
        /// Notifies the user about the version status
        /// </summary>
        ForcedVersionCheck
    }

    public class UpdateCheck : INotifyPropertyChanged
    {
        /*[DllImport("kernel32.dll", SetLastError = true)]
        static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs, int Flags);*/

        private readonly MainWindow mainWindow;

        private bool _notUpdating = true;

        public bool NotUpdating
        {
            get
            {
                return _notUpdating;
            }
            set
            {
                _notUpdating = value;
                OnPropertyChanged();
            }
        }

        private Version _currentVersion = new Version("0.0.0.0");
        
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

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

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
        //private const string setupURL = "https://drive.google.com/uc?export=download&id=19Ds5qxy4QVFlFljIgZjHenXOA1qALQ8R";
        //private const string changelogURL = "https://drive.google.com/uc?export=download&id=1cqCjCSZpo3bSF8G9Wrk0fT-ypQY7RKMn";

        private const string setupURL = "https://github.com/nevaran/FIV/releases/latest/download/FIV.Setup.exe";
        private const string changelogURL = "https://github.com/nevaran/FIV/releases/latest/download/VersionControl.txt";

        public UpdateCheck(MainWindow mw)
        {
            mainWindow = mw;

            CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public async Task CheckForUpdates(UpdateCheckType updateType)
        {
            await _lock.WaitAsync();

            try
            {
                NotUpdating = false;

                switch (updateType)
                {
                    case UpdateCheckType.FullUpdate:
                        await Download_Full();
                        break;
                    case UpdateCheckType.FullUpdateForced:
                        await Download_FullForced();
                        break;
                    case UpdateCheckType.SilentVersionCheck:
                        await Download_VersionInfo(false);
                        break;
                    case UpdateCheckType.ForcedVersionCheck:
                        await Download_VersionInfo(true);
                        break;
                }
            }
            catch// (Exception e)
            {
                //mainWindow.notifier.ShowError(e.ToString());
                //e.Message
                //e.StackTrace
                //Console.WriteLine(e.ToString());
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task Download_Full()
        {
            mainWindow.notifier.ShowInformation(Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CheckingForUpdatesInfo)));

            await GetHttpChangelog();

            if (HasLaterVersion())
            {
                mainWindow.notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.AlreadyOnLatestVerInfo))}({DownloadVersion.ToString()})");

                UpdaterMessage = "";
                NotUpdating = true;
            }
            else
            {
                mainWindow.notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NewVerAvailableInfo))}. {Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo))} ({DownloadVersion.ToString()})");

                await DownloadNewAppVersion();
            }
        }

        private async Task Download_FullForced()
        {
            mainWindow.notifier.ShowInformation(Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.CheckingForUpdatesInfo)));

            await GetHttpChangelog();

            mainWindow.notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo))} ({DownloadVersion.ToString()})");

            await DownloadNewAppVersion();
        }

        private async Task Download_VersionInfo(bool notifies)
        {
            await GetHttpChangelog();

            if (HasLaterVersion())
            {
                if (notifies)
                    mainWindow.notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.AlreadyOnLatestVerInfo))}({DownloadVersion.ToString()})");

                UpdaterMessage = "";
                NotUpdating = true;
            }
            else
            {
                if (notifies)
                    mainWindow.notifier.ShowInformation($"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NewVerAvailableInfo))}: {DownloadVersion.ToString()}");

                NotUpdating = true;

                if (mainWindow.Settings.CheckForUpdatesStartToggle && mainWindow.Settings.AutoupdateToggle)
                {
                    await DownloadNewAppVersion();
                }
            }
        }

        private async Task DownloadNewAppVersion()
        {
            if (CheckForInternetConnection())
            {
                UpdaterMessage = Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.UpdatingInfo));

                using var client = new HttpClientDownloadWithProgress(setupURL, Path.Combine(mainWindow.StartupPath, "FIV Setup.exe"));
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    string info = $"{progressPercentage}%\n{totalBytesDownloaded / 1048576}/{(totalFileSize / 1048576)}mB";
                    //string info = $"{totalBytesDownloaded / 1024}kB";//1048576 = mB
                    UpdaterMessage = $"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DownloadingInfo))}: {info}";
                };
                client.DownloadComplete += Client_DownloadComplete;

                await client.StartDownload();
            }
            else
            {
                UpdaterMessage = $"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.NoInternetInfo))}!";
            }
        }

        private void Client_DownloadComplete()
        {
            UpdaterMessage = $"{Translator.Translate(Properties.Resources.ResourceManager, nameof(Properties.Resources.DownloadFinsihedInfo))}!";
            //Thread.Sleep(500);

            if (File.Exists(Path.Combine(mainWindow.StartupPath, "FIV Setup.exe")))
            {
                //TODO: add valid argument of opened file/image
                //RegisterApplicationRestart("", 2);

                //TODO check if it installs where the already installed one is
                Process.Start(Path.Combine(mainWindow.StartupPath, "FIV Setup.exe"), "/SILENT /CLOSEAPPLICATIONS");//TODO add working /RESTARTAPPLICATIONS /LOG

                /*var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                foreach (Process proc in processes)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(proc.ProcessName);
                    });
                    proc.Kill();
                }*/

                /*Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });*/
            }

            NotUpdating = true;
        }

        private async Task GetHttpChangelog()
        {
            //txt file containing version and update notes
            HttpClient httpClient = new HttpClient();

            using var reader = new StreamReader(await httpClient.GetStreamAsync(changelogURL));
            DownloadVersion = new Version(reader.ReadLine().Trim(new Char[] { '•' }));
            reader.ReadLine();//empty line between version and notes
            DownloadedChangelog = reader.ReadToEnd();
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using var client = new WebClient();
                using var stream = client.OpenRead("http://www.google.com");
                return true;
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