using SilkenImpact.Patch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public class BossHealthBarController : MonoBehaviour {

        Dictionary<GameObject, GameObject> healthBarGoOf = new();
        BossHealthBarContainer container;
        void prepareContainer() {
            var containerGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/Container.prefab", "BossHealthBarContainer");
            container = containerGO.GetComponent<BossHealthBarContainer>();
            container.transform.SetParent(ScreenSpaceCanvas.GetScreenSpaceCanvas.transform);
            var rect = container.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 1.3f);
            rect.sizeDelta = new Vector2(Configs.Instance.bossBarWidth.Value, rect.sizeDelta.y);
            var image = container.GetComponent<Image>();
            if (image)
                image.color = new Color(1, 1, 1, 0);
        }

        void Awake() {
            prepareContainer();

            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnBossSpawn);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Die, OnBossDie);

            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnBossHeal);
            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnBossDamage);

            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnBossHide);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Show, OnBossShow);

            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.SetHP, OnBossSetHP);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.CheckHP, (GameObject go) => OnCheckHP(go));
        }

        private void OnCheckHP(GameObject bossGO, bool fixMismatch = false) {
            float realHp = bossGO.GetComponent<HealthManager>().hp;
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            if (Mathf.Abs(bar.CurrentHealth - realHp) > 0.01f) {
                Plugin.Logger.LogError("BossHealthBarController: OnCheckHP detected HP mismatch for bossGO " + bossGO.name +
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


        private bool guardExist(GameObject bossGO) {
            if (!healthBarGoOf.ContainsKey(bossGO)) {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
                Plugin.Logger.LogWarning($"BossHealthBarController: GuardExist failed, bossGO {bossGO.name} not found in healthBarGoOf");
#endif
                return false;
            }
            return true;
        }

        public GameObject GetRandomBossGO() {
            foreach (var kvp in healthBarGoOf) {
                if (kvp.Key != null)
                    return kvp.Key;
            }
            return null;
        }

        private void OnBossShow(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            Plugin.Logger.LogWarning($"BossHealthBarController: OnBossShow called on {bossGO.name}");
            var barGO = healthBarGoOf[bossGO];
            container.AddBar(barGO.GetComponent<HealthBar>());
            barGO.GetComponent<HealthBar>().SetVisibility(true);
            barGO.GetComponentInChildren<Text>().enabled = true;
        }

        private void OnBossSpawn(GameObject bossGO, float maxHp) {
            if (healthBarGoOf.ContainsKey(bossGO)) {

                Plugin.Logger.LogWarning($"BossHealthBarController: OnBossSpawn called but bossGO {bossGO.name} already has a health bar");
                Plugin.Logger.LogWarning($"BossHealthBarController: Overwriting maxHp bossGO {bossGO.name} with [{maxHp}]");

                var go = healthBarGoOf[bossGO];
                var bar = go.GetComponent<HealthBar>();
                bar.SetMaxHealth(maxHp);
                return;
            }
            Plugin.Logger.LogInfo($"BossHealthBarController: OnBossSpawn called for bossGO {bossGO.name} with maxHp {maxHp}");
            GameObject healthBarGO;

            healthBarGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealthBarBossWithName.prefab", "BossHealthBar");
            healthBarGoOf[bossGO] = healthBarGO;
            healthBarGO.transform.SetParent(ScreenSpaceCanvas.GetScreenSpaceCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one;
            var text = healthBarGO.GetComponentInChildren<Text>();
            if (text) {
                string locolisedName = HealthManagerPatch.LocalisedName(__instance: bossGO.GetComponent<HealthManager>());
                Plugin.Logger.LogInfo($"BossHealthBarController: Localised name for bossGO {bossGO.name} is {locolisedName}");
                text.text = locolisedName;
            }
            healthBarGO.GetComponent<HealthBar>().SetMaxHealth(maxHp);
            if (!bossGO.GetComponent<BossHealthBarOwner>())
                bossGO.AddComponent<BossHealthBarOwner>();
        }

        private void OnBossDamage(GameObject bossGO, float amount) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            bar.TakeDamage(amount);
            // OnCheckHP(bossGO, true);
            OnCheckHP(bossGO);
        }

        private void OnBossHeal(GameObject bossGO, float amount) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            bar.Heal(amount);
            OnCheckHP(bossGO);
        }

        private void OnBossHide(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            //Plugin.Logger.LogWarning($"BossHealthBarController: Ignoring Hide event on {bossGO.name}");
            var go = healthBarGoOf[bossGO];
            go.GetComponent<HealthBar>().SetVisibility(false);
            go.GetComponentInChildren<Text>().enabled = false;
            container.RemoveBar(go.GetComponent<HealthBar>());
        }

        private void OnBossDie(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            container.RemoveBar(go.GetComponent<HealthBar>());
            Destroy(go);
            healthBarGoOf.Remove(bossGO);
        }

        private void OnBossSetHP(GameObject bossGO, float hp) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            bar.ResetHealth(hp);
            OnCheckHP(bossGO, true);
        }
    }
}