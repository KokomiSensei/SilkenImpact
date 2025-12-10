using SilkenImpact.Patch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public abstract class BaseHealthBarController<EventType, OwnerType> : MonoBehaviour where OwnerType : MonoBehaviour, IHealthBarOwner {

        protected Dictionary<GameObject, HealthBar> healthBarOf = new();
        public abstract GameObject GetNewHealthBar { get; }
        public abstract Canvas BarCanvas { get; }

        protected LinkBuffer linkBuffer = null;

        protected void RegisterEventHandlers() {
            EventHandle<EventType>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnEnemySpawn);
            EventHandle<EventType>.Register<GameObject, GameObject>(HealthBarOwnerEventType.Link, LinkEnemy);
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
            linkBuffer = new LinkBuffer(TryLinkEnemy);
            InvokeRepeating(nameof(MatchVisualsWithConfigs), 0f, 1f);
        }

        protected void OnCheckHP(GameObject enemyGO, bool fixMismatch = false) {
            float realHp = enemyGO.GetComponent<HealthManager>().hp;
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
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
            if (!healthBarOf.ContainsKey(enemyGO)) {
                PluginLogger.LogWarning($"{GetType().Name}: GuardExist failed, enemyGO {enemyGO.name} not found in healthBarOf");
                return false;
            }
            return true;
        }

        public GameObject GetRandomEnemyGO() {
            foreach (var kvp in healthBarOf) {
                if (kvp.Key != null)
                    return kvp.Key.gameObject;
            }
            return null;
        }

        protected virtual void OnEnemyShow(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogWarning($"{GetType().Name}: OnEnemyShow called on {enemyGO.name}");
            var bar = healthBarOf[enemyGO];
            bar.SetVisibility(true);
        }

        protected abstract float BarWidth(float maxHp);

        protected bool TryLinkEnemy(GameObject originGO, GameObject relayGO) {
            if (!guardExist(originGO)) return false;
            if (healthBarOf.ContainsKey(relayGO)) {
                PluginLogger.LogInfo($"{GetType().Name}: LinkEnemy called on {relayGO.name}. Destroying existing health bar");
                var withBarGO = healthBarOf[relayGO].gameObject;
                Destroy(withBarGO);
            }
            healthBarOf[relayGO] = healthBarOf[originGO];
            if (relayGO.TryGetComponent<IHealthBarOwner>(out var relayOwner)) {
                relayOwner.RemoveVisibilityController();
            }
            if (originGO.TryGetComponent<IHealthBarOwner>(out var originOwner)) {
                originOwner.LinkVisibilityControl(relayGO);
            }
            return true;
        }

        protected void LinkEnemy(GameObject sourceGO, GameObject withGO) {
            if (!withGO.GetComponent<OwnerType>()) {
                var owner = withGO.AddComponent<OwnerType>();
                owner.RemoveVisibilityController();
            }
            linkBuffer.RegisterRelay(sourceGO, withGO);
        }

        protected void MatchVisualsWithConfigs(HealthBar bar) {
            if (!bar) return;
            bar.SetHpColor(Configs.Instance.hpColor.Value);
            bar.SetDelayedEffectColor(Configs.Instance.delayedEffectColor.Value);
            bar.SetBackgroundColor(Configs.Instance.hpBarBackgroundColor.Value);
            bar.SetWidth(BarWidth(bar.MaxHealth));
            var uiBar = bar.GetComponent<UIHealthBar>();
            if (uiBar != null) {
                PluginLogger.LogInfo($"{GetType().Name}: Setting up UIHealthBar for bar: {bar.gameObject.name}");
                uiBar.SetHpTextEnabled(Configs.Instance.displayHpNumbers.Value);
                uiBar.SetHpTextColor(Configs.Instance.hpNumberColor.Value);
                uiBar.SetFont(FontManager.instance.HpBarFontLoader.Load());
            }
        }

        protected virtual void MatchVisualsWithConfigs() {
            foreach (var kvp in healthBarOf) {
                MatchVisualsWithConfigs(kvp.Value);
            }
        }

        protected virtual void OnEnemySpawn(GameObject enemyGO, float maxHp) {
            GameObject healthBarGO;
            HealthBar bar;
            if (healthBarOf.ContainsKey(enemyGO)) {
                PluginLogger.LogWarning($"{GetType().Name}: OnEnemySpawn called but enemyGO {enemyGO.name} already has a health bar");
                PluginLogger.LogWarning($"{GetType().Name}: Overwriting maxHp enemyGO {enemyGO.name} with [{maxHp}]");
                bar = healthBarOf[enemyGO];
                bar.SetMaxHealth(maxHp);
                return;
            }
            PluginLogger.LogInfo($"{GetType().Name}: OnEnemySpawn called for enemyGO {enemyGO.name} with maxHp {maxHp}");

            healthBarGO = GetNewHealthBar;

            // Setup HealthBarGO
            healthBarGO.transform.SetParent(BarCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one;

            // Setup HealthBar
            bar = healthBarGO.GetComponent<HealthBar>();
            bar.SetMaxHealth(maxHp);
            MatchVisualsWithConfigs(bar);

            // Update Dictionary
            healthBarOf[enemyGO] = bar;

            if (!enemyGO.GetComponent<OwnerType>()) {
                enemyGO.AddComponent<OwnerType>();
            }

            ApplySpecialPatches(enemyGO);

            linkBuffer.RegisterOrigin(enemyGO);
        }

        protected void ApplySpecialPatches(GameObject enemyGO) {
            if (DeathWatcher.IngameGameObjectNames.Contains(enemyGO.name.ToLower()) && !enemyGO.GetComponent<DeathWatcher>()) {
                var deathWatcher = enemyGO.AddComponent<DeathWatcher>();
                // CAUTION: Init() needs to be called after enemyGO.AddComponent<OwnerType>();
                deathWatcher.Init(enemyGO);
            }

            if (!enemyGO.GetComponent<InitHpWatcher>()) {
                enemyGO.AddComponent<InitHpWatcher>();
            }
        }

        protected void OnEnemyDamage(GameObject enemyGO, float amount) {
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            bar.TakeDamage(amount);
            // OnCheckHP(enemyGO, true); // Must not call this here
            OnCheckHP(enemyGO);
        }

        protected void OnEnemyHeal(GameObject enemyGO, float amount) {
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            bar.Heal(amount);
            OnCheckHP(enemyGO);
        }

        protected virtual void OnEnemyHide(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            bar.SetVisibility(false);
        }

        protected virtual void OnEnemyDie(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            Destroy(bar.gameObject);
            healthBarOf.Remove(enemyGO);
        }

        protected void OnEnemySetHP(GameObject enemyGO, float hp) {
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            bar.ResetHealth(hp);
            OnCheckHP(enemyGO, true);
        }
    }
}