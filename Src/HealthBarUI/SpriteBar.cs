

using UnityEngine;
namespace SilkenImpact {
    public class SpriteBar : Bar {
        private SpriteRenderer spriteRenderer;
        private void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override float CurrentPercentage =>
            (transform.localScale.x) / (maxWidth - 2 * horizontalMargin);

        public override void SetVisibility(bool visible) {
            spriteRenderer.enabled = visible;
        }

        public override void UpdateSize(float maxHeight, float maxWidth, float verticalMargin, float horizontalMargin) {
            this.verticalMargin = verticalMargin;
            this.horizontalMargin = horizontalMargin;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            transform.localScale = new Vector3(maxWidth - 2 * verticalMargin, maxHeight - 2 * horizontalMargin, 1);
        }

        protected override void MatchWithPercentage(float percentage) {
            Vector3 newScale = transform.localScale;
            Vector3 newPosition = transform.localPosition;

            newScale.x = (maxWidth - 2 * horizontalMargin) * percentage;
            newPosition.x = -(maxWidth / 2 - horizontalMargin - newScale.x / 2);

            transform.localScale = newScale;
            transform.localPosition = newPosition;
        }

        public override void SetColor(Color color) {
            spriteRenderer.color = color;
        }
    }
}