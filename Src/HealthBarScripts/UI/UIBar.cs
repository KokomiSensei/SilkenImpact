using UnityEngine;
namespace SilkenImpact {
    public class UIBar : Bar {
        private RectTransform rectTransform;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        public override float CurrentPercentage =>
            rectTransform.sizeDelta.x / maxWidth;

        public override void Init(float maxHeight, float maxWidth, float margin) {
            this.maxHeight = maxHeight - 2 * margin;
            this.maxWidth = maxWidth - 2 * margin;
            this.margin = margin;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(margin, margin);
            rectTransform.offsetMax = new Vector2(-margin, -margin);
            MatchWithPercentage(1);
        }

        protected override void MatchWithPercentage(float percentage) {
            float newWidth = maxWidth * percentage;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }
    }
}