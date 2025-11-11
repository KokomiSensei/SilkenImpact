using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SilkenImpact {
    [Serializable]
    public class ConfigEntry {
        [JsonProperty] public string key;
        [JsonProperty] public string description;
    }

    [Serializable]
    public class ConfigSection {
        [JsonProperty] public string name;
        [JsonProperty] public Dictionary<string, ConfigEntry> entries = new Dictionary<string, ConfigEntry>();

        public ConfigEntry this[string key] => entries[key];

        // Helper method for creating chain-friendly tuple
        public (string key, string description) Get(string key) {
            var entry = entries[key];
            return (entry.key, entry.description);
        }
    }

    [Serializable]
    public class LocalizationData {
        [JsonProperty("sections")]
        private Dictionary<string, ConfigSection> sections = new Dictionary<string, ConfigSection>();

        public ConfigSection this[string sectionName] => sections[sectionName];
    }

    internal class LocalizedConfigs {
        private static LocalizationData localization;
        private static LanguageOption? loadedLanguage = null;
        protected static string JsonFileName(LanguageOption option) {
            return option switch {
                LanguageOption.English => "en.json",
                LanguageOption.简体中文 => "zh.json",
                LanguageOption.Deutsch => "de.json",
                LanguageOption.Español => "es.json",
                LanguageOption.Français => "fr.json",
                LanguageOption.Italiano => "it.json",
                LanguageOption.日本語 => "ja.json",
                LanguageOption.한국어 => "ko.json",
                LanguageOption.Português => "pt.json",
                LanguageOption.Русский => "ru.json",
                _ => "en.json",
            };
        }

        private static void LoadLocalizationIfNeeded(LanguageOption option) {
            if (loadedLanguage == null || loadedLanguage != option) {
                loadedLanguage = option;
            }
            try {
                string path = Path.Combine(Plugin.AssetsFolder, "Localization", JsonFileName(loadedLanguage.Value));
                string json = File.ReadAllText(path);
                localization = JsonConvert.DeserializeObject<LocalizationData>(json);
            } catch (Exception e) {
                loadedLanguage = null;
                throw e;
            }
        }
        public static void Load(Configs configs, LanguageOption option) {
            try {
                LoadLocalizationIfNeeded(option);
            } catch (Exception e) {
                PluginLogger.LogFatal($"Failed to load localization: {e}");
                return;
            }
            var configFile = Plugin.Instance.Config;
            // Health Bar Sizes
            var barSizes = localization["User: Health Bar Sizes"];
            new MyConfigSection(barSizes.name, Plugin.Instance.Config)
                .AddEntry(ref configs.shortBarWidth, 2f, barSizes.Get("Short Bar Width"))
                .AddEntry(ref configs.mediumBarWidth, 4f, barSizes.Get("Medium Bar Width"))
                .AddEntry(ref configs.longBarWidth, 6f, barSizes.Get("Long Bar Width"))
                .AddEntry(ref configs.bossBarWidth, 13f, barSizes.Get("Boss Bar Width"));

            // Visibility Control
            var visibility = localization["User: Visibility Control"];
            new MyConfigSection(visibility.name, configFile)
                .AddEntry(ref configs.displayMobHpBar, true, visibility.Get("Display Mob HP Bar"))
                .AddEntry(ref configs.displayBossHpBar, true, visibility.Get("Display Boss HP Bar"))
                .AddEntry(ref configs.displayDamageText, true, visibility.Get("Display Damage Text"))
                .AddEntry(ref configs.displayHealText, true, visibility.Get("Display Heal Text"))
                .AddEntry(ref configs.displayHpNumbers, false, visibility.Get("Display Health Numbers"));

            // Health Bar Thresholds
            var thresholds = localization["User: Health Bar Thresholds"];
            new MyConfigSection(thresholds.name, configFile)
                .AddAdvancedEntry(ref configs.minMobHp, 5f, thresholds.Get("Min Mob HP"))
                .AddAdvancedEntry(ref configs.minMediumBarHp, 50f, thresholds.Get("Min Medium Bar HP"))
                .AddAdvancedEntry(ref configs.minLongBarHp, 100f, thresholds.Get("Min Long Bar HP"))
                .AddAdvancedEntry(ref configs.minBossBarHp, 120f, thresholds.Get("Min Boss Bar HP"));

            // Health Bar Colors
            var colors = localization["User: Health Bar Colors"];
            new MyConfigSection(colors.name, configFile)
                .AddEntry(ref configs.hpColor, ColourPalette.HP, colors.Get("HP Color"))
                .AddEntry(ref configs.delayedEffectColor, ColourPalette.DelayedEffect, colors.Get("Delayed Effect Color"))
                .AddEntry(ref configs.hpBarBackgroundColor, ColourPalette.HpBarBackground, colors.Get("HP Bar Background Color"))
                .AddEntry(ref configs.hpNumberColor, ColourPalette.HpNumber, colors.Get("HP Number Color"));

            // Damage Text Colors
            var textColors = localization["User: Damage Text Colors"];
            new MyConfigSection(textColors.name, configFile)
                .AddEntry(ref configs.defaultColor, ColourPalette.HornetDress, textColors.Get("Default Color"))
                .AddEntry(ref configs.critHitColor, ColourPalette.Geo, textColors.Get("Crit Hit Color"))
                .AddEntry(ref configs.poisonColor, ColourPalette.Electro, textColors.Get("Poison Color"))
                .AddEntry(ref configs.fireColor, ColourPalette.Pyro, textColors.Get("Fire Color"))
                .AddEntry(ref configs.healTextColor, ColourPalette.HealTextColor, textColors.Get("Heal Color"));

            // Developer Settings
            var visibilitySettings = localization["Dev: Visibility Controller Settings"];
            new MyConfigSection(visibilitySettings.name, configFile)
                .AddDebugOnlyEntry(ref configs.maxZPosition, 1f, visibilitySettings.Get("Max Z Position"))
                .AddDebugOnlyEntry(ref configs.visibleCacheSeconds, 0.5f, visibilitySettings.Get("Visible Cache Seconds"))
                .AddDebugOnlyEntry(ref configs.invisibleCacheSeconds, 0.5f, visibilitySettings.Get("Invisible Cache Seconds"))
                .AddDebugOnlyEntry(ref configs.infHp, 10000f, visibilitySettings.Get("Infinite HP Threshold"))
                .AddDebugOnlyEntry(ref configs.spriteTrackerZOffset, -1.5f, visibilitySettings.Get("Sprite Tracker Z Offset"));

            // Damage Text Settings
            var damageTextSettings = localization["Dev: Damage Text Settings"];
            new MyConfigSection(damageTextSettings.name, configFile)
                .AddDebugOnlyEntry(ref configs.weightOfNewHit, 0.2f, damageTextSettings.Get("Weight Of New Hit"));

        }
    }
}