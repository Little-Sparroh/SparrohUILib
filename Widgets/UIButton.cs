using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparroh.UI
{
    public enum UIButtonStyle
    {
        Default,
        Primary,
        Danger,
        Active
    }

    /// <summary>
    /// Themed button with hover/press color transitions.
    /// </summary>
    public class UIButton
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Button Button { get; }
        public Image Background { get; }
        public TextMeshProUGUI Label { get; }

        private Color _normal;
        private Color _hover;
        private Color _pressed;

        private UIButton(GameObject go, RectTransform rt, Button btn, Image bg, TextMeshProUGUI label)
        {
            GameObject = go;
            Rect = rt;
            Button = btn;
            Background = bg;
            Label = label;
        }

        public static UIButton Create(
            Transform parent,
            string text,
            Action onClick = null,
            UIButtonStyle style = UIButtonStyle.Default,
            string name = null,
            float? preferredHeight = null)
        {
            GetStyleColors(style, out var normal, out var hover, out var pressed);

            var img = UIFactory.CreateImage(name ?? $"Button_{text}", parent, normal, raycast: true);
            UIFactory.ApplyWhiteSprite(img);
            var rt = img.rectTransform;

            float h = preferredHeight ?? UITheme.ScaledButtonHeight;
            UIHelpers.EnsureLayoutElement(img.gameObject, preferredHeight: h, minHeight: h);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            var label = UIFactory.CreateTmp(
                "Label",
                rt,
                text,
                UITheme.ScaledFontBody,
                UIColors.TextPrimary,
                TextAlignmentOptions.Center);
            UIHelpers.SetFillParent(label.rectTransform, UITheme.S(4f));
            label.raycastTarget = false;

            var result = new UIButton(img.gameObject, rt, btn, img, label)
            {
                _normal = normal,
                _hover = hover,
                _pressed = pressed
            };

            // Manual hover colors (ColorBlock multiplies image color; we drive image.color directly)
            var trigger = img.gameObject.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, _ => { if (btn.interactable) img.color = result._hover; });
            AddTrigger(trigger, EventTriggerType.PointerExit, _ => { if (btn.interactable) img.color = result._normal; });
            AddTrigger(trigger, EventTriggerType.PointerDown, _ => { if (btn.interactable) img.color = result._pressed; });
            AddTrigger(trigger, EventTriggerType.PointerUp, _ => { if (btn.interactable) img.color = result._hover; });

            if (onClick != null)
                btn.onClick.AddListener(new UnityAction(onClick));

            return result;
        }

        public UIButton OnClick(Action action)
        {
            if (action != null)
                Button.onClick.AddListener(new UnityAction(action));
            return this;
        }

        public UIButton SetText(string text)
        {
            Label.text = text ?? string.Empty;
            return this;
        }

        public UIButton SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
            Background.color = interactable ? _normal : UIColors.WithAlpha(_normal, 0.45f);
            return this;
        }

        public UIButton SetStyle(UIButtonStyle style)
        {
            GetStyleColors(style, out _normal, out _hover, out _pressed);
            Background.color = _normal;
            return this;
        }

        public UIButton SetWidth(float width)
        {
            UIHelpers.EnsureLayoutElement(GameObject, preferredWidth: width);
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);

        private static void GetStyleColors(UIButtonStyle style, out Color normal, out Color hover, out Color pressed)
        {
            switch (style)
            {
                case UIButtonStyle.Primary:
                    normal = UIColors.ButtonPrimary;
                    hover = UIColors.ButtonPrimaryHover;
                    pressed = UIColors.ButtonPressed;
                    break;
                case UIButtonStyle.Danger:
                    normal = UIColors.ButtonDanger;
                    hover = UIColors.ButtonDangerHover;
                    pressed = UIColors.ButtonPressed;
                    break;
                case UIButtonStyle.Active:
                    normal = UIColors.ButtonActive;
                    hover = UIColors.WithAlpha(UIColors.ButtonActive, 0.85f);
                    pressed = UIColors.ButtonPressed;
                    break;
                default:
                    normal = UIColors.ButtonNormal;
                    hover = UIColors.ButtonHover;
                    pressed = UIColors.ButtonPressed;
                    break;
            }
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> cb)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(cb);
            trigger.triggers.Add(entry);
        }
    }
}
