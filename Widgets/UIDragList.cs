using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparroh.UI
{
    /// <summary>
    /// Vertical drag-reorderable list of themed rows.
    /// </summary>
    public class UIDragList
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public RectTransform Content { get; }

        private readonly List<Row> _rows = new List<Row>();
        private Action<int, int> _onReordered; // from, to
        private int _dragIndex = -1;
        private float _rowHeight;

        private class Row
        {
            public GameObject Go;
            public RectTransform Rt;
            public UIText Label;
            public int Index;
        }

        private UIDragList(GameObject go, RectTransform rt, RectTransform content)
        {
            GameObject = go;
            Rect = rt;
            Content = content;
            _rowHeight = UITheme.S(28f);
        }

        public static UIDragList Create(Transform parent, string name = "DragList")
        {
            var scroll = UIScrollView.Create(parent, name);
            return new UIDragList(scroll.GameObject, scroll.Rect, scroll.Content);
        }

        public UIDragList FillParent(float padding = 0f)
        {
            UIHelpers.SetFillParent(Rect, padding);
            return this;
        }

        public UIDragList OnReordered(Action<int, int> action)
        {
            _onReordered = action;
            return this;
        }

        public void SetItems(IList<string> labels)
        {
            UIHelpers.DestroyChildren(Content);
            _rows.Clear();
            _dragIndex = -1;

            if (labels == null)
                return;

            for (int i = 0; i < labels.Count; i++)
            {
                int index = i;
                var rowGo = UIFactory.CreateImage($"Row_{i}", Content, UIColors.EntryBg, raycast: true).gameObject;
                UIFactory.ApplyWhiteSprite(rowGo.GetComponent<Image>());
                UIHelpers.EnsureLayoutElement(rowGo, preferredHeight: _rowHeight, minHeight: _rowHeight);

                var label = UIText.Create(rowGo.transform, "Label", $"{i + 1}. {labels[i]}",
                    UITheme.ScaledFontBody, UIColors.TextPrimary, TMPro.TextAlignmentOptions.Left);
                UIHelpers.SetFillParent(label.Rect, UITheme.S(8f));
                label.Tmp.raycastTarget = false;

                var row = new Row
                {
                    Go = rowGo,
                    Rt = rowGo.GetComponent<RectTransform>(),
                    Label = label,
                    Index = i
                };
                _rows.Add(row);

                var trigger = rowGo.AddComponent<EventTrigger>();
                Add(trigger, EventTriggerType.BeginDrag, _ => BeginDrag(row));
                Add(trigger, EventTriggerType.Drag, data => OnDrag(data as PointerEventData));
                Add(trigger, EventTriggerType.EndDrag, _ => EndDrag());

            }
        }

        public void SetLabels(IList<string> labels)
        {
            if (labels == null || labels.Count != _rows.Count)
            {
                SetItems(labels);
                return;
            }

            for (int i = 0; i < _rows.Count; i++)
                _rows[i].Label.Text = $"{i + 1}. {labels[i]}";
        }

        private void BeginDrag(Row row)
        {
            _dragIndex = _rows.IndexOf(row);
            if (_dragIndex >= 0 && _dragIndex < _rows.Count)
                _rows[_dragIndex].Go.GetComponent<Image>().color = UIColors.ButtonHover;
        }


        private void OnDrag(PointerEventData data)
        {
            if (_dragIndex < 0 || data == null || Content == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, data.position, data.pressEventCamera, out var local))
                return;

            // Content is top-anchored; y decreases downward
            float yFromTop = -local.y;
            int newIndex = Mathf.Clamp(Mathf.FloorToInt(yFromTop / _rowHeight), 0, _rows.Count - 1);
            if (newIndex != _dragIndex)
            {
                // Reorder sibling
                _rows[_dragIndex].Go.transform.SetSiblingIndex(newIndex);
                var item = _rows[_dragIndex];
                _rows.RemoveAt(_dragIndex);
                _rows.Insert(newIndex, item);
                int from = _dragIndex;
                _dragIndex = newIndex;
                Renumber();
                _onReordered?.Invoke(from, newIndex);
            }
        }

        private void EndDrag()
        {
            if (_dragIndex >= 0 && _dragIndex < _rows.Count)
                _rows[_dragIndex].Go.GetComponent<Image>().color = UIColors.EntryBg;
            _dragIndex = -1;
        }

        private void Renumber()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].Index = i;
                // Keep text after the number prefix if possible
                string t = _rows[i].Label.Text;
                int dot = t.IndexOf('.');
                string rest = dot >= 0 && dot + 1 < t.Length ? t.Substring(dot + 1).TrimStart() : t;
                _rows[i].Label.Text = $"{i + 1}. {rest}";
            }
        }

        private static void Add(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> cb)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(cb);
            trigger.triggers.Add(entry);
        }
    }
}
