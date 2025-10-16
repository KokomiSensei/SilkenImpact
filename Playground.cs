using UnityEngine;
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


        private void Update() {
            //if (target != null) {
            //    if (Input.GetKeyDown(KeyCode.Comma)) {
            //        target.TakeDamage(10);
            //    }
            //    if (Input.GetKeyDown(KeyCode.Period)) {
            //        target.Heal(10);
            //    }
            //}
            if (Input.GetKeyDown(KeyCode.F2)) {
                Vector3 pos = Player.transform.position;
                Plugin.Logger.LogInfo($"Player Position {pos}");

            }
            if (Input.GetKeyDown(KeyCode.F8)) {
                Plugin.Logger.LogInfo("F8 Pressed");
                Vector3 mousePosition = Input.mousePosition;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                mousePosition.z = 1;
                Plugin.Logger.LogInfo($"Mouse Position {mousePosition}");

                HealthManager hm = null;
                if (bean == null) {
                    Plugin.Logger.LogInfo("Instantiating Bean");
                    bean = Plugin.InstantiateFromAssetsBundle("Assets/Addressables/Prefabs/Bean.prefab", "Bean");
                    bean.transform.position = mousePosition;
                    hm = bean.AddComponent<HealthManager>();
                }
                hm = bean.GetComponent<HealthManager>();
                if (hm) {
                    hm.hp = 500;
                    hm.IsInvincible = false;
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