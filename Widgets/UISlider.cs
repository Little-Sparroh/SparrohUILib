using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Themed horizontal slider with optional value label.
    /// </summary>
    public class UISlider
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Slider Slider { get; }
        public TextMeshProUGUI Label { get; }
        public TextMeshProUGUI ValueLabel { get; }

        public float Value
        {
            get => Slider.value;
            set => Slider.value = value;
        }

        private UISlider(GameObject go, RectTransform rt, Slider slider, TextMeshProUGUI label, TextMeshProUGUI valueLabel)
        {
            GameObject = go;
            Rect = rt;
            Slider = slider;
            Label = label;
            ValueLabel = valueLabel;
        }

        public static UISlider Create(
            Transform parent,
            string label = null,
            float min = 0f,
            float max = 1f,
            float initial = 0.5f,
            Action<float> onChanged = null,
            string name = "Slider",
            bool wholeNumbers = false,
            string valueFormat = "F2")
        {
            float h = UITheme.S(UITheme.SliderHeight + 16f);
            var root = UIFactory.CreateRect(name, parent);
            UIHelpers.EnsureLayoutElement(root.gameObject, preferredHeight: h, minHeight: h);
            UIFactory.AddVerticalLayout(root.gameObject, UITheme.S(2f),
                UITheme.ScaledPadding(4, 4, 2, 2), TextAnchor.UpperLeft,
                controlChildHeight: true, expandHeight: false);

            TextMeshProUGUI labelTmp = null;
            TextMeshProUGUI valueTmp = null;

            if (!string.IsNullOrEmpty(label))
            {
                var header = UIFactory.CreateRect("Header", root);
                UIHelpers.EnsureLayoutElement(header.gameObject, preferredHeight: UITheme.S(18f));
                UIFactory.AddHorizontalLayout(header.gameObject, 4f, new RectOffset(0, 0, 0, 0),
                    TextAnchor.MiddleLeft, controlChildWidth: false, expandWidth: false);

                labelTmp = UIFactory.CreateTmp("Label", header, label, UITheme.ScaledFontSmall,
                    UIColors.TextSecondary, TextAlignmentOptions.Left);
                UIHelpers.EnsureLayoutElement(labelTmp.gameObject, preferredHeight: UITheme.S(18f));
                labelTmp.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

                valueTmp = UIFactory.CreateTmp("Value", header, initial.ToString(valueFormat),
                    UITheme.ScaledFontSmall, UIColors.Sky, TextAlignmentOptions.Right);
                UIHelpers.EnsureLayoutElement(valueTmp.gameObject, preferredWidth: UITheme.S(60f), preferredHeight: UITheme.S(18f));
            }

            var sliderRoot = UIFactory.CreateRect("SliderRoot", root);
            float trackH = UITheme.S(UITheme.SliderHeight);
            UIHelpers.EnsureLayoutElement(sliderRoot.gameObject, preferredHeight: trackH, minHeight: trackH);

            var track = UIFactory.CreateImage("Track", sliderRoot, UIColors.SliderTrack, raycast: true);
            UIFactory.ApplyWhiteSprite(track);
            UIHelpers.SetFillParent(track.rectTransform);

            var fillArea = UIFactory.CreateRect("Fill Area", track.rectTransform);
            UIHelpers.SetFillParent(fillArea, 0f);

            var fill = UIFactory.CreateImage("Fill", fillArea, UIColors.SliderFill, raycast: false);
            UIFactory.ApplyWhiteSprite(fill);
            UIHelpers.SetFillParent(fill.rectTransform);

            var handleArea = UIFactory.CreateRect("Handle Slide Area", track.rectTransform);
            UIHelpers.SetFillParent(handleArea);

            float handleSize = UITheme.S(16f);
            var handle = UIFactory.CreateImage("Handle", handleArea, UIColors.SliderHandle, raycast: true);
            UIFactory.ApplyWhiteSprite(handle);
            handle.rectTransform.sizeDelta = new Vector2(handleSize, handleSize);

            var slider = sliderRoot.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.value = initial;

            // Configure fill for horizontal left-to-right
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            handle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            handle.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            slider.onValueChanged.AddListener(v =>
            {
                if (valueTmp != null)
                    valueTmp.text = v.ToString(valueFormat);
                onChanged?.Invoke(v);
            });

            return new UISlider(root.gameObject, root, slider, labelTmp, valueTmp);
        }

        public UISlider OnChanged(Action<float> action)
        {
            if (action != null)
                Slider.onValueChanged.AddListener(new UnityAction<float>(action));
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
