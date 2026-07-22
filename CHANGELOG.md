# Changelog

## 1.1.4

### Fixes
- **Quit-to-menu / lobby reload** — HUD handles parented to the player reticle no longer stay "alive" as stale C# wrappers after scene unload. `HudHandle.IsAlive` / `HudHandle.IsValid` let consumers detect teardown and rebuild
- `HudHandleLife` marks handles dead immediately when Unity destroys the root (player despawn / scene unload)
- `HudVisibility` prunes dead handles every tick and resets hide-state cache on scene unload
- `GearActionBar` host is invalidated on scene unload and ticked from the library plugin (no longer depends on a single consumer calling `Tick`)
- `UITheme` font cache cleared on scene unload so destroyed in-scene TMP fonts are not reused
- `UIText` accessors no-op safely when the underlying TMP was destroyed

### API
- `HudHandle.IsAlive` — true while the Unity GameObject still exists
- `HudHandle.IsValid(handle)` — null-safe validity check
- `HudVisibility.PruneDead` / `ResetSessionState`
- `UITheme.ClearFontCache`

## 1.1.3
- HUD elements built via `HudBuilder` / `HudHandle` now respect the vanilla Hide HUD option (`PlayerLook.DisablePlayerHUD`)
- Added `HudVisibility` helper; `HudHandle.SetActive` stores desired visibility and applies it only when the vanilla HUD is shown
- Menu/overlay UI (windows, gear action bar) is unaffected

## 1.1.2

- Fixed gear action bar buttons requiring clicks slightly above the visible control
- `GearActionBar` now parents under the game `Menu` canvas (camera/blit UI space) instead of a separate overlay canvas
- Bar sizing uses menu reference pixels to avoid double-scaling with the menu `CanvasScaler`
- Bar re-attaches if the menu is destroyed/recreated; stays on top while gear details is open
- Nudged bar down/right so it sits cleanly in the curved menu chrome
- Gear action buttons are centered and grow outward as more mods register slots


## 1.1.1

- Fixed gear-bar / button hover being wiped when `SetInteractable` or `SetStyle` ran every frame
- `UIButton` now tracks hover/press state and reapplies the correct color after style/interactable changes
- `GearActionBar.Register` / `SetText` / `SetInteractable` only apply when values actually change
- Stronger, style-specific button hover/pressed colors for Default, Primary, Danger, and Active

## 1.1.0
- Added hex color parsing helpers: `UIColors.TryParseHex`, `UIColors.ParseHex`
- Added `ConfigColor` helper for binding hex color config entries with cached `Color` values

## 1.0.0

- Initial release of SparrohUILib
- Theme system with Mycopunk-inspired palette and resolution-aware scaling
- Core UI factory and layout helpers
- HUD builders (single-line and multi-line panels)
- Widgets: text, button, toggle, input field, panel, scroll view, separator, dropdown, slider, progress bar, tabs, tooltip
- Overlay windows and confirmation dialogs (per-window canvases)
