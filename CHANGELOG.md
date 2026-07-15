# Changelog

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
