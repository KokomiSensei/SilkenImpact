using UnityEngine;

namespace SilkenImpact {

    public class HealthBarOwner : MonoBehaviour {
        private HealthManager hm => gameObject.GetComponent<HealthManager>();
        void Start() {

        }
        void Update() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);

        }
        void OnDisable() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }
        void OnEnable() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }

        void OnDestroy() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }
    }
}
