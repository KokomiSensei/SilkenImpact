using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkenImpact.Patch {
    [HarmonyPatch]
    [HarmonyPatch(typeof(SpellFluke))]
    public class SpellFlukePatch {
        // TODO Memo

        //[HarmonyPatch("DoDamage")]
        //[HarmonyPostfix]
        //public static void SpellFluke_DoDamage_Postfix(SpellFluke __instance) {


        //}
    }
}

