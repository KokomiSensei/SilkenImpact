using UnityEngine;

namespace SilkenImpact {
    public class OSFontLoader : CachedFontLoader {
        public OSFontLoader(string fontName) {
            FontName = fontName;
        }
        public string FontName { get; set; }
        protected override Font DoLoad() {
            string[] osFontNames = Font.GetOSInstalledFontNames();
            PluginLogger.LogDebug($"[OsFontLoader][DoLoad][InstalledFonts] Number of installed fonts: {osFontNames.Length}");
            bool found = false;
            foreach (string name in osFontNames) {
                if (name == FontName) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                PluginLogger.LogWarning($"[OsFontLoader][DoLoad][FontNotFound] font={FontName}");
            }
            Font osFont = Font.CreateDynamicFontFromOSFont(FontName, 16);
            return osFont;

        }
    }
}
