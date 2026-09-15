using BepInEx.Configuration;
using UnityEngine;

namespace ImmersiveHud
{
    public partial class ImmersiveHud
    {
        private static bool isMiniMapActive;
        private static float fadeDuration = 0.5f;
        private static float notificationTimer = 0f;
        private static bool hudHidden;

        public static void HudUpdateTransparency(HudElement hudElement, ConfigEntry<float> duration)
        {
            float lerpedAlpha;
            lerpedAlpha = Mathf.Lerp(hudElement.lastSetAlpha, hudElement.targetAlpha, hudElement.timeFade / duration.Value);

            if (hudElement.elementName == "MiniMap")
            {
                hudElement.element.GetComponent<Minimap>().m_mapImageSmall.CrossFadeAlpha(hudElement.targetAlpha, hudElement.timeFade, false);
            }
            hudElement.lastSetAlpha = lerpedAlpha;
            hudElement.element.GetComponent<CanvasGroup>().alpha = lerpedAlpha;
        }

        public static void GetPlayerState(Transform hud, Player player)
        {
            Minimap playerMap = hud.Find("MiniMap").GetComponent<Minimap>();
            bool prevState = isMiniMapActive;

            isMiniMapActive = playerMap.m_smallRoot.activeSelf;

            // Reset timer when changing map modes.
            if (prevState != isMiniMapActive)
            {
                hudElements["MiniMap"].timeFade = 0;
                hudElements["MUIMap"].timeFade = 0;
            }

            GetPlayerTotalFoodValue(player);

            ItemDrop.ItemData playerEquippedWeapon = player.GetCurrentWeapon();

            if (playerEquippedWeapon != null)
            {
                playerHasItemEquipped = true;
            }
            else
            {
                playerHasItemEquipped = false;
            }
        }

        public static void GetPlayerTotalFoodValue(Player player)
        {
            playerTotalFoodValue = playerCurrentFoodValue = playerHungerCount = 0;
            playerEarliestFoodPercentage = 1f;

            foreach (Player.Food food in player.GetFoods())
            {
                playerTotalFoodValue += food.m_item.m_shared.m_food;
                playerCurrentFoodValue += food.m_health;

                if (food.CanEatAgain())
                    playerHungerCount++;

                // Track the earliest (smallest) remaining percentage of any food buff
                if (food.m_item != null && food.m_item.m_shared != null && food.m_item.m_shared.m_food > 0f)
                {
                    float remaining = food.m_health / food.m_item.m_shared.m_food;
                    if (remaining < playerEarliestFoodPercentage)
                        playerEarliestFoodPercentage = remaining;
                }
            }

            if (playerTotalFoodValue > 0f)
                playerFoodPercentage = playerCurrentFoodValue / playerTotalFoodValue;
            else
                playerFoodPercentage = 0f;

            if (playerEarliestFoodPercentage < 0f || playerEarliestFoodPercentage > 1f)
                playerEarliestFoodPercentage = 0f;
        }

        public static void HudSetValues(bool pressedHideKey, bool pressedShowKey)
        {
            Player player = Player.m_localPlayer;

            // Store previous target alpha for timer reset.
            foreach (string name in hudElementNames)
            {
                if (hudElements[name].element != null)
                    hudElements[name].targetAlphaPrev = hudElements[name].targetAlpha;
            }

            CheckPressedHideHud(pressedHideKey);
            CheckHungerNotification();

            if (hudHidden)
            {
                foreach (string name in hudElementNames)
                    hudElements[name].targetAlpha = 0;
                if (isMiniMapActive)
                {
                    CheckMinimapActive();
                }
            }
            else if (pressedShowKey)
            {
                CheckPressedShowHud();
            }
            else
            {
                CheckHealthBar(player);
                CheckFoodBar();
                CheckStaminaBar(player);
                CheckForsakenPower();
                CheckHotbar();
                CheckStatusEffect();
                CheckMinimap();
                CheckCompass();
                CheckDayTime();
                CheckKeyHints();
                CheckQuickSlots();
            }

            // Reset timer when the target alpha changed.
            foreach (string name in hudElementNames)
            {
                //if (name == GetHealthbarElement() && displayHealthOnDamageSeparateTimer.Value && playerTookDamage)
                //{
                //    hudElements[name].HudElementCheckDisplayTimer(displayHealthOnDamageDuration);
                //}
                //else
                //{
                //    hudElements[name].HudElementCheckDisplayTimer(showHudDuration);
                //}
                hudElements[name].HudElementCheckDisplayTimer(showHudDuration);

                if (!hudElements[name].exists || hudElements[name].element == null)
                    continue;

                if (hudElements[name].targetAlphaPrev != hudElements[name].targetAlpha)
                {
                    hudElements[name].timeFade = 0;
                }
            }
            SetPlayerStates();
        }

        private static void ForceVanillaStaminabar(Hud __instance)
        {
            // FIX: vanilla stam bar when eating food.
            if (!hudElements["BetterUI_StaminaBar"].exists || !hudElements["MUI_StaminaBar"].exists)
            {
                hudElements["staminapanel"].element.gameObject.SetActive(true);
                __instance.m_staminaAnimator.SetBool("Visible", true);
            }
        }

        private static void HudElementAddCanvasGroup(Transform hudRoot)
        {
            // Add CanvasGroup to each hud element on awake.
            foreach (string name in hudElementNames)
            {
                hudElements.Add(name, new HudElement(name));

                if (hudRoot.Find(name) == null)
                    continue;

                hudElements[name].setElement(hudRoot.Find(name));
                hudElements[name].element.GetComponent<RectTransform>().gameObject.AddComponent<CanvasGroup>();
                Debug.Log(name + " added CanvasGroup");
            }
        }

        private static void HudAddCanvasGroupStealth()
        {
            playerStealthBar.transform.gameObject.AddComponent<CanvasGroup>();
            playerStealthIndicator.transform.gameObject.AddComponent<CanvasGroup>();
            playerStealthIndicatorTargeted.transform.gameObject.AddComponent<CanvasGroup>();
            playerStealthIndicatorAlert.transform.gameObject.AddComponent<CanvasGroup>();
        }

        private static void HudSetValueCrosshair(Hud __instance)
        {
            playerCrosshair = __instance.m_crosshair;
            playerBowCrosshair = __instance.m_crosshairBow;
        }

        private static void HudSetValuesStealth(Hud __instance)
        {
            playerStealthBar = __instance.m_stealthBar;
            playerStealthIndicator = __instance.m_hidden;
            playerStealthIndicatorTargeted = __instance.m_targeted;
            playerStealthIndicatorAlert = __instance.m_targetedAlert;
        }

        private static void DebugListOfMissingElements(Transform hud, bool keyPressed)
        {
            if (keyPressed)
            {
                foreach (Transform t in hud.GetComponentsInChildren<Transform>(true))
                {
                    //Debug.Log("UI element: [" + t.name + "]");
                    // Check if the name exists in the list of hud element names
                    if (hudElements.ContainsKey(t.name))
                    {
                        // Check if the element exists
                        if (!hudElements[t.name].exists)
                        {
                            Debug.Log("WARN: Can't find UI element: [" + t.name + "] does not exist");
                        }
                    }
                }
            }
        }

        private static void DebugListOfHudElements(Transform hud, bool keyPressed)
        {
            if (keyPressed)
            {
                Debug.Log("-------------------------------------------");
                foreach (Transform t in hud.GetComponentsInChildren<Transform>(false))
                {
                    Debug.Log("[" + t.name + "]");
                }
                Debug.Log("-------------------------------------------");
            }
        }
        
        public static Transform FindHudRoot(Transform root)
        {
            if (root == null)
                return null;

            // Try exact match first
            Transform t = root.Find("hudroot");
            if (t != null)
                return t;

            // Fallback: search children for likely hud root names (case-insensitive)
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == null || string.IsNullOrEmpty(child.name))
                    continue;

                string name = child.name.ToLower();
                if (name.Contains("hudroot") || name.Contains("hud") || name.Contains("hud_root"))
                    return child;
            }

            // As a last resort return the provided root
            return root;
        }
    }
}