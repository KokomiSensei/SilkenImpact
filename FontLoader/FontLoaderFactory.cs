using System;

namespace SilkenImpact {
    public static class FontLoaderFactory {
        private static IFontLoader FallbackFontLoader(FontArgs args) {
            PluginLogger.LogError($"Unknown FontOption: {args.FontOption}, FontArgs: {args}");
            return new SmileySansLoader();
        }
        public static IFontLoader CreateFontLoader(FontArgs args) {
            return args.FontOption switch {
                FontOption.SmileySans => new SmileySansLoader(),
                FontOption.InGame => new GameFontLoader(),
                FontOption.LoadFromOS => new OSFontLoader(((OSFontArgs)args).FontName),
                _ => FallbackFontLoader(args),
            };
        }
    }
}