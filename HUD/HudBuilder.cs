using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sparroh.UI
{
    /// <summary>
    /// Fluent builder for HUD elements parented to the player reticle.
    /// Uses normalized anchors (0-1) for resolution-independent positioning.
    /// </summary>
    public class HudBuilder
    {
        private readonly string _name;
        private Transform _parent;
        private float _anchorX = 0.5f;
        private float _anchorY = 0.5f;
        private Vector2 _pivot = new Vector2(0f, 1f);
        private Vector2 _size;
        private Vector2 _anchoredPosition = Vector2.zero;
        private bool _parentToReticle = true;
        private readonly List<LineSpec> _lines = new List<LineSpec>();
        private bool _withBackground;
        private Color _backgroundColor = UIColors.WithAlpha(UIColors.PanelBg, 0.55f);

        private struct LineSpec
        {
            public string Name;
            public float FontSize;
            public TextAlignmentOptions Alignment;
        }

        private HudBuilder(string name)
        {
            _name = name;
            _size = new Vector2(UITheme.S(UITheme.HudDefaultWidth), UITheme.S(UITheme.HudLineHeight));
        }

        public static HudBuilder Create(string name) => new HudBuilder(name);

        public HudBuilder ParentToReticle(bool enabled = true)
        {
            _parentToReticle = enabled;
            return this;
        }

        public HudBuilder Parent(Transform parent)
        {
            _parent = parent;
            _parentToReticle = false;
            return this;
        }

        /// <summary>Normalized 0-1 anchor position (resolution independent).</summary>
        public HudBuilder Anchor(float x, float y)
        {
            _anchorX = Mathf.Clamp01(x);
            _anchorY = Mathf.Clamp01(y);
            return this;
        }

        public HudBuilder Pivot(Vector2 pivot)
        {
            _pivot = pivot;
            return this;
        }

        /// <summary>Size in reference pixels (will be scaled) or pass pre-scaled values with scale: false.</summary>
        public HudBuilder Size(float width, float height, bool scale = true)
        {
            _size = scale ? UITheme.ScaledSize(width, height) : new Vector2(width, height);
            return this;
        }

        public HudBuilder AnchoredPosition(Vector2 pos, bool scale = true)
        {
            _anchoredPosition = scale ? UITheme.S(pos) : pos;
            return this;
        }

        public HudBuilder WithBackground(Color? color = null)
        {
            _withBackground = true;
            if (color.HasValue)
                _backgroundColor = color.Value;
            return this;
        }

        /// <summary>Add a single text line (call multiple times for multi-line HUDs).</summary>
        public HudBuilder AddText(string name = "Text", float fontSize = -1f, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            _lines.Add(new LineSpec
            {
                Name = name,
                FontSize = fontSize > 0f ? fontSize : UITheme.FontHud,
                Alignment = alignment
            });
            return this;
        }

        /// <summary>Add N evenly spaced lines; outputs the created UIText array after Build via out param pattern.</summary>
        public HudBuilder AddLines(int count, float fontSize = -1f, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
        {
            for (int i = 0; i < count; i++)
                AddText($"Line{i}", fontSize, alignment);
            return this;
        }

        public HudHandle Build()
        {
            Transform parent = _parent;
            if (_parentToReticle)
            {
                if (!UIHelpers.TryGetReticle(out parent))
                    return null;
            }

            if (parent == null)
                return null;

            // Auto-size height from line count if multiple lines and default height
            float lineH = UITheme.ScaledHudLineHeight;
            if (_lines.Count > 1)
            {
                float needed = lineH * _lines.Count + UITheme.S(4f);
                if (_size.y < needed)
                    _size.y = needed;
            }
            else if (_lines.Count == 0)
            {
                AddText();
            }

            var root = UIFactory.CreateRect(_name, parent);
            UIHelpers.SetPointAnchor(root, _anchorX, _anchorY, _pivot);
            root.sizeDelta = _size;
            root.anchoredPosition = _anchoredPosition;

            if (_withBackground)
            {
                var bg = UIHelpers.EnsureImage(root.gameObject, _backgroundColor, raycast: false);
                UIFactory.ApplyWhiteSprite(bg);
            }

            var texts = new UIText[_lines.Count];
            for (int i = 0; i < _lines.Count; i++)
            {
                var spec = _lines[i];
                float fontSize = UITheme.S(spec.FontSize);
                var uiText = UIText.Create(root, spec.Name, "", fontSize, UIColors.TextPrimary, spec.Alignment);
                var rt = uiText.Rect;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(0f, lineH);
                rt.anchoredPosition = new Vector2(0f, -i * lineH);
                texts[i] = uiText;
            }

            return new HudHandle(root.gameObject, root, texts);
        }
    }

    /// <summary>
    /// Handle to a built HUD element.
    /// Visibility respects the vanilla Hide HUD option (<see cref="HudVisibility"/>).
    /// After quit-to-menu / scene unload the underlying GameObject is destroyed — check
    /// <see cref="IsAlive"/> and rebuild rather than keeping a stale handle.
    /// </summary>
    public class HudHandle
    {
        public GameObject GameObject { get; private set; }
        public RectTransform Rect { get; private set; }
        public UIText[] Lines { get; private set; }

        public UIText Primary => Lines != null && Lines.Length > 0 ? Lines[0] : null;

        /// <summary>Whether the mod wants this HUD shown (ignores vanilla Hide HUD).</summary>
        public bool WantActive { get; private set; } = true;

        /// <summary>
        /// True while the underlying Unity objects still exist.
        /// Becomes false after scene teardown (quit to menu, lobby reload, etc.).
        /// </summary>
        public bool IsAlive => GameObject != null;

        /// <summary>Effective on-screen state (want active and vanilla HUD not hidden).</summary>
        public bool IsActive => IsAlive && GameObject.activeSelf;

        public Vector2 Size => Rect != null ? Rect.sizeDelta : Vector2.zero;

        /// <summary>True when <paramref name="handle"/> is non-null and its GameObject still exists.</summary>
        public static bool IsValid(HudHandle handle) => handle != null && handle.IsAlive;

        internal HudHandle(GameObject go, RectTransform rt, UIText[] lines)
        {
            GameObject = go;
            Rect = rt;
            Lines = lines;

            // Mark dead as soon as Unity destroys the root (scene unload / player despawn).
            var life = go.AddComponent<HudHandleLife>();
            life.Handle = this;

            HudVisibility.Register(this);
            ApplyEffectiveVisibility();
        }

        internal void MarkDestroyed()
        {
            // Called from HudHandleLife.OnDestroy — Unity objects are already going away.
            HudVisibility.Unregister(this);
            GameObject = null;
            Rect = null;
            Lines = null;
        }

        public void SetAnchor(float x, float y)
        {
            if (!IsAlive || Rect == null) return;
            UIHelpers.SetPointAnchor(Rect, x, y, Rect.pivot);
        }

        /// <summary>
        /// Set whether this HUD should be shown when the vanilla HUD is visible.
        /// Actual GameObject active state also respects <see cref="HudVisibility.IsHidden"/>.
        /// </summary>
        public void SetActive(bool active)
        {
            WantActive = active;
            ApplyEffectiveVisibility();
        }

        internal void ApplyEffectiveVisibility()
        {
            if (!IsAlive)
                return;

            bool show = WantActive && !HudVisibility.IsHidden;
            if (GameObject.activeSelf != show)
                GameObject.SetActive(show);
        }

        public void Destroy()
        {
            HudVisibility.Unregister(this);
            var go = GameObject;
            GameObject = null;
            Rect = null;
            Lines = null;
            UIHelpers.DestroySafe(go);
        }
    }

    /// <summary>
    /// Lives on the HUD root so scene/player teardown clears the C# handle immediately.
    /// </summary>
    internal sealed class HudHandleLife : MonoBehaviour
    {
        public HudHandle Handle;

        private void OnDestroy()
        {
            Handle?.MarkDestroyed();
            Handle = null;
        }
    }
}
