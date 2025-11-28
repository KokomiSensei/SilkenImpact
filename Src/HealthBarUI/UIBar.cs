using UnityEngine;
using UnityEngine.UI;
namespace SilkenImpact {
    public class UIBar : Bar {
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Image image;

        public override float CurrentPercentage => rectTransform.sizeDelta.x / maxWidth;

        /// <summary>
        /// Update the size of the bar, while keeping its current percentage.
        /// If called repeatedly, the width will be wrong due to float precision loss.
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="maxWidth"></param>
        /// <param name="verticalMargin"></param>
        /// <param name="horizontalMargin"></param>
        public override void UpdateSize(float maxHeight, float maxWidth, float verticalMargin, float horizontalMargin) {
            float p = CurrentPercentage;
            this.maxHeight = maxHeight - 2 * verticalMargin;
            this.maxWidth = maxWidth - 2 * horizontalMargin;
            this.verticalMargin = verticalMargin;
            this.horizontalMargin = horizontalMargin;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(horizontalMargin, verticalMargin);
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

        public override void SetColor(Color color) {
            if (image != null) {
                image.color = color;
            }
        }
    }
}