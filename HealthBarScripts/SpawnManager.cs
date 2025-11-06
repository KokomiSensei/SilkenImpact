using System;
using System.Net;
using UnityEngine;

namespace SilkenImpact {
    public class SpawnManager : Singleton<SpawnManager> {
        public bool IsBoss(HealthManager hm, float? overrideHp = null) {
            if (hm.CompareTag("Boss")) {
                // This only works for some of the bosses in Act 1.
                // I would guess that Team Cherry forgot to tag some of the later bosses?

                // Update on 2025/11/5. Turned out that some regular enemy are also tagged as "Boss", e.g., the enemies summoned by "The last conductor".
                return true;
            }

            float hp = overrideHp ?? hm.hp;
            if (hp >= Configs.Instance.minBossBarHp.Value) {
                // So sadly we need this heurisitic approach as fallback.
                return true;
            }
            return false;
        }
        private void Link(HealthManager origin, HealthManager relay) {
            float? overrideHp = LinkPolicy.Instance.GetOverrideHpIfAny(origin);
            bool isBoss = IsBoss(origin, overrideHp);
            // TODO 
            // WARNING: isBoss calculated here may be different from the one calculated when origin was spawned, because the user may have changed the config in between.
            // But we will stick with this design for now.
            if (isBoss) {
                EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Link, origin.gameObject, relay.gameObject);
            } else {
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Link, origin.gameObject, relay.gameObject);
            }
        }

        private void spawnHealthBar(HealthManager hm, float? overrideHp = null) {
            float hp = overrideHp ?? hm.hp;
            if (IsBoss(hm, overrideHp)) {
                EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, hm.gameObject, hp);
                EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, hm.gameObject);
            } else {
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, hm.gameObject, hp);
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, hm.gameObject);
            }
        }

        public void SpawnHealthBar(HealthManager hm) {
            if (LinkPolicy.Instance.TryGetOriginHealthManager(hm, out var originHm)) {
                if (originHm == null) {
                    PluginLogger.LogError($"[SpawnManager] Failed to get origin endpoint for relay endpoint {hm.gameObject.name}");
                    return;
                }
                Link(originHm, hm);
                return;
            }
            float? overrideHp = LinkPolicy.Instance.GetOverrideHpIfAny(hm);
            spawnHealthBar(hm, overrideHp);
        }
    }
}