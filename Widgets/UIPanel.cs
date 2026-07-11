using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Background panel with optional border and layout group.
    /// </summary>
    public class UIPanel
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Image Background { get; }
        public Image Border { get; private set; }

        private UIPanel(GameObject go, RectTransform rt, Image bg)
        {
            GameObject = go;
            Rect = rt;
            Background = bg;
        }

        public static UIPanel Create(
            Transform parent,
            string name = "Panel",
            Color? background = null,
            bool withBorder = true)
        {
            // Border sits behind content as a slightly larger sibling structure:
            // root (border color) -> inner (bg)
            if (withBorder)
            {
                var borderImg = UIFactory.CreateImage(name, parent, UIColors.Border, raycast: true);
                UIFactory.ApplyWhiteSprite(borderImg);
                var borderRt = borderImg.rectTransform;

                float bw = UITheme.S(UITheme.BorderWidth);
                var inner = UIFactory.CreateImage("Background", borderRt, background ?? UIColors.PanelBg, raycast: true);
                UIFactory.ApplyWhiteSprite(inner);
                UIHelpers.SetFillParent(inner.rectTransform, bw);

                var panel = new UIPanel(borderImg.gameObject, borderRt, inner)
                {
                    Border = borderImg
                };
                return panel;
            }
            else
            {
                var img = UIFactory.CreateImage(name, parent, background ?? UIColors.PanelBg, raycast: true);
                UIFactory.ApplyWhiteSprite(img);
                return new UIPanel(img.gameObject, img.rectTransform, img);
            }
        }

        public UIPanel SetSize(Vector2 size)
        {
            Rect.sizeDelta = size;
            return this;
        }

        public UIPanel SetSize(float width, float height) => SetSize(new Vector2(width, height));

        public UIPanel SetAnchor(Vector2 min, Vector2 max, Vector2? pivot = null)
        {
            UIHelpers.SetAnchor(Rect, min, max, pivot);
            return this;
        }

        public UIPanel SetPointAnchor(float x, float y, Vector2? pivot = null)
        {
            UIHelpers.SetPointAnchor(Rect, x, y, pivot);
            return this;
        }

        public UIPanel FillParent(float padding = 0f)
        {
            UIHelpers.SetFillParent(Rect, padding);
            return this;
        }

        public UIPanel WithVerticalLayout(float spacing = -1f, RectOffset padding = null)
        {
            var target = Background != null ? Background.gameObject : GameObject;
            UIFactory.AddVerticalLayout(target, spacing, padding);
            return this;
        }

        public UIPanel WithHorizontalLayout(float spacing = -1f, RectOffset padding = null)
        {
            var target = Background != null ? Background.gameObject : GameObject;
            UIFactory.AddHorizontalLayout(target, spacing, padding);
            return this;
        }

        /// <summary>Content parent for children (inner background when bordered).</summary>
        public Transform Content => Background != null ? Background.transform : GameObject.transform;

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
