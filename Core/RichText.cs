using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Helpers for TMP rich-text formatting used across HUD and panels.
    /// </summary>
    public static class RichText
    {
        public static string Colorize(string text, Color color)
        {
            return $"<color=#{UIColors.ToHex(color)}>{text}</color>";
        }

        public static string Colorize(string text, string hexWithoutHash)
        {
            return $"<color=#{hexWithoutHash}>{text}</color>";
        }

        public static string Bold(string text) => $"<b>{text}</b>";

        public static string Italic(string text) => $"<i>{text}</i>";

        public static string Size(string text, int percent) => $"<size={percent}%>{text}</size>";

        /// <summary>
        /// "Label: value" with colored value.
        /// </summary>
        public static string Labeled(string label, string value, Color valueColor)
        {
            return $"{label}: {Colorize(value, valueColor)}";
        }

        /// <summary>
        /// "Label: value unit" with colored value.
        /// </summary>
        public static string Labeled(string label, string value, Color valueColor, string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return Labeled(label, value, valueColor);
            return $"{label}: {Colorize(value, valueColor)} {unit}";
        }

        public static string Labeled(string label, float value, Color valueColor, string unit = null, string format = "F1")
        {
            return Labeled(label, value.ToString(format), valueColor, unit);
        }

        public static string Labeled(string label, int value, Color valueColor, string unit = null)
        {
            return Labeled(label, value.ToString(), valueColor, unit);
        }

        /// <summary>
        /// "Label: value (rate/s)" pattern used by meters.
        /// </summary>
        public static string LabeledWithRate(string label, float value, float rate, Color color, string format = "F0", string rateFormat = "F1")
        {
            return $"{label}: {Colorize(value.ToString(format), color)} ({Colorize(rate.ToString(rateFormat), color)}/s)";
        }
    }
}
