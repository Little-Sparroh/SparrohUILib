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

        private const float CompactHeightRef = 24f;
        private const float CompactFontRef = 12f;
        private const float SlotGapRef = 4f;

        private static Canvas _canvas;
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

            EnsureBuilt();

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
                        existing.Button.SetWidth(Mathf.Max(UITheme.S(64f), EstimateWidth(label)));
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

            float h = UITheme.S(CompactHeightRef);
            var btn = UIButton.Create(_row, label, onClick, style, preferredHeight: h);
            if (btn.Label != null)
                btn.Label.fontSize = UITheme.S(CompactFontRef);

            btn.SetWidth(Mathf.Max(UITheme.S(64f), EstimateWidth(label)));

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
            s.Button.SetWidth(Mathf.Max(UITheme.S(64f), EstimateWidth(text)));
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
                // Still sync active state if context visibility changed.
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
            if (_contextVisible == visible && _built)
            {
                // No change; avoid thrashing SetActive every Tick.
                return;
            }

            _contextVisible = visible;
            if (!_built)
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

        private static void EnsureBuilt()
        {
            if (_built && _canvas != null)
                return;

            UITheme.Initialize();
            _canvas = UIFactory.CreateOverlayCanvas("Sparroh_GearActionBar", UITheme.WindowSortingOrder + 8);

            // Full-width top strip
            var bar = UIFactory.CreateRect("Bar", _canvas.transform);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            float barH = UITheme.S(CompactHeightRef + 12f);
            bar.sizeDelta = new Vector2(0f, barH);
            bar.anchoredPosition = new Vector2(0f, -UITheme.S(6f));

            var bg = bar.gameObject.AddComponent<Image>();
            bg.color = UIColors.WithAlpha(UIColors.PanelBg, 0.55f);
            UIFactory.ApplyWhiteSprite(bg);
            bg.raycastTarget = false;

            UIFactory.AddHorizontalLayout(bar.gameObject,
                UITheme.S(SlotGapRef),
                UITheme.ScaledPadding(8, 8, 4, 4),
                TextAnchor.MiddleLeft,
                controlChildWidth: false,
                expandWidth: false,
                controlChildHeight: true,
                expandHeight: false);

            _row = bar;
            _built = true;
            _canvas.gameObject.SetActive(false);
        }

        private static void RebuildOrder()
        {
            if (_row == null)
                return;

            foreach (var s in _slots.Values.OrderBy(x => x.Order))
            {
                if (s.Button == null) continue;
                s.Button.GameObject.transform.SetParent(_row, false);
                s.Button.GameObject.transform.SetAsLastSibling();
            }
        }

        private static void ApplyVisibility()
        {
            if (_canvas == null)
                return;

            bool any = _slots.Values.Any(s => s.WantVisible);
            bool show = _contextVisible && any;
            if (_canvas.gameObject.activeSelf != show)
                _canvas.gameObject.SetActive(show);

            foreach (var s in _slots.Values)
            {
                if (s.Button != null)
                    s.Button.SetActive(_contextVisible && s.WantVisible);
            }
        }

        private static float EstimateWidth(string label)
        {
            if (string.IsNullOrEmpty(label))
                return UITheme.S(64f);
            return UITheme.S(12f + label.Length * 7.2f + 16f);
        }
    }
}
