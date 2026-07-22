# SparrohUILib

Shared UI library for Sparroh Mycopunk mods. One theme, one set of widgets, resolution-aware layout.

## Features

- **Theme system** — Mycopunk-inspired teal/slate surfaces with bioluminescent accents
- **Resolution scaling** — Reference 1920×1080; scales cleanly across aspect ratios via `CanvasScaler` + `UITheme.Scale`
- **HUD builder** — Single- and multi-line HUD text under the player reticle (normalized anchors); respects vanilla Hide HUD

- **Widgets** — Text, Button, Toggle, InputField, Panel, ScrollView, Separator, Dropdown, Slider, ProgressBar, Tabs, Tooltip
- **Windows & dialogs** — Overlay windows (per-window canvas), confirm/alert dialogs
- **Rich text helpers** — `Label: value unit` formatting with colored values

## Install

**Thunderstore / r2modman:** install `Sparroh-SparrohUILib` as a dependency of your mod.

**Manual:** place `SparrohUILib.dll` in `BepInEx/plugins/`.

## Consumer setup

### csproj

```xml
<Reference Include="SparrohUILib">
  <HintPath>..\SparrohUILib\bin\Release\net48\SparrohUILib.dll</HintPath>
</Reference>
```

### Plugin

```csharp
[BepInDependency("sparroh.uilibrary")]
[BepInPlugin(...)]
public class MyPlugin : BaseUnityPlugin { }
```

### thunderstore.toml

```toml
[package.dependencies]
BepInEx-BepInExPack_Mycopunk = "5.4.2403"
Sparroh-SparrohUILib = "1.0.0"
```

## Quick examples

```csharp
using Sparroh.UI;

// HUD (Altimeter-style) — rebuild when the handle dies after quit-to-menu
if (!HudHandle.IsValid(hud))
{
    hud = HudBuilder.Create("AltimeterHUD")
        .ParentToReticle()
        .Anchor(0.15f, 0.84f)
        .Size(300, 25)
        .AddText("AltitudeText")
        .Build();
}

if (HudHandle.IsValid(hud))
    hud.Primary.SetRich("Altitude", 12.3f, UIColors.Shamrock, "m");


// Multi-line HUD
var meter = HudBuilder.Create("Carnometer")
    .ParentToReticle()
    .Anchor(0.15f, 0.95f)
    .Size(320, 100)
    .AddLines(4)
    .Build();

meter.Lines[0].SetRichWithRate("Total Damage", total, dps, UIColors.Rose);

// Overlay window
var window = UIWindow.Create("Settings", new Vector2(800, 600), "Mod Settings", scrollable: true);
UIWindow.CreateSectionHeader(window.Content, "General");
UIToggle.Create(window.Content, "Enable HUD", true, on => { /* ... */ });
UISlider.Create(window.Content, "Opacity", 0f, 1f, 0.8f, v => { /* ... */ });
UIButton.Create(window.Content, "Save", () => { /* ... */ }, UIButtonStyle.Primary);

// Dialog
UIDialog.Confirm("Scrap upgrades?", "This cannot be undone.", onConfirm: DoScrap);

// Tooltip
UITooltip.Attach(someButton.GameObject, "Does the thing");
```

## Theme & scaling

| API | Purpose |
|-----|---------|
| `UIColors.*` | Palette (Sky, Rose, Shamrock, PanelBg, ButtonPrimary, …) |
| `UIColors.TryParseHex` / `ParseHex` | Parse RRGGBB / #RRGGBB hex strings |
| `ConfigColor.Bind(...)` | Bind a hex color config entry with cached `Color` |
| `UITheme.S(px)` | Scale reference pixels to current resolution |
| `UITheme.ScaledSize(w, h)` | Scaled Vector2 |
| `UITheme.ClampToScreen(size)` | Keep windows on-screen |
| `RichText.Labeled(...)` | Colored label/value strings |

### Configurable HUD colors

```csharp
// In your mod constructor:
valueColor = ConfigColor.Bind(config, "Colors", "ValueColor", UIColors.Sky,
    "Rich-text value color (hex RRGGBB or #RRGGBB).");

// When drawing:
hud.Primary.SetRich("Speed", speed, valueColor.Value, "m/s");
```


HUD positions use **normalized anchors (0–1)** so they stay consistent across resolutions. Window canvases use `CanvasScaler` with reference 1920×1080 and match width/height 0.5.

## Vanilla Hide HUD

Gameplay HUDs created with `HudBuilder` automatically hide when the player enables the vanilla **Hide HUD** option (`PlayerLook.DisablePlayerHUD`). Call `hud.SetActive(yourConfigEnabled)` as usual — the library combines your desired state with the vanilla toggle.

```csharp
// Optional: read the vanilla toggle yourself (e.g. custom non-HudHandle UI)
if (HudVisibility.IsHidden) { /* skip drawing */ }
```

Menu overlays (`UIWindow`, `GearActionBar`) are not affected.

## Scene transitions (quit to menu / lobby)

Reticle-parented HUD is destroyed with the player when you quit to menu. The C# `HudHandle` wrapper is **not** a `MonoBehaviour`, so you must treat a dead handle as missing and rebuild:

```csharp
// Every frame (or whenever you would create/update HUD):
if (!HudHandle.IsValid(hud))
{
    // optional: unregister reposition / other side state tied to the old rect
    hud = null;
    hud = HudBuilder.Create("MyHUD")
        .ParentToReticle()
        .Anchor(x, y)
        .Size(300, 25)
        .AddText()
        .Build();
    // re-register HudRepositionClient etc. after a successful Build()
}

if (!HudHandle.IsValid(hud))
    return; // player/reticle not ready yet

hud.Primary.SetRich("Speed", speed, UIColors.Sky, "m/s");
```

- `hud.IsAlive` / `HudHandle.IsValid(hud)` become false after scene unload
- Do **not** use `if (hud != null) return` alone in your create helper — a destroyed handle is still a non-null C# object
- `GearActionBar` rebuilds its host under the live Menu canvas automatically (library ticks it each frame)


## HUD repositioning

SparrohUILib does **not** own HUD repositioning. Register with ModSettingsMenu yourself:


```csharp
HudRepositionClient.Register(guid, "Altimeter", hud.Rect, anchorX, anchorY);
```

## License

MIT — see LICENSE
