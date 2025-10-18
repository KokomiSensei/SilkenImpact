using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UIR.BestFitAllocator;
namespace SilkenImpact {

    public class Playground : MonoBehaviour {
        public HealthBarController target;
        public GameObject bean;
        public GameObject bean2;
        private GameObject _player;
        public GameObject Player {
            get {
                if (_player == null) {
                    _player = GameObject.Find("Hero_Hornet(Clone)");
                }
                return _player;
            }
        }


        private List<Color> colors = ColourPalette.AllElementColors;

        private GameObject _stubTemplate;
        private static Playground _instance;

        public static Playground Instance {
            get {
                if (_instance == null) {
                    _instance = FindFirstObjectByType<Playground>();
                    if (_instance != null) return _instance;

                    GameObject obj = new GameObject(typeof(Playground).Name);
                    _instance = obj.AddComponent<Playground>();
                }
                return _instance;
            }
        }

        protected virtual void Awake() {
            if (_instance == null) {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (_instance != this) {
                Destroy(gameObject);
            }
        }

        private void makeCleanStubTemplate(GameObject go) {
            if (_stubTemplate == null) {
                return;
            }
            // Remove all Children GameObjects
            var children = new List<GameObject>();
            foreach (Transform child in go.transform) {
                children.Add(child.gameObject);
            }
            foreach (var child in children) {
                DestroyImmediate(child);
            }
            // [Not Necessary?]
            // Remove most of the components
            // 需要保留的组件类型
            System.Type[] typesToKeep = new System.Type[] {
                typeof(Transform),          // Transform必须保留
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(tk2dSprite),
                typeof(tk2dSpriteAnimator),
                typeof(Rigidbody2D),
                typeof(Collider2D),         // 包含所有2D碰撞体类型
                typeof(HealthManager),
                typeof(HealthBarOwner),
                typeof(DamageHero),
                typeof(EnemyDeathEffects),
                typeof(EnemyHitEffectsRegular),
                typeof(TagDamageTaker),
                typeof(PlayMakerProxyBase),
                typeof(PlayMakerFSM)
            };
            Component[] allComponents = go.GetComponents<Component>();

            // 收集需要删除的组件
            List<Component> componentsToRemove = new List<Component>();

            foreach (Component component in allComponents) {
                bool shouldKeep = false;

                // 跳过空组件
                if (component == null) continue;

                System.Type componentType = component.GetType();

                // 检查组件是否为要保留的类型或者是其子类
                foreach (System.Type typeToKeep in typesToKeep) {
                    if (typeToKeep.IsAssignableFrom(componentType)) {
                        shouldKeep = true;
                        break;
                    }
                }

                // 如果不需要保留，加入待删除列表
                if (!shouldKeep) {
                    componentsToRemove.Add(component);
                }
            }

            // 删除收集到的组件
            foreach (Component componentToRemove in componentsToRemove) {
                try {
                    Plugin.Logger.LogInfo($"移除组件: {componentToRemove.GetType().Name} 从 {go.name}");
                    DestroyImmediate(componentToRemove);
                } catch (System.Exception e) {
                    Plugin.Logger.LogError($"无法移除组件 {componentToRemove.GetType().Name}: {e.Message}");
                }
            }

            Plugin.Logger.LogInfo($"已完成清理模板 {go.name}");
        }
        private GameObject Stub {
            get {
                if (_stubTemplate == null) {
                    GameObject original = null;
                    original = GameObject.FindFirstObjectByType<MobHealthBarController>()?.GetRandomMobGO();
                    if (!original)
                        return null;
                    _stubTemplate = Instantiate(original);
                    _stubTemplate.name = "Stub Template";
                    _stubTemplate.SetActive(false);
                    makeCleanStubTemplate(_stubTemplate);
                    Object.DontDestroyOnLoad(_stubTemplate);
                }

                var copy = Instantiate(_stubTemplate);
                copy.name = "Stub";
                copy.SetActive(true);

                List<MonoBehaviour> toDestroy = new();
                toDestroy.AddRange(copy.GetComponents<tk2dSpriteAnimator>());
                toDestroy.AddRange(copy.GetComponents<DamageHero>());
                foreach (MonoBehaviour comp in toDestroy) {
                    comp.enabled = false;
                }
                return copy;
            }
        }


        private void Update() {
            if (Input.GetKeyDown(KeyCode.F2)) {
                Vector3 pos = Player.transform.position;
                Plugin.Logger.LogInfo($"Player Position {pos}");

            }
            if (Input.GetKeyDown(KeyCode.F3)) {
                Vector3 pos = Player.transform.position;
                var copy = Stub;
                if (copy == null) {
                    Plugin.Logger.LogWarning("No Stub found to instantiate");
                    return;
                }
                Plugin.Logger.LogInfo($"Instantiating Stub {copy.name} at Player Position {pos}");
                Renderer renderer = copy.GetComponent<Renderer>();
                if (renderer != null) {
                    float bottomOffset = renderer.bounds.min.y - copy.transform.position.y;
                    Vector3 adjustedPos = pos;
                    adjustedPos.y -= bottomOffset;
                    copy.transform.position = adjustedPos;
                } else {
                    copy.transform.position = pos;
                }
            }
            //if (Input.GetKeyDown(KeyCode.F9)) {
            //    Plugin.Logger.LogInfo("F9 Pressed");
            //    Vector3 mousePosition = Input.mousePosition;
            //    mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            //    mousePosition.z = 1;
            //    Plugin.Logger.LogInfo($"Mouse Position {mousePosition}");

            //    HealthManager hm = null;
            //    if (bean2 == null) {
            //        Plugin.Logger.LogInfo("Instantiating Bean");
            //        bean2 = Plugin.InstantiateFromAddressable("Bean", "Bean");
            //        bean2.transform.position = mousePosition;
            //        hm = bean.AddComponent<HealthManager>();
            //    }
            //    hm = bean.GetComponent<HealthManager>();
            //    if (hm) {
            //        hm.hp = 500;
            //        hm.IsInvincible = false;
            //    }
            //}
            //if (Input.GetKey(KeyCode.F10)) {
            //    StartCoroutine(UpdateAddressableOnAwake());
            //}
        }

        //private IEnumerator UpdateAddressableOnAwake() {
        //    // remote catalog at <PluginFolder>/catalog_1.0.json
        //    string path = Path.Combine(Plugin.PluginFolder, "catalog_1.0.json");
        //    Plugin.Logger.LogInfo($"Loading Addressable Catalog from {path}");
        //    //AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(path, true);
        //    //if (handle.Status != AsyncOperationStatus.Succeeded) {
        //    //    Logger.LogWarning($"Failed to load Addressable Catalog from {path}");
        //    //    Logger.LogError(handle.OperationException);
        //    //    yield break;
        //    //}
        //    AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(path);
        //    yield return handle;

        //    if (handle.Status != AsyncOperationStatus.Succeeded) {
        //        Plugin.Logger.LogWarning($"Failed to load Addressable Catalog from {path}, {handle.Status}");
        //        Plugin.Logger.LogError(handle.OperationException);
        //        handle.Release();
        //        yield break;
        //    }
        //    handle.Release();
        //    Plugin.isAddressableUpdated = true;
        //}
    }

}