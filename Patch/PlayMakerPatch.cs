using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using UnityEngine;


namespace SilkenImpact.Patch {

    [HarmonyPatch]
    public class PlayMakerPatch {
        // I have seen it called on Grand Silk Mother.
        [HarmonyPatch(typeof(AddHP))] // HealToMax or += AddHp.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPrefix]
        public static void AddHP_OnEnter_Prefix(AddHP __instance, ref Tuple<int, int> __state) {
            var go = __instance.target.GetSafe(__instance);
            go.TryGetComponent<HealthManager>(out HealthManager hm);

            Plugin.Logger.LogWarning($"{go.name} PlayMaker AddHP");
            if (hm) {
                int currentHP = Mathf.Max(hm.hp, 0);
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<SetHpEventArgs>() ?? -1;
                __state = new Tuple<int, int>(currentHP, handle);

                Plugin.Logger.LogWarning($"PlayMaker AddHP Prefix: {hm.gameObject.name}.isDead = {hm.isDead}");
            }
        }

        [HarmonyPatch(typeof(AddHP))] // HealToMax or += AddHp.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void AddHP_OnEnter_Postfix(AddHP __instance, ref Tuple<int, int> __state) {
            var go = __instance.target.GetSafe(__instance);
            Plugin.Logger.LogWarning($"{go.name} PlayMaker AddHP");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                float amount = hp - __state.Item1;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new SetHpEventArgs(hp));
                if (!hm.isDead)
                    HealthManagerPatch.SpawnHealText(hm, amount);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                Plugin.Logger.LogWarning($"{go.name} PlayMaker AddHP hp:{hp}");
                Plugin.Logger.LogWarning($"PlayMaker AddHP Postfix: {hm.gameObject.name}.isDead = {hm.isDead}");
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
            Plugin.Logger.LogWarning($"{go.name} PlayMaker SetHP");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                int currentHP = Mathf.Max(hm.hp, 0);
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<SetHpEventArgs>() ?? -1;
                __state = new Tuple<int, int>(currentHP, handle);

                Plugin.Logger.LogWarning($"PlayMaker SetHP Prefix: {hm.gameObject.name}.isDead = {hm.isDead}");
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
            Plugin.Logger.LogWarning($"{go.name} PlayMaker SetHP");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                float amount = hp - __state.Item1;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(__state.Item2, new SetHpEventArgs(hp));
                if (!hm.isDead)
                    HealthManagerPatch.SpawnHealText(hm, amount);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                Plugin.Logger.LogWarning($"{go.name} PlayMaker SetHP hp:{__state}");
                Plugin.Logger.LogWarning($"PlayMaker SetHP Postfix: {hm.gameObject.name}.isDead = {hm.isDead}");
            }
        }

        [HarmonyPatch(typeof(SubtractHP))] // hp -= amount.Value;
        [HarmonyPatch("OnEnter")]
        [HarmonyPrefix]
        public static void SubtractHP_OnEnter_Prefix(SubtractHP __instance) {
            var go = __instance.target.GetSafe(__instance);
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            Plugin.Logger.LogWarning($"{go.name} PlayMaker SubtractHP");
            if (hm) {
                int damage = __instance.amount.Value;
                hm.GetComponent<IHealthBarOwner>()?.Dispatcher.EnqueueReady(new DamageEventArgs(damage));
            }
        }


        // [HarmonyPatch(typeof(SubtractHP))] // hp -= amount.Value;
        // [HarmonyPatch("OnEnter")]
        // [HarmonyPostfix]
        // public static void SubtractHP_OnEnter_Postfix(SubtractHP __instance) {
        //     var go = __instance.target.GetSafe(__instance);
        //     go.TryGetComponent<HealthManager>(out HealthManager hm);
        //     Plugin.Logger.LogWarning($"{go.name} PlayMaker SubtractHP");
        //     if (hm) {
        //         float hp = hm.hp;
        //         hm.GetComponent<IHealthBarOwner>()?.SetHP(hp);
        //         //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
        //         //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
        //         Plugin.Logger.LogWarning($"{go.name} PlayMaker SubtractHP hp:{hp}");
        //     }
        // }


    }
}

