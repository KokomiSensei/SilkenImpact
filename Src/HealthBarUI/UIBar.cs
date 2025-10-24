using UnityEngine;
using UnityEngine.UIElements;
namespace SilkenImpact {
    public class UIBar : Bar {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        public override float CurrentPercentage =>
            rectTransform.sizeDelta.x / maxWidth;




        /// <summary>
        /// Update the size of the bar, while keeping its current percentage.
        /// If called repeatedly, the width will be wrong due to float precision loss.
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="maxWidth"></param>
        /// <param name="margin"></param>
        public override void UpdateSize(float maxHeight, float maxWidth, float margin) {
            float p = CurrentPercentage;
            this.maxHeight = maxHeight - 2 * margin;
            this.maxWidth = maxWidth - 2 * margin;
            this.margin = margin;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(margin, margin);
            //rectTransform.offsetMax = new Vector2(maxWidth - margin, -margin);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.maxHeight);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.maxWidth * p);
        }

        protected override void MatchWithPercentage(float percentage) {
            float newWidth = maxWidth * percentage;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }

        public override void SetVisibility(bool visible) {
            canvasGroup.alpha = visible ? 1 : 0;
        }
    }
}