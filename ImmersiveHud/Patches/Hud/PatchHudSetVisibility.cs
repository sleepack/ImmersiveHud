using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace ImmersiveHud
{
    public partial class ImmersiveHud : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(Hud), "Update")]
        public static class PatchHudSetVisibility
        {
            private static void Postfix(Hud __instance)
            {
                Player player = Player.m_localPlayer;

                // Check if the player is in the game and if the Hud instance exists.
                if (!isEnabled.Value || !player || !__instance)
                    return;

                Transform hudRoot = FindHudRoot(__instance.transform);

                GetPlayerState(hudRoot, player);
                setCompatibility(hudRoot);
                HudSetValues(Input.GetKeyDown(hideHudKey.Value.MainKey), Input.GetKey(showHudKey.Value.MainKey));

                // Set vanilla stamina bar to always be active so hiding and showing works properly
                if (!AzuMinimalUiEnabled.Value)
                {
                    ForceVanillaStaminabar(__instance);
                }

                foreach (string name in hudElementNames)
                {
                    if (!hudElements[name].exists || hudElements[name].element == null)
                        continue;

                    hudElements[name].HudCheckLerpDuration(hudFadeDuration);

                    if (!hudElements[name].targetAlphaReached)
                    {
                        hudElements[name].timeFade += Time.deltaTime;
                        HudUpdateTransparency(hudElements[name], showHudDuration);
                    }
                    else
                    {
                        hudElements[name].timeDisplayed += Time.deltaTime;
                    }
                }
            }
        }
    }
}