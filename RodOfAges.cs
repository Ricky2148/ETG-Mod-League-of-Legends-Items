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
    public class RodOfAges : PassiveItem
    {
        private static float HealthStat = 1f;
        private static float TimelessDamageIncreaseMax = 0.3f;
        private static float TimelessDamageIncrementValue = 0.02f;
        private static float TimelessIncreaseMax = 0.75f;
        private static float TimelessIncrementValue = 0.05f;
        private static float TimelessIncrementTimeInterval = 90f; // seconds
        private int TimelessStackCount = 0;
        private static float TimelessMaxStackHealthIncrease = 1f;

        private static float EternityAmmoRestorePercent = 0.5f;

        public static void Init()
        {
            string itemName = "Rod of Ages";
            string resourceName = "LOLItems/Resources/passive_item_sprites/rod_of_ages_item_sprite";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<RodOfAges>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "Power through time";
            string longDesc = "A mystical rod that grows stronger over time. Grants bonus health.";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);
            
            item.quality = PickupObject.ItemQuality.A;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log("Player picked up Rod of Ages");
            // Start the Timeless buff coroutine
            player.StartCoroutine(TimelessStackingTracker(player));
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log("Player dropped or got rid of Rod of Ages");
            // Stop all coroutines when the item is dropped
            player.StopAllCoroutines();
        }

        private System.Collections.IEnumerator TimelessStackingTracker(PlayerController player)
        {
            while (TimelessStackCount * TimelessIncrementValue < TimelessIncreaseMax)
            {
                yield return new WaitForSeconds(TimelessIncrementTimeInterval);

                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);

                TimelessStackCount++;
                Plugin.Log($"Rod of Ages Timeless Stack Count: {TimelessStackCount}");

                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1f + (TimelessDamageIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + (TimelessIncrementValue * TimelessStackCount), StatModifier.ModifyMethod.MULTIPLICATIVE);
            
                player.stats.RecalculateStats(player, false, false);
            }

            Plugin.Log("Rod of Ages has reached max Timeless stacks");
            player.healthHaver.OnDamaged += EternityEffect;
        }

        private void EternityEffect(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
        {
            PlayerController player = this.Owner;
            int ammoToGain = Mathf.Max(1, (int)(player.CurrentGun.maxAmmo * EternityAmmoRestorePercent));
            Plugin.Log($"Rod of Ages Eternity Effect triggered, granting {ammoToGain} ammo");
            player.CurrentGun.GainAmmo(ammoToGain);
        }
    }
}
