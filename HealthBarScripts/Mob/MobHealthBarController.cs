using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace SilkenImpact {
    public class MobHealthBarController : MonoBehaviour {
        Dictionary<GameObject, GameObject> healthBarGoOf = new();

        void Awake() {
            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnMobSpawn);
            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Die, OnMobDie);

            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnMobHeal);
            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnMobDamage);

            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnMobHide);
            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Show, OnMobShow);

            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.SetHP, OnMobSetHP);
            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.CheckHP, (GameObject go) => OnCheckHP(go));
        }
#if DEBUG
        void Update() {
            foreach (var kvp in healthBarGoOf) {
                var hm = kvp.Key.GetComponent<HealthManager>();
                var barGO = kvp.Value;
                var bar = barGO.GetComponent<HealthBar>();
                if (hm == null || barGO == null) continue;
                barGO.name = hm.name + "_HealthBar";
                if (bar) bar.name = hm.name + "_HealthBar_Component";
            }
        }

#endif
        private void OnCheckHP(GameObject mobGO, bool fixMismatch = false) {
            float realHp = mobGO.GetComponent<HealthManager>().hp;
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            if (Mathf.Abs(bar.CurrentHealth - realHp) > 0.01f) {
                PluginLogger.LogError("MobHealthBarController: OnCheckHP detected HP mismatch for mobGO " + mobGO.name +
                    $", HealthBar has {bar.CurrentHealth}, but HealthManager has {realHp}");
                float damage = bar.CurrentHealth - realHp;
                if (fixMismatch) {
                    if (damage > 0) {
                        bar.TakeDamage(damage);
                    } else {
                        bar.Heal(-damage);
                    }
                }
            }
        }


        private bool guardExist(GameObject mobGO) {
            if (!healthBarGoOf.ContainsKey(mobGO)) {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
                PluginLogger.LogWarning($"MobHealthBarController: GuardExist failed, mobGO {mobGO.name} not found in healthBarGoOf");
#endif
                return false;
            }
            return true;
        }

        public GameObject GetRandomMobGO() {
            foreach (var kvp in healthBarGoOf) {
                if (kvp.Key != null)
                    return kvp.Key;
            }
            return null;
        }

        private void OnMobShow(GameObject mobGO) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.SetVisibility(true);
        }

        private void OnMobSpawn(GameObject mobGO, float maxHp) {
            if (healthBarGoOf.ContainsKey(mobGO)) {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
                PluginLogger.LogWarning($"MobHealthBarController: OnMobSpawn called but mobGO {mobGO.name} already has a health bar");
                PluginLogger.LogWarning($"MobHealthBarController: Overwriting maxHp mobGO {mobGO.name} with [{maxHp}]");
#endif
                var go = healthBarGoOf[mobGO];
                var bar = go.GetComponent<HealthBar>();
                bar.SetMaxHealth(maxHp);
                return;
            }

            GameObject prefab;
            GameObject healthBarGO;
#if UNITY_EDITOR || UNITY_STANDALONE
            prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Addressables/Prefabs/HealthBar.prefab").WaitForCompletion();
            healthBarGO = Instantiate(prefab);
#else
            healthBarGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealthBarSmall.prefab", "MobHealthBar");
#endif

            healthBarGoOf[mobGO] = healthBarGO;
            healthBarGO.transform.SetParent(WorldSpaceCanvas.GetWorldSpaceCanvas.transform);
            healthBarGO.GetComponent<SpriteTracker>().SetTarget(mobGO);
            healthBarGO.GetComponent<HealthBar>().SetMaxHealth(maxHp);
            if (!mobGO.GetComponent<MobHealthBarOwner>())
                mobGO.AddComponent<MobHealthBarOwner>();


            float barWidth = Configs.Instance.GetHpBarWidth(maxHp, false);
            healthBarGO.GetComponent<HealthBar>().SetWidth(barWidth);
        }

        private void OnMobDamage(GameObject mobGO, float amount) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.TakeDamage(amount);
            // OnCheckHP(mobGO, true);
            OnCheckHP(mobGO);
        }

        private void OnMobHeal(GameObject mobGO, float amount) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.Heal(amount);
            OnCheckHP(mobGO);
        }

        private void OnMobHide(GameObject mobGO) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            // TODO what? NRE here? how?
            //try {
            var bar = go.GetComponent<HealthBar>();
            bar.SetVisibility(false);
            //} catch (Exception e) {
            //#if !(UNITY_EDITOR || UNITY_STANDALONE)
            //PluginLogger.LogError($"MobHealthBarController: OnMobHide failed for mobGO {mobGO.name} with exception {e}");
            //#endif
            //}
        }

        private void OnMobDie(GameObject mobGO) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            Destroy(go);
            healthBarGoOf.Remove(mobGO);
        }

        private void OnMobSetHP(GameObject mobGO, float hp) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.ResetHealth(hp);
            OnCheckHP(mobGO, true);
        }
    }
}
