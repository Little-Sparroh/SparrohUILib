using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Mycopunk-inspired color palette for consistent mod UI.
    /// Surfaces use deep teal/slate tones; accents are vivid bioluminescent colors.
    /// </summary>
    public static class UIColors
    {
        // ── Accent colors (values, highlights, status) ──────────────────────

        /// <summary>Soft cyan — speed, ammo, secondary stats.</summary>
        public static readonly Color Sky = new Color(0.45f, 0.85f, 0.95f, 1f);

        /// <summary>Coral red — damage, danger, kills.</summary>
        public static readonly Color Rose = new Color(0.95f, 0.30f, 0.35f, 1f);

        /// <summary>Bioluminescent green — altitude, range, positive status.</summary>
        public static readonly Color Shamrock = new Color(0.20f, 0.90f, 0.45f, 1f);

        /// <summary>Warm gold — fire rate, burst, timers.</summary>
        public static readonly Color Macaroon = new Color(0.98f, 0.82f, 0.35f, 1f);

        /// <summary>Magenta/orchid — explosion, special stats.</summary>
        public static readonly Color Orchid = new Color(0.85f, 0.40f, 0.90f, 1f);

        /// <summary>Amber gold — boss timers, warnings.</summary>
        public static readonly Color Amber = new Color(1.00f, 0.75f, 0.15f, 1f);

        /// <summary>Electric blue — interactive highlights, links.</summary>
        public static readonly Color Electric = new Color(0.30f, 0.65f, 1.00f, 1f);

        /// <summary>Soft white for primary text.</summary>
        public static readonly Color TextPrimary = new Color(0.95f, 0.96f, 0.98f, 1f);

        /// <summary>Muted text for labels and secondary info.</summary>
        public static readonly Color TextSecondary = new Color(0.70f, 0.74f, 0.80f, 1f);

        /// <summary>Dimmed / disabled text.</summary>
        public static readonly Color TextMuted = new Color(0.50f, 0.54f, 0.60f, 1f);

        // ── Surface colors (panels, chrome) ─────────────────────────────────

        /// <summary>Main window / panel background — deep slate with teal undertone.</summary>
        public static readonly Color PanelBg = new Color(0.08f, 0.10f, 0.14f, 0.94f);

        /// <summary>Slightly elevated surface (cards, entries).</summary>
        public static readonly Color Surface = new Color(0.12f, 0.15f, 0.20f, 0.96f);

        /// <summary>Title bar / header strip.</summary>
        public static readonly Color TitleBar = new Color(0.10f, 0.18f, 0.24f, 1f);

        /// <summary>Section header bar.</summary>
        public static readonly Color SectionBar = new Color(0.12f, 0.22f, 0.30f, 1f);

        /// <summary>Row / entry background.</summary>
        public static readonly Color EntryBg = new Color(0.14f, 0.17f, 0.22f, 0.90f);

        /// <summary>Scroll viewport background.</summary>
        public static readonly Color ScrollBg = new Color(0.06f, 0.08f, 0.11f, 0.85f);

        /// <summary>Tooltip background.</summary>
        public static readonly Color TooltipBg = new Color(0.06f, 0.08f, 0.12f, 0.96f);

        /// <summary>Subtle border / outline color.</summary>
        public static readonly Color Border = new Color(0.25f, 0.40f, 0.50f, 0.70f);

        /// <summary>Accent border (focused / active).</summary>
        public static readonly Color BorderAccent = new Color(0.30f, 0.70f, 0.85f, 0.90f);

        /// <summary>Separator line.</summary>
        public static readonly Color Separator = new Color(0.30f, 0.40f, 0.50f, 0.40f);

        // ── Interactive states ──────────────────────────────────────────────
        // Default = slate utility, Primary = teal constructive, Danger = coral destructive,
        // Active = green selected. Each style has its own hover/pressed for clear feedback.

        public static readonly Color ButtonNormal = new Color(0.16f, 0.22f, 0.30f, 1f);
        public static readonly Color ButtonHover = new Color(0.26f, 0.38f, 0.50f, 1f);
        public static readonly Color ButtonPressed = new Color(0.12f, 0.17f, 0.24f, 1f);

        public static readonly Color ButtonPrimary = new Color(0.14f, 0.42f, 0.62f, 1f);
        public static readonly Color ButtonPrimaryHover = new Color(0.22f, 0.55f, 0.80f, 1f);
        public static readonly Color ButtonPrimaryPressed = new Color(0.10f, 0.32f, 0.48f, 1f);

        public static readonly Color ButtonDanger = new Color(0.58f, 0.18f, 0.22f, 1f);
        public static readonly Color ButtonDangerHover = new Color(0.78f, 0.28f, 0.32f, 1f);
        public static readonly Color ButtonDangerPressed = new Color(0.42f, 0.12f, 0.15f, 1f);

        public static readonly Color ButtonActive = new Color(0.16f, 0.48f, 0.40f, 1f);
        public static readonly Color ButtonActiveHover = new Color(0.22f, 0.60f, 0.50f, 1f);
        public static readonly Color ButtonActivePressed = new Color(0.12f, 0.36f, 0.30f, 1f);


        public static readonly Color ToggleOff = new Color(0.25f, 0.28f, 0.32f, 1f);
        public static readonly Color ToggleOn = new Color(0.18f, 0.55f, 0.40f, 1f);
        public static readonly Color Checkmark = new Color(0.30f, 0.95f, 0.55f, 1f);

        public static readonly Color InputBg = new Color(0.10f, 0.13f, 0.18f, 0.95f);
        public static readonly Color InputText = new Color(0.92f, 0.94f, 0.96f, 1f);

        public static readonly Color SliderTrack = new Color(0.15f, 0.18f, 0.24f, 1f);
        public static readonly Color SliderFill = new Color(0.25f, 0.65f, 0.80f, 1f);
        public static readonly Color SliderHandle = new Color(0.90f, 0.93f, 0.96f, 1f);

        public static readonly Color ProgressTrack = new Color(0.12f, 0.15f, 0.20f, 1f);
        public static readonly Color ProgressFill = new Color(0.25f, 0.75f, 0.55f, 1f);

        public static readonly Color TabInactive = new Color(0.12f, 0.16f, 0.22f, 1f);
        public static readonly Color TabActive = new Color(0.18f, 0.30f, 0.40f, 1f);
        public static readonly Color TabHover = new Color(0.16f, 0.24f, 0.32f, 1f);

        // ── Status ──────────────────────────────────────────────────────────

        public static readonly Color Success = Shamrock;
        public static readonly Color Warning = Amber;
        public static readonly Color Error = Rose;
        public static readonly Color Info = Sky;

        /// <summary>
        /// Try to pull game UI colors when Global is available; otherwise use library defaults.
        /// </summary>
        public static Color GameRed
        {
            get
            {
                try
                {
                    if (Global.Instance != null)
                        return Global.Instance.RedUIColor;
                }
                catch { /* Global may not be ready */ }
                return Rose;
            }
        }

        public static Color GameGreen
        {
            get
            {
                try
                {
                    if (Global.Instance != null)
                        return Global.Instance.GreenUIColor;
                }
                catch { /* Global may not be ready */ }
                return Shamrock;
            }
        }

        public static string ToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Parse a hex color string. Accepts RRGGBB, #RRGGBB, RRGGBBAA, #RRGGBBAA.
        /// </summary>
        public static bool TryParseHex(string hex, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex))
                return false;

            string s = hex.Trim();
            if (s.StartsWith("#"))
                s = s.Substring(1);

            if (s.Length != 6 && s.Length != 8)
                return false;

            // ColorUtility expects #RRGGBB or #RRGGBBAA
            return ColorUtility.TryParseHtmlString("#" + s, out color);
        }

        /// <summary>
        /// Parse hex or return <paramref name="fallback"/> when invalid/empty.
        /// </summary>
        public static Color ParseHex(string hex, Color fallback)
        {
            return TryParseHex(hex, out Color c) ? c : fallback;
        }
    }
}

