using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class RodOfAges : PassiveItem
    {
        public static string ItemName = "Rod of Ages";

        private static float TimelessDamageIncreaseMax = 0.3f;
        private static float TimelessDamageIncrementValue = 0.02f;
        private static float TimelessIncreaseMax = 0.75f;
        private static float TimelessIncrementValue = 0.05f;
        private static float TimelessIncrementTimeInterval = 120f; // seconds
        private int TimelessStackCount = 0;
        private static float TimelessMaxStackHealthIncrease = 1f;

        private Coroutine TimelessStackTrackerCoroutine;

        private bool EternityActivated = false;
        private static float EternityAmmoRestorePercent = 0.25f;

        public bool SUPERTRAININGActivated = false;
        private static float SUPERTRAININGTimelessStackMultiplier = 2f;
        public bool AGEOLDWISDOMActivated = false;
        public bool ARCANEMASTERYActivated = false;
        private static float ARCANEMASTERYMovementSpeedStat = 1.0f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/rod_of_ages_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<RodOfAges>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "Power through wisdom";
            // maybe add effect explanation?
            string longDesc = $"Gains a stack every {TimelessIncrementTimeInterval} seconds. Each stack increases max ammo, clip size, and damage a little. " +
                $"At max stacks, gain an extra heart and taking damage restores some ammo.\n\n" +
                "A staff once wielded by a legendary sorcerer, said to have achieved immortality " +
                "and still lives on to this day. The vast knowledge within this tool takes ages to fully grasp.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:rod_of_ages",
                "macho_brace"
            };
            CustomSynergies.Add("Training Up!", mandatoryConsoleIDs, null, true);

            List<string> mandatoryConsoleIDs2 = new List<string>
            {
                "LOLItems:rod_of_ages"
            };
            List<string> optionalConsoleIDs2 = new List<string>
            {
                "old_knights_shield",
                "old_knights_helm",
                "old_knights_flask",
            };
            CustomSynergies.Add("Age Old Wisdom", mandatoryConsoleIDs2, optionalConsoleIDs2, true);*/
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            //Plugin.Log($"{EternityActivated}");

            // Start the Timeless buff coroutine
            TimelessStackTrackerCoroutine = StartCoroutine(TimelessStackingTracker(player));
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");
            // Stop all coroutines when the item is dropped

            //Plugin.Log($"{EternityActivated}");

            if (player != null)
            {
                if (TimelessStackTrackerCoroutine != null)
                {
                    StopCoroutine(TimelessStackTrackerCoroutine);
                }
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.SUPER_TRAINING) && !SUPERTRAININGActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                    // apply new increased stat mods
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);

                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    SUPERTRAININGActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.SUPER_TRAINING) && SUPERTRAININGActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                    // apply new increased stat mods
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);

                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    SUPERTRAININGActivated = false;
                }

                if (Owner.HasSynergy(Synergy.AGE_OLD_WISDOM) && !AGEOLDWISDOMActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + TimelessDamageIncreaseMax, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    AGEOLDWISDOMActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.AGE_OLD_WISDOM) && AGEOLDWISDOMActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    AGEOLDWISDOMActivated = false;
                }

                if (Owner.HasSynergy(Synergy.ARCANE_MASTERY) && !ARCANEMASTERYActivated)
                {
                    if (TimelessStackTrackerCoroutine != null)
                    {
                        StopCoroutine(TimelessStackTrackerCoroutine);
                    }

                    TimelessStackCount = 15;

                    // reset current stat mods
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                    // apply new increased stat mods
                    if (SUPERTRAININGActivated)
                    {
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    }
                    else
                    {
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    }

                    //Plugin.Log($"pre activation in synergy:{EternityActivated}");
                    if (!EternityActivated)
                    {
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Health, TimelessMaxStackHealthIncrease, StatModifier.ModifyMethod.ADDITIVE);

                        Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                        Owner.healthHaver.ApplyHealing(1f);

                        HelpfulMethods.CustomNotification("Achieved Eternity!", "", this.sprite, UINotificationController.NotificationColor.PURPLE);

                        Owner.OnReceivedDamage += EternityEffect;

                        EternityActivated = true;
                        //Plugin.Log($"post activation in synergy:{EternityActivated}");
                    }

                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, ARCANEMASTERYMovementSpeedStat, StatModifier.ModifyMethod.ADDITIVE);

                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);
                    //Plugin.Log($"postprocessproj on");

                    ARCANEMASTERYActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ARCANE_MASTERY) && ARCANEMASTERYActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);
                    //Plugin.Log($"postprocessproj off");

                    ARCANEMASTERYActivated = false;
                }
            }

            base.Update();
        }

        // coroutine starts as soon as item is picked up
        private System.Collections.IEnumerator TimelessStackingTracker(PlayerController player)
        {
            // loops until stackCount is at maxstacks
            while (TimelessStackCount * TimelessIncrementValue < TimelessIncreaseMax)
            {
                // wait timer
                if (player.HasSynergy(Synergy.SUPER_TRAINING))
                {
                    yield return new WaitForSeconds(TimelessIncrementTimeInterval / 2f);
                }
                else
                {
                    yield return new WaitForSeconds(TimelessIncrementTimeInterval);
                }

                // reset current stat mods
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                TimelessStackCount++;
                //Plugin.Log($"Rod of Ages Timeless Stack Count: {TimelessStackCount}");

                // apply new increased stat mods
                if (SUPERTRAININGActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount * SUPERTRAININGTimelessStackMultiplier), StatModifier.ModifyMethod.MULTIPLICATIVE);
                }
                else
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                }
                    
                //player.stats.RecalculateStats(player, false, false);
                player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
            }

            // when at max stacks, increase health and provide eternity effect
            //Plugin.Log("Rod of Ages has reached max Timeless stacks");

            //Plugin.Log($"pre activation in coroutine:{EternityActivated}");

            if (!EternityActivated)
            {
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Health, TimelessMaxStackHealthIncrease, StatModifier.ModifyMethod.ADDITIVE);

                player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

                player.healthHaver.ApplyHealing(1f);

                HelpfulMethods.CustomNotification("Achieved Eternity!", "", this.sprite, UINotificationController.NotificationColor.PURPLE);

                player.OnReceivedDamage += EternityEffect;
    
                EternityActivated = true;

                //Plugin.Log($"post activation in coroutine:{EternityActivated}");
            }
        }

        // when player is damaged, refill all their weapons with some percent ammo
        private void EternityEffect(PlayerController source)
        {
            for (int i = 0; i < source.inventory.AllGuns.Count; i++)
            {
                Gun gun = source.inventory.AllGuns[i];
                if (!gun.InfiniteAmmo && gun.CanGainAmmo)
                {
                    int ammoToGain = Mathf.CeilToInt((float)gun.AdjustedMaxAmmo * EternityAmmoRestorePercent);
                    if (source.HasSynergy(Synergy.AGE_OLD_WISDOM))
                    {
                        ammoToGain *= 2;
                    }
                    gun.GainAmmo(ammoToGain);
                }
            }
        }
    }
}
