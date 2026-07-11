using TMPro;
using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Central theme: font sizes, spacing, and resolution-aware scaling.
    /// Reference design resolution is 1920x1080; scale factors adapt to the current screen.
    /// </summary>
    public static class UITheme
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        // ── Font sizes (reference 1080p; use Scaled* for runtime) ────────────

        public static float FontTitle = 24f;
        public static float FontHeader = 20f;
        public static float FontBody = 16f;
        public static float FontHud = 18f;
        public static float FontSmall = 13f;
        public static float FontTiny = 11f;

        // ── Spacing / sizing (reference pixels) ─────────────────────────────

        public static float PaddingSmall = 6f;
        public static float PaddingMedium = 12f;
        public static float PaddingLarge = 20f;
        public static float SpacingTight = 4f;
        public static float SpacingNormal = 8f;
        public static float SpacingLoose = 14f;

        public static float ButtonHeight = 32f;
        public static float InputHeight = 30f;
        public static float ToggleSize = 22f;
        public static float SliderHeight = 20f;
        public static float ProgressHeight = 12f;
        public static float TitleBarHeight = 44f;
        public static float SeparatorHeight = 2f;
        public static float TabHeight = 34f;
        public static float BorderWidth = 1.5f;
        public static float CornerRadiusHint = 4f; // visual only; no mask radius without sprites

        public static float HudLineHeight = 22f;
        public static float HudDefaultWidth = 320f;

        public static int WindowSortingOrder = 100;
        public static int TooltipSortingOrder = 250;
        public static int DialogSortingOrder = 200;

        public static float WindowMinWidth = 320f;
        public static float WindowMinHeight = 200f;

        private static bool _initialized;
        private static TMP_FontAsset _cachedFont;

        public static void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;
            TryCacheFont();
        }

        // ── Resolution scaling ──────────────────────────────────────────────

        /// <summary>
        /// Scale factor based on the shorter screen axis vs 1080p.
        /// Clamped so UI stays usable on very small or very large displays.
        /// </summary>
        public static float Scale
        {
            get
            {
                float h = Screen.height > 0 ? Screen.height : ReferenceHeight;
                float s = h / ReferenceHeight;
                return Mathf.Clamp(s, 0.65f, 1.75f);
            }
        }

        /// <summary>Width scale (for horizontal-only adjustments).</summary>
        public static float ScaleX
        {
            get
            {
                float w = Screen.width > 0 ? Screen.width : ReferenceWidth;
                float s = w / ReferenceWidth;
                return Mathf.Clamp(s, 0.65f, 1.75f);
            }
        }

        public static float S(float referencePixels) => referencePixels * Scale;

        public static Vector2 S(Vector2 reference) => reference * Scale;

        public static RectOffset ScaledPadding(int left, int right, int top, int bottom)
        {
            float s = Scale;
            return new RectOffset(
                Mathf.RoundToInt(left * s),
                Mathf.RoundToInt(right * s),
                Mathf.RoundToInt(top * s),
                Mathf.RoundToInt(bottom * s));
        }

        public static float ScaledFontTitle => S(FontTitle);
        public static float ScaledFontHeader => S(FontHeader);
        public static float ScaledFontBody => S(FontBody);
        public static float ScaledFontHud => S(FontHud);
        public static float ScaledFontSmall => S(FontSmall);
        public static float ScaledFontTiny => S(FontTiny);

        public static float ScaledButtonHeight => S(ButtonHeight);
        public static float ScaledInputHeight => S(InputHeight);
        public static float ScaledTitleBarHeight => S(TitleBarHeight);
        public static float ScaledHudLineHeight => S(HudLineHeight);

        // ── Fonts ───────────────────────────────────────────────────────────

        /// <summary>
        /// Best available TMP font. Prefers a font already used by the game UI,
        /// falls back to TMP defaults / Resources.
        /// </summary>
        public static TMP_FontAsset Font
        {
            get
            {
                if (_cachedFont != null)
                    return _cachedFont;
                TryCacheFont();
                return _cachedFont;
            }
        }

        private static void TryCacheFont()
        {
            // Prefer an existing in-scene TMP text font (game UI)
            var existing = Object.FindObjectOfType<TextMeshProUGUI>();
            if (existing != null && existing.font != null)
            {
                _cachedFont = existing.font;
                return;
            }

            // TMP resources
            var fromResources = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (fromResources != null)
            {
                _cachedFont = fromResources;
                return;
            }

            // Last resort: TMP default
            if (TMP_Settings.defaultFontAsset != null)
                _cachedFont = TMP_Settings.defaultFontAsset;
        }

        /// <summary>
        /// Apply standard text styling to a TMP component.
        /// </summary>
        public static void ApplyTextStyle(
            TextMeshProUGUI tmp,
            float fontSize,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left,
            bool wrap = false)
        {
            if (tmp == null)
                return;

            if (Font != null)
                tmp.font = Font;

            tmp.fontSize = fontSize;
            tmp.color = color ?? UIColors.TextPrimary;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = wrap;
            tmp.richText = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
        }

        /// <summary>
        /// Reference size converted for current resolution, useful for window sizeDelta.
        /// Uses uniform Scale (height-based) so aspect ratio changes don't stretch UI oddly.
        /// </summary>
        public static Vector2 ScaledSize(float width, float height)
        {
            return new Vector2(S(width), S(height));
        }

        /// <summary>
        /// Clamp a window size so it never exceeds a fraction of the screen.
        /// </summary>
        public static Vector2 ClampToScreen(Vector2 size, float maxWidthFraction = 0.92f, float maxHeightFraction = 0.90f)
        {
            float maxW = Screen.width * maxWidthFraction;
            float maxH = Screen.height * maxHeightFraction;
            return new Vector2(
                Mathf.Clamp(size.x, S(WindowMinWidth), maxW),
                Mathf.Clamp(size.y, S(WindowMinHeight), maxH));
        }
    }
}
