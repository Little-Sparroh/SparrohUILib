using TMPro;
using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Styled TextMeshProUGUI wrapper with rich-text helpers.
    /// </summary>
    public class UIText
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public TextMeshProUGUI Tmp { get; }

        public string Text
        {
            get => Tmp.text;
            set => Tmp.text = value ?? string.Empty;
        }

        public Color Color
        {
            get => Tmp.color;
            set => Tmp.color = value;
        }

        public float FontSize
        {
            get => Tmp.fontSize;
            set => Tmp.fontSize = value;
        }

        private UIText(GameObject go, RectTransform rt, TextMeshProUGUI tmp)
        {
            GameObject = go;
            Rect = rt;
            Tmp = tmp;
        }

        public static UIText Create(
            Transform parent,
            string name = "Text",
            string text = "",
            float fontSize = -1f,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left,
            bool wrap = false)
        {
            var tmp = UIFactory.CreateTmp(name, parent, text, fontSize, color, alignment, wrap);
            return new UIText(tmp.gameObject, tmp.rectTransform, tmp);
        }

        public UIText SetText(string text)
        {
            Text = text;
            return this;
        }

        public UIText SetRich(string label, string value, Color valueColor, string unit = null)
        {
            Text = RichText.Labeled(label, value, valueColor, unit);
            return this;
        }

        public UIText SetRich(string label, float value, Color valueColor, string unit = null, string format = "F1")
        {
            Text = RichText.Labeled(label, value, valueColor, unit, format);
            return this;
        }

        public UIText SetRich(string label, int value, Color valueColor, string unit = null)
        {
            Text = RichText.Labeled(label, value, valueColor, unit);
            return this;
        }

        public UIText SetRichWithRate(string label, float value, float rate, Color color, string format = "F0", string rateFormat = "F1")
        {
            Text = RichText.LabeledWithRate(label, value, rate, color, format, rateFormat);
            return this;
        }

        public UIText FillParent(float padding = 0f)
        {
            UIHelpers.SetFillParent(Rect, padding);
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
