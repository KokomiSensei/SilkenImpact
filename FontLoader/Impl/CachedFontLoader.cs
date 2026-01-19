using UnityEngine;

namespace SilkenImpact {
    public abstract class CachedFontLoader : IFontLoader {
        protected Font cachedFont;
        protected abstract Font DoLoad();
        public Font Load() {
            if (cachedFont == null) {
                cachedFont = DoLoad();
            }
            return cachedFont;
        }
    }
}