
using FIVStandard.Model;

namespace FIVStandard.Core
{
    static class SettingsStore
    {
        public static ISettings Settings { get; private set; }

        public static void InitSettingsStore(SettingsJson jsettings) => Settings = jsettings;
    }
}
