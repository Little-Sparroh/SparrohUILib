using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Low-level builders for GameObjects, RectTransforms, Images, and TMP text.
    /// Higher-level widgets call into this.
    /// </summary>
    public static class UIFactory
    {
        public static GameObject Create(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform CreateRect(string name, Transform parent = null)
        {
            var go = Create(name, parent);
            var rt = go.AddComponent<RectTransform>();
            return rt;
        }

        public static Image CreateImage(string name, Transform parent, Color color, bool raycast = true)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static TextMeshProUGUI CreateTmp(
            string name,
            Transform parent,
            string text = "",
            float fontSize = -1f,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left,
            bool wrap = false)
        {
            var rt = CreateRect(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            float size = fontSize > 0f ? fontSize : UITheme.ScaledFontBody;
            UITheme.ApplyTextStyle(tmp, size, color, alignment, wrap);
            tmp.text = text ?? string.Empty;
            return tmp;
        }

        public static Canvas CreateOverlayCanvas(string name, int sortingOrder)
        {
            var go = Create(name);
            Object.DontDestroyOnLoad(go);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UITheme.ReferenceWidth, UITheme.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Bias toward height so HUD/text stays consistent across ultrawide / 16:10 / 4:3
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static VerticalLayoutGroup AddVerticalLayout(
            GameObject go,
            float spacing = -1f,
            RectOffset padding = null,
            TextAnchor childAlignment = TextAnchor.UpperLeft,
            bool controlChildHeight = true,
            bool expandHeight = false,
            bool controlChildWidth = true,
            bool expandWidth = true)
        {
            var v = go.GetComponent<VerticalLayoutGroup>();
            if (v == null)
                v = go.AddComponent<VerticalLayoutGroup>();

            v.spacing = spacing >= 0f ? spacing : UITheme.S(UITheme.SpacingNormal);
            v.padding = padding ?? UITheme.ScaledPadding(12, 12, 12, 12);
            v.childAlignment = childAlignment;
            v.childControlHeight = controlChildHeight;
            v.childForceExpandHeight = expandHeight;
            v.childControlWidth = controlChildWidth;
            v.childForceExpandWidth = expandWidth;
            return v;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(
            GameObject go,
            float spacing = -1f,
            RectOffset padding = null,
            TextAnchor childAlignment = TextAnchor.MiddleLeft,
            bool controlChildWidth = true,
            bool expandWidth = false,
            bool controlChildHeight = true,
            bool expandHeight = true)
        {
            var h = go.GetComponent<HorizontalLayoutGroup>();
            if (h == null)
                h = go.AddComponent<HorizontalLayoutGroup>();

            h.spacing = spacing >= 0f ? spacing : UITheme.S(UITheme.SpacingNormal);
            h.padding = padding ?? UITheme.ScaledPadding(8, 8, 4, 4);
            h.childAlignment = childAlignment;
            h.childControlWidth = controlChildWidth;
            h.childForceExpandWidth = expandWidth;
            h.childControlHeight = controlChildHeight;
            h.childForceExpandHeight = expandHeight;
            return h;
        }

        public static ContentSizeFitter AddContentSizeFitter(
            GameObject go,
            ContentSizeFitter.FitMode horizontal = ContentSizeFitter.FitMode.Unconstrained,
            ContentSizeFitter.FitMode vertical = ContentSizeFitter.FitMode.PreferredSize)
        {
            var f = go.GetComponent<ContentSizeFitter>();
            if (f == null)
                f = go.AddComponent<ContentSizeFitter>();
            f.horizontalFit = horizontal;
            f.verticalFit = vertical;
            return f;
        }

        /// <summary>
        /// Simple 1x1 white sprite for Image components that need a sprite (sliders, etc.).
        /// </summary>
        private static Sprite _whiteSprite;

        public static Sprite WhiteSprite
        {
            get
            {
                if (_whiteSprite != null)
                    return _whiteSprite;

                var tex = Texture2D.whiteTexture;
                _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return _whiteSprite;
            }
        }

        public static void ApplyWhiteSprite(Image img)
        {
            if (img != null && img.sprite == null)
                img.sprite = WhiteSprite;
        }
    }
}
