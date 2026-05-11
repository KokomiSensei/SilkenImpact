using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(LifebloodState))]
    public class LifebloodStatePatch {
        private static HealthManager GetPrivateHealthManager(LifebloodState instance) {
            Traverse tr = Traverse.Create(instance);
            var hm = tr.Field<HealthManager>("healthManager").Value;
            return hm;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void LifebloodState_Update_Prefix(LifebloodState __instance, ref Tuple<int, int> __state) {
            var hm = GetPrivateHealthManager(__instance);
            if (hm) {
                int currentHP = hm.hp;
                int handle = hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Enqueue<HealEventArgs>() ?? -1;
                __state = new Tuple<int, int>(currentHP, handle);
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void LifebloodState_Update_Postfix(LifebloodState __instance, ref Tuple<int, int> __state) {
            var hm = GetPrivateHealthManager(__instance);
            if (hm) {
                int currentHP = hm.hp;
                (int previousHP, int handle) = __state;
                if (currentHP > previousHP) {
                    PluginLogger.LogInfo($"[LifebloodStatePatch][Update][Heal] Submitting Heal event in Dispatcher. enemy={hm.gameObject.name} hpInPrefix={previousHP} hpInPostFix={currentHP}");
                    float healAmount = currentHP - previousHP;
                    hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Submit(handle, new HealEventArgs(healAmount));
                    DamageTextSpawnUtils.SpawnHealText(hm, healAmount, ColourPalette.Hydro);
                } else {
                    if (currentHP < previousHP)
                        PluginLogger.LogWarning($"[LifebloodStatePatch][Update][UnexpectedHpDrop] hpInPostfix < hpInPrefix, Cancelling Heal Event in Dispatcher. enemy={hm.gameObject.name} hpInPrefix={previousHP} hpInPostFix={currentHP}");
                    hm.GetComponent<IHealthBarOwner>()?.Dispatcher.Cancel(handle);
                }
            }
        }

    }
}

