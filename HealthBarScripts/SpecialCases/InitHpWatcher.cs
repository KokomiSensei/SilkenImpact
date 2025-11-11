using HarmonyLib;
using SilkenImpact.Patch;
using UnityEngine;

namespace SilkenImpact {
    internal class InitHpWatcher : MonoBehaviour {
        private int? previousInitHp = null;
        private Traverse<int> initHpGetter;
        private IHealthBarOwner owner;
        private HealthManager hm;
        void Awake() {
            hm = GetComponent<HealthManager>();
            owner = GetComponent<IHealthBarOwner>();
            if (hm == null || owner == null) {
                PluginLogger.LogError($"InitHpWatcher: Missing components on {gameObject.name}");
                return;
            }
            initHpGetter = Traverse.Create(hm).Field<int>("initHp");
            previousInitHp = initHpGetter.Value;
        }
        void Update() {
            MonitorInitHp();
        }
        void MonitorInitHp() {
            if (hm == null || owner == null || initHpGetter == null) {
                PluginLogger.LogError($"InitHpWatcher: Missing components on {gameObject.name}");
                return;
            }

            if (previousInitHp == null) {
                previousInitHp = initHpGetter.Value;
                return;
            }
            int currentHP = initHpGetter.Value;
            if (currentHP != previousInitHp) {
                PluginLogger.LogFatal($"Detected initHP change on {hm.name}: {previousInitHp} -> {currentHP}!");
                PluginLogger.LogFatal($"Either Team Cherry added a new method to modify initHP after initialization, or another mod is interfering.");
                PluginLogger.LogFatal($"Current HP: {currentHP}");
                PluginLogger.LogFatal($"Attempting to make automatic adaptation. Updating health bar accordingly.");

                owner.SetHP(currentHP);
                previousInitHp = currentHP;
            }
        }
    }


}