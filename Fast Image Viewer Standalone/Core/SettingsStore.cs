
namespace FIVStandard.Core
{
    static class SettingsStore
    {
        public static ISettings Settings { get; private set; }

        public static void InitSettingsStore(SettingsManager settings) => Settings = settings;
    }
}
