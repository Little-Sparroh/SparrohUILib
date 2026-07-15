using BepInEx;
using BepInEx.Logging;
using Sparroh.UI;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohUILibPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.uilibrary";
    public const string PluginName = "SparrohUILib";
    public const string PluginVersion = "1.1.1";


    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        UITheme.Initialize();
        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }
}
