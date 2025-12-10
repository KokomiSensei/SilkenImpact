using UnityEngine;

namespace SilkenImpact {
    public class OSFontLoader : CachedFontLoader {
        public OSFontLoader(string fontName) {
            FontName = fontName;
        }
        public string FontName { get; set; }
        protected override Font DoLoad() {
            Font osFont = Font.CreateDynamicFontFromOSFont(FontName, 16);
            if (osFont == null) {
                PluginLogger.LogError($"{FontName} Not Found");
            }
            return osFont;

        }
    }
}