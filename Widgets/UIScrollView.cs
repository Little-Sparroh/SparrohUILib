using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Vertical scroll view with themed viewport and content root.
    /// </summary>
    public class UIScrollView
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public ScrollRect ScrollRect { get; }
        public RectTransform Viewport { get; }
        public RectTransform Content { get; }

        private UIScrollView(GameObject go, RectTransform rt, ScrollRect sr, RectTransform viewport, RectTransform content)
        {
            GameObject = go;
            Rect = rt;
            ScrollRect = sr;
            Viewport = viewport;
            Content = content;
        }

        public static UIScrollView Create(
            Transform parent,
            string name = "ScrollView",
            bool vertical = true,
            bool horizontal = false)
        {
            var rootImg = UIFactory.CreateImage(name, parent, UIColors.ScrollBg, raycast: true);
            UIFactory.ApplyWhiteSprite(rootImg);
            var rootRt = rootImg.rectTransform;

            var viewportImg = UIFactory.CreateImage("Viewport", rootRt, UIColors.WithAlpha(UIColors.ScrollBg, 0.01f), raycast: true);
            UIFactory.ApplyWhiteSprite(viewportImg);
            var viewportRt = viewportImg.rectTransform;
            UIHelpers.SetFillParent(viewportRt, UITheme.S(2f));

            var mask = viewportImg.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = UIFactory.CreateRect("Content", viewportRt);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            UIFactory.AddVerticalLayout(content.gameObject,
                UITheme.S(UITheme.SpacingNormal),
                UITheme.ScaledPadding(12, 12, 10, 14),
                TextAnchor.UpperLeft,
                controlChildHeight: true,
                expandHeight: false,
                controlChildWidth: true,
                expandWidth: true);

            UIFactory.AddContentSizeFitter(content.gameObject,
                ContentSizeFitter.FitMode.Unconstrained,
                ContentSizeFitter.FitMode.PreferredSize);

            var scroll = rootImg.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewportRt;
            scroll.content = content;
            scroll.horizontal = horizontal;
            scroll.vertical = vertical;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.15f;

            return new UIScrollView(rootImg.gameObject, rootRt, scroll, viewportRt, content);
        }

        public UIScrollView FillParent(float padding = 0f)
        {
            UIHelpers.SetFillParent(Rect, padding);
            return this;
        }

        public void ClearContent() => UIHelpers.DestroyChildren(Content);

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
