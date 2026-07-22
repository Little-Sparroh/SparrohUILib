using BepInEx;
using BepInEx.Logging;
using Sparroh.UI;
using UnityEngine.SceneManagement;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohUILibPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.uilibrary";
    public const string PluginName = "SparrohUILib";
    public const string PluginVersion = "1.1.4";


    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        UITheme.Initialize();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        // Player/reticle/menu hierarchy is gone with the scene. Clear stale static state so
        // consumers can rebuild HUD/gear UI when they next enter a lobby/mission.
        HudVisibility.ResetSessionState();
        GearActionBar.InvalidateHostForSceneUnload();
        UITheme.ClearFontCache();
    }

    private void Update()
    {
        HudVisibility.Tick();
        GearActionBar.Tick();
    }
}
