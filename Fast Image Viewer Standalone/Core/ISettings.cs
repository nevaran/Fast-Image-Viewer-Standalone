
using System.Collections.Generic;

namespace FIVStandard.Core
{
    interface ISettings
    {
        bool DownsizeImageToggle { get; }

        int ThumbnailRes { get; }

        List<string> ThemeAccents { get; }

        int ThemeAccentDropIndex { get; }
    }
}
