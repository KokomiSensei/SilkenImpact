using System;
using UnityEngine;
using UnityEngine.UI;
namespace SilkenImpact {
    public class UIHealthBar : HealthBar {
        [SerializeField] protected Text hpText;
        [SerializeField] protected Text nameText;
        [SerializeField] protected CanvasGroup canvasGroup;

        Vector3 nameTextOriginalScale;
        Vector3 hpTextOriginalScale;


        protected override void OnStart() {
            base.OnStart();
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            hpTextOriginalScale = hpText.transform.localScale;
            nameTextOriginalScale = nameText.transform.localScale;
        }
        protected override void Redraw() {
            base.Redraw();
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            OnHealthChanged();
        }

        protected override void OnHealthChanged() {
            hpText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        public void SetNameText(string name) {
            if (nameText == null) return;
            nameText.text = name;
        }


        public void SetHpTextColor(Color color) {
            if (hpText == null) return;
            hpText.color = color;
        }

        public void SetHpTextEnabled(bool enabled) {
            if (hpText == null) return;
            hpText.enabled = enabled;
        }

        public override void SetVisibility(bool visible) {
            base.SetVisibility(visible);
            canvasGroup.alpha = visible ? 1 : 0;
        }

        public void SetFont(Font font) {
            if (font == null) return;
            if (hpText != null) {
                hpText.font = font;
            }
            if (nameText != null) {
                nameText.font = font;
            }
        }

        internal void SetNameScale(float value) {
            nameText.transform.localScale = nameTextOriginalScale * value;
        }

        internal void SetHpNumberScale(float value) {
            hpText.transform.localScale = hpTextOriginalScale * value;
        }
    }
}