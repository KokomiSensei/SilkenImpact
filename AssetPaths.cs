namespace SilkenImpact {
    internal static class AssetPaths {
        internal static class Prefabs {
            internal const string DamageText = "Assets/Addressables/Prefabs/DamageOldText.prefab";
            internal const string HealText = "Assets/Addressables/Prefabs/HealOldText.prefab";
            internal const string BossContainer = "Assets/Addressables/Prefabs/Container.prefab";
        }

        internal static class HealthBars {
            internal const string RoundedMob = "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/Rounded.prefab";
            internal const string DiamondMob = "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/Diamond.prefab";
            internal const string RoundedBoss = "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/RoundedBoss.prefab";
            internal const string DiamondBoss = "Assets/Addressables/Prefabs/Health Bars/Masked UI Health Bars/DiamondBoss.prefab";

            internal static string ForMobShape(HealthBarShape shape) {
                return shape switch {
                    HealthBarShape.Diamond => DiamondMob,
                    _ => RoundedMob,
                };
            }

            internal static string ForBossShape(HealthBarShape shape) {
                return shape switch {
                    HealthBarShape.Diamond => DiamondBoss,
                    _ => RoundedBoss,
                };
            }
        }
    }
}

