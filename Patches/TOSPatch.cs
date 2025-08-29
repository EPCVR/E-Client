using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EClient.Patches
{
    [HarmonyPatch(typeof(LegalAgreements), "Update")]
    public class TOSPatch
    {
        public static bool enabled;

        private static bool Prefix(LegalAgreements __instance)
        {
            if (enabled)
            {
                ControllerInputPoller.instance.leftControllerPrimary2DAxis.y = -1f;

                // Use AccessTools to change private fields
                AccessTools.FieldRef<LegalAgreements, float> scrollSpeedRef =
                    AccessTools.FieldRefAccess<LegalAgreements, float>("scrollSpeed");

                AccessTools.FieldRef<LegalAgreements, float> maxScrollSpeedRef =
                    AccessTools.FieldRefAccess<LegalAgreements, float>("_maxScrollSpeed");

                scrollSpeedRef(__instance) = 10f;
                maxScrollSpeedRef(__instance) = 10f;

                return false; // skip original
            }
            return true; // run original
        }
    }

    [HarmonyPatch(typeof(ModIOTermsOfUse_v1), "PostUpdate")]
    public class TOSPatch2
    {
        // cache so we don’t reflect every frame
        private static FieldInfo _holdField;
        private static PropertyInfo _holdProp;

        private static void ResolveHoldMembers(ModIOTermsOfUse_v1 inst)
        {
            if (_holdField != null || _holdProp != null) return;

            var t = inst.GetType();

            // Try common names first
            _holdField = AccessTools.Field(t, "holdTime")
                      ?? AccessTools.Field(t, "_holdTime")
                      ?? AccessTools.Field(t, "requiredHoldTime")
                      ?? AccessTools.Field(t, "_requiredHoldTime");

            if (_holdField == null)
            {
                // Fallback: any float field with “hold” in the name
                _holdField = AccessTools.GetDeclaredFields(t)
                            .FirstOrDefault(f => f.FieldType == typeof(float) &&
                                                 f.Name.IndexOf("hold", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _holdProp = AccessTools.Property(t, "holdTime")
                    ?? AccessTools.Property(t, "RequiredHoldTime");

            if (_holdProp == null)
            {
                // Fallback: any float property with “hold” in the name
                _holdProp = AccessTools.GetDeclaredProperties(t)
                            .FirstOrDefault(p => p.PropertyType == typeof(float) &&
                                                 p.Name.IndexOf("hold", StringComparison.OrdinalIgnoreCase) >= 0);
            }

#if UNITY_EDITOR
        if (_holdField == null && _holdProp == null)
            Debug.LogWarning($"[TOSPatch2] No hold* float found on {t.FullName}. Members: " +
                             string.Join(", ", t.GetMembers(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Select(m => m.Name)));
#endif
        }

        private static void SetHold(ModIOTermsOfUse_v1 inst, float value)
        {
            ResolveHoldMembers(inst);

            if (_holdField != null)
            {
                _holdField.SetValue(inst, value);
                return;
            }
            if (_holdProp != null && _holdProp.CanWrite)
            {
                _holdProp.SetValue(inst, value);
                return;
            }
            // nothing found -> silently bail
        }

        private static bool Prefix(ModIOTermsOfUse_v1 __instance)
        {
            if (!TOSPatch.enabled) return true;

            // jump to last page fast
            __instance.TurnPage(999);

            // “hold to accept” be gone
            ControllerInputPoller.instance.leftControllerPrimary2DAxis.y = -1f;

            // replace: __instance.holdTime = 0.1f;
            SetHold(__instance, 0.1f);

            return false; // skip original
        }
    }

    [HarmonyPatch(typeof(AgeSlider), "PostUpdate")]
    public class TOSPatch3
    {
        private static bool Prefix(AgeSlider __instance)
        {
            if (!TOSPatch.enabled) return true;

            // Try to set private field "_currentAge"
            var ageRef = AccessTools.FieldRefAccess<AgeSlider, int>("_currentAge");
            ageRef(__instance) = 21;

            // Try to set private field "holdTime"
            var holdRef = AccessTools.FieldRefAccess<AgeSlider, float>("holdTime");
            holdRef(__instance) = 0.1f;

            return false; // skip original method
        }
    }

    [HarmonyPatch(typeof(PrivateUIRoom), "StartOverlay")]
    public class TOSPatch4
    {
        private static bool Prefix() =>
            !TOSPatch.enabled;
    }

    [HarmonyPatch(typeof(KIDManager), "UseKID")]
    public class TOSPatch5
    {
        private static bool Prefix(ref Task<bool> __result)
        {
            if (!TOSPatch.enabled)
                return true;

            __result = Task.FromResult(false);
            return false;
        }
    }
}
