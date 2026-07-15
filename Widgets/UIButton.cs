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
    /// Color is driven manually so SetStyle/SetInteractable preserve hover state.
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
        private bool _hovered;
        private bool _pressedDown;
        private UIButtonStyle _style;

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
            // ColorBlock multiplies image color; keep white so we drive image.color ourselves.
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            colors.fadeDuration = 0f;
            btn.colors = colors;
            btn.transition = Selectable.Transition.ColorTint;

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
                _pressed = pressed,
                _style = style
            };

            var trigger = img.gameObject.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, _ =>
            {
                result._hovered = true;
                result.ApplyVisualState();
            });
            AddTrigger(trigger, EventTriggerType.PointerExit, _ =>
            {
                result._hovered = false;
                result._pressedDown = false;
                result.ApplyVisualState();
            });
            AddTrigger(trigger, EventTriggerType.PointerDown, _ =>
            {
                result._pressedDown = true;
                result.ApplyVisualState();
            });
            AddTrigger(trigger, EventTriggerType.PointerUp, _ =>
            {
                result._pressedDown = false;
                result.ApplyVisualState();
            });

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
            if (Label != null)
                Label.text = text ?? string.Empty;
            return this;
        }

        public UIButton SetInteractable(bool interactable)
        {
            if (Button.interactable == interactable)
                return this;

            Button.interactable = interactable;
            if (!interactable)
            {
                _hovered = false;
                _pressedDown = false;
            }
            ApplyVisualState();
            return this;
        }

        public UIButton SetStyle(UIButtonStyle style)
        {
            if (_style == style)
                return this;

            _style = style;
            GetStyleColors(style, out _normal, out _hover, out _pressed);
            ApplyVisualState();
            return this;
        }


        public UIButtonStyle Style => _style;

        public UIButton SetWidth(float width)
        {
            UIHelpers.EnsureLayoutElement(GameObject, preferredWidth: width);
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);

        /// <summary>
        /// Recompute background color from interactable + hover/press state.
        /// Safe to call every frame; does not clear hover.
        /// </summary>
        public void ApplyVisualState()
        {
            if (Background == null || Button == null)
                return;

            if (!Button.interactable)
            {
                Background.color = UIColors.WithAlpha(_normal, 0.45f);
                return;
            }

            if (_pressedDown)
                Background.color = _pressed;
            else if (_hovered)
                Background.color = _hover;
            else
                Background.color = _normal;
        }

        private static void GetStyleColors(UIButtonStyle style, out Color normal, out Color hover, out Color pressed)
        {
            switch (style)
            {
                case UIButtonStyle.Primary:
                    normal = UIColors.ButtonPrimary;
                    hover = UIColors.ButtonPrimaryHover;
                    pressed = UIColors.ButtonPrimaryPressed;
                    break;
                case UIButtonStyle.Danger:
                    normal = UIColors.ButtonDanger;
                    hover = UIColors.ButtonDangerHover;
                    pressed = UIColors.ButtonDangerPressed;
                    break;
                case UIButtonStyle.Active:
                    normal = UIColors.ButtonActive;
                    hover = UIColors.ButtonActiveHover;
                    pressed = UIColors.ButtonActivePressed;
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
