using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sparroh.UI
{
    /// <summary>
    /// Floating tooltip. Create once, Show/Hide as needed, or attach to a target via Attach.
    /// Uses its own overlay canvas so it draws above windows.
    /// </summary>
    public class UITooltip
    {
        public GameObject GameObject { get; }
        public RectTransform Rect { get; }
        public UIText Text { get; }
        public Canvas Canvas { get; }

        private static UITooltip _shared;

        private UITooltip(GameObject go, RectTransform rt, UIText text, Canvas canvas)
        {
            GameObject = go;
            Rect = rt;
            Text = text;
            Canvas = canvas;
        }

        public static UITooltip Shared
        {
            get
            {
                if (_shared == null || _shared.GameObject == null)
                    _shared = Create();
                return _shared;
            }
        }

        public static UITooltip Create(string name = "SparrohUITooltip")
        {
            var canvas = UIFactory.CreateOverlayCanvas(name + "_Canvas", UITheme.TooltipSortingOrder);
            var panel = UIPanel.Create(canvas.transform, name, UIColors.TooltipBg, withBorder: true);
            panel.Rect.pivot = new Vector2(0f, 1f);
            panel.SetPointAnchor(0f, 1f, new Vector2(0f, 1f));
            panel.SetSize(UITheme.S(280f), UITheme.S(60f));

            var text = UIText.Create(panel.Content, "TooltipText", "",
                UITheme.ScaledFontSmall, UIColors.TextPrimary, TextAlignmentOptions.TopLeft, wrap: true);
            UIHelpers.SetFillParent(text.Rect, UITheme.S(8f));

            var tip = new UITooltip(panel.GameObject, panel.Rect, text, canvas);
            tip.Hide();
            return tip;
        }

        public void Show(string message, Vector2 screenPosition)
        {
            if (string.IsNullOrEmpty(message))
            {
                Hide();
                return;
            }

            Text.Text = message;
            GameObject.SetActive(true);
            Canvas.gameObject.SetActive(true);

            // Position in canvas space
            var canvasRt = Canvas.transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPosition, null, out var local))
            {
                // Offset slightly from cursor
                local += new Vector2(UITheme.S(14f), UITheme.S(-14f));
                Rect.anchoredPosition = local;
            }

            // Fit height to text roughly
            float width = UITheme.S(280f);
            float height = Mathf.Max(UITheme.S(40f), Text.Tmp.preferredHeight + UITheme.S(20f));
            Rect.sizeDelta = new Vector2(width, height);

            ClampToScreen();
        }

        public void ShowAtMouse(string message)
        {
            Show(message, Input.mousePosition);
        }

        public void Hide()
        {
            if (GameObject != null)
                GameObject.SetActive(false);
        }

        public void Destroy()
        {
            if (Canvas != null)
                UIHelpers.DestroySafe(Canvas.gameObject);
            if (_shared == this)
                _shared = null;
        }

        /// <summary>
        /// Attach hover tooltip behavior to a UI element.
        /// </summary>
        public static void Attach(GameObject target, string message)
        {
            if (target == null)
                return;

            var trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = target.AddComponent<EventTrigger>();

            var follower = target.GetComponent<TooltipHoverFollower>();
            if (follower == null)
                follower = target.AddComponent<TooltipHoverFollower>();
            follower.Message = message;

            // Also wire EventTrigger for targets that already use it
            Add(trigger, EventTriggerType.PointerEnter, _ =>
            {
                follower.Message = message;
                Shared.ShowAtMouse(message);
            });
            Add(trigger, EventTriggerType.PointerExit, _ => Shared.Hide());
        }


        private void ClampToScreen()
        {
            var canvasRt = Canvas.transform as RectTransform;
            if (canvasRt == null)
                return;

            Vector3[] corners = new Vector3[4];
            Rect.GetWorldCorners(corners);
            // corners: 0=bl, 1=tl, 2=tr, 3=br in world; for overlay, world ~ screen
            float minX = corners[0].x;
            float maxX = corners[2].x;
            float minY = corners[0].y;
            float maxY = corners[1].y;

            Vector2 pos = Rect.anchoredPosition;
            if (maxX > Screen.width)
                pos.x -= (maxX - Screen.width);
            if (minX < 0)
                pos.x -= minX;
            if (minY < 0)
                pos.y -= minY;
            if (maxY > Screen.height)
                pos.y -= (maxY - Screen.height);

            Rect.anchoredPosition = pos;
        }

        private static void Add(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> cb)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(cb);
            trigger.triggers.Add(entry);
        }
    }

    /// <summary>
    /// Follows the mouse with the shared tooltip while the pointer is over the target.
    /// </summary>
    public class TooltipHoverFollower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Message;
        private bool _hovering;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
            if (!string.IsNullOrEmpty(Message))
                UITooltip.Shared.ShowAtMouse(Message);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            UITooltip.Shared.Hide();
        }

        private void Update()
        {
            if (_hovering && !string.IsNullOrEmpty(Message) && UITooltip.Shared.GameObject != null &&
                UITooltip.Shared.GameObject.activeSelf)
            {
                UITooltip.Shared.ShowAtMouse(Message);
            }
        }
    }
}

