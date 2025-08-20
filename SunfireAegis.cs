using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

//health, armor, burn aura around player that deals set dmg per second to enemies in radius, scales with max health of player
//seems to be a bug with health modifiers bugging out sometimes

namespace LOLItems
{
    internal class SunfireAegis : AuraItem
    {
        // stats pool for item
        private static float HealthStat = 1f;
        private static float HealthToGive = 1f;
        private static float ArmorStat = 1f;
        private bool hasGainedArmor = false;

        private static float ImmolateBaseDamage = 0f;
        private static float ImmolateDamagePerHeart = 1.5f;
        private static float ImmolateBaseRadius = 2f;
        private static float ImmolateRadiusPerHeart = 1f;

        public static void Init()
        {
            string itemName = "Sunfire Aegis";
            string resourceName = "LOLItems/Resources/passive_item_sprites/sunfire_aegis_item_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<SunfireAegis>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Radiates Heat";
            string longDesc = "The golden armor glows with a warmth not unlike the sun. Appears to have been blessed " +
                "by the gods to burn the wicked around it.\n";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            // sets damage aura stats
            item.AuraRadius = ImmolateBaseRadius;
            item.DamagePerSecond = ImmolateBaseDamage;
            item.quality = PickupObject.ItemQuality.A;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Sunfire Aegis");

            player.healthHaver.OnHealthChanged += UpdateImmolateStats;

            // increase player's max health and current health
            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() + HealthStat, HealthToGive);
            HealthToGive = 0f; // after the initial pickup, stop increasing current health upon pickup
            if (!hasGainedArmor) player.healthHaver.Armor += ArmorStat;
            hasGainedArmor = true;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Sunfire Aegis");

            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() - HealthStat);
            player.healthHaver.OnHealthChanged -= UpdateImmolateStats;
        }

        // updates the immolate stats based on the player's current health
        private void UpdateImmolateStats(float oldHealth, float newHealth)
        {
            this.DamagePerSecond = (newHealth) * ImmolateDamagePerHeart;
            this.AuraRadius = ImmolateBaseRadius + (newHealth) * ImmolateRadiusPerHeart;
            this.Update();
        }
    }
}
