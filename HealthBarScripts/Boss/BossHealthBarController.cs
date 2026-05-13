using SilkenImpact.Patch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public class BossHealthBarController : BaseHealthBarController<BossOwnerEvent, BossHealthBarOwner> {

        BossHealthBarContainer container;
        protected string healthBarPrefabPath {
            get {
                return AssetPaths.HealthBars.ForBossShape(Configs.Instance.healthBarShape.Value);
            }
        }
        protected string containerPrefabPath = AssetPaths.Prefabs.BossContainer;
        public override GameObject GetNewHealthBar => PooledObjectService.Instance.Acquire(healthBarPrefabPath, "BossHealthBar", BarCanvas.transform);

        public override Canvas BarCanvas => ScreenSpaceCanvas.GetScreenSpaceCanvas;

        void UpdateContainerWidth() {
            float targetWidth = Configs.Instance.bossBarWidth.Value;
            container.SetWidth(targetWidth);
        }

        void prepareContainer() {
            var containerGO = Plugin.InstantiateFromAssetsBundle(containerPrefabPath, "BossHealthBarContainer");
            container = containerGO.GetComponent<BossHealthBarContainer>();
            container.transform.SetParent(ScreenSpaceCanvas.GetScreenSpaceCanvas.transform);
            var rect = container.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 1.3f);
            rect.sizeDelta = new Vector2(Configs.Instance.bossBarWidth.Value, rect.sizeDelta.y);
            var image = container.GetComponent<Image>();
            if (image)
                image.color = new Color(1, 1, 1, 0);
        }

        protected override void Awake() {
            base.Awake();
            prepareContainer();
        }

        protected override void MatchVisualsWithConfigs() {
            base.MatchVisualsWithConfigs();
            UpdateContainerWidth();
        }


        protected override void OnEnemyShow(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            base.OnEnemyShow(enemyGO);

            var bar = healthBarOf[enemyGO];
            container.AddBar(bar);
        }

        protected override void OnEnemySpawn(GameObject bossGO, float maxHp) {
            base.OnEnemySpawn(bossGO, maxHp);

            var healthBar = healthBarOf[bossGO];
            UIHealthBar uIHealthBar = healthBar.GetComponent<UIHealthBar>();
            if (uIHealthBar) {
                HealthManager hm = bossGO.GetComponent<HealthManager>();

                string localizedName = HealthManagerPatch.LocalisedName(instance: hm);
                PluginLogger.LogInfo($"[BossHealthBarController][OnEnemySpawn][SetHealthBarDisplayName] boss={bossGO.name} localizedName={localizedName}");
                uIHealthBar.SetNameText(localizedName);
            } else {
                PluginLogger.LogError($"[BossHealthBarController][OnEnemySpawn][MissingUIHealthBar] boss={bossGO.name}");
            }
        }

        protected override void OnEnemyHide(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            base.OnEnemyHide(bossGO);

            var bar = healthBarOf[bossGO];
            container.RemoveBar(bar);
        }

        protected override void OnEnemyDie(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            var bar = healthBarOf[bossGO];
            container.RemoveBar(bar);

            base.OnEnemyDie(bossGO);
        }

        protected override float BarWidth(float maxHp) {
            return Configs.Instance.bossBarWidth.Value;
        }
    }
}
