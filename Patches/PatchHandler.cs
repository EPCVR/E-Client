using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EClient.Classes;
using HarmonyLib;
using EClient.Classes;

namespace EClient.Patches
{
    public class PatchHandler
    {
        public static bool IsPatched { get; private set; }
        public static int PatchErrors { get; private set; }

        public static void PatchAll()
        {
            if (!IsPatched)
            {
                instance ??= new Harmony(PluginInfo.GUID);

                foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && t.GetCustomAttribute<HarmonyPatch>() != null))
                {
                    try
                    {
                        instance.CreateClassProcessor(type).Patch();
                    }
                    catch (Exception ex)
                    {
                        PatchErrors++;
                        LogManager.LogError($"Failed to patch {type.FullName}: {ex}");
                    }
                }

                LogManager.Log($"Patched with {PatchErrors} errors");

                IsPatched = true;
            }
        }

        public static void UnpatchAll()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
                instance = null;
            }
        }

        private static Harmony instance;
        public const string InstanceId = PluginInfo.GUID;
    }
}
