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
        public abstract Canvas BarCanvas { get; }

        protected void RegisterEventHandlers() {
            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnEnemySpawn);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Die, OnEnemyDie);

            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnEnemyHeal);
            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnEnemyDamage);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnEnemyHide);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.Show, OnEnemyShow);

            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.SetHP, OnEnemySetHP);
            EventHandle<EventType>.Register<GameObject>(HealthBarOwnerEventType.CheckHP, (GameObject go) => OnCheckHP(go));
        }

        protected virtual void Awake() {
            RegisterEventHandlers();
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

        public GameObject GetRandomEnemyGO() {
            foreach (var kvp in healthBarGoOf) {
                if (kvp.Key != null)
                    return kvp.Key;
            }
            return null;
        }

        protected virtual void OnEnemyShow(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogWarning($"{GetType().Name}: OnEnemyShow called on {enemyGO.name}");
            var barGO = healthBarGoOf[enemyGO];
            barGO.GetComponent<HealthBar>().SetVisibility(true);
        }

        protected abstract float BarWidth(float maxHp);

        protected virtual void OnEnemySpawn(GameObject enemyGO, float maxHp) {
            GameObject healthBarGO;
            HealthBar bar;
            if (healthBarGoOf.ContainsKey(enemyGO)) {
                PluginLogger.LogWarning($"{GetType().Name}: OnEnemySpawn called but enemyGO {enemyGO.name} already has a health bar");
                PluginLogger.LogWarning($"{GetType().Name}: Overwriting maxHp enemyGO {enemyGO.name} with [{maxHp}]");
                healthBarGO = healthBarGoOf[enemyGO];
                bar = healthBarGO.GetComponent<HealthBar>();
                bar.SetMaxHealth(maxHp);
                return;
            }
            PluginLogger.LogInfo($"{GetType().Name}: OnEnemySpawn called for enemyGO {enemyGO.name} with maxHp {maxHp}");

            healthBarGO = GetNewHealthBar;

            healthBarGoOf[enemyGO] = healthBarGO;
            healthBarGO.transform.SetParent(BarCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one;

            bar = healthBarGO.GetComponent<HealthBar>();
            bar.SetHpColor(Configs.Instance.hpColor.Value);
            bar.SetDelayedEffectColor(Configs.Instance.delayedEffectColor.Value);
            bar.SetBackgroundColor(Configs.Instance.hpBarBackgroundColor.Value);
            bar.SetWidth(BarWidth(maxHp));
            bar.SetMaxHealth(maxHp);

            if (!enemyGO.GetComponent<OwnerType>()) {
                enemyGO.AddComponent<OwnerType>();
            }

            if (enemyGO.name.ToLower() == GarmondAndZaza.IngameGameObjectName && !enemyGO.GetComponent<GarmondAndZaza>()) {
                var garmondPatch = enemyGO.AddComponent<GarmondAndZaza>();
                // CAUTION: Init() needs to be called after enemyGO.AddComponent<OwnerType>();
                garmondPatch.Init(enemyGO);
            }
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

        protected virtual void OnEnemyHide(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            var go = healthBarGoOf[enemyGO];
            go.GetComponent<HealthBar>().SetVisibility(false);
        }

        protected virtual void OnEnemyDie(GameObject enemyGO) {
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