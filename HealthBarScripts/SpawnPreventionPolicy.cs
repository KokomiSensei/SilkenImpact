namespace SilkenImpact {
    class SpawnPreventionPolicy {
        public static float minMobHealth => Configs.Instance.minMobHp.Value;
        public static float INF => Configs.Instance.infHp.Value;
        public static bool ShouldPreventSpawn(HealthManager hm) {
            if (hm.hp < minMobHealth) {
                return true;
            }
            if (hm.hp >= INF) {
                return true;
            }
            if (hm.SendDamageTo != null) {
                Plugin.Logger.LogWarning($"{hm.gameObject.name}.HealthManager.SendDamageTo is {hm.SendDamageTo.name}, skipping health bar spawn.");
                return true;
            }

            return false;
        }
    }

}