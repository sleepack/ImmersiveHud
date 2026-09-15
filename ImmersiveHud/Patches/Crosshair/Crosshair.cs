using BepInEx;
using HarmonyLib;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveHud
{
    public partial class ImmersiveHud : BaseUnityPlugin
    {
        private static float targetCrosshairAlpha;
        private static float targetBowDrawCrosshairAlpha;
        private static Image playerCrosshair;
        private static Image playerBowCrosshair;
        private static Sprite crosshairSpriteOriginal;
        private static Sprite crosshairBowSpriteOriginal;

        private static string crosshairSprite = "ImmersiveHud/crosshair.png";
        private static string crosshairBowSprite = "ImmersiveHud/bowcrosshair.png";

        public static void UpdateCrosshairHudElement(float bowDrawPercentage)
        {
            playerCrosshair.CrossFadeAlpha(targetCrosshairAlpha, fadeDuration, false);

            if (bowDrawPercentage > 0.0)
            {
                if (useCustomBowCrosshair.Value)
                {
                    float num = Mathf.Lerp(0.75f, 0.25f, bowDrawPercentage);
                    playerBowCrosshair.transform.localScale = new Vector3(num, num, num);
                }

                if (displayBowDrawCrosshair.Value)
                    playerBowCrosshair.color = Color.Lerp(new Color(1f, 1f, 1f, 0.0f), crosshairBowDrawColor.Value, bowDrawPercentage);
                else
                    playerBowCrosshair.color = new Color(0, 0, 0, 0);
            }
        }

        public static void UpdateStealthHudElement()
        {
            playerStealthBar.GetComponent<CanvasGroup>().alpha = targetStealthHudAlpha;
            playerStealthIndicator.GetComponent<CanvasGroup>().alpha = targetStealthHudAlpha;
            playerStealthIndicatorTargeted.GetComponent<CanvasGroup>().alpha = targetStealthHudAlpha;
            playerStealthIndicatorAlert.GetComponent<CanvasGroup>().alpha = targetStealthHudAlpha;
        }

        public static void SetCrosshairValues(Player player)
        {
            GameObject hoverObject = player.GetHoverObject();
            Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
            if (hoverable != null && !TextViewer.instance.IsVisible())
                isLookingAtActivatable = true;
            else
                isLookingAtActivatable = false;

            UpdateTargetCrosshairAlpha(player);
        }

        private static void UpdateTargetCrosshairAlpha(Player player)
        {
            if (displayCrosshairAlways.Value)
            {
                targetCrosshairAlpha = crosshairColor.Value.a;
                return;
            }

            if (displayCrosshairWhenBuilding.Value && player.InPlaceMode())
            {
                targetCrosshairAlpha = crosshairColor.Value.a;
                return;
            }

            if (displayCrosshairOnActivation.Value && isLookingAtActivatable)
            {
                targetCrosshairAlpha = crosshairColor.Value.a;
                return;
            }

            if (displayCrosshairOnEquipped.Value && characterEquippedItem)
            {
                targetCrosshairAlpha = crosshairColor.Value.a;
                return;
            }

            if (displayCrosshairOnBowEquipped.Value && characterEquippedBow)
            {
                targetCrosshairAlpha = crosshairColor.Value.a;
                return;
            }
            targetCrosshairAlpha = 0;
        }

        public static void SetStealthHudValues()
        {
            if (disableStealthHud.Value || hudHidden)
                targetStealthHudAlpha = 0;
            else
                targetStealthHudAlpha = crosshairColor.Value.a;
        }

        public static Sprite LoadCrosshairTexture(string filename)
        {
            string filePath = Path.Combine(Paths.PluginPath, filename);

            if (File.Exists(filePath))
            {
                Texture2D texture = new Texture2D(0, 0);
                byte[] bytes = File.ReadAllBytes(filePath);
                // Try calling Texture2D.LoadImage via reflection first (avoids compile-time dependency)
                var loadMethod = typeof(Texture2D).GetMethod("LoadImage", new[] { typeof(byte[]) });
                if (loadMethod != null)
                {
                    loadMethod.Invoke(texture, new object[] { bytes });
                }
                else
                {
                    // Fallback: try UnityEngine.ImageConversion.LoadImage via reflection (module may be present at runtime)
                    var icType = System.Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
                    if (icType != null)
                    {
                        var staticLoad = icType.GetMethod("LoadImage", new[] { typeof(Texture2D), typeof(byte[]) });
                        if (staticLoad != null)
                            staticLoad.Invoke(null, new object[] { texture, bytes });
                        else
                            Debug.Log("ImmersiveHud: No suitable ImageConversion.LoadImage method found.");
                    }
                    else
                    {
                        Debug.Log("ImmersiveHud: Unable to load crosshair image - ImageConversion module not available.");
                    }
                }

                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.Log("ImmersiveHud: Error. Couldn't load provided crosshair image. Check if the folder ImmersiveHud in the plugins folder has a file named crosshair.png or bowcrosshair.png");
                return null;
            }
        }

        private static void SetCrosshairSprite()
        {
            crosshairSpriteOriginal = playerCrosshair.sprite;
            crosshairBowSpriteOriginal = playerBowCrosshair.sprite;

            if (useCustomCrosshair.Value && crosshairSprite != null)
                playerCrosshair.sprite = LoadCrosshairTexture(crosshairSprite);

            if (useCustomBowCrosshair.Value && crosshairBowSprite != null)
                playerBowCrosshair.sprite = LoadCrosshairTexture(crosshairBowSprite);
        }

        private static void RemovePieceHealthBar()
        {
            Player pl = Player.m_localPlayer;
            Piece hoveringPiece = pl.GetHoveringPiece();
            Debug.Log(hoveringPiece);
            WearNTear component = hoveringPiece.GetComponent<WearNTear>();
            Debug.Log(component);
            component.enabled = false;
            //ZNetView m_nview = component.GetComponent<ZNetView>();

            //float num = m_nview.GetZDO().GetFloat(ZDOVars.s_health, component.m_health);
            //if (disableCrosshairStamina.Value && component.GetHealthPercentage() < num)
            //{
            //    __instance.m_pieceHealthRoot.gameObject.SetActive(value: false);
            //__instance.m_pieceHealthBar.SetValue(component.GetHealthPercentage());
        }
    }
}