using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilkenImpact {
    public class BossHealthBarController : MonoBehaviour {

        Dictionary<GameObject, GameObject> healthBarGoOf = new();

        void Awake() {
            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnBossSpawn);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Die, OnBossDie);

            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnBossHeal);
            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnBossDamage);

            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnBossHide);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Show, OnBossShow);

            EventHandle<BossOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.SetHP, OnBossSetHP);
            EventHandle<BossOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.CheckHP, OnCheckHP);
        }

        private void OnCheckHP(GameObject bossGO) {
            float realHp = bossGO.GetComponent<HealthManager>().hp;
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            if (Mathf.Abs(bar.CurrentHealth - realHp) > 0.01f) {
                Plugin.Logger.LogError("BossHealthBarController: OnCheckHP detected HP mismatch for bossGO " + bossGO.name +
                    $", HealthBar has {bar.CurrentHealth}, but HealthManager has {realHp}");
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
            var bar = healthBarGoOf[bossGO];
            bar.SetActive(true);
        }

        private void OnBossSpawn(GameObject bossGO, float maxHp) {
            if (healthBarGoOf.ContainsKey(bossGO)) {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
                Plugin.Logger.LogWarning($"BossHealthBarController: OnBossSpawn called but bossGO {bossGO.name} already has a health bar");
                Plugin.Logger.LogWarning($"BossHealthBarController: Overwriting maxHp bossGO {bossGO.name} with [{maxHp}]");
#endif
                var go = healthBarGoOf[bossGO];
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
            healthBarGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealthBarSmall.prefab", "BossHealthBar");
#endif

            healthBarGoOf[bossGO] = healthBarGO;
            healthBarGO.transform.SetParent(WorldSpaceCanvas.GetWorldSpaceCanvas.transform);
            healthBarGO.GetComponent<SpriteTracker>().SetTarget(bossGO);
            healthBarGO.GetComponent<HealthBar>().SetMaxHealth(maxHp);
            if (!bossGO.GetComponent<BossHealthBarOwner>())
                bossGO.AddComponent<BossHealthBarOwner>();

            //TODO set the size of the health bar based on bossGO's sprite size?
            bool isBoss = bossGO.CompareTag("Boss");
            float barWidth = Configs.Instance.GetHpBarWidth(maxHp, false); //TODO boss bar is not implemented yet
            healthBarGO.GetComponent<HealthBar>().SetWidth(barWidth);
        }

        private void OnBossDamage(GameObject bossGO, float amount) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            bar.TakeDamage(amount);
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
            var go = healthBarGoOf[bossGO];
            go.SetActive(false);
        }

        private void OnBossDie(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            Destroy(go);
            healthBarGoOf.Remove(bossGO);
        }

        private void OnBossSetHP(GameObject bossGO, float hp) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            var bar = go.GetComponent<HealthBar>();
            bar.ResetHealth(hp);
        }
    }
}