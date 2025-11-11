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
        public ConfigEntry<float> infHp;

        public ConfigEntry<bool> displayMobHpBar;
        public ConfigEntry<bool> displayBossHpBar;
        public ConfigEntry<float> spriteTrackerZOffset;

        public ConfigEntry<float> maxZPosition;
        public ConfigEntry<float> visibleCacheSeconds;
        public ConfigEntry<float> invisibleCacheSeconds;

        public ConfigEntry<Color> hpColor;
        public ConfigEntry<Color> delayedEffectColor;
        public ConfigEntry<Color> hpBarBackgroundColor;
        public ConfigEntry<Color> hpNumberColor;
        public ConfigEntry<bool> displayHpNumbers;

        #endregion

        #region Damage Text Configs
        public ConfigEntry<Color> defaultColor;
        public ConfigEntry<Color> poisonColor;
        public ConfigEntry<Color> fireColor;
        public ConfigEntry<Color> critHitColor;
        public ConfigEntry<Color> healTextColor;

        public ConfigEntry<float> weightOfNewHit;
        public ConfigEntry<bool> displayDamageText;
        public ConfigEntry<bool> displayHealText;
        #endregion


        private ConfigFile config => Plugin.Instance.Config;

        internal ConfigEntry<LanguageOption> selectedLanguage;

        void Awake() {
            if (__instance != null && __instance != this) {
                Destroy(this.gameObject);
                return;
            }
            BindLanguage(LanguageOption.English);
            LoadLanguageConfigs(selectedLanguage.Value);
        }

        void BindLanguage(LanguageOption defaultOption) {
            new MyConfigSection("Language", config)
                .AddAdvancedEntry(ref selectedLanguage, defaultOption, "Selected Language", "Choose config language (requires reopening the config manager window)");
            selectedLanguage.SettingChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, System.EventArgs e) {
            var autoSave = config.SaveOnConfigSet;
            config.SaveOnConfigSet = false;

            try {
                var newLanguage = ((SettingChangedEventArgs)e).ChangedSetting.BoxedValue;
                var currentLang = selectedLanguage;

                config.Clear();
                LanguageOption newLang = newLanguage as LanguageOption? ?? LanguageOption.English;
                LoadLanguageConfigs(newLang);
                BindLanguage(newLang);

                PluginLogger.LogInfo("Language changed to " + newLanguage + ". Restart game to apply config descriptions.");
            } finally {
                config.SaveOnConfigSet = autoSave;
            }
        }

        private void LoadLanguageConfigs(LanguageOption language) {
            LocalizedConfigs.Load(this, language);
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