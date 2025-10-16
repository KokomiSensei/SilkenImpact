using UnityEngine;
namespace SilkenImpact {


    public class HealthBar : MonoBehaviour {
        [SerializeField] protected HealthBarConfig config;

        [SerializeField] protected Bar hp;
        [SerializeField] protected Bar delayedEffect;
        [SerializeField] protected Bar background;

        [SerializeField] protected float height;
        [SerializeField] protected float width;
        [SerializeField] protected float margin;

        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth = 100f;
        [SerializeField]
        protected float decayTimer = 0f;
        [SerializeField]
        protected float DecayingHealth {
            get {
                float decayingHealth = delayedEffect.CurrentPercentage * maxHealth;
                if (Mathf.Abs(currentHealth - decayingHealth) < 1e-5) {
                    return currentHealth;
                } else {
                    return decayingHealth;
                }
            }
        }
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        public void SetHeight(float height) {
            this.height = height;
            OnStart();
        }

        public void SetWidth(float width) {
            this.width = width;
            OnStart();
        }

        public void SetMargin(float margin) {
            this.margin = margin;
            OnStart();
        }

        public void SetMaxHealth(float maxHealth) {
            this.maxHealth = maxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); //TODO negative hp??
            OnStart();
        }

        protected void Start() {
            OnStart();
        }

        protected virtual void OnStart() {
            hp.Init(height, width, margin);
            delayedEffect.Init(height, width, margin);
            background.Init(height, width, 0);
            currentHealth = maxHealth;
        }

        protected void Update() {
            if (DecayingHealth > currentHealth) {
                if (decayTimer < config.decayThresholdSeconds) {
                    decayTimer += Time.deltaTime;
                } else {
                    delayedEffect.SetPercentage(currentHealth / maxHealth, (DecayingHealth - currentHealth) / maxHealth / config.decayPercentagePerSecond);
                    //decayingHealth = currentHealth;
                    decayTimer = 0;
                }
            } else {
                decayTimer = 0;
            }
        }

        public void TakeDamage(float amount) {
            if (decayTimer > 0) {
                decayTimer -= config.timerReductionPercentageOnDamage * config.decayThresholdSeconds;
            }
            decayTimer = Mathf.Clamp(decayTimer, -0.3f * config.decayThresholdSeconds, config.decayThresholdSeconds);
            float newHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            currentHealth = newHealth;
            hp.SetPercentage(currentHealth / maxHealth, config.healthChangeDuration);
        }

        public void Heal(float amount) {
            float newHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            currentHealth = newHealth;
            //if (currentHealth > DecayingHealth) {
            //    //DecayingHealth = currentHealth;
            //}
            delayedEffect.SetPercentage(currentHealth / maxHealth, config.healthChangeDuration);
            hp.SetPercentage(currentHealth / maxHealth, config.healthChangeDuration);
        }
    }
}