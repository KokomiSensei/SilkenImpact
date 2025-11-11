using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SilkenImpact {
    public class MobHealthBarController : BaseHealthBarController<MobOwnerEvent, MobHealthBarOwner> {
        private string prefabPath = "Assets/Addressables/Prefabs/HealthBarSmall.prefab";
        public override GameObject GetNewHealthBar => Plugin.InstantiateFromAssetsBundle(prefabPath, "MobHealthBar");

        public override Canvas BarCanvas => WorldSpaceCanvas.GetWorldSpaceCanvas;

        protected override float BarWidth(float maxHp) {
            return Configs.Instance.GetHpBarWidth(maxHp, false);
        }

        protected override void OnEnemySpawn(GameObject enemyGO, float maxHp) {
            base.OnEnemySpawn(enemyGO, maxHp);
            var healthBarGO = healthBarGoOf[enemyGO];
            var spriteTracker = healthBarGO.GetComponent<SpriteTracker>();
            spriteTracker.SetTarget(enemyGO);
            spriteTracker.zOffset = Configs.Instance.spriteTrackerZOffset.Value;
        }

#if DEBUG
        void Update() {
            foreach (var kvp in healthBarGoOf) {
                var hm = kvp.Key.GetComponent<HealthManager>();
                var barGO = kvp.Value;
                var bar = barGO.GetComponent<HealthBar>();
                if (hm == null || barGO == null) continue;
                barGO.name = hm.name + "_HealthBar";
                if (bar) bar.name = hm.name + "_HealthBar_Component";
            }
        }

#endif
    }
}
