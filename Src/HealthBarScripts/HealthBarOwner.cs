using UnityEngine;

namespace SilkenImpact {
    public class HealthBarOwner : MonoBehaviour {
        void Update() {
            //TODO maybe check if the health is correct?

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
