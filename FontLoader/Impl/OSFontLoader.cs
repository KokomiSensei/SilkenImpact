using UnityEngine;

namespace SilkenImpact {
    public class OSFontLoader : CachedFontLoader {
        public OSFontLoader(string fontName) {
            FontName = fontName;
        }
        public string FontName { get; set; }
        protected override Font DoLoad() {
            string[] osFontNames = Font.GetOSInstalledFontNames();
            PluginLogger.LogInfo("Installed OS Fonts:");
            foreach (string name in osFontNames) {
                PluginLogger.LogInfo(name);
            }
            bool found = false;
            foreach (string name in osFontNames) {
                if (name == FontName) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                PluginLogger.LogError($"{FontName} Not Found");
            }
            Font osFont = Font.CreateDynamicFontFromOSFont(FontName, 16);
            return osFont;

        }
    }
}