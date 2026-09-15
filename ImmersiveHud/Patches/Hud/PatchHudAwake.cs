using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;

namespace ImmersiveHud
{
    public partial class ImmersiveHud : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class PatchHudAwake
        {
            private static void Postfix(Hud __instance)
            {
                if (!isEnabled.Value)
                    return;

                Transform hudRoot = FindHudRoot(__instance.transform);
                hudElements = new Dictionary<string, HudElement>();

                HudElementAddCanvasGroup(hudRoot);

                hudHidden = hudHiddenOnStart.Value;

                HudSetValueCrosshair(__instance);
                HudSetValuesStealth(__instance);
                HudAddCanvasGroupStealth();
                SetCrosshairSprite();
                setCompatibilityInit();
                setCompatibility(hudRoot);
            }
        }
    }
}