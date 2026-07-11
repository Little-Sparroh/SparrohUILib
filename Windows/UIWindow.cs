using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Overlay window with title bar, optional close button, and body content.
    /// Each window creates its own canvas (per user preference).
    /// Sizes are resolution-scaled and clamped to the screen.
    /// </summary>
    public class UIWindow
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Canvas Canvas { get; }

        /// <summary>Parent transform for window body content (scroll content if scrollable).</summary>
        public Transform Content { get; private set; }

        public UIScrollView ScrollView { get; private set; }
        public TextMeshProUGUI TitleText { get; }
        public bool IsVisible { get; private set; }

        private readonly GameObject _canvasGo;
        private Action _onClose;

        private UIWindow(
            GameObject canvasGo,
            Canvas canvas,
            GameObject panelGo,
            RectTransform panelRt,
            TextMeshProUGUI titleText)
        {
            _canvasGo = canvasGo;
            Canvas = canvas;
            GameObject = panelGo;
            Rect = panelRt;
            TitleText = titleText;
        }

        public static UIWindow Create(
            string name,
            Vector2? referenceSize = null,
            string title = null,
            bool scrollable = false,
            bool closeButton = true,
            int? sortingOrder = null)
        {
            UITheme.Initialize();

            var size = referenceSize ?? new Vector2(720f, 520f);
            size = UITheme.ScaledSize(size.x, size.y);
            size = UITheme.ClampToScreen(size);

            var canvas = UIFactory.CreateOverlayCanvas(name + "_Canvas", sortingOrder ?? UITheme.WindowSortingOrder);
            var canvasGo = canvas.gameObject;

            var backdrop = UIFactory.CreateImage("Backdrop", canvas.transform,
                UIColors.WithAlpha(Color.black, 0.45f), raycast: true);
            UIFactory.ApplyWhiteSprite(backdrop);
            UIHelpers.SetFillParent(backdrop.rectTransform);

            var panel = UIPanel.Create(canvas.transform, name, UIColors.PanelBg, withBorder: true);
            panel.Rect.anchorMin = panel.Rect.anchorMax = new Vector2(0.5f, 0.5f);
            panel.Rect.pivot = new Vector2(0.5f, 0.5f);
            panel.Rect.sizeDelta = size;
            panel.Rect.anchoredPosition = Vector2.zero;

            float titleH = UITheme.ScaledTitleBarHeight;

            var titleBar = UIFactory.CreateImage("TitleBar", panel.Content, UIColors.TitleBar, raycast: true);
            UIFactory.ApplyWhiteSprite(titleBar);
            UIHelpers.SetTopStretch(titleBar.rectTransform, titleH, left: 0f, right: 0f, top: 0f);

            var accent = UIFactory.CreateImage("Accent", titleBar.rectTransform, UIColors.BorderAccent, raycast: false);
            UIFactory.ApplyWhiteSprite(accent);
            var accentRt = accent.rectTransform;
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(1f, 0f);
            accentRt.pivot = new Vector2(0.5f, 0f);
            accentRt.sizeDelta = new Vector2(0f, UITheme.S(2f));
            accentRt.anchoredPosition = Vector2.zero;

            var titleTmp = UIFactory.CreateTmp("Title", titleBar.rectTransform, title ?? name,
                UITheme.ScaledFontHeader, UIColors.TextPrimary, TextAlignmentOptions.Center);
            UIHelpers.SetFillParent(titleTmp.rectTransform, UITheme.S(8f));
            if (closeButton)
                titleTmp.rectTransform.offsetMax = new Vector2(-UITheme.S(40f), -UITheme.S(4f));

            var drag = titleBar.gameObject.AddComponent<WindowDragHandle>();
            drag.Target = panel.Rect;

            var window = new UIWindow(canvasGo, canvas, panel.GameObject, panel.Rect, titleTmp);

            if (closeButton)
            {
                var close = UIButton.Create(titleBar.rectTransform, "X", () => window.Hide(),
                    UIButtonStyle.Danger, "CloseButton", preferredHeight: UITheme.S(28f));
                var crt = close.Rect;
                crt.anchorMin = crt.anchorMax = new Vector2(1f, 0.5f);
                crt.pivot = new Vector2(1f, 0.5f);
                crt.sizeDelta = new Vector2(UITheme.S(32f), UITheme.S(28f));
                crt.anchoredPosition = new Vector2(-UITheme.S(8f), 0f);
                var le = close.GameObject.GetComponent<LayoutElement>();
                if (le != null)
                    UnityEngine.Object.Destroy(le);
            }

            var body = UIFactory.CreateRect("Body", panel.Content);
            body.anchorMin = Vector2.zero;
            body.anchorMax = Vector2.one;
            body.offsetMin = new Vector2(UITheme.S(10f), UITheme.S(10f));
            body.offsetMax = new Vector2(-UITheme.S(10f), -(titleH + UITheme.S(8f)));

            if (scrollable)
            {
                var scroll = UIScrollView.Create(body, "Scroll");
                scroll.FillParent();
                window.ScrollView = scroll;
                window.Content = scroll.Content;
            }
            else
            {
                window.Content = body;
            }

            window.IsVisible = true;
            return window;
        }

        public UIWindow WithTitle(string title)
        {
            if (TitleText != null)
                TitleText.text = title ?? string.Empty;
            return this;
        }

        public UIWindow OnClose(Action action)
        {
            _onClose = action;
            return this;
        }

        public void Show()
        {
            if (_canvasGo != null)
                _canvasGo.SetActive(true);
            IsVisible = true;
        }

        /// <summary>
        /// Hide the window. OnClose is only invoked when transitioning from visible → hidden
        /// (avoids re-entrancy if the close handler also calls Hide).
        /// </summary>
        public void Hide(bool invokeClose = true)
        {
            bool wasVisible = IsVisible;
            if (_canvasGo != null)
                _canvasGo.SetActive(false);
            IsVisible = false;

            if (invokeClose && wasVisible)
                _onClose?.Invoke();
        }

        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        public void Destroy()
        {
            // Suppress close callback during teardown
            _onClose = null;
            UIHelpers.DestroySafe(_canvasGo);
        }


        /// <summary>
        /// Section header bar for use inside window content.
        /// </summary>
        public static UIText CreateSectionHeader(Transform parent, string text)
        {
            var bar = UIFactory.CreateImage("Section_" + text, parent, UIColors.SectionBar, raycast: false);
            UIFactory.ApplyWhiteSprite(bar);
            UIHelpers.EnsureLayoutElement(bar.gameObject, preferredHeight: UITheme.S(32f), minHeight: UITheme.S(32f));

            var label = UIText.Create(bar.rectTransform, "Label", text,
                UITheme.ScaledFontBody, UIColors.TextPrimary, TextAlignmentOptions.Center);
            label.FillParent(UITheme.S(4f));
            return label;
        }
    }

    /// <summary>
    /// Drag handle for window title bars.
    /// </summary>
    public class WindowDragHandle : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public RectTransform Target;
        private Vector2 _dragOffset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Target == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Target, eventData.position, eventData.pressEventCamera, out var local);
            _dragOffset = (Vector2)Target.anchoredPosition - local;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Target == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    Target.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var local))
            {
                Target.anchoredPosition = local + _dragOffset;
            }
        }
    }
}
