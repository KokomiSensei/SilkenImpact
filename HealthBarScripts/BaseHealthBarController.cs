using SilkenImpact.Patch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public abstract class BaseHealthBarController<EventType, OwnerType> : MonoBehaviour where OwnerType : MonoBehaviour, IHealthBarOwner {

        protected Dictionary<GameObject, GameObject> healthBarGoOf = new();
        public abstract GameObject GetNewHealthBar { get; }

        void Awake() {
            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnEnemySpawn);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Die, OnEnemyDie);

            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnEnemyHeal);
            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnEnemyDamage);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnEnemyHide);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Show, OnEnemyShow);

            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.SetHP, OnEnemySetHP);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.CheckHP, (GameObject go) => OnCheckHP(go));
        }

        protected void OnCheckHP(GameObject enemyGO, bool fixMismatch = false) {
            float realHp = enemyGO.GetComponent<HealthManager>().hp;
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            var bar = go.GetComponent<HealthBar>();
            if (Mathf.Abs(bar.CurrentHealth - realHp) > 0.01f) {
                PluginLogger.LogError($"{GetType().Name}: OnCheckHP detected HP mismatch for enemyGO {enemyGO.name}, HealthBar has {bar.CurrentHealth}, but HealthManager has {realHp}");
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



        protected bool guardExist(GameObject enemyGO) {
            if (!healthBarGoOf.ContainsKey(enemyGO)) {
                PluginLogger.LogWarning($"{GetType().Name}: GuardExist failed, enemyGO {enemyGO.name} not found in healthBarGoOf");
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

        protected void OnEnemyShow(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogWarning($"{GetType().Name}: OnEnemyShow called on {enemyGO.name}");
            var barGO = healthBarGoOf[enemyGO];
            barGO.GetComponent<HealthBar>().SetVisibility(true);
        }

        protected void OnEnemySpawn(GameObject enemyGO, float maxHp) {
            if (healthBarGoOf.ContainsKey(enemyGO)) {
                PluginLogger.LogWarning($"{GetType().Name}: OnEnemySpawn called but enemyGO {enemyGO.name} already has a health bar");
                PluginLogger.LogWarning($"{GetType().Name}: Overwriting maxHp enemyGO {enemyGO.name} with [{maxHp}]");
                var go = healthBarGoOf[enemyGO];
                var bar = go.GetComponent<HealthBar>();
                bar.SetMaxHealth(maxHp);
                return;
            }
            PluginLogger.LogInfo($"{GetType().Name}: OnEnemySpawn called for enemyGO {enemyGO.name} with maxHp {maxHp}");
            GameObject healthBarGO;

            healthBarGO = GetNewHealthBar;

            healthBarGoOf[enemyGO] = healthBarGO;
            healthBarGO.transform.SetParent(ScreenSpaceCanvas.GetScreenSpaceCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one;

            healthBarGO.GetComponent<HealthBar>().SetMaxHealth(maxHp);
            if (!enemyGO.GetComponent<OwnerType>())
                enemyGO.AddComponent<OwnerType>();
        }

        protected void OnEnemyDamage(GameObject enemyGO, float amount) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            var bar = go.GetComponent<HealthBar>();
            bar.TakeDamage(amount);
            // OnCheckHP(enemyGO, true); // Must not call this here
            OnCheckHP(enemyGO);
        }

        protected void OnEnemyHeal(GameObject enemyGO, float amount) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            var bar = go.GetComponent<HealthBar>();
            bar.Heal(amount);
            OnCheckHP(enemyGO);
        }

        protected void OnEnemyHide(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            go.GetComponent<HealthBar>().SetVisibility(false);
            go.GetComponentInChildren<Text>().enabled = false;
        }

        protected void OnEnemyDie(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            Destroy(go);
            healthBarGoOf.Remove(enemyGO);
        }

        protected void OnEnemySetHP(GameObject enemyGO, float hp) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            var bar = go.GetComponent<HealthBar>();
            bar.ResetHealth(hp);
            OnCheckHP(enemyGO, true);
        }
    }
}