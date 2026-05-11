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
                PluginLogger.LogError($"[InitHpWatcher][Awake][MissingComponent] hm == null or owner == null. Enemy={gameObject.name}, hm={hm}, owner={owner}");
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
                PluginLogger.LogError($"[InitHpWatcher][MonitorInitHp][MissingRuntimeDependency] hm == null or owner == null or initHpGetter == null. Enemy={gameObject.name}, hm={hm}, owner={owner}, initHpGetter={initHpGetter}");
                return;
            }

            if (previousInitHp == null) {
                previousInitHp = initHpGetter.Value;
                return;
            }
            int currentHP = initHpGetter.Value;
            if (currentHP != previousInitHp) {
                PluginLogger.LogWarning($"[InitHpWatcher][MonitorInitHp][InitHpChanged] enemy={hm.name} initHpBefore={previousInitHp} initHpAfter={currentHP}");
                PluginLogger.LogWarning("[InitHpWatcher][MonitorInitHp][Reason] Potential game update behavior change or external mod interference.");
                PluginLogger.LogWarning($"[InitHpWatcher][MonitorInitHp][AutoAdapt] enemy={hm.name} currentHp={currentHP}");

                owner.SetHP(currentHP);
                previousInitHp = currentHP;
            }
        }
    }


}
