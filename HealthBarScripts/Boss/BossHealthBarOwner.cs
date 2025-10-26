using UnityEngine;

namespace SilkenImpact {

    public class BossHealthBarOwner : MonoBehaviour, IHealthBarOwner {

        private VisibilityController visibilityController;
        public Dispatcher Dispatcher { get; private set; }

        void Awake() {
            HealthManager hm = GetComponent<HealthManager>();
            visibilityController = new VisibilityController(hm);
            Dispatcher = new Dispatcher(this);
        }


        void Update() {
            if (!visibilityController.Update())
                return;
            CheckHP();
            if (visibilityController.IsVisible) {
                Show();
            } else {
                Hide();
            }
        }

        void OnDisable() {
            Hide();
        }
        void OnEnable() {
            Show();
        }

        void OnDestroy() {
            Die();
        }

        void updateVisibilityImmediate() {
            if (visibilityController.Update(forceCheck: true)) {
                if (visibilityController.IsVisible) {
                    Show();
                } else {
                    Hide();
                }
            }
        }

        public void Heal(float amount) {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void TakeDamage(float amount) {
            if (!visibilityController.IsVisible) {
                Show();
                visibilityController.IsVisible = true;
            }
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void Die() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
            updateVisibilityImmediate();
        }

        public void Hide() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }

        public void SetHP(float hp) {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.SetHP, gameObject, hp);
            updateVisibilityImmediate();
        }

        public void CheckHP() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
        }
    }
}
