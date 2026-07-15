using System;
using BepInEx.Configuration;
using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Binds a hex-string <see cref="ConfigEntry{T}"/> and caches the parsed <see cref="Color"/>.
    /// Invalid or empty values fall back to the color supplied at construction.
    /// </summary>
    public sealed class ConfigColor
    {
        private readonly ConfigEntry<string> _entry;
        private readonly Color _fallback;
        private Color _cached;

        public ConfigEntry<string> Entry => _entry;
        public Color Value => _cached;
        public string Hex => _entry.Value;

        public ConfigColor(ConfigEntry<string> entry, Color fallback)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            _fallback = fallback;
            _cached = Parse(_entry.Value);
            _entry.SettingChanged += OnSettingChanged;
        }

        /// <summary>
        /// Bind a hex color config entry and wrap it.
        /// Default value is written as RRGGBB from <paramref name="fallback"/>.
        /// </summary>
        public static ConfigColor Bind(
            ConfigFile config,
            string section,
            string key,
            Color fallback,
            string description = null)
        {
            string defaultHex = UIColors.ToHex(fallback);
            string desc = description ?? $"Rich-text value color as hex (RRGGBB or #RRGGBB). Default: {defaultHex}.";
            var entry = config.Bind(section, key, defaultHex, desc);
            return new ConfigColor(entry, fallback);
        }

        /// <summary>
        /// Re-parse from the current config value (e.g. after ConfigFile.Reload()).
        /// </summary>
        public void Refresh()
        {
            _cached = Parse(_entry.Value);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private Color Parse(string hex)
        {
            return UIColors.ParseHex(hex, _fallback);
        }

        public static implicit operator Color(ConfigColor c) => c != null ? c.Value : default;
    }
}
