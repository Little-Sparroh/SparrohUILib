using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Shared top toolbar for gear-menu action buttons across Sparroh mods.
    /// Single left-to-right row (no center gap).
    /// Hosted on the game Menu canvas so hitboxes match the menu's camera/blit UI space.
    /// </summary>
    public static class GearActionBar
    {
        public const int OrderClearGrid = 10;
        public const int OrderSolve = 20;
        public const int OrderCancelSolve = 30;
        public const int OrderClearSelection = 40;
        public const int OrderCopyGrid = 50;
        public const int OrderPasteCode = 60;

        public const int OrderFilter = 100;
        public const int OrderPriority = 110;
        public const int OrderScrapMarked = 120;
        public const int OrderScrapNonFav = 130;
        public const int OrderUndoScrap = 140;
        public const int OrderGunStats = 150;

        // Menu canvas uses 1920x1080 reference units via the game's CanvasScaler.
        // Do not apply UITheme.S() here — that would double-scale under the menu scaler.
        private const float CompactHeightRef = 24f;
        private const float CompactFontRef = 12f;
        private const float SlotGapRef = 4f;
        private const float BarPadH = 8f;
        private const float BarPadV = 4f;
        // Menu UI is curved/warped; nudge the bar into the comfortable click/view band.
        private const float BarTopInset = 18f;
        private const float BarRightNudge = 24f;
        private const float MinButtonWidth = 64f;


        private static RectTransform _root;
        private static RectTransform _row;
        private static readonly Dictionary<string, Slot> _slots = new Dictionary<string, Slot>(StringComparer.Ordinal);

        private static bool _contextVisible;
        private static bool _built;

        private class Slot
        {
            public string Id;
            public int Order;
            public UIButton Button;
            public bool WantVisible = true;
            public string Label = string.Empty;
            public UIButtonStyle Style = UIButtonStyle.Default;
            public bool Interactable = true;
            public Action OnClick;
        }

        public static void Register(
            string id,
            string label,
            int order,
            Action onClick,
            UIButtonStyle style = UIButtonStyle.Default)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (!EnsureBuilt())
                return;

            label = label ?? string.Empty;

            if (_slots.TryGetValue(id, out var existing))
            {
                bool orderChanged = existing.Order != order;
                bool labelChanged = !string.Equals(existing.Label, label, StringComparison.Ordinal);
                bool styleChanged = existing.Style != style;
                bool clickChanged = !ReferenceEquals(existing.OnClick, onClick);

                existing.Order = order;
                existing.OnClick = onClick;

                if (existing.Button != null)
                {
                    if (labelChanged)
                    {
                        existing.Label = label;
                        existing.Button.SetText(label);
                        existing.Button.SetWidth(Mathf.Max(MinButtonWidth, EstimateWidth(label)));
                    }

                    if (styleChanged)
                    {
                        existing.Style = style;
                        existing.Button.SetStyle(style);
                    }

                    if (clickChanged)
                    {
                        existing.Button.Button.onClick.RemoveAllListeners();
                        if (onClick != null)
                            existing.Button.OnClick(onClick);
                    }
                }

                if (orderChanged)
                    RebuildOrder();

                ApplyVisibility();
                return;
            }

            var btn = UIButton.Create(_row, label, onClick, style, preferredHeight: CompactHeightRef);
            if (btn.Label != null)
                btn.Label.fontSize = CompactFontRef;

            btn.SetWidth(Mathf.Max(MinButtonWidth, EstimateWidth(label)));
            // Slight vertical pad so residual warp/AA doesn't leave a dead click strip on the glyph.
            if (btn.Background != null)
                btn.Background.raycastPadding = new Vector4(0f, 2f, 0f, 2f);

            _slots[id] = new Slot
            {
                Id = id,
                Order = order,
                Button = btn,
                WantVisible = true,
                Label = label,
                Style = style,
                Interactable = true,
                OnClick = onClick
            };
            RebuildOrder();
            ApplyVisibility();
        }

        public static void SetText(string id, string text)
        {
            if (!_slots.TryGetValue(id, out var s) || s.Button == null)
                return;

            text = text ?? string.Empty;
            if (string.Equals(s.Label, text, StringComparison.Ordinal))
                return;

            s.Label = text;
            s.Button.SetText(text);
            s.Button.SetWidth(Mathf.Max(MinButtonWidth, EstimateWidth(text)));
        }

        public static void SetInteractable(string id, bool interactable)
        {
            if (!_slots.TryGetValue(id, out var s) || s.Button == null)
                return;

            if (s.Interactable == interactable)
                return;

            s.Interactable = interactable;
            s.Button.SetInteractable(interactable);
        }

        public static void SetStyle(string id, UIButtonStyle style)
        {
            if (!_slots.TryGetValue(id, out var s) || s.Button == null)
                return;

            if (s.Style == style)
                return;

            s.Style = style;
            s.Button.SetStyle(style);
        }

        public static void SetSlotVisible(string id, bool visible)
        {
            if (!_slots.TryGetValue(id, out var s))
                return;

            if (s.WantVisible == visible)
            {
                if (s.Button != null)
                    s.Button.SetActive(visible && _contextVisible);
                return;
            }

            s.WantVisible = visible;
            if (s.Button != null)
                s.Button.SetActive(visible && _contextVisible);
        }

        public static void SetContextVisible(bool visible)
        {
            if (_contextVisible == visible && _built && IsHostAlive())
            {
                // Still keep bar on top while open (menu windows reorder siblings).
                if (visible)
                    BringToFront();
                return;
            }

            _contextVisible = visible;
            if (!_built || !IsHostAlive())
            {
                if (visible)
                    EnsureBuilt();
                else
                    return;
            }

            ApplyVisibility();
        }

        public static bool IsGearMenuOpen()
        {
            try
            {
                if (Menu.Instance == null || !Menu.Instance.IsOpen || Menu.Instance.WindowSystem == null)
                    return false;
                var top = Menu.Instance.WindowSystem.GetTop();
                return top is GearDetailsWindow || top is OuroGearWindow;
            }
            catch
            {
                return false;
            }
        }

        public static void Tick()
        {
            // Menu can be destroyed/recreated; drop stale host refs.
            if (_built && !IsHostAlive())
                InvalidateHost();

            SetContextVisible(IsGearMenuOpen());
        }

        public static void Unregister(string id)
        {
            if (!_slots.TryGetValue(id, out var s))
                return;
            if (s.Button != null && s.Button.GameObject != null)
                UnityEngine.Object.Destroy(s.Button.GameObject);
            _slots.Remove(id);
        }

        private static bool IsHostAlive()
        {
            return _root != null && _row != null;
        }

        private static void InvalidateHost()
        {
            // Buttons were children of the destroyed menu hierarchy.
            foreach (var s in _slots.Values)
                s.Button = null;

            _root = null;
            _row = null;
            _built = false;
            _contextVisible = false;
        }

        /// <summary>
        /// Called on scene unload so the next gear-menu open rebuilds under the new Menu canvas.
        /// </summary>
        internal static void InvalidateHostForSceneUnload()
        {
            InvalidateHost();
        }

        private static Transform TryGetMenuParent()
        {
            try
            {
                if (Menu.Instance == null)
                    return null;

                if (Menu.Instance.Canvas != null)
                    return Menu.Instance.Canvas.transform;

                return Menu.Instance.transform;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Build (or rebuild) the bar under the live Menu canvas.
        /// Returns false if the menu is not available yet.
        /// </summary>
        private static bool EnsureBuilt()
        {
            if (_built && IsHostAlive())
                return true;

            var parent = TryGetMenuParent();
            if (parent == null)
                return false;

            UITheme.Initialize();

            // If we had a previous root that somehow survived, drop it.
            if (_root != null)
            {
                UIHelpers.DestroySafe(_root.gameObject);
                _root = null;
                _row = null;
            }

            var root = UIFactory.CreateRect("Sparroh_GearActionBar", parent);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            float barH = CompactHeightRef + (BarPadV * 2f) + 4f;
            root.sizeDelta = new Vector2(0f, barH);
            root.anchoredPosition = new Vector2(BarRightNudge, -BarTopInset);

            root.localScale = Vector3.one;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = UIColors.WithAlpha(UIColors.PanelBg, 0.55f);
            UIFactory.ApplyWhiteSprite(bg);
            // Background must not steal clicks outside button bounds.
            bg.raycastTarget = false;

            // MiddleCenter: button cluster grows outward from the bar midpoint as slots are added.
            UIFactory.AddHorizontalLayout(root.gameObject,
                SlotGapRef,
                new RectOffset(
                    Mathf.RoundToInt(BarPadH),
                    Mathf.RoundToInt(BarPadH),
                    Mathf.RoundToInt(BarPadV),
                    Mathf.RoundToInt(BarPadV)),
                TextAnchor.MiddleCenter,
                controlChildWidth: false,
                expandWidth: false,
                controlChildHeight: true,
                expandHeight: false);


            _root = root;
            _row = root;
            _built = true;


            // Recreate button visuals for any slots registered before the menu existed.
            RecreateButtons();
            _root.gameObject.SetActive(false);
            return true;
        }

        private static void RecreateButtons()
        {
            if (_row == null)
                return;

            foreach (var s in _slots.Values.OrderBy(x => x.Order))
            {
                if (s.Button != null && s.Button.GameObject != null)
                {
                    // Re-parent surviving button if needed.
                    s.Button.GameObject.transform.SetParent(_row, false);
                    continue;
                }

                var btn = UIButton.Create(_row, s.Label, s.OnClick, s.Style, preferredHeight: CompactHeightRef);
                if (btn.Label != null)
                    btn.Label.fontSize = CompactFontRef;
                btn.SetWidth(Mathf.Max(MinButtonWidth, EstimateWidth(s.Label)));
                btn.SetInteractable(s.Interactable);
                if (btn.Background != null)
                    btn.Background.raycastPadding = new Vector4(0f, 2f, 0f, 2f);
                s.Button = btn;
            }

            RebuildOrder();
        }

        private static void BringToFront()
        {
            if (_root == null)
                return;
            _root.SetAsLastSibling();
        }

        private static void RebuildOrder()
        {
            if (_row == null)
                return;

            foreach (var s in _slots.Values.OrderBy(x => x.Order))
            {
                if (s.Button == null || s.Button.GameObject == null) continue;
                s.Button.GameObject.transform.SetParent(_row, false);
                s.Button.GameObject.transform.SetAsLastSibling();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_row);
        }

        private static void ApplyVisibility()
        {
            if (!IsHostAlive())
                return;

            bool any = _slots.Values.Any(s => s.WantVisible);
            bool show = _contextVisible && any;

            if (_root.gameObject.activeSelf != show)
                _root.gameObject.SetActive(show);

            if (show)
                BringToFront();

            foreach (var s in _slots.Values)
            {
                if (s.Button != null)
                    s.Button.SetActive(_contextVisible && s.WantVisible);
            }

            if (show)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_row);
        }

        private static float EstimateWidth(string label)
        {
            if (string.IsNullOrEmpty(label))
                return MinButtonWidth;
            return 12f + label.Length * 7.2f + 16f;
        }
    }
}
