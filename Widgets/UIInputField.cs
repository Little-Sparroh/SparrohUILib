using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Themed TMP input field.
    /// </summary>
    public class UIInputField
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public TMP_InputField Input { get; }
        public TextMeshProUGUI TextComponent { get; }
        public TextMeshProUGUI Placeholder { get; }

        public string Text
        {
            get => Input.text;
            set => Input.text = value ?? string.Empty;
        }

        private UIInputField(GameObject go, RectTransform rt, TMP_InputField input, TextMeshProUGUI text, TextMeshProUGUI placeholder)
        {
            GameObject = go;
            Rect = rt;
            Input = input;
            TextComponent = text;
            Placeholder = placeholder;
        }

        public static UIInputField Create(
            Transform parent,
            string initial = "",
            string placeholder = "",
            Action<string> onChanged = null,
            string name = "InputField")
        {
            float h = UITheme.ScaledInputHeight;

            var bg = UIFactory.CreateImage(name, parent, UIColors.InputBg, raycast: true);
            UIFactory.ApplyWhiteSprite(bg);
            UIHelpers.EnsureLayoutElement(bg.gameObject, preferredHeight: h, minHeight: h);

            // Subtle border via outline-like second image is overkill; keep flat with accent on focus later if needed
            var textArea = UIFactory.CreateRect("TextArea", bg.rectTransform);
            UIHelpers.SetFillParent(textArea, UITheme.S(8f));
            // RectMask2D clips caret/text
            textArea.gameObject.AddComponent<RectMask2D>();

            var placeholderTmp = UIFactory.CreateTmp("Placeholder", textArea, placeholder,
                UITheme.ScaledFontBody, UIColors.TextMuted, TextAlignmentOptions.Left);
            UIHelpers.SetFillParent(placeholderTmp.rectTransform);
            placeholderTmp.fontStyle = FontStyles.Italic;

            var textTmp = UIFactory.CreateTmp("Text", textArea, initial,
                UITheme.ScaledFontBody, UIColors.InputText, TextAlignmentOptions.Left);
            UIHelpers.SetFillParent(textTmp.rectTransform);
            textTmp.raycastTarget = true;

            var input = bg.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = textArea;
            input.textComponent = textTmp;
            input.placeholder = placeholderTmp;
            input.fontAsset = UITheme.Font;
            input.pointSize = UITheme.ScaledFontBody;
            input.text = initial ?? string.Empty;
            input.caretColor = UIColors.Sky;
            input.selectionColor = UIColors.WithAlpha(UIColors.Electric, 0.35f);
            input.targetGraphic = bg;

            if (onChanged != null)
                input.onValueChanged.AddListener(new UnityAction<string>(onChanged));

            return new UIInputField(bg.gameObject, bg.rectTransform, input, textTmp, placeholderTmp);
        }

        public UIInputField OnChanged(Action<string> action)
        {
            if (action != null)
                Input.onValueChanged.AddListener(new UnityAction<string>(action));
            return this;
        }

        public UIInputField OnEndEdit(Action<string> action)
        {
            if (action != null)
                Input.onEndEdit.AddListener(new UnityAction<string>(action));
            return this;
        }

        public UIInputField SetInteractable(bool interactable)
        {
            Input.interactable = interactable;
            return this;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
