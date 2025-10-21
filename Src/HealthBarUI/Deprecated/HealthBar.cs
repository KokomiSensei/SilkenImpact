using System.Collections;
using UnityEngine;
namespace SilkenImpact.Deprecated {
    public class HealthBar : MonoBehaviour {
        // Renderers
        public SpriteRenderer hp;
        public SpriteRenderer delayedEffect;
        public SpriteRenderer background;


        // Config and state
        public float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float delayedHealth;
        public float timerReductionPercentageOnDamage = 0.5f;
        public float decayThresholdSeconds = 0.2f;
        public float decayPercentagePerSecond = 0.4f;
        [SerializeField] private float decayTimer = 0f;
        public float healthChangeDuration = 0.2f;
        private Coroutine healthAnimationCoroutine;



        private void Awake() {
            // make sure 'hp' and 'delayedEffect' are the exact same size as 'background', and that 'hp' is on top of 'delayedEffect', which is on top of 'background'
            hp.size = background.size;
            delayedEffect.size = background.size;

            // 确保大小一致 - 使用Transform的本地缩放来匹配大小
            Vector3 backgroundScale = background.transform.localScale;
            hp.transform.localScale = backgroundScale;
            delayedEffect.transform.localScale = backgroundScale;

            // 确保位置对齐
            Vector3 backgroundPosition = background.transform.localPosition;
            hp.transform.localPosition = backgroundPosition;
            delayedEffect.transform.localPosition = backgroundPosition;

            hp.sortingOrder = 2;           // hp在最上层
            delayedEffect.sortingOrder = 1; // delayedEffect在中间层
            background.sortingOrder = 0;

            currentHealth = maxHealth;
            delayedHealth = maxHealth;
        }
        private void SetSpritePercentage(SpriteRenderer sprite, float percentage) {
            percentage = Mathf.Clamp01(percentage);
            // the sprite may be 'hp' or 'delayedEffect', make sure the size is set correctly, and the pivot is on the left side
            Vector3 newScale = sprite.transform.localScale;
            newScale.x = background.transform.localScale.x * percentage;
            sprite.transform.localScale = newScale;

            // 3. 调整位置以保持左侧对齐（锚点在左侧）
            Vector3 newPosition = sprite.transform.localPosition;
            float width = background.size.x;
            float scaleDifference = background.transform.localScale.x - newScale.x;
            // 根据缩放差值计算X轴位移（左对齐）
            newPosition.x = background.transform.localPosition.x - (width * scaleDifference * 0.5f);
            sprite.transform.localPosition = newPosition;
        }

        private void Update() {
            // if get key down a, take 10 damage; if get key down d, heal 10
            if (Input.GetKeyDown(KeyCode.A)) {
                TakeDamage(10f);
            }
            if (Input.GetKeyDown(KeyCode.D)) {
                Heal(10f);
            }

            SetSpritePercentage(hp, currentHealth / maxHealth);

            if (delayedHealth > currentHealth) {
                if (decayTimer < decayThresholdSeconds) {
                    decayTimer += Time.deltaTime;
                } else {
                    delayedHealth -= maxHealth * Time.deltaTime * decayPercentagePerSecond;
                    delayedHealth = Mathf.Clamp(delayedHealth, currentHealth, maxHealth);
                }
            } else {
                delayedHealth = currentHealth;
                decayTimer = 0f;
            }
            SetSpritePercentage(delayedEffect, delayedHealth / maxHealth);
        }

        public void TakeDamage(float amount) {
            decayTimer -= timerReductionPercentageOnDamage * decayThresholdSeconds;
            decayTimer = Mathf.Clamp(decayTimer, -0.3f * decayThresholdSeconds, decayThresholdSeconds);
            float newHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            AnimateHealthTo(newHealth);
        }

        public void Heal(float amount) {
            float newHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            AnimateHealthTo(newHealth);
        }

        public void SetHealth(float percentage) {
            percentage = Mathf.Clamp01(percentage);
            var newHealth = percentage * maxHealth;
            if (newHealth >= currentHealth) {
                Heal(newHealth - currentHealth);
            } else {
                TakeDamage(currentHealth - newHealth);
            }
        }
        private void AnimateHealthTo(float newHealth) {
            if (healthAnimationCoroutine != null) {
                StopCoroutine(healthAnimationCoroutine);
            }
            healthAnimationCoroutine = StartCoroutine(AnimateHealthChange(newHealth));
        }

        private IEnumerator AnimateHealthChange(float targetHealth) {
            float timer = 0f;
            float startHealth = currentHealth;

            while (timer < healthChangeDuration) {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / healthChangeDuration);
                currentHealth = Mathf.Lerp(startHealth, targetHealth, t);
                yield return null;
            }

            currentHealth = targetHealth; // 确保动画结束时血量精确
            healthAnimationCoroutine = null;
        }
    }
}