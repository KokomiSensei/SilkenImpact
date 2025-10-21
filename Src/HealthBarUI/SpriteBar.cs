

using UnityEngine;
namespace SilkenImpact {
    public class SpriteBar : Bar {

        public override float CurrentPercentage =>
            (transform.localScale.x) / (maxWidth - 2 * margin);

        public override void UpdateSize(float maxHeight, float maxWidth, float margin) {
            this.margin = margin;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            transform.localScale = new Vector3(maxWidth - 2 * margin, maxHeight - 2 * margin, 1);
        }

        protected override void MatchWithPercentage(float percentage) {
            Vector3 newScale = transform.localScale;
            Vector3 newPosition = transform.localPosition;

            newScale.x = (maxWidth - 2 * margin) * percentage;
            newPosition.x = -(maxWidth / 2 - margin - newScale.x / 2);

            transform.localScale = newScale;
            transform.localPosition = newPosition;
        }
    }
}