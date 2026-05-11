using System.Collections.Generic;
using UnityEngine;

namespace SilkenImpact {

    class DeathWatcher : MonoBehaviour {
        public static HashSet<string> IngameGameObjectNames = new HashSet<string> {
            "garmond fighter", // TODO The map guy?
            // "shakra" 
        };
        private HealthManager hm;
        private IHealthBarOwner hpBarOwner;

        void Start() {
            float intervalSeconds = Configs.Instance.visibleCacheSeconds.Value;
            InvokeRepeating(nameof(CheckHealth), 0f, intervalSeconds);
        }

        private void CheckHealth() {
            if (hm && hm.hp <= 0) {
                PluginLogger.LogInfo($"[DeathWatcher][CheckHealth][DieDetected] enemy={hm.name} hp={hm.hp} Canceling further checks, calling Die() on HealthbarOwner and destroying self (DeathWatcher).");
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
