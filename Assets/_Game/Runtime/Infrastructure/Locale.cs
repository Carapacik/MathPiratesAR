using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MathPirates.Infrastructure
{
    public static class Locale
    {
        [Serializable] private sealed class Catalog { public Entry[] entries; }
        [Serializable] private sealed class Entry { public string key; public string value; }
        private static Dictionary<string, string> strings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => strings = null;

        private static Dictionary<string, string> ReadCatalog(string language)
        {
            var result = new Dictionary<string, string>();
            foreach (string suffix in new[] { "", "-levels" })
            {
                var asset = Resources.Load<TextAsset>($"Localization/{language}{suffix}");
                if (asset == null) throw new InvalidOperationException($"Missing localization catalog: {language}{suffix}");
                foreach (var entry in JsonUtility.FromJson<Catalog>(asset.text).entries)
                    result.Add(entry.key, entry.value);
            }
            return result;
        }

        private static void Initialize(SystemLanguage language)
        {
            string selected = language == SystemLanguage.Russian ? "ru" : "en";
            strings = ReadCatalog("en");
            if (selected != "en")
                foreach (var entry in ReadCatalog(selected)) strings[entry.Key] = entry.Value;
        }

        private static void EnsureInitialized()
        {
            if (strings == null) Initialize(Application.systemLanguage);
        }

        public static bool Has(string key) { EnsureInitialized(); return key != null && strings.ContainsKey(key); }
        public static string Text(string key)
        {
            EnsureInitialized();
            return strings.TryGetValue(key, out string value) ? value : throw new KeyNotFoundException($"Missing localized text: {key}");
        }
        public static string Format(string key, params object[] values) => string.Format(CultureInfo.InvariantCulture, Text(key), values);
    }
}
