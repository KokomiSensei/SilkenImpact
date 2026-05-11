using UnityEngine;

namespace SilkenImpact {
    public static class DamageTextSpawnUtils {
        public static float avgDamagePerHit = -1;
        public static float weightOfNew => Configs.Instance.weightOfNewHit.Value;

        /// <summary>
        /// Spawns damage/heal text on the specified HealthManager.
        /// Parameters:
        /// - horizontalOffsetScale / verticalOffsetScale:
        ///     -1 corresponds to the sprite's left/bottom edge, 0 to the sprite center, 1 to the right/top edge.
        /// - sizeScale: scale factor for text size
        /// Calculation:
        /// Uses renderer.bounds.size as the base size, multiplies by offsetScale / 2 to compute the offset,
        /// and positions the text in world space at the target position plus that offset.
        /// </summary>
        private static void spawnTextOn(HealthManager hm, GameObject textGO, string content, float horizontalOffsetScale, float verticalOffsetScale, float sizeScale, Color color) {
            textGO.name = $"{content} -> {hm.gameObject.name}";
            var renderer = hm.gameObject.GetComponent<Renderer>();
            Vector3 spriteSize = renderer ? renderer.bounds.size : new Vector3(1, 1, 0);

            Vector3 randomOffset = spriteSize;
            randomOffset.x *= horizontalOffsetScale / 2;
            randomOffset.y *= verticalOffsetScale / 2;

            textGO.transform.position = hm.gameObject.transform.position + randomOffset;
            textGO.transform.SetParent(WorldSpaceCanvas.GetWorldSpaceCanvas.transform, true);

            var text = textGO.GetComponent<DamageText>();
            text.DamageString = content;
            text.TextColor = color;
            text.BaseScale *= sizeScale;
            text.TextFont = FontManager.instance.DamageFontLoader.Load();
        }

        public static void SpawnDamageText(HealthManager hm, float damage, bool isCritHit, NailElements element = NailElements.None, Color? color = null) {
            if (damage <= 0) {
                PluginLogger.LogWarning($"[DamageTextSpawnUtils][SpawnDamageText][Skip] enemy={hm?.gameObject?.name} damage={damage}. Refusing Text Spawn.");
                return;
            }
            if (!Configs.Instance.displayDamageText.Value) {
                return;
            }
            float horizontalOffsetScale = Random.Range(-1f, 1f);
            float verticalOffsetScale = Random.Range(-0.4f, 0.7f);

            float randomSizeScale = Random.Range(1f, 1.1f) * (isCritHit ? Mathf.Clamp(damageScale(damage), 2, 2.5f) : Mathf.Clamp(damageScale(damage), 0.5f, 1.5f));
            randomSizeScale *= Configs.Instance.damageFontSizeScaler.Value;
            updateAvgDamagePerHit(damage);
            var textGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/DamageOldText.prefab", "DamageText");
            spawnTextOn(hm, textGO, ((int)damage).ToString(), horizontalOffsetScale, verticalOffsetScale, randomSizeScale, color ?? textColor(element, isCritHit));
        }

        public static void SpawnHealText(HealthManager hm, float amount, Color? color = null) {
            if (amount <= 0) {
                PluginLogger.LogWarning($"[DamageTextSpawnUtils][SpawnHealText][Skip] enemy={hm?.gameObject?.name} amount={amount}. Refusing Text Spawn.");
                return;
            }
            if (!Configs.Instance.displayHealText.Value) {
                return;
            }
            float horizontalOffsetScale = Random.Range(-0.5f, 0.5f);
            float verticalOffsetScale = Random.Range(0, 0.8f);

            float randomSizeScale = Random.Range(1.5f, 1.8f);
            randomSizeScale *= Configs.Instance.damageFontSizeScaler.Value;
            var textGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealOldText.prefab", "HealText");
            spawnTextOn(hm, textGO, $"+{(int)amount}", horizontalOffsetScale, verticalOffsetScale, randomSizeScale, color ?? Configs.Instance.healTextColor.Value);
        }


        private static void updateAvgDamagePerHit(float damage) {
            if (damage <= 0 || damage >= 9999) {
                return;
            }
            if (avgDamagePerHit <= 0) {
                avgDamagePerHit = damage;
                return;
            }
            avgDamagePerHit = weightOfNew * damage + (1 - weightOfNew) * avgDamagePerHit;
        }

        private static float damageScale(float damage) {
            if (avgDamagePerHit <= 0) return 1;
            float p = damage / avgDamagePerHit;
            return 0.5f + 0.5f * Mathf.Pow(p, 0.5f);
        }
        private static Color textColor(NailElements element, bool isCritHit) {
            return element switch {
                NailElements.Poison => Configs.Instance.poisonColor.Value,
                NailElements.Fire => Configs.Instance.fireColor.Value,
                _ => isCritHit ? Configs.Instance.critHitColor.Value : Configs.Instance.defaultColor.Value,
            };
        }
    }
}
