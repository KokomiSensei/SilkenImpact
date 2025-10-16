using TMPro;
using UnityEngine;
namespace SilkenImpact {
    public class DamageTextTMPro : DamageText {
        public TextMeshProUGUI textComponent;
        protected override string text {
            get => textComponent.text;
            set => textComponent.text = text;
        }
        protected override Color color {
            get => textComponent.color;
            set => textComponent.color = value;
        }
    }
}