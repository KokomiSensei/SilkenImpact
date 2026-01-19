using System;
using System.Linq;
using UnityEngine;

namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit { }
}


namespace SilkenImpact {
    public interface IFontLoader {
        Font Load();
    }

    public enum FontOption {
        SmileySans,
        InGameFont,
        OSFont,
    }

    public abstract record FontArgs(FontOption FontOption);

    public sealed record SmileySansFontArgs() : FontArgs(FontOption.SmileySans);

    public sealed record InGameFontArgs() : FontArgs(FontOption.InGameFont);

    public sealed record OSFontArgs(string FontName) : FontArgs(FontOption.OSFont);
}