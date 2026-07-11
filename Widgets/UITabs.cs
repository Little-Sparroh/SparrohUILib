using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Horizontal tab strip with associated page roots.
    /// </summary>
    public class UITabs
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public RectTransform TabBar { get; }
        public RectTransform PageHost { get; }
        public int ActiveIndex { get; private set; } = -1;

        private readonly List<TabEntry> _tabs = new List<TabEntry>();
        private Action<int> _onTabChanged;

        private class TabEntry
        {
            public string Title;
            public UIButton Button;
            public GameObject Page;
        }

        private UITabs(GameObject go, RectTransform rt, RectTransform tabBar, RectTransform pageHost)
        {
            GameObject = go;
            Rect = rt;
            TabBar = tabBar;
            PageHost = pageHost;
        }

        public static UITabs Create(Transform parent, string name = "Tabs")
        {
            var root = UIFactory.CreateRect(name, parent);
            UIHelpers.SetFillParent(root);
            UIFactory.AddVerticalLayout(root.gameObject, UITheme.S(UITheme.SpacingTight),
                new RectOffset(0, 0, 0, 0), TextAnchor.UpperLeft,
                controlChildHeight: true, expandHeight: false,
                controlChildWidth: true, expandWidth: true);

            float tabH = UITheme.S(UITheme.TabHeight);
            var tabBar = UIFactory.CreateRect("TabBar", root);
            UIHelpers.EnsureLayoutElement(tabBar.gameObject, preferredHeight: tabH, minHeight: tabH);
            var tabBarBg = UIHelpers.EnsureImage(tabBar.gameObject, UIColors.WithAlpha(UIColors.TitleBar, 0.6f), raycast: false);
            UIFactory.ApplyWhiteSprite(tabBarBg);
            UIFactory.AddHorizontalLayout(tabBar.gameObject, UITheme.S(4f),
                UITheme.ScaledPadding(6, 6, 4, 0), TextAnchor.MiddleLeft,
                controlChildWidth: false, expandWidth: false,
                controlChildHeight: true, expandHeight: true);

            var pageHost = UIFactory.CreateRect("Pages", root);
            var pageLe = UIHelpers.EnsureLayoutElement(pageHost.gameObject);
            pageLe.flexibleHeight = 1f;
            pageLe.minHeight = UITheme.S(100f);
            UIHelpers.EnsureImage(pageHost.gameObject, UIColors.WithAlpha(UIColors.PanelBg, 0.5f), raycast: false);

            return new UITabs(root.gameObject, root, tabBar, pageHost);
        }

        /// <summary>
        /// Add a tab. Returns the page transform for content.
        /// </summary>
        public Transform AddTab(string title, bool select = false)
        {
            float tabH = UITheme.S(UITheme.TabHeight) - UITheme.S(4f);
            int index = _tabs.Count;

            var page = UIFactory.CreateRect($"Page_{title}", PageHost);
            UIHelpers.SetFillParent(page);
            page.gameObject.SetActive(false);

            var btn = UIButton.Create(TabBar, title, () => Select(index), UIButtonStyle.Default,
                preferredHeight: tabH);
            btn.SetWidth(UITheme.S(100f));

            _tabs.Add(new TabEntry
            {
                Title = title,
                Button = btn,
                Page = page.gameObject
            });

            if (select || ActiveIndex < 0)
                Select(index, notify: false);

            return page;
        }

        public void Select(int index, bool notify = true)
        {
            if (index < 0 || index >= _tabs.Count)
                return;

            ActiveIndex = index;
            for (int i = 0; i < _tabs.Count; i++)
            {
                bool on = i == index;
                _tabs[i].Page.SetActive(on);
                _tabs[i].Button.SetStyle(on ? UIButtonStyle.Active : UIButtonStyle.Default);
            }

            if (notify)
                _onTabChanged?.Invoke(index);
        }

        public UITabs OnTabChanged(Action<int> action)
        {
            _onTabChanged = action;
            return this;
        }

        public GameObject GetPage(int index)
        {
            if (index < 0 || index >= _tabs.Count)
                return null;
            return _tabs[index].Page;
        }

        public void SetActive(bool active) => GameObject.SetActive(active);
    }
}
