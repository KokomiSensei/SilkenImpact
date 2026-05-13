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

        #region Exposed Private Thingys
        private static MethodInfo isImmuneToMethod = AccessTools.Method(typeof(HealthManager), "IsImmuneTo");
        private static MethodInfo applyDamageScalingMethod = AccessTools.Method(typeof(HealthManager), "ApplyDamageScaling");
        private static bool IsImmune(HealthManager __instance, HitInstance hitInstance) {
            return (bool)isImmuneToMethod.Invoke(__instance, new object[] { hitInstance, true });
        }
        public static string LocalisedName(HealthManager instance) {
            return BossResolver.BossName(instance) ?? JournalRecordResolver.ResolveLocalizedEnemyName(instance);
        }

        private static HitInstance applyDamageScaling(HealthManager __instance, HitInstance hit) {
            return (HitInstance)applyDamageScalingMethod.Invoke(__instance, new object[] { hit });
        }
        #endregion


        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void HealthManager_Awake_Postfix(HealthManager __instance) {
            PluginLogger.LogInfo($"[HealthManagerPatch][AwakePostfix] enemy={__instance.gameObject.name} hp={__instance.hp}");
            if (SpawnPreventionPolicy.ShouldPreventSpawn(__instance)) {
                PluginLogger.LogInfo($"[HealthManagerPatch][AwakePostfix] HealthBar spawn prevented by SpawnPreventionPolicy: enemy={__instance.gameObject.name} hp={__instance.hp}");
                return;
            }
            SpawnManager.instance.SpawnHealthBar(__instance);
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
            PluginLogger.LogInfo($"[HealthManagerPatch][SendDeathEventPrefix] Calling Die on HealthBarOwner of enemy={__instance.gameObject.name} hp={__instance.hp} isDead={__instance.isDead}");
            __instance.GetComponent<IHealthBarOwner>()?.Die();
        }
        /*
        [HarmonyPatch("HealToMax")]
        [HarmonyPrefix]
        public static void HealthManager_HealToMax_Prefix(HealthManager __instance, ref int __state) {
            __state = __instance.hp;
        }

        [HarmonyPatch("HealToMax")]
        [HarmonyPostfix]
        public static void HealthManager_HealToMax_Postfix(HealthManager __instance, ref int __state) {
            int healAmount = __instance.hp - __state;
            SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Heal(healAmount);
        }
        */
        [HarmonyPatch("RefillHP")]
        [HarmonyPrefix]
        public static void HealthManager_RefillHP_Prefix(HealthManager __instance, ref Tuple<int, int> __state) {
            PluginLogger.LogInfo($"[HealthManagerPatch][RefillHP Prefix] Enqueuing HealEvent for enemy={__instance.gameObject.name} hpBefore={__instance.hp}");
            int currentHP = Mathf.Max(__instance.hp, 0);
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<HealEventArgs>();
            __state = new Tuple<int, int>(currentHP, handle ?? -1);
        }

        [HarmonyPatch("RefillHP")]
        [HarmonyPostfix]
        public static void HealthManager_RefillHP_Postfix(HealthManager __instance, Tuple<int, int> __state) {
            int healAmount = __instance.hp - __state.Item1;
            if (!__instance.isDead)
                DamageTextSpawnUtils.SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new HealEventArgs(healAmount));
            PluginLogger.LogInfo($"[HealthManagerPatch][RefillHP Postfix] Submited HealEvent to Dispatcher for enemy={__instance.gameObject.name}, hpBefore={__state.Item1} hpAfter={__instance.hp} heal={healAmount} isDead={__instance.isDead}");
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPrefix]
        public static void HealthManager_AddHP_Prefix(HealthManager __instance, int hpAdd, int hpMax, ref Tuple<int, int> __state) {
            PluginLogger.LogInfo($"[HealthManagerPatch][AddHP Prefix] Enqueuing HealEvent for enemy={__instance.gameObject.name} hpBefore={__instance.hp}");
            int currentHP = Mathf.Max(__instance.hp, 0);
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<HealEventArgs>();
            __state = new Tuple<int, int>(currentHP, handle ?? -1);
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPostfix]
        public static void HealthManager_AddHP_Postfix(HealthManager __instance, int hpAdd, int hpMax, ref Tuple<int, int> __state) {
            int healAmount = __instance.hp - __state.Item1;
            // TODO: if hp > bar.hpMax? bruh there is no backward reference...
            if (!__instance.isDead)
                DamageTextSpawnUtils.SpawnHealText(__instance, healAmount);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new HealEventArgs(healAmount));
            PluginLogger.LogInfo($"[HealthManagerPatch][AddHP Postfix] Submited HealEvent to Dispatcher for enemy={__instance.gameObject.name}, hpBefore={__state.Item1} hpAfter={__instance.hp} heal={healAmount} isDead={__instance.isDead}");
        }
        #endregion

        #region Damage Patches
        [HarmonyPatch("TakeDamage")]
        [HarmonyPrefix]
        public static void HealthManager_TakeDamage_Prefix(HitInstance hitInstance, HealthManager __instance, ref int __state) {
            PluginLogger.LogDebug($"[HealthManagerPatch][TakeDamagePrefix] enemy={__instance.gameObject.name} hp={__instance.hp} tag={__instance.gameObject.tag} displayName={LocalisedName(__instance)}");
            if (IsImmune(__instance, hitInstance)) {
                PluginLogger.LogInfo($"[HealthManagerPatch][TakeDamagePrefix] Enemy is immune to this hit, skipping DamageEvent enqueue. enemy={__instance.gameObject.name} hp={__instance.hp} tag={__instance.gameObject.tag} displayName={LocalisedName(__instance)}");
                return;
            }
            int? handle = __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<DamageEventArgs>();
            PluginLogger.LogInfo($"[HealthManagerPatch][TakeDamagePrefix] Enqueuing DamageEvent for enemy={__instance.gameObject.name} hpBefore={__instance.hp} handle={handle}");
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
            DamageTextSpawnUtils.SpawnDamageText(__instance, damage, hitInstance.CriticalHit, hitInstance.NailElement);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state, new DamageEventArgs(damage));
            __instance.GetComponent<IHealthBarOwner>()?.CheckHP();
            PluginLogger.LogInfo($"[HealthManagerPatch][TakeDamage] enemy={__instance.gameObject.name} damage={damage} hpAfter={__instance.hp} isDead={__instance.isDead} nailElement={hitInstance.NailElement}");
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
            PluginLogger.LogInfo($"[HealthManagerPatch][ApplyExtraDamageHitInstance] enemy={__instance.gameObject.name} damage={damage} hpAfter={__instance.hp} nailElement={hitInstance.NailElement}");
            DamageTextSpawnUtils.SpawnDamageText(__instance, damage, false, hitInstance.NailElement);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(handle, new DamageEventArgs(damage));
        }


        [HarmonyPatch("ApplyExtraDamage", new[] { typeof(int) })]
        [HarmonyPostfix]
        public static void ApplyExtraDamage2(HealthManager __instance, int damageAmount) {
            int damage = damageAmount;
            PluginLogger.LogInfo($"[HealthManagerPatch][ApplyExtraDamageInt] enemy={__instance.gameObject.name} damage={damage} hpAfter={__instance.hp}");
            DamageTextSpawnUtils.SpawnDamageText(__instance, damage, false);
            __instance.GetComponent<IHealthBarOwner>()?.Dispatcher.EnqueueReady(new DamageEventArgs(damage));
        }

        //TODO stun damage? -> take damage!

        #endregion


    }
}
