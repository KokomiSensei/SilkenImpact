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
    }
}