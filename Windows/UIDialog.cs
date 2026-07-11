using System;
using TMPro;
using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Modal confirmation and alert dialogs.
    /// </summary>
    public static class UIDialog
    {
        public static UIWindow Confirm(
            string title,
            string message,
            Action onConfirm,
            Action onCancel = null,
            string confirmText = "Confirm",
            string cancelText = "Cancel")
        {
            var window = UIWindow.Create(
                "Dialog",
                new Vector2(420f, 220f),
                title,
                scrollable: false,
                closeButton: true,
                sortingOrder: UITheme.DialogSortingOrder);

            window.OnClose(() => onCancel?.Invoke());

            var content = window.Content;
            UIFactory.AddVerticalLayout(content.gameObject,
                UITheme.S(UITheme.SpacingNormal),
                UITheme.ScaledPadding(16, 16, 12, 12),
                TextAnchor.MiddleCenter,
                controlChildHeight: true,
                expandHeight: false);

            var msg = UIText.Create(content, "Message", message,
                UITheme.ScaledFontBody, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            UIHelpers.EnsureLayoutElement(msg.GameObject, preferredHeight: UITheme.S(80f), minHeight: UITheme.S(60f));
            msg.GameObject.GetComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1f;

            var buttons = UIFactory.CreateRect("Buttons", content);
            UIHelpers.EnsureLayoutElement(buttons.gameObject, preferredHeight: UITheme.ScaledButtonHeight + UITheme.S(8f));
            UIFactory.AddHorizontalLayout(buttons.gameObject, UITheme.S(12f),
                new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter,
                controlChildWidth: false, expandWidth: false,
                controlChildHeight: true, expandHeight: true);

            UIButton.Create(buttons, cancelText, () =>
            {
                window.Destroy();
                onCancel?.Invoke();
            }, UIButtonStyle.Default).SetWidth(UITheme.S(120f));

            UIButton.Create(buttons, confirmText, () =>
            {
                window.Destroy();
                onConfirm?.Invoke();
            }, UIButtonStyle.Primary).SetWidth(UITheme.S(120f));

            return window;
        }

        public static UIWindow Alert(
            string title,
            string message,
            Action onOk = null,
            string okText = "OK")
        {
            var window = UIWindow.Create(
                "Alert",
                new Vector2(400f, 200f),
                title,
                scrollable: false,
                closeButton: true,
                sortingOrder: UITheme.DialogSortingOrder);

            var content = window.Content;
            UIFactory.AddVerticalLayout(content.gameObject,
                UITheme.S(UITheme.SpacingNormal),
                UITheme.ScaledPadding(16, 16, 12, 12),
                TextAnchor.MiddleCenter);

            var msg = UIText.Create(content, "Message", message,
                UITheme.ScaledFontBody, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            UIHelpers.EnsureLayoutElement(msg.GameObject, preferredHeight: UITheme.S(70f));
            msg.GameObject.GetComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1f;

            UIButton.Create(content, okText, () =>
            {
                window.Destroy();
                onOk?.Invoke();
            }, UIButtonStyle.Primary);

            return window;
        }
    }
}
