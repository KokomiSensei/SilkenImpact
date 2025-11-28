using System.Collections;
using UnityEngine;
namespace SilkenImpact {
    public abstract class Bar : MonoBehaviour {
        public float verticalMargin;
        public float horizontalMargin;
        public float maxHeight;
        public float maxWidth;

        //public float targetPercentage;

        public abstract float CurrentPercentage { get; }
        protected abstract void MatchWithPercentage(float percentage);


        /// <summary>
        /// Update the size of the bar, while keeping its current percentage.
        /// </summary>
        public abstract void UpdateSize(float maxHeight, float maxWidth, float verticalMargin, float horizontalMargin);

        public abstract void SetVisibility(bool visible);

        public abstract void SetColor(Color color);

        public void Init(float maxHeight, float maxWidth, float verticalMargin, float horizontalMargin) {
            UpdateSize(maxHeight, maxWidth, verticalMargin, horizontalMargin);
            MatchWithPercentage(1);
        }

        public void SetPercentage(float percentage, float animationDurationSeconds) {
            percentage = Mathf.Clamp01(percentage);

            if (animationDurationSeconds <= 0) {
                MatchWithPercentage(percentage);
            } else {
                StopAllCoroutines();
                StartCoroutine(AnimateTo(CurrentPercentage, percentage, animationDurationSeconds));
            }
        }
        private IEnumerator AnimateTo(float startPercentage, float targetPercentage, float duration) {
            float elapsedTime = 0f;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                // 使用 Lerp 平滑地插值计算当前帧的缩放和位置
                float percentage = Mathf.Lerp(startPercentage, targetPercentage, t);
                MatchWithPercentage(percentage);

                yield return null; // 等待下一帧
            }

            // 动画结束后，确保最终值被精确设置
            MatchWithPercentage(targetPercentage);
        }
    }
}