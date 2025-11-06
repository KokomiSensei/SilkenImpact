using UnityEngine;

namespace SilkenImpact {

    public class BaseHealthBarOwner<EventType> : MonoBehaviour, IHealthBarOwner {

        private IVisibilityController visibilityController;
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
            if (visibilityController != null) {
                if (!visibilityController.Update())
                    return;
                CheckHP();
                if (visibilityController.IsVisible) {
                    Show();
                } else {
                    Hide();
                }
            }
#if DEBUG
            var hm = GetComponent<HealthManager>();
            hm.name = originalName + $" visible={visibilityController?.IsVisible} hp={hm.hp}";
#endif
        }

        // void OnDisable() {
        //     Hide();
        // }
        // void OnEnable() {
        //     Show();
        // }

        void OnDestroy() {
            Die();
        }

        void updateVisibilityImmediate() {
            if (visibilityController == null) return;
            if (visibilityController.Update(forceCheck: true)) {
                if (visibilityController.IsVisible) {
                    Show();
                } else {
                    Hide();
                }
            }
        }

        public void Heal(float amount) {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void TakeDamage(float amount) {
            updateVisibilityImmediate();
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
            updateVisibilityImmediate();
        }

        public void Die() {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
            updateVisibilityImmediate();
        }

        public void Hide() {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show() {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }

        public void SetHP(float hp) {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.SetHP, gameObject, hp);
            updateVisibilityImmediate();
        }

        public void CheckHP() {
            EventHandle<EventType>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
        }

        public void RemoveVisibilityController() {
            visibilityController = null;
        }

        public void LinkVisibilityControl(GameObject go) {
            if (visibilityController == null || visibilityController is VisibilityController) {
                visibilityController = new LinkedVisibilityController();
                visibilityController.Inspect(GetComponent<HealthManager>());
            }
            if (go.TryGetComponent<HealthManager>(out var hm)) {
                visibilityController.Inspect(hm);
            }
        }
    }
}
