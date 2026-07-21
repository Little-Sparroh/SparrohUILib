using System.Collections.Generic;
using Pigeon.Movement;


namespace Sparroh.UI
{
    /// <summary>
    /// Tracks vanilla Hide HUD (<see cref="PlayerLook.DisablePlayerHUD"/>) and
    /// applies it to all registered <see cref="HudHandle"/> instances.
    /// Menu/overlay UI (windows, gear bar) is intentionally not affected.
    /// </summary>
    public static class HudVisibility
    {
        private static readonly List<HudHandle> Handles = new List<HudHandle>(8);
        private static bool? _lastHidden;

        /// <summary>
        /// True when the vanilla options Hide HUD toggle is active.
        /// </summary>
        public static bool IsHidden
        {
            get
            {
                try
                {
                    return PlayerLook.Instance != null && PlayerLook.Instance.DisablePlayerHUD;
                }
                catch
                {
                    return false;
                }
            }
        }

        internal static void Register(HudHandle handle)
        {
            if (handle == null || Handles.Contains(handle))
                return;
            Handles.Add(handle);
        }

        internal static void Unregister(HudHandle handle)
        {
            if (handle == null)
                return;
            Handles.Remove(handle);
        }

        /// <summary>
        /// Poll vanilla hide state and re-apply registered HUD handles when it changes.
        /// Safe to call every frame; only does work on transitions.
        /// </summary>
        public static void Tick()
        {
            bool hidden = IsHidden;
            if (_lastHidden.HasValue && _lastHidden.Value == hidden)
                return;

            _lastHidden = hidden;
            ApplyAll();
        }

        internal static void ApplyAll()
        {
            for (int i = Handles.Count - 1; i >= 0; i--)
            {
                var h = Handles[i];
                if (h == null || h.GameObject == null)
                {
                    Handles.RemoveAt(i);
                    continue;
                }

                h.ApplyEffectiveVisibility();
            }
        }
    }
}
