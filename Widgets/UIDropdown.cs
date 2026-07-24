using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Simple themed dropdown (button + expandable option list).
    /// Open lists reparent to the root canvas and use override sorting so they
    /// paint above later siblings and are not clipped by scroll masks.
    /// </summary>
    public class UIDropdown
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public TextMeshProUGUI Label { get; }
        public int SelectedIndex { get; private set; }
        public string SelectedValue =>
            SelectedIndex >= 0 && SelectedIndex < _options.Count ? _options[SelectedIndex] : null;

        /// <summary>True while the option list is open (may be reparented off the dropdown root).</summary>
        public bool IsOpen => _open;


        private readonly List<string> _options = new List<string>();
        private readonly GameObject _listRoot;
        private readonly RectTransform _listContent;
        private readonly RectTransform _mainBtnRt;
        private Action<int, string> _onChanged;
        private bool _open;

        // Restored when the popup closes (reparent back under the dropdown root).
        private Transform _listHomeParent;
        private int _listHomeSiblingIndex;
        private Vector2 _listHomeAnchorMin;
        private Vector2 _listHomeAnchorMax;
        private Vector2 _listHomePivot;
        private Vector2 _listHomeAnchoredPos;
        private Vector2 _listHomeSizeDelta;
        private bool _listHomeCaptured;

        private UIDropdown(
            GameObject go,
            RectTransform rt,
            TextMeshProUGUI label,
            GameObject listRoot,
            RectTransform listContent,
            RectTransform mainBtnRt)
        {
            GameObject = go;
            Rect = rt;
            Label = label;
            _listRoot = listRoot;
            _listContent = listContent;
            _mainBtnRt = mainBtnRt;
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

            // List starts as a sibling under root, positioned below the main button.
            // On open it is reparented to the root canvas so masks / later siblings cannot cover it.
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

            // Popup must not participate in parent layout while open (or closed under root).
            var listLe = listBg.gameObject.AddComponent<LayoutElement>();
            listLe.ignoreLayout = true;

            var dropdown = new UIDropdown(
                root.gameObject,
                root,
                label,
                listBg.gameObject,
                listRt,
                mainBtn.rectTransform)
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
            if (_open)
                return;

            _open = true;
            float itemH = UITheme.ScaledButtonHeight * 0.85f;
            float height = itemH * Mathf.Max(1, _options.Count) + UITheme.S(8f);
            float gap = UITheme.S(2f);

            CaptureListHomeIfNeeded();
            EnsureListOverlayComponents();

            // Reparent to root canvas so scroll Mask / later siblings cannot clip or cover the list.
            var host = ResolvePopupHost();
            if (host != null && _listContent.parent != host)
                _listContent.SetParent(host, worldPositionStays: false);

            _listRoot.SetActive(true);
            _listContent.SetAsLastSibling();

            // Match the main button's screen-space width and sit just below it.
            PositionListBelowButton(height, gap);

            // Force layout so option buttons size correctly after reparent.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_listContent);
        }

        public void CloseList()
        {
            if (!_open && (_listRoot == null || !_listRoot.activeSelf))
            {
                _open = false;
                return;
            }

            _open = false;

            if (_listRoot != null)
                _listRoot.SetActive(false);

            RestoreListHome();
        }

        private void CaptureListHomeIfNeeded()
        {
            if (_listHomeCaptured || _listContent == null)
                return;

            _listHomeParent = _listContent.parent;
            _listHomeSiblingIndex = _listContent.GetSiblingIndex();
            _listHomeAnchorMin = _listContent.anchorMin;
            _listHomeAnchorMax = _listContent.anchorMax;
            _listHomePivot = _listContent.pivot;
            _listHomeAnchoredPos = _listContent.anchoredPosition;
            _listHomeSizeDelta = _listContent.sizeDelta;
            _listHomeCaptured = true;
        }

        private void RestoreListHome()
        {
            if (!_listHomeCaptured || _listContent == null)
                return;

            // Home parent may have been destroyed (e.g. list rebuild / window teardown).
            if (_listHomeParent == null)
                return;

            if (_listContent.parent != _listHomeParent)
                _listContent.SetParent(_listHomeParent, worldPositionStays: false);

            int sibling = Mathf.Clamp(_listHomeSiblingIndex, 0, _listHomeParent.childCount - 1);
            _listContent.SetSiblingIndex(sibling);

            _listContent.anchorMin = _listHomeAnchorMin;
            _listContent.anchorMax = _listHomeAnchorMax;
            _listContent.pivot = _listHomePivot;
            _listContent.anchoredPosition = _listHomeAnchoredPos;
            _listContent.sizeDelta = _listHomeSizeDelta;
        }

        private void EnsureListOverlayComponents()
        {
            if (_listRoot == null)
                return;

            var canvas = _listRoot.GetComponent<Canvas>();
            if (canvas == null)
                canvas = _listRoot.AddComponent<Canvas>();

            canvas.overrideSorting = true;
            canvas.sortingOrder = UITheme.DropdownSortingOrder;

            if (_listRoot.GetComponent<GraphicRaycaster>() == null)
                _listRoot.AddComponent<GraphicRaycaster>();

            var le = _listRoot.GetComponent<LayoutElement>();
            if (le == null)
                le = _listRoot.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
        }

        /// <summary>
        /// Prefer the root overlay canvas transform so the list escapes nested Masks
        /// while staying in the same screen-space UI tree (correct EventCamera / scaler).
        /// </summary>
        private RectTransform ResolvePopupHost()
        {
            if (Rect == null)
                return null;

            var canvas = Rect.GetComponentInParent<Canvas>();
            if (canvas == null)
                return null;

            var root = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
            return root.transform as RectTransform;
        }

        private void PositionListBelowButton(float height, float gap)
        {
            if (_listContent == null || _mainBtnRt == null)
                return;

            var host = _listContent.parent as RectTransform;
            if (host == null)
                return;

            // Corners of the main button in host-local space.
            Vector3[] corners = new Vector3[4];
            _mainBtnRt.GetWorldCorners(corners);
            // 0=BL, 1=TL, 2=TR, 3=BR
            Vector3 worldBl = corners[0];
            Vector3 worldBr = corners[3];
            Vector3 worldTl = corners[1];

            Camera eventCam = null;
            var canvas = host.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                eventCam = canvas.worldCamera;

            Vector2 localBl = WorldToLocal(host, worldBl, eventCam);
            Vector2 localBr = WorldToLocal(host, worldBr, eventCam);
            Vector2 localTl = WorldToLocal(host, worldTl, eventCam);

            float width = Mathf.Abs(localBr.x - localBl.x);
            float centerX = (localBl.x + localBr.x) * 0.5f;
            // Sit just under the button's bottom edge (pivot at top-center of list).
            float topY = localBl.y - gap;

            // If the list would go off the bottom of the host, flip above the button.
            // Pivot is top-center, so topY is the top edge of the list.
            float hostBottom = GetHostLocalBottom(host);
            if (topY - height < hostBottom)
                topY = localTl.y + gap + height;


            _listContent.anchorMin = new Vector2(0.5f, 0.5f);
            _listContent.anchorMax = new Vector2(0.5f, 0.5f);
            _listContent.pivot = new Vector2(0.5f, 1f);
            _listContent.sizeDelta = new Vector2(Mathf.Max(1f, width), height);
            _listContent.anchoredPosition = new Vector2(centerX, topY);
        }

        private static Vector2 WorldToLocal(RectTransform host, Vector3 world, Camera eventCam)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(eventCam, world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(host, screen, eventCam, out var local);
            return local;
        }

        private static float GetHostLocalBottom(RectTransform host)
        {
            // Host rect in its own local space: pivot-relative.
            Rect r = host.rect;
            return r.yMin;
        }

        private void RebuildOptions()
        {
            // Close first so we don't rebuild while reparented to the canvas.
            if (_open)
                CloseList();

            UIHelpers.DestroyChildren(_listContent);
            float itemH = UITheme.ScaledButtonHeight * 0.85f;

            for (int i = 0; i < _options.Count; i++)
            {
                int index = i;
                string opt = _options[i];
                UIButton.Create(_listContent, opt, () => Select(index), UIButtonStyle.Default,
                    preferredHeight: itemH);
            }
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
