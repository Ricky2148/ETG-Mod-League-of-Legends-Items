using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LOLItems
{
    internal class RodOfAges : PassiveItem
    {
        private static float TimelessDamageIncreaseMax = 0.3f;
        private static float TimelessDamageIncrementValue = 0.02f;
        private static float TimelessIncreaseMax = 0.75f;
        private static float TimelessIncrementValue = 0.05f;
        private static float TimelessIncrementTimeInterval = 90f; // seconds
        private int TimelessStackCount = 0;
        private static float TimelessMaxStackHealthIncrease = 1f;

        private static float EternityAmmoRestorePercent = 0.25f;

        public static void Init()
        {
            string itemName = "Rod of Ages";
            string resourceName = "LOLItems/Resources/passive_item_sprites/rod_of_ages_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<RodOfAges>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "Power through wisdom";
            // maybe add effect explanation?
            string longDesc = "A staff once wielded by a legendary sorcerer, said to have achieved immortality " +
                "and still lives on to this day. The vast knowledge within this tool takes ages to fully grasp.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.B;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            // Start the Timeless buff coroutine
            player.StartCoroutine(TimelessStackingTracker(player));
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");
            // Stop all coroutines when the item is dropped
            player.StopAllCoroutines();
        }

        // coroutine starts as soon as item is picked up
        private System.Collections.IEnumerator TimelessStackingTracker(PlayerController player)
        {
            // loops until stackCount is at maxstacks
            while (TimelessStackCount * TimelessIncrementValue < TimelessIncreaseMax)
            {
                // wait timer
                yield return new WaitForSeconds(TimelessIncrementTimeInterval);

                // reset current stat mods
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                TimelessStackCount++;
                Plugin.Log($"Rod of Ages Timeless Stack Count: {TimelessStackCount}");

                // apply new increased stat mods
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
            
                player.stats.RecalculateStats(player, false, false);
            }

            // when at max stacks, increase health and provide eternity effect
            Plugin.Log("Rod of Ages has reached max Timeless stacks");
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Health, TimelessMaxStackHealthIncrease, StatModifier.ModifyMethod.ADDITIVE);

            player.OnReceivedDamage += EternityEffect;
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
                    gun.GainAmmo(ammoToGain);
                }
            }
        }
    }
}
