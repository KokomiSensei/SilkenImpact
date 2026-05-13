using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SilkenImpact {
    internal class PooledObjectService {
        private class PoolBucket {
            internal readonly Stack<GameObject> inactive = new();
            internal ObjectPoolSettings settings;
            internal string prefabPath;
        }

        private static PooledObjectService _instance;
        internal static PooledObjectService Instance => _instance ??= new PooledObjectService();

        private readonly Dictionary<string, PoolBucket> _buckets = new();
        private readonly Dictionary<string, ObjectPoolSettings> _defaultSettings = new() {
            [AssetPaths.Prefabs.DamageText] = new ObjectPoolSettings(4, 24),
            [AssetPaths.Prefabs.HealText] = new ObjectPoolSettings(2, 8),
            [AssetPaths.HealthBars.RoundedMob] = new ObjectPoolSettings(8, 64),
            [AssetPaths.HealthBars.DiamondMob] = new ObjectPoolSettings(8, 64),
            [AssetPaths.HealthBars.RoundedBoss] = new ObjectPoolSettings(2, 12),
            [AssetPaths.HealthBars.DiamondBoss] = new ObjectPoolSettings(2, 12),
        };

        private Transform _poolRoot;

        private PooledObjectService() {
        }

        private void EnsureRoot() {
            if (_poolRoot != null) {
                return;
            }
            var root = new GameObject("SilkenImpact.ObjectPool");
            Object.DontDestroyOnLoad(root);
            _poolRoot = root.transform;
        }

        private PoolBucket GetOrCreateBucket(string prefabPath) {
            if (_buckets.TryGetValue(prefabPath, out var existing)) {
                PluginLogger.LogDebug($"[PooledObjectService][GetOrCreateBucket] Reusing existing bucket for: {prefabPath}");
                return existing;
            }
            PluginLogger.LogDebug($"[PooledObjectService][GetOrCreateBucket] Creating new bucket for: {prefabPath}");
            var settings = _defaultSettings.TryGetValue(prefabPath, out var cfg)
                ? cfg
                : new ObjectPoolSettings(0, 32);
            var bucket = new PoolBucket {
                prefabPath = prefabPath,
                settings = settings
            };
            _buckets[prefabPath] = bucket;
            return bucket;
        }

        private GameObject InstantiateForPool(string prefabPath, string displayName) {
            var go = Plugin.InstantiateFromAssetsBundle(prefabPath, displayName);
            if (go == null) {
                return null;
            }
            var marker = go.GetComponent<PooledObject>();
            if (marker == null) {
                marker = go.AddComponent<PooledObject>();
            }
            marker.PoolKey = prefabPath;
            marker.Owner = this;
            return go;
        }

        internal void InitializeAndPrewarm() {
            PluginLogger.LogInfo("[PooledObjectService] Initializing and prewarming object pools...");
            EnsureRoot();
            foreach (var kvp in _defaultSettings) {
                var path = kvp.Key;
                var bucket = GetOrCreateBucket(path);
                for (int i = bucket.inactive.Count; i < bucket.settings.initialSize; i++) {
                    var go = InstantiateForPool(path, "Pooled");
                    if (go == null) {
                        break;
                    }
                    Release(go);
                }
            }
        }

        internal GameObject Acquire(string prefabPath, string displayName, Transform parent = null) {
            if (string.IsNullOrEmpty(prefabPath)) {
                return null;
            }
            PluginLogger.LogDebug($"[PooledObjectService][Acquire] Acquiring object from pool: {prefabPath}");
            EnsureRoot();
            var bucket = GetOrCreateBucket(prefabPath);
            GameObject go = null;
            while (bucket.inactive.Count > 0 && go == null) {
                bucket.inactive.AsEnumerable().ToList().ForEach(g => PluginLogger.LogDebug($"[PooledObjectService][Acquire] Pool contains: {g.name}"));
                go = bucket.inactive.Pop();
                PluginLogger.LogDebug($"[PooledObjectService][Acquire] Popped object from pool: {go.name}");
            }
            if (go == null) {
                go = InstantiateForPool(prefabPath, displayName);
                if (go == null) {
                    return null;
                }
            }
            PluginLogger.LogDebug($"[PooledObjectService][Acquire] Using object: {go.name} as the acquired instance. Setting GO up.");
            go.name = displayName;
            go.transform.SetParent(parent, false);
            go.SetActive(true);
            if (go.TryGetComponent<IPoolable>(out var poolable)) {
                poolable.OnAcquireFromPool();
            }
            return go;
        }

        internal void Release(GameObject go) {
            if (go == null) {
                return;
            }
            PluginLogger.LogDebug($"[PooledObjectService][Release] Releasing object to pool: {go.name}");
            var marker = go.GetComponent<PooledObject>();
            if (marker == null || marker.Owner != this || string.IsNullOrEmpty(marker.PoolKey)) {
                Object.Destroy(go);
                return;
            }

            var bucket = GetOrCreateBucket(marker.PoolKey);
            if (bucket.inactive.Count >= bucket.settings.maxSize) {
                PluginLogger.LogWarning($"[PooledObjectService][Release][DestroyOnRelease] Pool for {marker.PoolKey} is at max capacity {bucket.settings.maxSize}. Destroying object instead of pooling, object name: {go.name}");
                Object.Destroy(go);
                return;
            }

            // Avoid double release of the same object
            if (bucket.inactive.Contains(go)) {
                PluginLogger.LogWarning($"[PooledObjectService][Release] Attempted to release an object that is already in the pool: {go.name}. Ignoring duplicate release.");
                return;
            }
            if (go.TryGetComponent<IPoolable>(out var poolable)) {
                poolable.OnReleaseToPool();
            }
            go.SetActive(false);
            go.transform.SetParent(_poolRoot, false);
            bucket.inactive.Push(go);
        }

        internal void ClearAll() {
            foreach (var bucket in _buckets.Values) {
                while (bucket.inactive.Count > 0) {
                    var go = bucket.inactive.Pop();
                    if (go != null) {
                        Object.Destroy(go);
                    }
                }
            }
            _buckets.Clear();
            if (_poolRoot != null) {
                Object.Destroy(_poolRoot.gameObject);
                _poolRoot = null;
            }
        }
    }
}

