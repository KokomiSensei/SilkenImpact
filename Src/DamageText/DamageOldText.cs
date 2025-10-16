using UnityEngine;
using UnityEngine.UI;
namespace SilkenImpact {


    public class DamageOldText : DamageText {
        public Text textComponent;

        public override string DamageString {
            get => textComponent.text;
            set => textComponent.text = value;
        }
        public override Color TextColor {
            get => textComponent.color;
            set => textComponent.color = value;
        }
    }
}