using System;
using System.Linq;
using UnityEngine;

namespace SilkenImpact {
    public interface IFontLoader {
        Font Load();
    }

    public enum FontOptions {
        SmileySans,
        InGame,
        LoadFromOS,
    }

    public abstract class FontArgs {
        public FontOptions FontOption { get; protected set; }
    }

    public sealed class SmileySansFontArgs : FontArgs {
        public SmileySansFontArgs() => FontOption = FontOptions.SmileySans;
    }

    public sealed class InGameFontArgs : FontArgs {
        public InGameFontArgs() => FontOption = FontOptions.InGame;
    }

    public sealed class OSFontArgs : FontArgs {
        public string FontName { get; }
        public OSFontArgs(string fontName) {
            FontOption = FontOptions.LoadFromOS;
            FontName = fontName;
        }
    }
}