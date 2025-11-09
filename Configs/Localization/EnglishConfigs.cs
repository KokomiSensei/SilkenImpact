namespace SilkenImpact {
    internal class EnglishConfigs {
        public static void Load(Configs configs) {
            var configFile = Plugin.Instance.Config;
            PluginLogger.LogInfo("Loading English Configs");

            // Health Bar Sizes
            new MyConfigSection("User: Health Bar Sizes", configFile)
                .AddEntry(ref configs.shortBarWidth, 2f, "Short Bar Width", "Width of short mob health bars")
                .AddEntry(ref configs.mediumBarWidth, 4f, "Medium Bar Width", "Width of medium mob health bars")
                .AddEntry(ref configs.longBarWidth, 6f, "Long Bar Width", "Width of long mob health bars")
                .AddEntry(ref configs.bossBarWidth, 13f, "Boss Bar Width", "Width of boss mob health bars");

            new MyConfigSection("User: Visibility Control", configFile)
                .AddEntry(ref configs.displayMobHpBar, true, "Display Mob HP Bar", "Whether to display health bars for regular mobs")
                .AddEntry(ref configs.displayBossHpBar, true, "Display Boss HP Bar", "Whether to display health bars for boss mobs")
                .AddEntry(ref configs.displayDamageText, true, "Display Damage Text", "Whether to display damage text upon hit")
                .AddEntry(ref configs.displayHealText, true, "Display Heal Text", "Whether to display healing text")
                .AddEntry(ref configs.displayHpNumbers, false, "Display Health Numbers", "Whether to display health numbers on health bars");

            new MyConfigSection("User: Health Bar Thresholds", configFile)
                .AddAdvancedEntry(ref configs.minMobHp, 5f, "Min Mob HP", "Minimum HP for showing mob health bars(e.g. Some environment objects have HP too)")
                .AddAdvancedEntry(ref configs.minMediumBarHp, 50f, "Min Medium Bar HP", "Minimum HP for switching to medium mob health bars")
                .AddAdvancedEntry(ref configs.minLongBarHp, 100f, "Min Long Bar HP", "Minimum HP for switching to long mob health bars")
                .AddAdvancedEntry(ref configs.minBossBarHp, 120f, "Min Boss Bar HP", "Minimum HP for switching to boss mob health bars. (120 is the HP of Moss Mother)");

            // Health Bar Colors
            new MyConfigSection("User: Health Bar Colors", configFile)
                .AddEntry(ref configs.hpColor, ColourPalette.HP, "HP Color", "Color of the health portion of the health bar")
                .AddEntry(ref configs.delayedEffectColor, ColourPalette.DelayedEffect, "Delayed Effect Color", "Color of the delayed effect portion of the health bar")
                .AddEntry(ref configs.hpBarBackgroundColor, ColourPalette.HpBarBackground, "HP Bar Background Color", "Color of the health bar background")
                .AddEntry(ref configs.hpNumberColor, ColourPalette.HpNumber, "HP Number Color", "Color of the health numbers on the health bar");

            // Damage Text Colors
            new MyConfigSection("User: Damage Text Colors", configFile)
                .AddEntry(ref configs.defaultColor, ColourPalette.HornetDress, "Default Color", "Color of default damage text")
                .AddEntry(ref configs.critHitColor, ColourPalette.Geo, "Crit Hit Color", "Color of critical hit damage text")
                .AddEntry(ref configs.poisonColor, ColourPalette.Electro, "Poison Color", "Color of poison damage text")
                .AddEntry(ref configs.fireColor, ColourPalette.Pyro, "Fire Color", "Color of fire damage text")
                .AddEntry(ref configs.healTextColor, ColourPalette.HealTextColor, "Heal Color", "Color of healing text");

            // Developer Settings
            new MyConfigSection("Dev: Visibility Controller Settings", configFile)
                .AddHiddenEntry(ref configs.maxZPosition, 1f, "Max Z Position", "Minimum absolute Z position to be considered as shown")
                .AddHiddenEntry(ref configs.visibleCacheSeconds, 0.5f, "Visible Cache Seconds", "Time in seconds to cache visibility state")
                .AddHiddenEntry(ref configs.invisibleCacheSeconds, 0.5f, "Invisible Cache Seconds", "Time in seconds to cache visibility state")
                .AddHiddenEntry(ref configs.infHp, 10000f, "Infinite HP Threshold", "HP value considered as infinite HP");

            // Damage Text Settings
            new MyConfigSection("Dev: Damage Text Settings", configFile)
                .AddHiddenEntry(ref configs.weightOfNewHit, 0.2f, "Weight Of New Hit", "Weight of new hit in exponential moving average calculation.");

        }
    }
}