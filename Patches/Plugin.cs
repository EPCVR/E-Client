using BepInEx;
using System.ComponentModel;

namespace EClient.Patches
{
    [Description(EClient.PluginInfo.Description)]
    [BepInPlugin(EClient.PluginInfo.GUID, EClient.PluginInfo.Name, EClient.PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        private void OnEnable()
        {
            Menu.ApplyHarmonyPatches();
        }

        private void OnDisable()
        {
            Menu.RemoveHarmonyPatches();
        }
    }
}
