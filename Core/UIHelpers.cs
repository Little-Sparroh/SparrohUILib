using UnityEngine;
using UnityEngine.UI;
using Pigeon.Movement;

namespace Sparroh.UI
{
    /// <summary>
    /// Layout, anchoring, parenting, and cleanup helpers.
    /// </summary>
    public static class UIHelpers
    {
        /// <summary>
        /// Parent under the local player's reticle (standard HUD attach point).
        /// Returns false if player/reticle is not ready.
        /// </summary>
        public static bool TryGetReticle(out Transform reticle)
        {
            reticle = null;
            try
            {
                if (Player.LocalPlayer == null ||
                    Player.LocalPlayer.PlayerLook == null ||
                    Player.LocalPlayer.PlayerLook.Reticle == null)
                    return false;

                reticle = Player.LocalPlayer.PlayerLook.Reticle;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAnchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2? pivot = null)
        {
            if (rt == null) return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            if (pivot.HasValue)
                rt.pivot = pivot.Value;
        }

        /// <summary>
        /// Point anchor (same min/max) — used for HUD elements with normalized 0-1 positions.
        /// </summary>
        public static void SetPointAnchor(RectTransform rt, float x, float y, Vector2? pivot = null)
        {
            var a = new Vector2(x, y);
            SetAnchor(rt, a, a, pivot ?? new Vector2(0.5f, 0.5f));
        }

        public static void SetFillParent(RectTransform rt, float padding = 0f)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        public static void SetTopStretch(RectTransform rt, float height, float left = 0f, float right = 0f, float top = 0f)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-(left + right), height);
            rt.anchoredPosition = new Vector2((left - right) * 0.5f, -top);
        }

        public static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        public static void DestroyChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroySafe(parent.GetChild(i).gameObject);
        }

        public static Image EnsureImage(GameObject go, Color color, bool raycast = true)
        {
            var img = go.GetComponent<Image>();
            if (img == null)
                img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static LayoutElement EnsureLayoutElement(GameObject go, float? preferredWidth = null, float? preferredHeight = null, float? minHeight = null)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null)
                le = go.AddComponent<LayoutElement>();
            if (preferredWidth.HasValue)
                le.preferredWidth = preferredWidth.Value;
            if (preferredHeight.HasValue)
                le.preferredHeight = preferredHeight.Value;
            if (minHeight.HasValue)
                le.minHeight = minHeight.Value;
            return le;
        }

        /// <summary>
        /// Convert a screen-space point to local point in a RectTransform.
        /// </summary>
        public static bool ScreenToLocal(RectTransform parent, Vector2 screenPoint, out Vector2 localPoint, Camera cam = null)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, cam, out localPoint);
        }

        /// <summary>
        /// Normalized anchor (0-1) from a screen pixel position relative to a parent rect.
        /// Useful for HUD reposition persistence.
        /// </summary>
        public static Vector2 ScreenPointToNormalizedAnchor(RectTransform parent, Vector2 screenPoint, Camera cam = null)
        {
            if (!ScreenToLocal(parent, screenPoint, out var local, cam))
                return new Vector2(0.5f, 0.5f);

            var rect = parent.rect;
            float nx = rect.width > 0 ? (local.x - rect.xMin) / rect.width : 0.5f;
            float ny = rect.height > 0 ? (local.y - rect.yMin) / rect.height : 0.5f;
            return new Vector2(Mathf.Clamp01(nx), Mathf.Clamp01(ny));
        }
    }
}
