using UnityEngine;

namespace SilkenImpact {
    public class HealthBarOwner : MonoBehaviour {
        // Start is called before the first frame update
        public float minMobHealth = 10;
        public float minBossHealth = 100;
        private bool spawned = false;
        void Start() {
#if !UNITY_EDITOR
            var hm = GetComponent<HealthManager>();
            float hp = hm.hp;
            if (hp < minMobHealth) {
                Destroy(this);
                return;
            }
            spawned = true;
            if (hp < minBossHealth) {
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, gameObject, hp);
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
            } else {
                //TODO boss thingy
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, gameObject, hp);
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
            }
#endif
        }
        bool shown = false;
        // Update is called once per frame
        void Update() {

        }

        void OnDestroy() {
            if (!spawned) return;
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }
    }
}
