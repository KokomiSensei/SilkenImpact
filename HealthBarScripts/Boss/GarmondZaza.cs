using System;
using UnityEngine;

namespace SilkenImpact {

    class GarmondAndZaza : MonoBehaviour {
        public static string IngameGameObjectName = "garmond fighter";
        private HealthManager hm;
        private IHealthBarOwner hpBarOwner;

        void Start() {
            float intervalSeconds = Configs.Instance.visibleCacheSeconds.Value;
            InvokeRepeating(nameof(CheckHealth), 0f, intervalSeconds);
        }

        private void CheckHealth() {
            if (hm && hm.hp <= 0) {
                PluginLogger.LogInfo($"{hm.name}.hp = {hm.hp} <= 0, Sending Die Event");
                hpBarOwner?.Die();
                CancelInvoke(nameof(CheckHealth)); // Stop checking after death
                Destroy(this);
            }
        }

        internal void Init(GameObject enemyGO) {
            hm = enemyGO.GetComponent<HealthManager>();
            hpBarOwner = enemyGO.GetComponent<IHealthBarOwner>();
        }
    }
}