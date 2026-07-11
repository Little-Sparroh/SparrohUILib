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
    /// </summary>
    public class HudHandle
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public UIText[] Lines { get; }

        public UIText Primary => Lines != null && Lines.Length > 0 ? Lines[0] : null;

        public bool IsActive => GameObject != null && GameObject.activeSelf;

        public Vector2 Size => Rect != null ? Rect.sizeDelta : Vector2.zero;

        internal HudHandle(GameObject go, RectTransform rt, UIText[] lines)
        {
            GameObject = go;
            Rect = rt;
            Lines = lines;
        }

        public void SetAnchor(float x, float y)
        {
            if (Rect == null) return;
            UIHelpers.SetPointAnchor(Rect, x, y, Rect.pivot);
        }

        public void SetActive(bool active)
        {
            if (GameObject != null)
                GameObject.SetActive(active);
        }

        public void Destroy()
        {
            UIHelpers.DestroySafe(GameObject);
        }
    }
}
