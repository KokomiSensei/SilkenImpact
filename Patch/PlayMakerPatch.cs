using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;


namespace SilkenImpact.Patch {

    [HarmonyPatch]
    public class PlayMakerPatch {
        // I have seen it called on Grand Silk Mother.
        [HarmonyPatch(typeof(AddHP))] // HealToMax or =+ AddHp.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void AddHP_OnEnter_Postfix(AddHP __instance) {
            var go = __instance.target.GetSafe(__instance);
            // Plugin.Logger.LogWarning($"{go.name} AddHP");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                hm.GetComponent<IHealthBarOwner>()?.SetHP(hp);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                // Plugin.Logger.LogWarning($"{go.name} AddHP hp:{hp}");
            }
        }

        /* 
         * 事实证明这个SetHP就是罪魁祸首，不走HealthManger API，直接修改血量
         * 
         */
        [HarmonyPatch(typeof(SetHP))] // hp += SetHP.Value
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void SetHP_OnEnter_Postfix(SetHP __instance) {
            var go = __instance.target.GetSafe(__instance);
            // Plugin.Logger.LogWarning($"{go.name} SetHP");
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            if (hm) {
                float hp = hm.hp;
                hm.GetComponent<IHealthBarOwner>()?.SetHP(hp);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                // Plugin.Logger.LogWarning($"{go.name} SetHP hp:{hp}");
            }
        }

        [HarmonyPatch(typeof(SubtractHP))] // hp -= amount.Value;
        [HarmonyPatch("OnEnter")]
        [HarmonyPostfix]
        public static void SubtractHP_OnEnter_Postfix(SubtractHP __instance) {
            var go = __instance.target.GetSafe(__instance);
            go.TryGetComponent<HealthManager>(out HealthManager hm);
            // Plugin.Logger.LogWarning($"{go.name} SubtractHP");
            if (hm) {
                float hp = hm.hp;
                hm.GetComponent<IHealthBarOwner>()?.SetHP(hp);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Die, go);
                //EventHandle<MobOwnerEvent>.SendEvent(HealthBarOwnerEventType.Spawn, go, hp);
                // Plugin.Logger.LogWarning($"{go.name} SubtractHP hp:{hp}");
            }
        }


    }
}

