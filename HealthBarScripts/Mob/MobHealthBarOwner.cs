using UnityEngine;

namespace SilkenImpact {

    public class MobHealthBarOwner : MonoBehaviour, IHealthBarOwner {

        private VisibilityController visibilityController;
        public Dispatcher Dispatcher { get; private set; }
#if DEBUG
        private string originalName;
#endif


        void Awake() {
            HealthManager hm = GetComponent<HealthManager>();
            visibilityController = new VisibilityController(hm);
            Dispatcher = new Dispatcher(this);
#if DEBUG
            originalName = gameObject.name;
#endif
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
#if DEBUG
            var hm = GetComponent<HealthManager>();
            hm.name = originalName + $" visible={visibilityController.IsVisible} hp={hm.hp}";
#endif
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
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void TakeDamage(float amount) {
            if (!visibilityController.IsVisible) {
                Show();
                visibilityController.IsVisible = true;
            }
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void SetHP(float hp) {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.SetHP, gameObject, hp);
            updateVisibilityImmediate();
        }

        public void Die() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
            updateVisibilityImmediate();
        }

        public void Hide() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }

        public void CheckHP() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
        }
    }
}
