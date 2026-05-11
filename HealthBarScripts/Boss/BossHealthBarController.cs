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
                switch (Configs.Instance.healthBarShape.Value) {
                    case HealthBarShape.Rounded:
                        return "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/RoundedBoss.prefab";
                    case HealthBarShape.Diamond:
                        return "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/DiamondBoss.prefab";
                    default:
                        return "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/RoundedBoss.prefab";
                }
            }
        }
        protected string containerPrefabPath = "Assets/Addressables/Prefabs/Container.prefab";
        public override GameObject GetNewHealthBar => Plugin.InstantiateFromAssetsBundle(healthBarPrefabPath, "BossHealthBar");

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
                string locolisedName = HealthManagerPatch.LocalisedName(__instance: bossGO.GetComponent<HealthManager>());
                PluginLogger.LogInfo($"[BossHealthBarController][OnEnemySpawn][SetHealthBarDisplayName] boss={bossGO.name} localizedName={locolisedName}");
                uIHealthBar.SetNameText(locolisedName);
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
