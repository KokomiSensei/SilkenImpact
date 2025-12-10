using System;
using UnityEngine;

namespace SilkenImpact {
    public class HpBarFontLoader : ConfigFontLoaderAdapter {
        protected override FontArgs CurrentArgs {
            get {
                var option = Configs.Instance.hpBarFont.Value;
                return option switch {
                    FontOption.SmileySans => new SmileySansFontArgs(),
                    FontOption.InGame => new InGameFontArgs(),
                    FontOption.LoadFromOS => new OSFontArgs(Configs.Instance.hpBarOsFontName.Value),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}