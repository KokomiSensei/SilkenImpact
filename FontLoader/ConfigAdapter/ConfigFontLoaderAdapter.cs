using System;
using UnityEngine;

namespace SilkenImpact {
    public abstract class ConfigFontLoaderAdapter : IFontLoader {
        protected abstract FontArgs CurrentArgs { get; }


        private FontArgs cachedArgs;
        private IFontLoader fontLoader;
        private IFontLoader fallbackLoader = new SmileySansLoader();


        public Font Load() {
            var currentOption = CurrentArgs;
            Font font = null;
            if (cachedArgs == null || !cachedArgs.Equals(currentOption)) {
                fontLoader = FontLoaderFactory.CreateFontLoader(currentOption);
                cachedArgs = currentOption;
            }
            font = fontLoader.Load();
            if (font == null) {
                PluginLogger.LogError($"[ConfigFontLoaderAdapter][{GetType().Name}][LoadFailed] args={currentOption}");
                return fallbackLoader.Load();
            }
            return font;
        }
    }
}
