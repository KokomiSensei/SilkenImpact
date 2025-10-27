using SilkenImpact.Patch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SilkenImpact {
    public class BossHealthBarController : BaseHealthBarController<BossOwnerEvent, BossHealthBarOwner> {

        BossHealthBarContainer container;
        protected string healthBarPrefabPath = "Assets/Addressables/Prefabs/HealthBarBossWithName.prefab";
        protected string containerPrefabPath = "Assets/Addressables/Prefabs/Container.prefab";
        public override GameObject GetNewHealthBar => Plugin.InstantiateFromAssetsBundle(healthBarPrefabPath, "BossHealthBar");

        public override Canvas BarCanvas => ScreenSpaceCanvas.GetScreenSpaceCanvas;

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


        protected override void OnEnemyShow(GameObject enemyGO) {
            if (!guardExist(enemyGO)) return;
            base.OnEnemyShow(enemyGO);

            var barGO = healthBarGoOf[enemyGO];
            container.AddBar(barGO.GetComponent<HealthBar>());
            barGO.GetComponentInChildren<Text>().enabled = true;
        }

        protected override void OnEnemySpawn(GameObject bossGO, float maxHp) {
            base.OnEnemySpawn(bossGO, maxHp);

            var healthBarGO = healthBarGoOf[bossGO];
            healthBarGO.transform.localScale = Vector3.one;
            var text = healthBarGO.GetComponentInChildren<Text>();
            if (text) {
                string locolisedName = HealthManagerPatch.LocalisedName(__instance: bossGO.GetComponent<HealthManager>());
                PluginLogger.LogInfo($"BossHealthBarController: Localised name for bossGO {bossGO.name} is {locolisedName}");
                text.text = locolisedName;
            }
        }

        protected override void OnEnemyHide(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            base.OnEnemyHide(bossGO);

            var go = healthBarGoOf[bossGO];
            go.GetComponentInChildren<Text>().enabled = false;
            container.RemoveBar(go.GetComponent<HealthBar>());
        }

        protected override void OnEnemyDie(GameObject bossGO) {
            if (!guardExist(bossGO)) return;
            var go = healthBarGoOf[bossGO];
            container.RemoveBar(go.GetComponent<HealthBar>());

            base.OnEnemyDie(bossGO);
        }

        protected override float BarWidth(float maxHp) {
            return Configs.Instance.bossBarWidth.Value;
        }
    }
}