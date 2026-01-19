using UnityEngine;

namespace SilkenImpact {
    public class SmileySansLoader : CachedFontLoader {
        protected override Font DoLoad() {
            var bundle = Plugin.bundle;
            if (bundle == null) {
                Plugin.Logger.LogError("AssetBundle is null!");
                return null;
            }
            var font = bundle.LoadAsset<Font>("Assets/Addressables/Fonts/SmileySans-Oblique.ttf");
            return font;
        }
    }
}