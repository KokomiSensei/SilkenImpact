using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static HealthManager;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(HealthManager))]
    public class HealthManagerPatch {

        #region Helpers
        public static float avgDamagePerHit = -1;
        public static float weightOfNew => Configs.Instance.weightOfNewHit.Value;
        private static void updateAvgDamagePerHit(float damage) {
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
        private static void spawnTextOn(HealthManager __instance, GameObject textGO, string content, float horizontalOffsetScale, float verticalOffsetScale, float sizeScale, Color color) {
            textGO.name = $"{content} -> {__instance.gameObject.name}";
            var renderer = __instance.gameObject.GetComponent<Renderer>();
            Vector3 spriteSize = renderer ? renderer.bounds.size : new Vector3(1, 1, 0);

            Vector3 randomOffset = spriteSize;
            randomOffset.x *= horizontalOffsetScale / 2;
            randomOffset.y *= verticalOffsetScale / 2;

            textGO.transform.position = __instance.gameObject.transform.position + randomOffset;
            textGO.transform.SetParent(WorldSpaceCanvas.GetWorldSpaceCanvas.transform, true);

            var text = textGO.GetComponent<DamageText>();
            text.DamageString = content;
            text.TextColor = color;
            text.maxHeight *= sizeScale;
            text.maxWidth *= sizeScale;
        }
        public static void SpawnDamageText(HealthManager __instance, float damage, bool isCritHit, NailElements element = NailElements.None, Color? color = null) {
            if (damage <= 0) {
                PluginLogger.LogWarning($"SpawnDamageText called with non-positive damage: {damage}");
                return;
            }
            // TODO scale up the text
            float horizontalOffsetScale = UnityEngine.Random.Range(-1f, 1f);
            float verticalOffsetScale = UnityEngine.Random.Range(-0.4f, 0.7f);

            // scale /= transform.lossyScale.x; ?
            float randomSizeScale = UnityEngine.Random.Range(1f, 1.1f) * (isCritHit ? Mathf.Clamp(damageScale(damage), 2, 2.5f) : Mathf.Clamp(damageScale(damage), 0.5f, 1.5f));
            updateAvgDamagePerHit(damage);
            var textGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/DamageOldText.prefab", "DamageText");
            spawnTextOn(__instance, textGO, ((int)damage).ToString(), horizontalOffsetScale, verticalOffsetScale, randomSizeScale, color ?? textColor(element, isCritHit));
        }

        public static void SpawnHealText(HealthManager __instance, float amount, Color? color = null) {
            if (amount <= 0) {
                PluginLogger.LogWarning($"SpawnHealText called with non-positive amount: {amount}");
                return;
            }
            float horizontalOffsetScale = UnityEngine.Random.Range(-0.5f, 0.5f);
            float verticalOffsetScale = UnityEngine.Random.Range(0, 0.8f);

            float randomSizeScale = UnityEngine.Random.Range(1.5f, 1.8f);
            var textGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealOldText.prefab", "HealText");
            spawnTextOn(__instance, textGO, $"+{(int)amount}", horizontalOffsetScale, verticalOffsetScale, randomSizeScale, color ?? Configs.Instance.healTextColor.Value);
        }

        #endregion


        #region Exposed Private Thingys
        private static MethodInfo isImmuneToMethod = AccessTools.Method(typeof(HealthManager), "IsImmuneTo");
        private static MethodInfo applyDamageScalingMethod = AccessTools.Method(typeof(HealthManager), "ApplyDamageScaling");
        private static bool IsImmune(HealthManager __instance, HitInstance hitInstance) {
            return (bool)isImmuneToMethod.Invoke(__instance, new object[] { hitInstance, true });
        }
        public static string LocalisedName(HealthManager __instance) {
            var em = __instance.gameObject.GetComponent<EnemyDeathEffects>();
            // Approach 1: __instance.GetComponent<EnemyDeathEffects>().journalRecord.DisplayName
            if (em != null) {
                var journalRecord = Traverse.Create(em).Field<EnemyJournalRecord>("journalRecord").Value;
                if (journalRecord) {
                    PluginLogger.LogWarning("Approach 1 Succeeded. Using: [__instance.GetComponent<EnemyDeathEffects>().journalRecord.DisplayName]");
                    PluginLogger.LogWarning("Found EnemyDeathEffects on " + __instance.gameObject.name + ", Boss Name: " + journalRecord.DisplayName);
                    return journalRecord.DisplayName;
                }
                PluginLogger.LogWarning("Approach 1 Failed. journalRecord not found on " + __instance.gameObject.name);
            }

            // Approach 2:
            PluginLogger.LogWarning("Approach 1 Failed. EnemyDeathEffects or journalRecord not found on " + __instance.gameObject.name);

            //return $"Enemy Death Effects Not Found on {__instance.gameObject.name}";

            // Great thanks to 小海 for developing the following fallback methods in (https://github.com/jcx515250418qq/Silksong_HealthBar)
            /*  Author | 作者
                Name | 姓名: Xiaohai 小海
                Email | 邮箱: 515250418@qq.com
                Bilibili | B站: https://space.bilibili.com/2055787437
             */
            // 方法2: 尝试通过EnemyJournalManager中的敌人记录匹配名称
            var allEnemies = EnemyJournalManager.GetAllEnemies();
            string gameObjectName = __instance.gameObject.name;
            string[] gameObjectWords = gameObjectName.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var enemy in allEnemies) {
                // enemy.name和healthManager.gameObject.name的格式一般都是  AAA BBB CCC(也许空格分割开的多段名字  但是段数不确定)
                //只要其中有两段字符匹配就算匹配成功. 比如healthManager.gameObject.name是 Moss Bone Mother 然后 allEnemies中有一个是Moss Mother,也视为匹配成功
                //匹配成功后取其enemy.DisplayName作为Boss名称 

                if (enemy == null || string.IsNullOrEmpty(enemy.name))
                    continue;

                string[] enemyWords = enemy.name.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

                // 计算匹配的单词数量
                int matchCount = 0;
                foreach (string gameWord in gameObjectWords) {
                    if (string.IsNullOrEmpty(gameWord) || gameWord.Length < 2)
                        continue;

                    foreach (string enemyWord in enemyWords) {
                        if (string.IsNullOrEmpty(enemyWord) || enemyWord.Length < 2)
                            continue;

                        // 忽略大小写进行比较
                        if (string.Equals(gameWord, enemyWord, System.StringComparison.OrdinalIgnoreCase)) {
                            matchCount++;
                            break; // 找到匹配后跳出内层循环
                        }
                    }
                }

                // 如果匹配了至少2个单词，认为是匹配成功
                if (matchCount >= 2 && !string.IsNullOrEmpty(enemy.DisplayName)) {
                    PluginLogger.LogInfo($"通过EnemyJournalManager匹配到Boss名称: {enemy.DisplayName} (匹配单词数: {matchCount})");
                    return enemy.DisplayName;
                }
            }

            // 如果没有找到匹配度>=2的，尝试匹配度为1的作为备选
            foreach (var enemy in allEnemies) {
                if (enemy == null || string.IsNullOrEmpty(enemy.name))
                    continue;

                string[] enemyWords = enemy.name.Split(new char[] { ' ', '(', ')', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (string gameWord in gameObjectWords) {
                    if (string.IsNullOrEmpty(gameWord) || gameWord.Length < 3) // 单个匹配时要求更长的单词
                        continue;

                    foreach (string enemyWord in enemyWords) {
                        if (string.IsNullOrEmpty(enemyWord) || enemyWord.Length < 3)
                            continue;

                        if (string.Equals(gameWord, enemyWord, System.StringComparison.OrdinalIgnoreCase)) {
                            if (!string.IsNullOrEmpty(enemy.DisplayName)) {
                                PluginLogger.LogInfo($"通过EnemyJournalManager单词匹配到Boss名称: {enemy.DisplayName} (匹配单词: {gameWord})");
                                return enemy.DisplayName;
                            }
                        }
                    }
                }
            }

            string CleanGameObjectName(string originalName) {
                if (string.IsNullOrEmpty(originalName))
                    return "未知Boss";

                // 移除括号及其内容
                int bracketIndex = originalName.IndexOf('(');
                if (bracketIndex >= 0) {
                    originalName = originalName.Substring(0, bracketIndex).Trim();
                }

                // 移除常见的后缀
                string[] suffixesToRemove = { " Clone", "(Clone)", " Instance", "(Instance)" };
                foreach (string suffix in suffixesToRemove) {
                    if (originalName.EndsWith(suffix)) {
                        originalName = originalName.Substring(0, originalName.Length - suffix.Length).Trim();
                    }
                }

                return string.IsNullOrEmpty(originalName) ? "未知Boss" : originalName;
            }
            return CleanGameObjectName(__instance.gameObject.name);
            // End of 小海's codes
        }



        private static HitInstance applyDamageScaling(HealthManager __instance, HitInstance hit) {
            return (HitInstance)applyDamageScalingMethod.Invoke(__instance, new object[] { hit });
        }


        #endregion


        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void HealthManager_Awake_Postfix(HealthManager __instance) {
            PluginLogger.LogInfo($"{__instance.gameObject.name}.Health Manager Awoken, hp -> {__instance.hp}");
            if (SpawnPreventionPolicy.ShouldPreventSpawn(__instance)) {
                PluginLogger.LogInfo($"Preventing health bar spawn for {__instance.gameObject.name} with hp {__instance.hp}");
                return;
            }

            float hp = __instance.hp;
            var go = __instance.gameObject;
            bool isBoss = false;

            if (__instance.CompareTag("Boss")) {
                // This only works for some of the bosses in Act 1.
                // I would guess that Team Cherry forgot to tag some of the later bosses?
                isBoss = true;
            }
            if (hp >= Configs.Instance.minBossBarHp.Value) {
                // So sadly we need this heurisitic approach as fallback.
                isBoss = true;
            }
            if (!isBoss) {
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, go);
            } else {
                EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                EventHandle<BossOwnerEvent>.SendEvent(HealthBarOwnerEventType.Hide, go);
            }
        }


        #region Cursed Patches
        /* 
         * 1. 
         * HealToMax 和 RefillHP: hp = initHp
         * AddHp(hpAdd, hpMax): hp = min(hp + hpAdd, hpMax) 理论上说可以让hp超过initHp (wtf)
         * initHp是永远不变的
         * 
         * 2.
         * 有的（很多）时候会不走这几个接口，转而直接调整hp，气笑了
         * 
         * 3.
         * HealToMax 只在 Awake 和 HutongGames.PlayMaker.Actions.AddHP.OnEnter() 调用
         * AddHp 会把 isDead 置为 false 【 wait，无人调用说是？】
         * RefillHP 也会把 isDead 置为 false
         * 
         */
        [HarmonyPatch("SendDeathEvent")]
        [HarmonyPrefix]
        public static void HealthManager_SendDeathEvent_Prefix(HealthManager __instance) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} Die, hp -> {__instance.hp}, isDead -> {__instance.isDead}");
            __instance.GetComponent<IHealthBarOwner>()?.Die();
        }
        /*
        [HarmonyPatch("HealToMax")]
        [HarmonyPrefix]
        public static void HealthManager_HealToMax_Prefix(HealthManager __instance, ref int __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} HealToMax, hp -> {__instance.hp}");
            __state = __instance.hp;
        }

        [HarmonyPatch("HealToMax")]
        [HarmonyPostfix]
        public static void HealthManager_HealToMax_Postfix(HealthManager __instance, ref int __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} HealToMax, hp -> {__instance.hp}");
            int healAmount = __instance.hp - __state;
            SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Heal(healAmount);
        }
        */
        [HarmonyPatch("RefillHP")]
        [HarmonyPrefix]
        public static void HealthManager_RefillHP_Prefix(HealthManager __instance, ref Tuple<int, int> __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} RefillHP, hp -> {__instance.hp}");
            int currentHP = Mathf.Max(__instance.hp, 0);
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<HealEventArgs>();
            __state = new Tuple<int, int>(currentHP, handle ?? -1);
        }

        [HarmonyPatch("RefillHP")]
        [HarmonyPostfix]
        public static void HealthManager_RefillHP_Postfix(HealthManager __instance, Tuple<int, int> __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} RefillHP, hp -> {__instance.hp}");
            int healAmount = __instance.hp - __state.Item1;
            if (!__instance.isDead)
                SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new HealEventArgs(healAmount));
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPrefix]
        public static void HealthManager_AddHP_Prefix(HealthManager __instance, int hpAdd, int hpMax, ref Tuple<int, int> __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} AddHP, hp -> {__instance.hp}");
            int currentHP = Mathf.Max(__instance.hp, 0);
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<HealEventArgs>();
            __state = new Tuple<int, int>(currentHP, handle ?? -1);
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPostfix]
        public static void HealthManager_AddHP_Postfix(HealthManager __instance, int hpAdd, int hpMax, ref Tuple<int, int> __state) {
            PluginLogger.LogWarning($"{__instance.gameObject.name} AddHP, hp -> {__instance.hp}");
            int healAmount = __instance.hp - __state.Item1;
            // TODO: if hp > bar.hpMax? bruh there is no backward reference...
            if (!__instance.isDead)
                SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new HealEventArgs(healAmount));
        }
        #endregion

        #region Damage Patches
        [HarmonyPatch("TakeDamage")]
        [HarmonyPrefix]
        public static void HealthManager_TakeDamage_Prefix(HitInstance hitInstance, HealthManager __instance, ref int __state) {
            PluginLogger.LogInfo($"{__instance.gameObject.name} is hit, hp -> {__instance.hp}");
            PluginLogger.LogWarning($"{__instance.gameObject.name}, Tag={__instance.gameObject.tag}, DisplayName={LocalisedName(__instance)}");
            if (IsImmune(__instance, hitInstance)) {
                return;
            }
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<DamageEventArgs>();
            __state = handle ?? -1;
        }


        [HarmonyPatch("TakeDamage")]
        [HarmonyPostfix]
        public static void HealthManager_TakeDamage_Postfix(HitInstance hitInstance, HealthManager __instance, ref int __state) {
            if (IsImmune(__instance, hitInstance)) {
                return;
            }
            // this is how the damage is calculated in HealthManager.TakeDamage
            // #TODO BUT, there may be more than one damage source, so be cautious
            // must calculate damage in Postfix, as hitInstance may be modified in TakeDamage() due to critical hit 
            int damage = Mathf.RoundToInt((float)hitInstance.DamageDealt * hitInstance.Multiplier);
            SpawnDamageText(__instance, damage, hitInstance.CriticalHit, hitInstance.NailElement);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state, new DamageEventArgs(damage));
            __instance.GetComponent<IHealthBarOwner>()?.CheckHP();

            PluginLogger.LogWarning($"TakeDamagePostfix: {__instance.gameObject.name}.isDead = {__instance.isDead}");
            PluginLogger.LogInfo($"{__instance.gameObject.name} took {damage} damage, hp -> {__instance.hp}");
            PluginLogger.LogWarning($"Neil=[{hitInstance.NailElement}]");
        }


        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(HitInstance) })]
        [HarmonyPrefix]
        public static void ApplyExtraDamage1_Pre(HealthManager __instance, HitInstance hitInstance, ref Tuple<int, int> __state) {
            hitInstance = applyDamageScaling(__instance, hitInstance);
            int newHp = Mathf.Max(__instance.hp - hitInstance.DamageDealt, -1000);
            int damage = __instance.hp - newHp;
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<DamageEventArgs>();
            __state = new Tuple<int, int>(damage, handle ?? -1);
        }

        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(HitInstance) })]
        [HarmonyPostfix]
        public static void ApplyExtraDamage1_Post(HealthManager __instance, HitInstance hitInstance, ref Tuple<int, int> __state) {
            int damage = __state.Item1;
            int handle = __state.Item2;
            PluginLogger.LogInfo($"{__instance.gameObject.name} took {damage} tag damage, hp -> {__instance.hp}");
            PluginLogger.LogWarning($"Neil=[{hitInstance.NailElement}]");
            SpawnDamageText(__instance, damage, false, hitInstance.NailElement);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(handle, new DamageEventArgs(damage));
        }


        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(int) })]
        [HarmonyPostfix]
        public static void ApplyExtraDamage2(HealthManager __instance, int damageAmount) {
            int damage = damageAmount;
            PluginLogger.LogInfo($"{__instance.gameObject.name} took {damage} tag damage, hp -> {__instance.hp}");
            SpawnDamageText(__instance, damage, false);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.EnqueueReady(new DamageEventArgs(damage));
        }

        //TODO stun damage? -> take damage!

        #endregion


    }
}
