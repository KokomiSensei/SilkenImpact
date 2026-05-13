namespace SilkenImpact {
    internal struct ObjectPoolSettings {
        public int initialSize;
        public int maxSize;

        public ObjectPoolSettings(int initialSize, int maxSize) {
            this.initialSize = initialSize;
            this.maxSize = maxSize;
        }
    }
}

