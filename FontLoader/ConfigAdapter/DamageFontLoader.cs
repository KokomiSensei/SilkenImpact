using System;
using UnityEngine;

namespace SilkenImpact {
    public class DamageFontLoader : ConfigFontLoaderAdapter {
        protected override FontArgs CurrentArgs {
            get {
                var option = Configs.Instance.damageFont.Value;
                return option switch {
                    FontOption.SmileySans => new SmileySansFontArgs(),
                    FontOption.InGame => new InGameFontArgs(),
                    FontOption.LoadFromOS => new OSFontArgs(Configs.Instance.damageOsFontName.Value),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}