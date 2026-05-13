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
            float realHp = 0;
            var hm = enemyGO.GetComponent<HealthManager>();
            if (hm.SendDamageTo != null) {
                realHp = hm.SendDamageTo.hp;
                PluginLogger.LogDebug($"[BaseHealthBarController][{GetType().Name}][OnCheckHP] enemy={enemyGO.name} hp from SendDamageTo={realHp}");
            } else {
                realHp = hm.hp;
                PluginLogger.LogDebug($"[BaseHealthBarController][{GetType().Name}][OnCheckHP] enemy={enemyGO.name} hp from GetComponent<HealthManager>()={realHp}");
            }
            if (!guardExist(enemyGO)) return;
            var bar = healthBarOf[enemyGO];
            if (Mathf.Abs(bar.CurrentHealth - realHp) > 0.01f) {
                PluginLogger.LogWarning($"[BaseHealthBarController][{GetType().Name}][OnCheckHP][CheckHPMismatch] enemy={enemyGO.name} barHp={bar.CurrentHealth} realHp={realHp}");
                float damage = bar.CurrentHealth - realHp;
                if (fixMismatch) {
                    PluginLogger.LogWarning($"[BaseHealthBarController][{GetType().Name}][OnCheckHP][CheckHPMismatch][OnCheckHP] Fixing HP mismatch on {enemyGO.name} by altering HP of the bar.");
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
                PluginLogger.LogWarning($"[HealthBar][{GetType().Name}][MissingBar] enemy={enemyGO.name}");
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
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyShow] enemy={enemyGO.name}");

            var bar = healthBarOf[enemyGO];
            bar.SetVisibility(true);
        }

        protected abstract float BarWidth(float maxHp);

        protected bool TryLinkEnemy(GameObject originGO, GameObject relayGO) {
            if (!guardExist(originGO)) return false;
            if (healthBarOf.ContainsKey(relayGO)) {
                PluginLogger.LogWarning($"[HealthBar][{GetType().Name}][LinkOverwrite] relay={relayGO.name}");
                var withBarGO = healthBarOf[relayGO].gameObject;
                PooledObjectService.Instance.Release(withBarGO);
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
                uiBar.SetHpTextEnabled(Configs.Instance.displayHpNumbers.Value);
                uiBar.SetHpTextColor(Configs.Instance.hpNumberColor.Value);
                uiBar.SetFont(FontManager.instance.HpBarFontLoader.Load());
                uiBar.SetNameScale(Configs.Instance.bossNameFontSizeScaler.Value);
                uiBar.SetHpNumberScale(Configs.Instance.hpNumberFontSizeScaler.Value);
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
                PluginLogger.LogWarning($"[HealthBar][{GetType().Name}][SpawnDuplicate] enemy={enemyGO.name} maxHp={maxHp}");
                bar = healthBarOf[enemyGO];
                bar.SetMaxHealth(maxHp);
                return;
            }
            PluginLogger.LogInfo($"[HealthBar][{GetType().Name}][OnEnemySpawn] enemy={enemyGO.name} maxHp={maxHp}");

            healthBarGO = GetNewHealthBar;
            healthBarGO.name = $"HealthBar_{enemyGO.name}";
            if (healthBarGO == null) {
                PluginLogger.LogError($"[HealthBar][{GetType().Name}][OnEnemySpawn][AcquireFailed] enemy={enemyGO.name}");
                return;
            }

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
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyDamage] enemy={enemyGO.name} damage={amount}");

            var bar = healthBarOf[enemyGO];
            bar.TakeDamage(amount);
            // OnCheckHP(enemyGO, true); // Must not call this here
            OnCheckHP(enemyGO);
        }

        protected void OnEnemyHeal(GameObject enemyGO, float amount) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyHeal] enemy={enemyGO.name} heal={amount}");

            var bar = healthBarOf[enemyGO];
            bar.Heal(amount);
            OnCheckHP(enemyGO);
        }

        protected virtual void OnEnemyHide(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyHide] enemy={enemyGO.name}");

            var bar = healthBarOf[enemyGO];
            bar.SetVisibility(false);
        }

        protected virtual void OnEnemyDie(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyDie] enemy={enemyGO.name}");

            var bar = healthBarOf[enemyGO];
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemyDie] Releasing health bar of {enemyGO.name}: {bar.gameObject.name}");
            PooledObjectService.Instance.Release(bar.gameObject);
            healthBarOf.Remove(enemyGO);
        }

        protected void OnEnemySetHP(GameObject enemyGO, float hp) {
            if (!guardExist(enemyGO)) return;
            PluginLogger.LogInfo($"[BaseHealthBarController][{GetType().Name}][OnEnemySetHP] enemy={enemyGO.name} hp={hp}");

            var bar = healthBarOf[enemyGO];
            bar.ResetHealth(hp);
            OnCheckHP(enemyGO, true);
        }
    }
}
