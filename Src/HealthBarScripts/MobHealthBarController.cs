using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SilkenImpact {
    public class MobHealthBarController : MonoBehaviour {
        Dictionary<GameObject, GameObject> healthBarGoOf = new();
        Canvas _worldSpaceCanvas;
        Canvas WorldSpaceCanvas {
            get {
                if (_worldSpaceCanvas == null) {
                    var go = new GameObject("WorldSpaceCanvas");
                    _worldSpaceCanvas = go.AddComponent<Canvas>();
                    _worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
                    _worldSpaceCanvas.worldCamera = Camera.main;
                }
                return _worldSpaceCanvas;
            }
        }

        void Awake() {
            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Spawn, OnMobSpawn);
            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Die, OnMobDie);

            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Heal, OnMobHeal);
            EventHandle<MobOwnerEvent>.Register<GameObject, float>(HealthBarOwnerEventType.Damage, OnMobDamage);

            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Hide, OnMobHide);
            EventHandle<MobOwnerEvent>.Register<GameObject>(HealthBarOwnerEventType.Show, OnMobShow);
        }

        private bool guardExist(GameObject mobGO) {
            if (!healthBarGoOf.ContainsKey(mobGO)) {
                Plugin.Logger.LogWarning($"MobHealthBarController: GuardExist failed, mobGO {mobGO.name} not found in healthBarGoOf");
                return false;
            }
            return true;
        }

        private void OnMobShow(GameObject mobGO) {
            var bar = healthBarGoOf[mobGO];
            bar.SetActive(true);
        }

        private void OnMobSpawn(GameObject mobGO, float maxHp) {
            GameObject prefab;
            GameObject healthBarGO;
#if UNITY_EDITOR
            prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Addressables/Prefabs/HealthBar.prefab").WaitForCompletion();
            healthBarGO = Instantiate(prefab);
#else
            healthBarGO = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/HealthBarSmall.prefab", "MobHealthBar");
#endif
            healthBarGO.transform.SetParent(WorldSpaceCanvas.transform);
            healthBarGO.GetComponent<SpriteTracker>().SetTarget(mobGO);
            healthBarGO.GetComponent<HealthBar>().SetMaxHealth(maxHp);
            //TODO set the size of the health bar based on mobGO's sprite size
            healthBarGoOf[mobGO] = healthBarGO;
        }

        private void OnMobDamage(GameObject mobGO, float amount) {
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.TakeDamage(amount);
        }

        private void OnMobHeal(GameObject mobGO, float amount) {
            if (!guardExist(mobGO)) return;
            var go = healthBarGoOf[mobGO];
            var bar = go.GetComponent<HealthBar>();
            bar.Heal(amount);
        }

        private void OnMobHide(GameObject mobGO) {
            var go = healthBarGoOf[mobGO];
            go.SetActive(false);
        }

        private void OnMobDie(GameObject mobGO) {
            var go = healthBarGoOf[mobGO];
            Destroy(go);
            healthBarGoOf.Remove(mobGO);
        }

        // Update is called once per frame
        void Update() {

        }
    }
}
