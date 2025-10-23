using UnityEngine;

namespace SilkenImpact {

    public class BossHealthBarOwner : MonoBehaviour, IHealthBarOwner {

        private VisibilityController visibilityController;

        void Awake() {
            HealthManager hm = GetComponent<HealthManager>();
            visibilityController = new VisibilityController(hm);
        }


        void Update() {
            if (!visibilityController.Update())
                return;
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
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


        public void Heal(float amount) {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
        }

        public void TakeDamage(float amount) {
            if (!visibilityController.IsVisible) {
                Show();
                visibilityController.IsVisible = true;
            }
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
        }

        public void Die() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }

        public void Hide() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show() {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }

        public void SetHP(float hp) {
            EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.SetHP, gameObject, hp);
        }
    }
}
