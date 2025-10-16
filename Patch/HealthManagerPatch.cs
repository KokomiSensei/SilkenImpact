using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(HealthManager))]
    public class HealthManagerPatch {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void HealthManager_Awake_Postfix(HealthManager __instance) {
            Plugin.Logger.LogInfo($"{__instance.gameObject.name}.Health Manager Awoken, hp -> {__instance.hp}");
            var go = __instance.gameObject;
            go.AddComponent<HealthBarOwner>();
        }

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
            Object.Destroy(__instance.gameObject.GetComponent<HealthBarOwner>());
        }

        [HarmonyPatch("HealToMax")]
        [HarmonyPostfix]
        public static void HealthManager_HealToMax_Postfix(HealthManager __instance) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} HealToMax, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            EventHandle<MobOwnerEvent>.SendEvent<GameObject, float>(HealthBarOwnerEventType.Heal, __instance.gameObject, maxHp);
        }

        [HarmonyPatch("RefillHP")]
        [HarmonyPostfix]
        public static void HealthManager_RefillHP_Postfix(HealthManager __instance) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} RefillHP, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            EventHandle<MobOwnerEvent>.SendEvent<GameObject, float>(HealthBarOwnerEventType.Heal, __instance.gameObject, maxHp);
        }

        [HarmonyPatch("AddHP")]
        [HarmonyPostfix]
        public static void HealthManager_AddHP_Postfix(HealthManager __instance, int hpAdd, int hpMax) {
            Plugin.Logger.LogWarning($"{__instance.gameObject.name} AddHP, hp -> {__instance.hp}");
            float maxHp = __instance.hp;
            EventHandle<MobOwnerEvent>.SendEvent<GameObject, float>(HealthBarOwnerEventType.Heal, __instance.gameObject, maxHp);
        }

        [HarmonyPatch("TakeDamage")]
        [HarmonyPrefix]
        public static void HealthManager_TakeDamage_Prefix(HitInstance hitInstance, HealthManager __instance) {
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} is hit, hp -> {__instance.hp}");
        }


        [HarmonyPatch("TakeDamage")]
        [HarmonyPostfix]
        public static void HealthManager_TakeDamage_Postfix(HitInstance hitInstance, HealthManager __instance) {
            int damage = Mathf.RoundToInt((float)hitInstance.DamageDealt * hitInstance.Multiplier);
            Plugin.Logger.LogInfo($"{__instance.gameObject.name} took {damage} damage, hp -> {__instance.hp}");
            EventHandle<MobOwnerEvent>.SendEvent<GameObject, float>(HealthBarOwnerEventType.Damage, __instance.gameObject, damage);
        }

    }
}
