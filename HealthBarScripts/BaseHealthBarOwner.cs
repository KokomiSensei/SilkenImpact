using UnityEngine;

namespace SilkenImpact {

    public class BaseHealthBarOwner<EventType> : MonoBehaviour, IHealthBarOwner {

        private IVisibilityController visibilityController;
        public Dispatcher Dispatcher { get; private set; }



        void Awake() {
            HealthManager hm = GetComponent<HealthManager>();
            visibilityController = new VisibilityController(hm);
            Dispatcher = new Dispatcher(this);
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
            var hm = GetComponent<HealthManager>();
            PluginLogger.LogDebug($"[BaseHealthBarOwner][Update][Status Report] Visible={visibilityController?.IsVisible} hp={hm.hp}");
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
