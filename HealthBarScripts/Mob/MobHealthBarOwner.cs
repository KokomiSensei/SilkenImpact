using UnityEngine;

namespace SilkenImpact {

    public class MobHealthBarOwner : MonoBehaviour, IHealthBarOwner {
        static private readonly int enemyLayer = LayerMask.NameToLayer("Enemies");

        static private readonly float maxZ = Configs.Instance.maxZPosition.Value;
        static private readonly float visibleCacheTime = Configs.Instance.visibleCacheSeconds.Value;
        static private readonly float invisibleCacheTime = Configs.Instance.invisibleCacheSeconds.Value;
        private HealthManager hm;

        //private Collider2D collider = null;
        private Renderer renderer = null;
        private bool visibilityCache = false;
        private float timeSinceLastCheck = 0f;

        void Awake() {
            //collider = GetComponent<Collider2D>();
            renderer = GetComponent<Renderer>();
            hm = GetComponent<HealthManager>();
        }
        void Update() {
            if (timeSinceLastCheck < (visibilityCache ? visibleCacheTime : invisibleCacheTime)) {
                timeSinceLastCheck += Time.deltaTime;
                return;
            }
            timeSinceLastCheck = 0f;
            bool showHealthBar = mobIsShowing();
            if (showHealthBar == visibilityCache)
                return;
            visibilityCache = showHealthBar;
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.CheckHP, gameObject);
            EventHandle<MobOwnerEvent>.SendEvent(showHealthBar ? HealthBarOwnerEventType.Show : HealthBarOwnerEventType.Hide, gameObject);

        }
        private bool mobIsShowing() {
            if (!hm.isActiveAndEnabled || hm.isDead)
                return false;

            if (gameObject.layer != enemyLayer)
                return false;

            //Plugin.Logger.LogInfo("1. Layer Passed"); // 1600 / 2600
            if (Mathf.Abs(transform.position.z) > maxZ) {
                return false;
            }
            //Plugin.Logger.LogInfo("2. Z Pos Passed"); // 500 / 1600
            // some mobs use a capsule collider on their 'physics pusher' instead
            //if (collider && (!collider.enabled || !collider.isActiveAndEnabled)) 
            //return false;

            //Plugin.Logger.LogInfo("3. Collider Passed"); 223 / 500
            if (renderer && (!renderer.enabled || !renderer.isVisible))
                return false;

            //Plugin.Logger.LogInfo("4. Renderer Passed"); 3 / 223
            return true;
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
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Heal, gameObject, amount);
        }

        public void TakeDamage(float amount) {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Damage, gameObject, amount);
        }

        public void SetHP(float hp) {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject, hp);
        }

        public void Die() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, gameObject);
        }

        public void Hide() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, gameObject);
        }

        public void Show() {
            EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Show, gameObject);
        }
    }
}
