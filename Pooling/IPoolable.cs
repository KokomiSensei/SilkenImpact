namespace SilkenImpact {
    internal interface IPoolable {
        void OnAcquireFromPool();
        void OnReleaseToPool();
    }
}

