using UnityEngine;

namespace SilkenImpact {
    public class HealthBarOwner : MonoBehaviour {
        void Update() {
            //TODO maybe check if the health is correct?

        }

        void OnDestroy() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }
    }
}
