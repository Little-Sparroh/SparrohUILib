using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Non-interactive progress / meter bar with optional label.
    /// </summary>
    public class UIProgressBar
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Image Track { get; }
        public Image Fill { get; }
        public TextMeshProUGUI Label { get; }

        private float _value;
        private float _min;
        private float _max;

        public float Value
        {
            get => _value;
            set => SetValue(value);
        }

        public float Normalized
        {
            get
            {
                float range = _max - _min;
                return range > 0.0001f ? Mathf.Clamp01((_value - _min) / range) : 0f;
            }
        }

        private UIProgressBar(GameObject go, RectTransform rt, Image track, Image fill, TextMeshProUGUI label, float min, float max, float initial)
        {
            GameObject = go;
            Rect = rt;
            Track = track;
            Fill = fill;
            Label = label;
            _min = min;
            _max = max;
            SetValue(initial);
        }

        public static UIProgressBar Create(
            Transform parent,
            string label = null,
            float min = 0f,
            float max = 1f,
            float initial = 0f,
            Color? fillColor = null,
            string name = "ProgressBar")
        {
            float barH = UITheme.S(UITheme.ProgressHeight);
            float totalH = string.IsNullOrEmpty(label) ? barH + UITheme.S(4f) : barH + UITheme.S(22f);

            var root = UIFactory.CreateRect(name, parent);
            UIHelpers.EnsureLayoutElement(root.gameObject, preferredHeight: totalH, minHeight: totalH);
            UIFactory.AddVerticalLayout(root.gameObject, UITheme.S(2f),
                UITheme.ScaledPadding(2, 2, 2, 2), TextAnchor.UpperLeft,
                controlChildHeight: true, expandHeight: false);

            TextMeshProUGUI labelTmp = null;
            if (!string.IsNullOrEmpty(label))
            {
                labelTmp = UIFactory.CreateTmp("Label", root, label, UITheme.ScaledFontSmall,
                    UIColors.TextSecondary, TextAlignmentOptions.Left);
                UIHelpers.EnsureLayoutElement(labelTmp.gameObject, preferredHeight: UITheme.S(16f));
            }

            var track = UIFactory.CreateImage("Track", root, UIColors.ProgressTrack, raycast: false);
            UIFactory.ApplyWhiteSprite(track);
            UIHelpers.EnsureLayoutElement(track.gameObject, preferredHeight: barH, minHeight: barH);

            var fill = UIFactory.CreateImage("Fill", track.rectTransform, fillColor ?? UIColors.ProgressFill, raycast: false);
            UIFactory.ApplyWhiteSprite(fill);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            return new UIProgressBar(root.gameObject, root, track, fill, labelTmp, min, max, initial);
        }

        public UIProgressBar SetValue(float value)
        {
            _value = Mathf.Clamp(value, _min, _max);
            float n = Normalized;
            var fillRt = Fill.rectTransform;
            // Parent is track; set anchorMax.x for fill width
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(n, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            return this;
        }

        public UIProgressBar SetRange(float min, float max)
        {
            _min = min;
            _max = max;
            return SetValue(_value);
        }

        public UIProgressBar SetFillColor(Color color)
        {
            Fill.color = color;
            return this;
        }

        public UIProgressBar SetLabel(string text)
        {
            if (Label != null)
                Label.text = text ?? string.Empty;
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
