using BepInEx.Configuration;
using System.Collections.Generic;

namespace SilkenImpact {
    class MyConfigEntry<T> {
        public string Section;
        public string Key;
        public T DefaultValue;
        public string Description;
        public bool IsAdvanced;
        public int Order;
        public bool Browsable;

        public ConfigEntry<T> Bind(ConfigFile config) {
            var entry = config.Bind(Section, Key, DefaultValue, new ConfigDescription(Description, null, new ConfigurationManagerAttributes {
                IsAdvanced = IsAdvanced,
                Order = Order,
                Browsable = Browsable
            }));
            return entry;
        }
    }

    class MyConfigSection {
        public string Name { get; }
        public ConfigFile configFile;
        private int _nextOrder = int.MaxValue;

        public MyConfigSection(string name, ConfigFile configFile) {
            Name = name;
            this.configFile = configFile;
        }

        public MyConfigSection AddEntry<T>(ref ConfigEntry<T> entry, T defaultValue, string key, string description, bool isAdvanced = false, bool browsable = true) {
            int order = _nextOrder--;
            var myEntry = new MyConfigEntry<T> {
                Section = Name,
                Key = key,
                DefaultValue = defaultValue,
                Description = description,
                IsAdvanced = isAdvanced,
                Order = order,
                Browsable = browsable
            };
            entry = myEntry.Bind(configFile);
            return this;
        }

        public MyConfigSection AddEntry<T>(ref ConfigEntry<T> entry, T defaultValue, (string key, string description) config, bool isAdvanced = false, bool browsable = true) {
            return AddEntry(ref entry, defaultValue, config.key, config.description, isAdvanced, browsable);
        }

        public MyConfigSection AddHiddenEntry<T>(ref ConfigEntry<T> entry, T defaultValue, string key, string description, bool isAdvanced = false) {
            return AddEntry(ref entry, defaultValue, key, description, isAdvanced, false);
        }

        public MyConfigSection AddHiddenEntry<T>(ref ConfigEntry<T> entry, T defaultValue, (string key, string description) config, bool isAdvanced = false) {
            return AddEntry(ref entry, defaultValue, config.key, config.description, isAdvanced, false);
        }

        public MyConfigSection AddAdvancedEntry<T>(ref ConfigEntry<T> entry, T defaultValue, string key, string description, bool browsable = true) {
            return AddEntry(ref entry, defaultValue, key, description, true, browsable);
        }

        public MyConfigSection AddAdvancedEntry<T>(ref ConfigEntry<T> entry, T defaultValue, (string key, string description) config, bool browsable = true) {
            return AddEntry(ref entry, defaultValue, config.key, config.description, true, browsable);
        }

        public MyConfigSection AddDebugOnlyEntry<T>(ref ConfigEntry<T> entry, T defaultValue, string key, string description) {
#if DEBUG
            return AddEntry(ref entry, defaultValue, key, description);
#else
            return AddHiddenEntry(ref entry, defaultValue, key, description, false);
#endif
        }

        public MyConfigSection AddDebugOnlyEntry<T>(ref ConfigEntry<T> entry, T defaultValue, (string key, string description) config) {
#if DEBUG
            return AddEntry(ref entry, defaultValue, config.key, config.description);
#else
            return AddHiddenEntry(ref entry, defaultValue, config.key, config.description, false);
#endif
        }

    }
}