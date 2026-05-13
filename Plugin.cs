using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using UnityEngine;


namespace SilkenImpact;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;
    public static string PluginFolder { private set; get; }
    public static string AssetsFolder => Path.Combine(PluginFolder, "Assets");
    public static AssetBundle bundle { private set; get; }
    private Harmony _harmony;
    private static Plugin __instance;
    public static Plugin Instance {
        get {
            return __instance;
        }
    }

    private void Awake() {
        __instance = this;
        // Plugin startup logic
        Logger = base.Logger;
        PluginLogger.LogDebug($"[Plugin] Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Asset Bundle
        LoadAssetBundle();
        PooledObjectService.Instance.InitializeAndPrewarm();

        // Patch 
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();

        // Debug
        var go = new GameObject("Playground");
        go.AddComponent<Playground>();
        UnityEngine.Object.DontDestroyOnLoad(go);

        // Addressables
        //StartCoroutine(UpdateAddressableOnAwake());

        summon();
        //bindConfigs();
    }
    void summon() {
        var go = new GameObject("MobHealthManager");
        go.AddComponent<MobHealthBarController>();
        DontDestroyOnLoad(go);
        go = new GameObject("BossHealthManager");
        go.AddComponent<BossHealthBarController>();
        DontDestroyOnLoad(go);
    }


    private void OnDestroy() {
        PluginLogger.LogDebug("[Plugin] Plugin is being unloaded");
        PooledObjectService.Instance.ClearAll();
        _harmony?.UnpatchSelf();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            PluginLogger.LogDebug("[Plugin] Reloading AssetBundle...");
            PooledObjectService.Instance.ClearAll();
            LoadAssetBundle();
            PooledObjectService.Instance.InitializeAndPrewarm();
        }
    }


    public string LatestBundleInFolder(string folderPath) {
        var directory = new DirectoryInfo(folderPath);
        var file = directory.GetFiles("default*.bundle")
                            .OrderByDescending(f => f.LastWriteTime)
                            .FirstOrDefault();
        return file?.FullName;
    }

    public void LoadAssetBundle() {
        if (bundle != null) {
            bundle.Unload(true);
        }
        PluginFolder = Path.GetDirectoryName(Info.Location);
        string latestBundlePath = LatestBundleInFolder(AssetsFolder);
        bundle = AssetBundle.LoadFromFile(latestBundlePath);
        PluginLogger.LogDebug($"[Plugin] Loaded AssetBundle from {latestBundlePath}");
    }

    public static GameObject InstantiateFromAssetsBundle(string path, string name) {
        PluginLogger.LogDebug($"[Plugin] Instantiating {name} from AssetBundle at path {path}");
        var prefab = bundle.LoadAsset<GameObject>(path);
        PluginLogger.LogDebug($"[Plugin] Loaded prefab: {prefab}");
        if (prefab == null)
            return null;
        PluginLogger.LogDebug($"[Plugin] Instantiating prefab {prefab.name}");
        GameObject go = null;
        try {
            go = Instantiate(prefab);
        } catch (Exception e) {
            PluginLogger.LogFatal(e.ToString());
            return null;
        }
        go.name = name;
        PluginLogger.LogDebug($"[Plugin] Instantiated GameObject <{path}> as <{go.name}>");
        return go;
    }
}
