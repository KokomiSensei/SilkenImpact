using BepInEx;
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
    private static AssetBundle bundle;
    private Harmony _harmony;
    public static bool isAddressableUpdated = false; //TODO private dude

    private void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Asset Bundle
        PluginFolder = Path.GetDirectoryName(Info.Location);
        string assetFolder = Path.Combine(PluginFolder, "Assets");
        string latestBundlePath = LatestBundleInFolder(assetFolder); //TODO For Quick Debugging Only
        bundle = AssetBundle.LoadFromFile(latestBundlePath);
        Logger.LogInfo($"Loaded AssetBundle from {latestBundlePath}");

        // Patch 
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();

        // Debug
        var go = new GameObject("Playground");
        go.AddComponent<Playground>();
        UnityEngine.Object.DontDestroyOnLoad(go);

        // Addressables
        //StartCoroutine(UpdateAddressableOnAwake());

        Summon();
    }
    void Summon() {
        var go = new GameObject("MobHealthManager");
        go.AddComponent<MobHealthBarController>();
        DontDestroyOnLoad(go);
    }


    private void OnDestroy() {
        Logger.LogDebug("Plugin is being unloaded");
        _harmony?.UnpatchSelf();
    }



    public string LatestBundleInFolder(string folderPath) {
        var directory = new DirectoryInfo(folderPath);
        var file = directory.GetFiles("default*.bundle")
                            .OrderByDescending(f => f.LastWriteTime)
                            .FirstOrDefault();
        return file?.FullName;
    }

    public static GameObject InstantiateFromAssetsBundle(string path, string name) {
        Logger.LogInfo($"Instantiating {name} from AssetBundle at path {path}");
        var prefab = bundle.LoadAsset<GameObject>(path);
        Logger.LogInfo($"Loaded prefab: {prefab}");
        if (prefab == null)
            return null;
        Logger.LogInfo($"Instantiating prefab {prefab.name}");
        GameObject go = null;
        try {
            go = Instantiate(prefab);
        } catch (Exception e) {
            Logger.LogFatal(e);
            return null;
        }
        go.name = name;
        Logger.LogInfo($"Instantiated GameObject <{path}> as <{go.name}>");
        return go;
    }

    //private IEnumerator UpdateAddressableOnAwake() {
    //    // remote catalog at <PluginFolder>/catalog_1.0.json
    //    string path = Path.Combine(PluginFolder, "catalog_1.0.bin");
    //    Logger.LogInfo($"Loading Addressable Catalog from {path}");
    //    //AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(path, true);
    //    //if (handle.Status != AsyncOperationStatus.Succeeded) {
    //    //    Logger.LogWarning($"Failed to load Addressable Catalog from {path}");
    //    //    Logger.LogError(handle.OperationException);
    //    //    yield break;
    //    //}
    //    AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(path);
    //    yield return handle;

    //    if (handle.Status != AsyncOperationStatus.Succeeded) {
    //        Logger.LogWarning($"Failed to load Addressable Catalog from {path}, {handle.Status}");
    //        Logger.LogError(handle.OperationException);
    //        handle.Release();
    //        yield break;
    //    }
    //    handle.Release();
    //    isAddressableUpdated = true;
    //}

    //public static GameObject InstantiateFromAddressable(string key, string name) {
    //    if (!isAddressableUpdated) {
    //        Logger.LogWarning("Addressables not initialized yet");
    //        return null;
    //    }
    //    Logger.LogInfo($"Instantiating {name} from Addressable with key {key}");
    //    var go = Addressables.InstantiateAsync(key).WaitForCompletion();
    //    if (go == null) {
    //        Logger.LogWarning($"Failed to load Addressable with key {key}");
    //    }
    //    go.name = name;
    //    return go;
    //}
}
