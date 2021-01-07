using Microsoft.Win32;
using System;
using System.Diagnostics;

#pragma warning disable CA1416 // Validate platform compatibility

namespace FIVStandard.Core
{
    public class FileAssociation
    {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
    }

    class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void EnsureAssociationsSet()//DEFAULT TEMPLATE EXAMPLE
        {
            var filePath = Process.GetCurrentProcess().MainModule.FileName;
            EnsureAssociationsSet(
                new FileAssociation
                {
                    Extension = ".webm",
                    ProgId = "Fast Image Viewer",
                    FileTypeDescription = "Image viewer for efficient viewing.",
                    ExecutableFilePath = filePath
                });
        }

        public static void EnsureAssociationsSet(params FileAssociation[] associations)
        {
            bool madeChanges = false;
            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(
                    association.Extension,
                    association.FileTypeDescription,
                    association.ExecutableFilePath,
                    association.ProgId);
            }

            if (madeChanges)
            {
                _ = SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static bool SetAssociation(string extension, string fileTypeDescription, string applicationFilePath, string progId = "Fast Image Viewer")
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{extension}", progId);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}", fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");

            /*using (var imgKey = Registry.ClassesRoot.OpenSubKey(extension))
            {
                var imgType = imgKey.GetValue("");
                string command = "\"" + applicationFilePath + "\"" + " \"%1\"";
                string keyName = imgType + @"\shell\Open\command";
                using (var key = Registry.ClassesRoot.CreateSubKey(keyName))
                {
                    key.SetValue("", command);
                }
            }*/

            return madeChanges;
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;//already set, so its left unchanged
        }

        public static bool GetAssociation(string extension, string progId = "Fast Image Viewer")
        {
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{extension}"))
            {
                if (key.GetValue(null) as string != progId)
                {
                    //key.SetValue(null, progId);
                    return true;
                }
            }

            return false;
        }
    }
}
