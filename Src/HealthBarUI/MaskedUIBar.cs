using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public class MaskedUIBar : UIBar {
        [SerializeField] private RectMask2D rectMask;

        private float currentWidth;
        public override float CurrentPercentage => currentWidth / maxWidth;

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

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.maxHeight);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.maxWidth);
            MatchWithPercentage(p);
        }

        protected override void MatchWithPercentage(float percentage) {
            float displayWidth = maxWidth * percentage;
            currentWidth = displayWidth;
            rectMask.padding = new Vector4(0, 0, maxWidth - displayWidth, 0);
        }
    }
}