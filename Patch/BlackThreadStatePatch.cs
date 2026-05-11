using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(BlackThreadState))]
    public class BlackThreadStatePatch {
        private static HealthManager GetPrivateHealthManager(BlackThreadState instance) {
            Traverse tr = Traverse.Create(instance);
            var hm = tr.Field<HealthManager>("healthManager").Value;
            return hm;
        }


        [HarmonyPatch("SetupThreaded")]
        [HarmonyPrefix]
        public static void BlackThreadState_DoDamage_Prefix(BlackThreadState __instance, ref Tuple<int, int> __state) {
            var hm = GetPrivateHealthManager(__instance);
            if (hm) {
                PluginLogger.LogInfo($"[BlackThreadStatePatch][DoDamagePrefix] Enqueueing SetHpEvent on mob: {hm.gameObject.name}, isDead = {hm.isDead}");
                int currentHP = hm.hp;
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<SetHpEventArgs>() ?? -1;
                __state = new Tuple<int, int>(currentHP, handle);
            }
        }

        [HarmonyPatch("SetupThreaded")]
        [HarmonyPostfix]
        public static void BlackThreadState_DoDamage_Postfix(BlackThreadState __instance, ref Tuple<int, int> __state) {
            var hm = GetPrivateHealthManager(__instance);
            if (hm) {
                PluginLogger.LogInfo($"[BlackThreadStatePatch][DoDamagePostfix] SetupThreaded Postfix of {hm.gameObject.name}, isDead = {hm.isDead}");
                int currentHP = hm.hp;
                (int previousHP, int handle) = __state;
                if (currentHP != previousHP) {
                    PluginLogger.LogInfo($"[BlackThreadStatePatch][DoDamagePostfix] SetupThreaded Modified the HP {previousHP} -> {currentHP} of {hm.gameObject.name}. Submitting SetHpEvent with handle {handle}");
                    hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(handle, new SetHpEventArgs(currentHP));
                    // TODO Healing? IsDead?
                } else {
                    PluginLogger.LogInfo($"[BlackThreadStatePatch][DoDamagePostfix] SetupThreaded did not modify the HP of {hm.gameObject.name}. Canceling SetHpEvent with handle {handle}");
                    hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Cancel(handle);
                }
            }
        }
    }
}

