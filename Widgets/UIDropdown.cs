using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Simple themed dropdown (button + expandable option list).
    /// </summary>
    public class UIDropdown
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public TextMeshProUGUI Label { get; }
        public int SelectedIndex { get; private set; }
        public string SelectedValue =>
            SelectedIndex >= 0 && SelectedIndex < _options.Count ? _options[SelectedIndex] : null;

        private readonly List<string> _options = new List<string>();
        private readonly GameObject _listRoot;
        private readonly RectTransform _listContent;
        private Action<int, string> _onChanged;
        private bool _open;

        private UIDropdown(
            GameObject go,
            RectTransform rt,
            TextMeshProUGUI label,
            GameObject listRoot,
            RectTransform listContent)
        {
            GameObject = go;
            Rect = rt;
            Label = label;
            _listRoot = listRoot;
            _listContent = listContent;
        }

        public static UIDropdown Create(
            Transform parent,
            IList<string> options,
            int initialIndex = 0,
            Action<int, string> onChanged = null,
            string name = "Dropdown")
        {
            float h = UITheme.ScaledButtonHeight;

            var root = UIFactory.CreateRect(name, parent);
            UIHelpers.EnsureLayoutElement(root.gameObject, preferredHeight: h, minHeight: h);

            var mainBtn = UIFactory.CreateImage("Main", root, UIColors.ButtonNormal, raycast: true);
            UIFactory.ApplyWhiteSprite(mainBtn);
            UIHelpers.SetFillParent(mainBtn.rectTransform);

            var label = UIFactory.CreateTmp("Label", mainBtn.rectTransform, "",
                UITheme.ScaledFontBody, UIColors.TextPrimary, TextAlignmentOptions.Center);
            UIHelpers.SetFillParent(label.rectTransform, UITheme.S(6f));

            // List is a sibling under root, positioned below
            var listBg = UIFactory.CreateImage("List", root, UIColors.Surface, raycast: true);
            UIFactory.ApplyWhiteSprite(listBg);
            var listRt = listBg.rectTransform;
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 0f);
            listRt.pivot = new Vector2(0.5f, 1f);
            listRt.anchoredPosition = new Vector2(0f, -2f);

            UIFactory.AddVerticalLayout(listBg.gameObject, UITheme.S(2f),
                UITheme.ScaledPadding(4, 4, 4, 4), TextAnchor.UpperLeft,
                controlChildHeight: true, expandHeight: false);

            var dropdown = new UIDropdown(root.gameObject, root, label, listBg.gameObject, listRt)
            {
                _onChanged = onChanged
            };

            if (options != null)
            {
                for (int i = 0; i < options.Count; i++)
                    dropdown._options.Add(options[i]);
            }

            dropdown.RebuildOptions();
            dropdown.Select(Mathf.Clamp(initialIndex, 0, Math.Max(0, dropdown._options.Count - 1)), notify: false);
            listBg.gameObject.SetActive(false);

            var btn = mainBtn.gameObject.AddComponent<Button>();
            btn.targetGraphic = mainBtn;
            btn.onClick.AddListener(() => dropdown.ToggleList());

            return dropdown;
        }

        public void SetOptions(IList<string> options, int selectIndex = 0)
        {
            _options.Clear();
            if (options != null)
            {
                for (int i = 0; i < options.Count; i++)
                    _options.Add(options[i]);
            }
            RebuildOptions();
            Select(Mathf.Clamp(selectIndex, 0, Math.Max(0, _options.Count - 1)), notify: false);
        }

        public void Select(int index, bool notify = true)
        {
            if (_options.Count == 0)
            {
                SelectedIndex = -1;
                Label.text = string.Empty;
                return;
            }

            SelectedIndex = Mathf.Clamp(index, 0, _options.Count - 1);
            Label.text = _options[SelectedIndex];
            CloseList();

            if (notify)
                _onChanged?.Invoke(SelectedIndex, _options[SelectedIndex]);
        }

        public UIDropdown OnChanged(Action<int, string> action)
        {
            _onChanged = action;
            return this;
        }

        public void ToggleList()
        {
            if (_open) CloseList();
            else OpenList();
        }

        public void OpenList()
        {
            _open = true;
            _listRoot.SetActive(true);
            float itemH = UITheme.ScaledButtonHeight * 0.85f;
            float height = itemH * Mathf.Max(1, _options.Count) + UITheme.S(8f);
            _listContent.sizeDelta = new Vector2(0f, height);
        }

        public void CloseList()
        {
            _open = false;
            _listRoot.SetActive(false);
        }

        private void RebuildOptions()
        {
            UIHelpers.DestroyChildren(_listContent);
            float itemH = UITheme.ScaledButtonHeight * 0.85f;

            for (int i = 0; i < _options.Count; i++)
            {
                int index = i;
                string opt = _options[i];
                var item = UIButton.Create(_listContent, opt, () => Select(index), UIButtonStyle.Default,
                    preferredHeight: itemH);
            }
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
