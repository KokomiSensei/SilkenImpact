using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using UnityEngine;


namespace SilkenImpact.Patch {

    [HarmonyPatch]
    public class PlayMakerPatch {
        public class AddHpArg {
            public int handle;
            public int hpInPrefix;
            public bool isDeadInPrefix;
        }

        // I have seen it called on Grand Silk Mother.
        [HarmonyPatch(typeof(AddHP))] // HealToMax or += AddHp.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPrefix]
        public static void AddHP_OnEnter_Prefix(AddHP __instance, ref AddHpArg __state) {
            var go = __instance.target.GetSafe(__instance);
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            PluginLogger.LogDebug($"[PlayMakerPatch][AddHP] OnEnter Prefix called. enemy={go.name}");
            if (hm) {
                int currentHP = Mathf.Max(hm.hp, 0);
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<SetHpEventArgs>() ?? -1;
                __state = new AddHpArg {
                    handle = handle,
                    hpInPrefix = currentHP,
                    isDeadInPrefix = hm.isDead
                };
            }
        }

        [HarmonyPatch(typeof(AddHP))] // HealToMax or += AddHp.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void AddHP_OnEnter_Postfix(AddHP __instance, ref AddHpArg __state) {
            var go = __instance.target.GetSafe(__instance);
            PluginLogger.LogDebug($"[PlayMakerPatch][AddHP] OnEnter Postfix called. enemy={go.name}");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.handle, new SetHpEventArgs(hp));

                if (!__state.isDeadInPrefix && !hm.isDead) {
                    float amount = hp - __state.hpInPrefix;
                    DamageTextSpawnUtils.SpawnHealText(hm, amount);
                }
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                PluginLogger.LogInfo($"[PlayMakerPatch][AddHP] enemy={go.name} hpBefore={__state.hpInPrefix} hpAfter={hp} isDeadBefore={__state.isDeadInPrefix} isDeadAfter={hm.isDead}");
            }
        }


        /* 
         * 事实证明这个SetHP就是罪魁祸首，不走HealthManger API，直接修改血量
         * 
         */
        [HarmonyPatch(typeof(SetHP))] // hp += SetHP.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPrefix]
        public static void SetHP_OnEnter_Prefix(SetHP __instance, ref Tuple<int, int> __state) {
            var go = __instance.target.GetSafe(__instance);
            PluginLogger.LogDebug($"[PlayMakerPatch][SetHP] OnEnter Prefix called. enemy={go.name}");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                int currentHP = Mathf.Max(hm.hp, 0);
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<SetHpEventArgs>() ?? -1;
                __state = new Tuple<int, int>(currentHP, handle);
            }
        }

        /* 
         * 事实证明这个SetHP就是罪魁祸首，不走HealthManger API，直接修改血量
         * 
         */
        [HarmonyPatch(typeof(SetHP))] // hp += SetHP.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void SetHP_OnEnter_Postfix(SetHP __instance, ref Tuple<int, int> __state) {
            var go = __instance.target.GetSafe(__instance);
            PluginLogger.LogDebug($"[PlayMakerPatch][SetHP] OnEnter Postfix called. enemy={go.name}");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                float amount = hp - __state.Item1;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new SetHpEventArgs(hp));
                if (!hm.isDead)
                    DamageTextSpawnUtils.SpawnHealText(hm, amount);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                PluginLogger.LogInfo($"[PlayMakerPatch][SetHP] enemy={go.name} hpBefore={__state.Item1} hpAfter={hp} isDead={hm.isDead}");
            }
        }

        [HarmonyPatch(typeof(SubtractHP))] // hp -= amount.Value;
        [HarmonyPatch("OnEnter")]
        [HarmonyPrefix]
        public static void SubtractHP_OnEnter_Prefix(SubtractHP __instance) {
            var go = __instance.target.GetSafe(__instance);
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                int damage = __instance.amount.Value;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.EnqueueReady(new DamageEventArgs(damage));
                PluginLogger.LogDebug($"[PlayMakerPatch][SubtractHP] enemy={go.name} damage={damage}");
            }
        }


        // [HarmonyPatch(typeof(SubtractHP))] // hp -= amount.Value;
        // [HarmonyPatch("OnEnter")]
        // [HarmonyPostfix]
        // public static void SubtractHP_OnEnter_Postfix(SubtractHP __instance) {
        //     var go = __instance.target.GetSafe(__instance);
        //     go.TryGetComponent<HealthManager>(out HealthManager hm);
        //     if (hm) {
        //         float hp = hm.hp;
        //         hm.GetComponent<IHealthBarOwner>()?.SetHP(hp);
        //         //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
        //         //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
        //     }
        // }


    }
}

