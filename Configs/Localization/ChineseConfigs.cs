namespace SilkenImpact {
    internal class ChineseConfigs {
        public static void Load(Configs configs) {
            var configFile = Plugin.Instance.Config;
            PluginLogger.LogInfo("加载中文配置");

            // 血条大小设置
            new MyConfigSection("用户: 血条大小设置", configFile)
                .AddEntry(ref configs.shortBarWidth, 2f, "小型血条宽度", "小型敌人血条的宽度")
                .AddEntry(ref configs.mediumBarWidth, 4f, "中型血条宽度", "中型敌人血条的宽度")
                .AddEntry(ref configs.longBarWidth, 6f, "大型血条宽度", "大型敌人血条的宽度")
                .AddEntry(ref configs.bossBarWidth, 13f, "Boss血条宽度", "Boss血条的宽度");

            // 显示控制设置
            new MyConfigSection("用户: 显示控制", configFile)
                .AddEntry(ref configs.displayMobHpBar, true, "显示普通怪血条", "是否显示普通怪物的血条")
                .AddEntry(ref configs.displayBossHpBar, true, "显示Boss血条", "是否显示Boss的血条")
                .AddEntry(ref configs.displayDamageText, true, "显示伤害数字", "是否显示伤害数字")
                .AddEntry(ref configs.displayHealText, true, "显示治疗数字", "是否显示治疗数字")
                .AddEntry(ref configs.displayHpNumbers, false, "显示血量数字", "是否在血条上显示血量数字");

            // 血条阈值设置
            new MyConfigSection("用户: 血条阈值设置", configFile)
                .AddAdvancedEntry(ref configs.minMobHp, 5f, "最小显示血条生命值", "显示血条所需的最小生命值（某些环境物体也有生命值）")
                .AddAdvancedEntry(ref configs.minMediumBarHp, 50f, "中型血条最小生命值", "切换到中型血条所需的最小生命值")
                .AddAdvancedEntry(ref configs.minLongBarHp, 100f, "大型血条最小生命值", "切换到大型血条所需的最小生命值")
                .AddAdvancedEntry(ref configs.minBossBarHp, 120f, "Boss血条最小生命值", "切换到Boss血条所需的最小生命值（苔藓之母的生命值为120）");

            // 血条颜色设置
            new MyConfigSection("用户: 血条颜色设置", configFile)
                .AddEntry(ref configs.hpColor, ColourPalette.HP, "生命值颜色", "血条中生命值部分的颜色")
                .AddEntry(ref configs.delayedEffectColor, ColourPalette.DelayedEffect, "延迟效果颜色", "血条中延迟效果部分的颜色")
                .AddEntry(ref configs.hpBarBackgroundColor, ColourPalette.HpBarBackground, "血条背景颜色", "血条背景的颜色")
                .AddEntry(ref configs.hpNumberColor, ColourPalette.HpNumber, "血量数字颜色", "血条上血量数字的颜色");

            // 伤害数字颜色设置
            new MyConfigSection("用户: 伤害数字颜色设置", configFile)
                .AddEntry(ref configs.defaultColor, ColourPalette.HornetDress, "默认颜色", "默认伤害数字的颜色")
                .AddEntry(ref configs.critHitColor, ColourPalette.Geo, "暴击颜色", "暴击伤害数字的颜色")
                .AddEntry(ref configs.poisonColor, ColourPalette.Electro, "中毒颜色", "中毒伤害数字的颜色")
                .AddEntry(ref configs.fireColor, ColourPalette.Pyro, "燃烧颜色", "燃烧伤害数字的颜色")
                .AddEntry(ref configs.healTextColor, ColourPalette.HealTextColor, "治疗颜色", "治疗数字的颜色");

            // 开发者可见性控制设置
            new MyConfigSection("开发: 可见性控制设置", configFile)
                .AddHiddenEntry(ref configs.maxZPosition, 1f, "最大Z坐标", "被视为可见的最大绝对Z坐标")
                .AddHiddenEntry(ref configs.visibleCacheSeconds, 0.5f, "可见状态缓存时间", "可见状态的缓存时间（秒）")
                .AddHiddenEntry(ref configs.invisibleCacheSeconds, 0.5f, "不可见状态缓存时间", "不可见状态的缓存时间（秒）")
                .AddHiddenEntry(ref configs.infHp, 10000f, "无限生命值阈值", "被视为无限生命值的阈值");

            // 开发者伤害数字设置
            new MyConfigSection("开发: 伤害数字设置", configFile)
                .AddHiddenEntry(ref configs.weightOfNewHit, 0.2f, "新伤害权重", "指数平均计算中新伤害的权重");
        }
    }
}
