using HutongGames.PlayMaker.Actions;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SilkenImpact {
    public static class BossResolver {
        public static bool IsBoss(HealthManager healthManager) {
            DisplayBossTitle bossTitleAction = GetBossTitleAction(healthManager.gameObject) ??
                GetBossTitleAction(healthManager.gameObject.transform.parent?.gameObject, searchInChildren: true);
            bool isBoss = bossTitleAction != null;
            PluginLogger.LogDebug($"[BossResolver][IsBoss] healthManager={healthManager.name} isBoss={isBoss}");
            return isBoss;
        }

        public static string BossName(HealthManager healthManager) {
            string titleKey = null;
            DisplayBossTitle bossTitleAction;

            if (healthManager.gameObject.name == "Dock Guard Thrower") {
                titleKey = "DOCK_GUARD_DUO";
            } else {
                bossTitleAction = GetBossTitleAction(healthManager.gameObject) ??
                    GetBossTitleAction(healthManager.gameObject.transform.parent?.gameObject, searchInChildren: true);
                titleKey = bossTitleAction?.bossTitle.Value;
            }


            if (string.IsNullOrEmpty(titleKey)) return null;
            return LocalizedDisplayName(titleKey);
        }
        private static string Localized(string titleKey) {
            string localizationKey = titleKey;
            string localizationSheet = "Titles";
            string localizedName = Language.Get(localizationKey, localizationSheet);
            PluginLogger.LogInfo($"[BossResolver][Localized] Resolved localized name for key {localizationKey}: {localizedName}");
            // It seem that Language will silently fail if the given key is not found returning something like "#!#KEY#!#" 
            if (localizedName.StartsWith("#")) {
                PluginLogger.LogWarning($"[BossResolver][Localized] Failed to resolve localized name for key {localizationKey}. The returned value is {localizedName}. This likely means that the localization key is missing in the language files. Please report this issue to the mod author.");
                return null;
            }
            if (string.IsNullOrEmpty(localizedName)) {
                PluginLogger.LogDebug($"[BossResolver][Localized] Resolved localized name for key {localizationKey} is null or empty.");
                return null;
            }
            return localizedName;
        }

        private static string LocalizedDisplayName(string titleKey) {
            PluginLogger.LogDebug($"[BossResolver][LocalizedDisplayName] Trying (Main, SUB, SUPER) => ({Localized(titleKey + "_MAIN")}, {Localized(titleKey + "_SUB")}, {Localized(titleKey + "_SUPER")})");
            string localized;

            if (titleKey == "DOCK_GUARD_DUO") {
                localized = Localized(titleKey + "_MAIN").Split("&").Last().Trim();
                localized = $"{Localized(titleKey + "_SUPER")}{localized}";
            } else {
                localized = $"{Localized(titleKey + "_SUPER")}{Localized(titleKey + "_MAIN")}{Localized(titleKey + "_SUB")}";
            }
            if (string.IsNullOrEmpty(localized)) {
                return null;
            }
            return localized;
        }



        private static DisplayBossTitle GetBossTitleAction(GameObject gameObject, bool searchInChildren = false) {
            if (gameObject == null) {
                PluginLogger.LogError($"[BossResolver][GetBossTitleAction] gameObject is null");
                return null;
            }
            var fsms = searchInChildren ?
                gameObject.GetComponentsInChildren<PlayMakerFSM>() :
                gameObject.GetComponents<PlayMakerFSM>();

            foreach (var fsm in fsms) {
                foreach (var state in fsm.FsmStates) {
                    foreach (var action in state.Actions) {
                        if (action is DisplayBossTitle) {
                            PluginLogger.LogDebug($"[BossResolver][GetBossTitleAction] Found DisplayBossTitle action for {gameObject.name}");
                            return action as DisplayBossTitle;
                        }
                    }
                }
            }
            PluginLogger.LogDebug($"[BossResolver][GetBossTitleAction] No DisplayBossTitle action found for {gameObject.name}");
            return null;
        }
    }
}


