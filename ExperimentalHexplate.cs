using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;

//health, dmg, and fire rate, extra dmg and fire rate when using an item

namespace LOLItems
{
    internal class ExperimentalHexplate : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float RateOfFireStat = 1.1f;
        private static float HealthStat = 1f;
        private static float HealthToGive = 1f;
        private static float OverdriveDuration = 8f;
        private static float OverdriveCooldown = 30f;
        private static float OverdriveRateOfFireStat = 1.5f;
        private static float overdriveMovementSpeedStat = 1.25f;
        private bool isOverdriveActive = false;
        public static void Init()
        {
            string itemName = "Experimental Hexplate";
            string resourceName = "LOLItems/Resources/passive_item_sprites/experimental_hexplate_item_sprite";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<ExperimentalHexplate>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Ethically Questionable";
            string longDesc = "This strange piece of armor appears to be mechanically equipped to help the user " +
                "enhance their physical abilities. It never passed testing phase for a reason.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Experimental Hexplate");

            // increase player's max health and current health
            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() + HealthStat, HealthToGive);
            HealthToGive = 0f; // after the initial pickup, stop increasing current health upon pickup
            player.OnUsedPlayerItem += OnPlayerItemUsed;
        }
        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Experimental Hexplate");

            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() - HealthStat);
            player.OnUsedPlayerItem -= OnPlayerItemUsed;
        }

        private void OnPlayerItemUsed(PlayerController player, PlayerItem item)
        {
            if (!isOverdriveActive) StartCoroutine(ApplyOverdriveBuff(player));
        }

        // upon item use, apply overdrive buff, track cooldown, and track duration
        private System.Collections.IEnumerator ApplyOverdriveBuff(PlayerController player)
        {
            isOverdriveActive = true;

            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, OverdriveRateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, overdriveMovementSpeedStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            player.stats.RecalculateStats(player, false, false);

            AkSoundEngine.PostEvent("experimental_hexplate_passive_triggered_SFX", player.gameObject);
            AkSoundEngine.PostEvent("experimental_hexplate_passive_effect_SFX", player.gameObject);

            yield return new WaitForSeconds(OverdriveDuration);

            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);

            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            
            player.stats.RecalculateStats(player, false, false);

            yield return new WaitForSeconds(OverdriveCooldown - OverdriveDuration);

            isOverdriveActive = false;
        }
    }
}
