namespace SilkenImpact {
    public class FontManager : Singleton<FontManager> {
        public IFontLoader DamageFontLoader { get; private set; } = new DamageFontLoader();
        public IFontLoader HpBarFontLoader { get; private set; } = new HpBarFontLoader();
    }
}