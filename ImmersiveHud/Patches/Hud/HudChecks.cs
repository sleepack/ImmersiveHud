using UnityEngine;

namespace ImmersiveHud
{
    public partial class ImmersiveHud
    {
        private static void CheckHungerNotification()
        {
            // Hunger notification
            if (hungerNotification.Value && ((hungerNotificationOption.Value == hungerNotificationOptions.FoodHungerAmount && playerHungerCount >= foodHungerAmount.Value) || (hungerNotificationOption.Value == hungerNotificationOptions.FoodPercentage && playerFoodPercentage <= foodPercentage.Value)))
            {
                notificationTimer += Time.deltaTime;

                if ((int)notificationTimer % hungerNotificationInterval.Value == 0)
                {
                    switch (hungerNotificationType.Value)
                    {
                        case notificationTypes.SmallTopLeft:
                            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, hungerNotificationText.Value);
                            break;

                        case notificationTypes.LargeCenter:
                            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, hungerNotificationText.Value);
                            break;

                        default:
                            break;
                    }
                    notificationTimer = 1;
                }
            }
        }

        private static void CheckPressedHideHud(bool pressedHideKey)
        {
            if (pressedHideKey)
            {
                hudHidden = !hudHidden;

                // Hud hidden notification
                if (hudHiddenNotification.Value)
                {
                    if (hudHidden)
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Hud is hidden.");
                    else
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Hud is visible.");
                }
                foreach (string name in hudElementNames)
                    hudElements[name].ResetTimers();
            }
        }

        private static void CheckMinimapActive()
        {
            if (isMiniMapActive)
            {
                hudElements[GetMinimapElement()].targetAlpha = 0;
            }
            else
            {
                hudElements[GetMinimapElement()].targetAlpha = 1;
            }
        }

        private static void CheckPressedShowHud()
        {
            // Health
            if (showHealthOnKeyPressed.Value)
            {
                hudElements[GetHealthbarElement()].ShowHudForDuration();
            }

            // Food Bar
            if (showFoodBarOnKeyPressed.Value)
            {
                hudElements["BetterUI_FoodBar"].ShowHudForDuration();
                hudElements["MUI_FoodBar"].ShowHudForDuration();
                hudElements["food0"].ShowHudForDuration();
                hudElements["food1"].ShowHudForDuration();
                hudElements["food2"].ShowHudForDuration();
            }

            // Stamina
            if (showStaminaBarOnKeyPressed.Value)
            {
                hudElements[GetStaminabarElement()].ShowHudForDuration();
            }

            // Forsaken Power
            if (showPowerOnKeyPressed.Value)
            {
                hudElements["GuardianPower"].ShowHudForDuration();
                hudElements["MUI_GuardianPowerBar"].ShowHudForDuration();
            }

            // Hot Key Bar
            if (showHotKeyBarOnKeyPressed.Value)
                hudElements["HotKeyBar"].ShowHudForDuration();

            // Status Effects
            if (showStatusEffectsOnKeyPressed.Value)
                hudElements["StatusEffects"].ShowHudForDuration();

            // Mini Map
            if (showMiniMapOnKeyPressed.Value)
            {
                hudElements[GetMinimapElement()].ShowHudForDuration();
            }

            // Compass
            if (showCompassOnKeyPressed.Value)
                hudElements["Compass"].ShowHudForDuration();

            // Day and Time
            if (showTimeOnKeyPressed.Value && oryxenTimeEnabled.Value)
                hudElements["DayTimePanel"].ShowHudForDuration();

            // Quick Slots
            if (showQuickSlotsOnKeyPressed.Value)
            {
                hudElements[GetQuickslotElement()].ShowHudForDuration();
            }

            // Key Hints
            if (showKeyHintsOnKeyPressed.Value)
                hudElements["KeyHints"].ShowHudForDuration();
        }

        private static void CheckHealthBar(Player player)
        {
            string nameHealthbar = GetHealthbarElement();

            // Health Display
            if (displayHealthAlways.Value || (displayHealthInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements[nameHealthbar].targetAlpha = 1;

                if (!displayHealthInInventory.Value && InventoryGui.IsVisible())
                {
                    hudElements[nameHealthbar].HudSetTargetAlpha(0);
                }
            }
            else if ((displayHealthWhenEating.Value && playerAteFood) || (displayHealthOnDamage.Value && playerTookDamage))
            {
                hudElements[nameHealthbar].ShowHudForDuration();
            }
                else if (
                    // Display health panel when below a given percentage
                    (displayHealthWhenBelow.Value && player.GetHealthPercentage() <= healthPercentage.Value) ||
                    (displayHealthWhenFoodBelow.Value && (playerFoodPercentage <= foodPercentage.Value || playerEarliestFoodPercentage <= foodPercentage.Value)) ||
                    // Show health when hungry (only if hunger count meets threshold)
                    (displayHealthWhenHungry.Value && playerHungerCount >= foodHungerAmount.Value)
                    )
            {
                hudElements[nameHealthbar].HudSetTargetAlpha(1);
            }
            else
            {
                hudElements[nameHealthbar].HudSetTargetAlpha(0);
            }
        }

        private static void CheckFoodBar()
        {
            // Food Bar Display
            if (displayFoodBarAlways.Value || (displayFoodBarInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements["BetterUI_FoodBar"].targetAlpha = 1;
                hudElements["MUI_FoodBar"].targetAlpha = 1;
                hudElements["food0"].targetAlpha = 1;
                hudElements["food1"].targetAlpha = 1;
                hudElements["food2"].targetAlpha = 1;

                if (!displayFoodBarInInventory.Value && InventoryGui.IsVisible())
                {
                    hudElements["BetterUI_FoodBar"].HudSetTargetAlpha(0);
                    hudElements["MUI_FoodBar"].HudSetTargetAlpha(0);
                    hudElements["food0"].HudSetTargetAlpha(0);
                    hudElements["food1"].HudSetTargetAlpha(0);
                    hudElements["food2"].HudSetTargetAlpha(0);
                }
            }
            else
            {
                if ((BetterUIEnabled.Value && hudElements["BetterUI_FoodBar"].exists) || (AzuMinimalUiEnabled.Value && hudElements["MUI_FoodBar"].exists))
                {
                    // Display food bar when eating food
                    if (displayFoodBarWhenEating.Value && playerAteFood)
                    {
                        hudElements["BetterUI_FoodBar"].ShowHudForDuration();
                        hudElements["MUI_FoodBar"].ShowHudForDuration();
                        hudElements["food0"].ShowHudForDuration();
                        hudElements["food1"].ShowHudForDuration();
                        hudElements["food2"].ShowHudForDuration();
                    }

                    // Display food bar when below a given percentage.
                    if (displayFoodBarWhenBelow.Value && ((playerFoodPercentage <= foodPercentage.Value) || (playerHungerCount >= foodHungerAmount.Value)))
                    {
                        hudElements["BetterUI_FoodBar"].HudSetTargetAlpha(1);
                        hudElements["MUI_FoodBar"].HudSetTargetAlpha(1);
                        hudElements["food0"].HudSetTargetAlpha(1);
                        hudElements["food1"].HudSetTargetAlpha(1);
                        hudElements["food2"].HudSetTargetAlpha(1);
                    }
                    else
                    {
                        hudElements["BetterUI_FoodBar"].HudSetTargetAlpha(0);
                        hudElements["MUI_FoodBar"].HudSetTargetAlpha(0);
                        hudElements["food0"].HudSetTargetAlpha(0);
                        hudElements["food1"].HudSetTargetAlpha(0);
                        hudElements["food2"].HudSetTargetAlpha(0);
                    }
                }
            }
        }

        private static void CheckStaminaBar(Player player)
        {
            string nameStaminabar = GetStaminabarElement();

            // Stamina Bar Display
            if (displayStaminaBarAlways.Value || (displayStaminaBarInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements[nameStaminabar].targetAlpha = 1;

                if (!displayStaminaBarInInventory.Value && InventoryGui.IsVisible())
                {
                    hudElements[nameStaminabar].HudSetTargetAlpha(0);
                }
            }
            else
            {
                // Display stamina bar when stamina is used
                if (displayStaminaBarOnUse.Value && playerUsedStamina)
                {
                    hudElements[nameStaminabar].ShowHudForDuration();
                }

                // Display stamina bar when eating food
                if (displayStaminaBarWhenEating.Value && playerAteFood)
                {
                    hudElements[nameStaminabar].ShowHudForDuration();
                }

                // Display stamina bar when below a given percentage.
                if (
                    (displayStaminaBarWhenBelow.Value && player.GetStaminaPercentage() <= staminaPercentage.Value) ||
                    (displayStaminaBarWhenFoodBelow.Value && playerFoodPercentage <= foodPercentage.Value) ||
                    (displayStaminaBarWhenHungry.Value && playerHungerCount >= foodHungerAmount.Value)
                   )
                {
                    hudElements[nameStaminabar].HudSetTargetAlpha(1);
                }
                else
                {
                    hudElements[nameStaminabar].HudSetTargetAlpha(0);
                }
            }
        }

        private static void CheckForsakenPower()
        {
            // Forsaken Power Display
            if (displayForsakenPowerAlways.Value || (displayPowerInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements["GuardianPower"].targetAlpha = 1;
                hudElements["MUI_GuardianPowerBar"].targetAlpha = 1;

                if (!displayPowerInInventory.Value && InventoryGui.IsVisible())
                    hudElements["GuardianPower"].HudSetTargetAlpha(0);
                hudElements["MUI_GuardianPowerBar"].HudSetTargetAlpha(0);
            }
            else
            {
                // Show the forsaken power for a duration when the key is pressed.
                if (displayPowerOnActivation.Value && (ZInput.GetButtonDown("GPower") || ZInput.GetButtonDown("JoyGPower")))
                {
                    hudElements["GuardianPower"].ShowHudForDuration();
                    hudElements["MUI_GuardianPowerBar"].ShowHudForDuration();
                }

                hudElements["GuardianPower"].HudSetTargetAlpha(0);
                hudElements["MUI_GuardianPowerBar"].HudSetTargetAlpha(0);
            }
        }

        private static void CheckHotbar()
        {
            // Hot Key Bar Display
            if (displayHotKeyBarAlways.Value || (displayHotKeyBarInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements["HotKeyBar"].targetAlpha = 1;

                if (!displayHotKeyBarInInventory.Value && InventoryGui.IsVisible())
                    hudElements["HotKeyBar"].HudSetTargetAlpha(0);
            }
            else
            {
                // Display on item switch/use
                if (displayHotKeyBarOnItemSwitch.Value && playerUsedHotBarItem)
                    hudElements["HotKeyBar"].ShowHudForDuration();
                else if (displayHotKeyBarWhenItemEquipped.Value && playerHasItemEquipped)
                    hudElements["HotKeyBar"].HudSetTargetAlpha(1);
                else
                    hudElements["HotKeyBar"].HudSetTargetAlpha(0);
            }
        }

        private static void CheckStatusEffect()
        {
            // Status Effects Display
            if (displayStatusEffectsAlways.Value || (displayStatusEffectsInInventory.Value && InventoryGui.IsVisible()))
            {
                hudElements["StatusEffects"].targetAlpha = 1;

                if (!displayStatusEffectsInInventory.Value && InventoryGui.IsVisible())
                    hudElements["StatusEffects"].HudSetTargetAlpha(0);
            }
            else
            {
                hudElements["StatusEffects"].HudSetTargetAlpha(0);
            }
        }

        private static void CheckMinimap()
        {
            // Mini Map Display
            if (displayMiniMapAlways.Value || (displayMiniMapInInventory.Value && InventoryGui.IsVisible()) || !isMiniMapActive)
            {
                hudElements["MiniMap"].targetAlpha = 1;
                hudElements["MUIMap"].targetAlpha = 1;

                if (!displayMiniMapInInventory.Value && InventoryGui.IsVisible())
                    hudElements["MiniMap"].HudSetTargetAlpha(0);
                hudElements["MUIMap"].HudSetTargetAlpha(0);
            }
            else
            {
                hudElements["MiniMap"].HudSetTargetAlpha(0);
                hudElements["MUIMap"].HudSetTargetAlpha(0);
            }
        }

        private static void CheckKeyHints()
        {
            if (displayKeyHintsAlways.Value && hudElements["KeyHints"].exists)
            {
                hudElements["KeyHints"].targetAlpha = 1;
            }
            else
            {
                hudElements["KeyHints"].HudSetTargetAlpha(0);
            }
        }

        private static void CheckQuickSlots()
        {
            CheckRandyQuickSlots();
            CheckAzuQuickSlots();
        }
    }
}