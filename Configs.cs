using BepInEx;
using BepInEx.Configuration;
using System.Collections;
using UnityEngine;

namespace SilkenImpact {
    public class Configs : MonoBehaviour {
        private static Configs __instance;
        public static Configs Instance {
            get {
                if (__instance == null) {
                    GameObject go = new GameObject("Configs");
                    __instance = go.AddComponent<Configs>();
                    DontDestroyOnLoad(go);
                }
                return __instance;
            }
        }
        # region Health Bar Configs
        public ConfigEntry<float> shortBarWidth;
        public ConfigEntry<float> mediumBarWidth;
        public ConfigEntry<float> longBarWidth;
        public ConfigEntry<float> bossBarWidth;

        public ConfigEntry<float> minMobHp;
        public ConfigEntry<float> minMediumBarHp;
        public ConfigEntry<float> minLongBarHp;
        public ConfigEntry<float> minBossBarHp;


        public ConfigEntry<float> maxZPosition;
        public ConfigEntry<float> visibleCacheSeconds;
        public ConfigEntry<float> invisibleCacheSeconds;

        #endregion

        #region Damage Text Configs
        public ConfigEntry<Color> defaultColor;
        public ConfigEntry<Color> poisonColor;
        public ConfigEntry<Color> fireColor;
        public ConfigEntry<Color> critHitColor;

        public ConfigEntry<float> weightOfNewHit;

        #endregion


        private ConfigFile config => Plugin.Instance.Config;

        void Awake() {
            shortBarWidth = config.Bind("Health Bar Sizes", "Short Bar Width", 2f, "Width of short mob health bars");
            mediumBarWidth = config.Bind("Health Bar Sizes", "Medium Bar Width", 4f, "Width of medium mob health bars");
            longBarWidth = config.Bind("Health Bar Sizes", "Long Bar Width", 6f, "Width of long mob health bars");
            bossBarWidth = config.Bind("Health Bar Sizes", "Boss Bar Width", 11f, "Width of boss mob health bars");

            minMobHp = config.Bind("Health Bar Sizes", "Min Mob HP", 5f, "Minimum HP for showing mob health bars(e.g. Some environment objects have HP too)");
            minMediumBarHp = config.Bind("Health Bar Sizes", "Min Medium Bar HP", 50f, "Minimum HP for switching to medium mob health bars");
            minLongBarHp = config.Bind("Health Bar Sizes", "Min Long Bar HP", 100f, "Minimum HP for switching to long mob health bars");
            minBossBarHp = config.Bind("Health Bar Sizes", "Min Boss Bar HP", 120f, "Minimum HP for switching to boss mob health bars. (120 is the HP of Moss Mother)");

            defaultColor = config.Bind("Damage Text Colors", "Default Color", ColourPalette.HornetDress, "Color of default damage text");
            critHitColor = config.Bind("Damage Text Colors", "Crit Hit Color", ColourPalette.Geo, "Color of critical hit damage text");
            poisonColor = config.Bind("Damage Text Colors", "Poison Color", ColourPalette.Electro, "Color of poison damage text");
            fireColor = config.Bind("Damage Text Colors", "Fire Color", ColourPalette.Pyro, "Color of fire damage text");

            weightOfNewHit = config.Bind("Damage Text Settings", "Weight Of New Hit", 0.2f, "Weight of new hit in exponential moving average calculation.");

            maxZPosition = config.Bind("Developer Settings", "Max Z Position", 1f, "Minimum absolute Z position to be considered as shown");
            visibleCacheSeconds = config.Bind("Developer Settings", "Visible Cache Seconds", 0.5f, "Time in seconds to cache visibility state");
            invisibleCacheSeconds = config.Bind("Developer Settings", "Invisible Cache Seconds", 0.5f, "Time in seconds to cache visibility state");
        }

        public float GetHpBarWidth(float maxHp, bool isBoss) {
            if (isBoss) {
                return bossBarWidth.Value;
            }
            if (maxHp >= minLongBarHp.Value) {
                return longBarWidth.Value;
            }
            if (maxHp >= minMediumBarHp.Value) {
                return mediumBarWidth.Value;
            }
            return shortBarWidth.Value;
        }
    }
}