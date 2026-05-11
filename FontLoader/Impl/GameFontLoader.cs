using System.Linq;
using UnityEngine;

namespace SilkenImpact {
    public class GameFontLoader : CachedFontLoader {
        private string fontName = "TrajanPro-Regular";
        protected override Font DoLoad() {
            Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
            Font font = allFonts.FirstOrDefault(f => f.name.Contains(fontName));

            if (font == null) {
                PluginLogger.LogError($"[GameFontLoader][DoLoad][LoadFailed] font={fontName}");
            }
            return font;
        }
    }
}
