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
        InGame,
        LoadFromOS,
    }

    public abstract record FontArgs(FontOption FontOption);

    public sealed record SmileySansFontArgs() : FontArgs(FontOption.SmileySans);

    public sealed record InGameFontArgs() : FontArgs(FontOption.InGame);

    public sealed record OSFontArgs(string FontName) : FontArgs(FontOption.LoadFromOS);
}