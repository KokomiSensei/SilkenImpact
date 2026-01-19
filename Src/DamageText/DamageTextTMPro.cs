using TMPro;
using UnityEngine;
namespace SilkenImpact {
    public class DamageTextTMPro : DamageText {
        public TextMeshProUGUI textComponent;
        public override string DamageString {
            get => textComponent.text;
            set => textComponent.text = DamageString;
        }
        public override Color TextColor {
            get => textComponent.color;
            set => textComponent.color = value;
        }
        public override Font TextFont {
            set {
                if (value == null) return;
                // TODO shared font asset management
                var tmpFont = TMP_FontAsset.CreateFontAsset(value);
                textComponent.font = tmpFont;
            }
        }
    }
}