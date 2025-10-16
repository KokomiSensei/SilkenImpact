using UnityEngine;
using UnityEngine.UI;
namespace SilkenImpact {


    public class DamageOldText : DamageText {
        public Text textComponent;

        protected override string text {
            get => textComponent.text;
            set => textComponent.text = value;
        }
        protected override Color color {
            get => textComponent.color;
            set => textComponent.color = value;
        }
    }
}