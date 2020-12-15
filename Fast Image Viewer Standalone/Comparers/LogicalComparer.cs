using FIVStandard.ViewModel;
using System.Collections;
using System.Runtime.InteropServices;

namespace FIVStandard.Comparers
{
    /// <summary>
    /// Orders like the Windows' "sort by name"
    /// </summary>
    public class LogicalComparer : IComparer
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public int Compare(object x1, object y1)
        {
            return StrCmpLogicalW(((ThumbnailItemData)x1).ThumbnailName, ((ThumbnailItemData)y1).ThumbnailName);
        }
    }
}
