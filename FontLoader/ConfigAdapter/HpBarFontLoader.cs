using System;
using UnityEngine;

namespace SilkenImpact {
    public class HpBarFontLoader : ConfigFontLoaderAdapter {
        protected override FontArgs CurrentArgs {
            get {
                var option = Configs.Instance.hpBarFont.Value;
                return option switch {
                    FontOptions.SmileySans => new SmileySansFontArgs(),
                    FontOptions.InGame => new InGameFontArgs(),
                    FontOptions.LoadFromOS => new OSFontArgs(Configs.Instance.hpBarOsFontName.Value),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}