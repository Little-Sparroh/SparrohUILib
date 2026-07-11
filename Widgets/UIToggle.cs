using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Themed toggle with label and colored on/off states.
    /// </summary>
    public class UIToggle
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Toggle Toggle { get; }
        public TextMeshProUGUI Label { get; }
        public Image Box { get; }
        public Image Checkmark { get; }

        public bool IsOn
        {
            get => Toggle.isOn;
            set => Toggle.isOn = value;
        }

        private UIToggle(GameObject go, RectTransform rt, Toggle toggle, TextMeshProUGUI label, Image box, Image check)
        {
            GameObject = go;
            Rect = rt;
            Toggle = toggle;
            Label = label;
            Box = box;
            Checkmark = check;
        }

        public static UIToggle Create(
            Transform parent,
            string label,
            bool initial = false,
            Action<bool> onChanged = null,
            string name = null)
        {
            float h = UITheme.S(UITheme.ButtonHeight);
            float boxSize = UITheme.S(UITheme.ToggleSize);

            var root = UIFactory.CreateRect(name ?? $"Toggle_{label}", parent);
            UIHelpers.EnsureLayoutElement(root.gameObject, preferredHeight: h, minHeight: h);
            UIFactory.AddHorizontalLayout(root.gameObject, UITheme.S(UITheme.SpacingNormal),
                UITheme.ScaledPadding(4, 8, 2, 2), TextAnchor.MiddleLeft,
                controlChildWidth: false, expandWidth: false);

            var boxImg = UIFactory.CreateImage("Box", root, UIColors.ToggleOff, raycast: true);
            UIFactory.ApplyWhiteSprite(boxImg);
            var boxRt = boxImg.rectTransform;
            boxRt.sizeDelta = new Vector2(boxSize, boxSize);
            UIHelpers.EnsureLayoutElement(boxImg.gameObject, preferredWidth: boxSize, preferredHeight: boxSize);

            var checkImg = UIFactory.CreateImage("Check", boxRt, UIColors.Checkmark, raycast: false);
            UIFactory.ApplyWhiteSprite(checkImg);
            UIHelpers.SetFillParent(checkImg.rectTransform, boxSize * 0.22f);

            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = boxImg;
            toggle.graphic = checkImg;
            toggle.isOn = initial;

            var labelTmp = UIFactory.CreateTmp("Label", root, label, UITheme.ScaledFontBody,
                UIColors.TextPrimary, TextAlignmentOptions.Left);
            labelTmp.raycastTarget = false;
            UIHelpers.EnsureLayoutElement(labelTmp.gameObject, preferredHeight: h);
            var labelLe = labelTmp.gameObject.GetComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;

            void ApplyVisual(bool on)
            {
                boxImg.color = on ? UIColors.ToggleOn : UIColors.ToggleOff;
            }

            ApplyVisual(initial);
            toggle.onValueChanged.AddListener(v =>
            {
                ApplyVisual(v);
                onChanged?.Invoke(v);
            });

            return new UIToggle(root.gameObject, root, toggle, labelTmp, boxImg, checkImg);
        }

        public UIToggle OnChanged(Action<bool> action)
        {
            if (action != null)
                Toggle.onValueChanged.AddListener(new UnityAction<bool>(action));
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
