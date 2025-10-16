using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkenImpact.Patch {

    [HarmonyPatch]
    [HarmonyPatch(typeof(EnemyDeathEffects))]
    public class EnemyDeathEffectsPatch {
    }
}
