using System;
using UnityEngine;

namespace SilkenImpact {
    public class DamageFontLoader : ConfigFontLoaderAdapter {
        protected override FontArgs CurrentArgs {
            get {
                var option = Configs.Instance.damageFont.Value;
                return option switch {
                    FontOptions.SmileySans => new SmileySansFontArgs(),
                    FontOptions.InGame => new InGameFontArgs(),
                    FontOptions.LoadFromOS => new OSFontArgs(Configs.Instance.damageOsFontName.Value),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}