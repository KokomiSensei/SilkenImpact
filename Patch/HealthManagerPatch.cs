using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Reflection;
using UnityEngine;
using static HealthManager;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(HealthManager))]
    public class HealthManagerPatch {

        #region Helpers
        //public static readonly float minMobHealth = 10;
        //public static readonly float minBossHealth = 100;
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
            return damage / avgDamagePerHit;
        }
        private static Color textColor(NailElements element, bool isCritHit) {
            return element switch {
                NailElements.Poison => Configs.Instance.poisonColor.Value,
                NailElements.Fire => Configs.Instance.fireColor.Value,
                _ => isCritHit ? Configs.Instance.critHitColor.Value : Configs.Instance.defaultColor.Value,
            };
        }

        public static void SpawnDamageText(HealthManager __instance, float damage, bool isCritHit, NailElements element = NailElements.None, Color? color = null) {
            if (damage <= 0) {
                Plugin.Logger.LogWarning($"SpawnDamageText called with non-positive damage: {damage}");
                return;
            }
            var damageTextGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/DamageOldText.prefab", $"{__instance}.DamageText = {damage}");
            var renderer = __instance.gameObject.GetComponent<Renderer>();
            Vector3 spriteSize = renderer ? renderer.bounds.size : new Vector3(1, 1, 0);

            Vector3 randomOffset = spriteSize;
            randomOffset.x *= Random.Range(-0.5f, 0.5f);
            randomOffset.y *= Random.Range(-0.2f, 0.5f);

            float randomSizeScale = Random.Range(0.8f, 1.2f) * (isCritHit ? Mathf.Clamp(damageScale(damage), 2, 2.5f) : Mathf.Clamp(damageScale(damage), 0.5f, 1.5f));
            updateAvgDamagePerHit(damage);

            damageTextGO.transform.position = __instance.gameObject.transform.position + randomOffset;
            damageTextGO.transform.SetParent(WorldSpaceCanvas.GetWorldSpaceCanvas.transform, true);

            var text = damageTextGO.GetComponent<DamageText>();
            text.DamageString = ((int)damage).ToString();
            text.TextColor = color ?? textColor(element, isCritHit);
            text.maxHeight *= randomSizeScale;
            text.maxWidth *= randomSizeScale;
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
            if (em == null) {
                return $"Enemy Death Effects Not Found on {__instance.gameObject.name}";
            }
            var journalRecord = Traverse.Create(em).Field<EnemyJournalRecord>("journalRecord").Value;
            if (journalRecord) {
                return journalRecord.DisplayName;
            }

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
                    Plugin.Logger.LogInfo($"通过EnemyJournalManager匹配到Boss名称: {enemy.DisplayName} (匹配单词数: {matchCount})");
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
                                Plugin.Logger.LogInfo($"通过EnemyJournalManager单词匹配到Boss名称: {enemy.DisplayName} (匹配单词: {gameWord})");
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
            Plugin.Logger.LogInfo($"{__instance.gameObject.name}.Health Manager Awoken, hp -> {__instance.hp}");

            float hp = __instance.hp;
            if (hp < Configs.Instance.minMobHp.Value) {
                return;
            }

            var go = __instance.gameObject;
            bool isBoss = hp >= Configs.Instance.minBossBarHp.Value;
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
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} Die, hp -> {__instance.hp}, isDead -> {__instance.isDead}");
            Object.Destroy(__instance.gameObject.GetComponent<MobHealthBarOwner>());
        }

        [HarmonyPatch("HealToMax")]
        [HarmonyPostfix]
        public static void HealthManager_HealToMax_Postfix(HealthManager __instance) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} HealToMax, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            __instance.GetComponent<IHealthBarOwner>()?.Heal(maxHp);
        }

        [HarmonyPatch("RefillHP")]
        [HarmonyPostfix]
        public static void HealthManager_RefillHP_Postfix(HealthManager __instance) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} RefillHP, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            __instance.GetComponent<IHealthBarOwner>()?.Heal(maxHp);
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPostfix]
        public static void HealthManager_AddHP_Postfix(HealthManager __instance, int hpAdd, int hpMax) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} AddHP, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            __instance.GetComponent<IHealthBarOwner>()?.Heal(maxHp);
        }
        #endregion

        #region Damage Patches
        [HarmonyPatch("TakeDamage")]
        [HarmonyPrefix]
        public static void HealthManager_TakeDamage_Prefix(HitInstance hitInstance, HealthManager __instance) {
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} is hit, hp -> {__instance.hp} ⬇");
            Plugin.Logger.LogWarning($"{__instance.gameObject.name}, Tag={__instance.gameObject.tag}, DisplayName={LocalisedName(__instance)}");
        }


        [HarmonyPatch("TakeDamage")]
        [HarmonyPostfix]
        public static void HealthManager_TakeDamage_Postfix(HitInstance hitInstance, HealthManager __instance) {
            if (IsImmune(__instance, hitInstance)) {
                return;
            }
            // this is how the damage is calculated in HealthManager.TakeDamage
            // #TODO BUT, there may be more than one damage source, so be cautious
            int damage = Mathf.RoundToInt((float)hitInstance.DamageDealt * hitInstance.Multiplier);
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} took {damage} damage, hp -> {__instance.hp}");
            SpawnDamageText(__instance, damage, hitInstance.CriticalHit, hitInstance.NailElement);
            Plugin.Logger.LogWarning($"Neil=[{hitInstance.NailElement}]");
            __instance.GetComponent<IHealthBarOwner>()?.TakeDamage(damage);
        }


        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(HitInstance) })]
        [HarmonyPostfix]
        public static void ApplyExtraDamage1(HealthManager __instance, HitInstance hitInstance) {
            int damage = applyDamageScaling(__instance, hitInstance).DamageDealt;
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} took {damage} tag damage, hp -> {__instance.hp}");
            Plugin.Logger.LogWarning($"Neil=[{hitInstance.NailElement}]");
            SpawnDamageText(__instance, damage, false, hitInstance.NailElement);
            __instance.GetComponent<IHealthBarOwner>()?.TakeDamage(damage);
        }


        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(int) })]
        [HarmonyPostfix]
        public static void ApplyExtraDamage2(HealthManager __instance, int damageAmount) {
            int damage = damageAmount;
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} took {damage} tag damage, hp -> {__instance.hp}");
            SpawnDamageText(__instance, damage, false);
            __instance.GetComponent<IHealthBarOwner>()?.TakeDamage(damage);
        }

        //TODO stun damage? -> take damage!

        #endregion


    }
}
