using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    public class UISeparator
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public Image Image { get; }

        private UISeparator(GameObject go, RectTransform rt, Image img)
        {
            GameObject = go;
            Rect = rt;
            Image = img;
        }

        public static UISeparator Create(Transform parent, string name = "Separator", bool horizontal = true)
        {
            var img = UIFactory.CreateImage(name, parent, UIColors.Separator, raycast: false);
            UIFactory.ApplyWhiteSprite(img);

            float thickness = UITheme.S(UITheme.SeparatorHeight);
            if (horizontal)
                UIHelpers.EnsureLayoutElement(img.gameObject, preferredHeight: thickness, minHeight: thickness);
            else
                UIHelpers.EnsureLayoutElement(img.gameObject, preferredWidth: thickness, minHeight: thickness);

            return new UISeparator(img.gameObject, img.rectTransform, img);
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
