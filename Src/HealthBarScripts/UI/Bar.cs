using System.Collections;
using UnityEngine;
namespace SilkenImpact {
    public abstract class Bar : MonoBehaviour {
        public float margin;
        public float maxHeight;
        public float maxWidth;

        public abstract float CurrentPercentage { get; }
        protected abstract void MatchWithPercentage(float percentage);

        public abstract void Init(float maxHeight, float maxWidth, float margin);


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