using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class NavoriQuickblades : PassiveItem
    {
        private static float RateOfFireStat = 1.2f;
        private static float TranscendenceCooldownReductionRatio = 0.02f;

        public static void Init()
        {
            string itemName = "Navori Quickblades";
            string resourceName = "LOLItems/Resources/passive_item_sprites/navori_quickblades_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<NavoriQuickblades>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"random bullshit go!\"";
            // maybe add effect explanation?
            string longDesc = "A set of knives that magically come back after they land. Somehow there's always " +
                "at least one in your hand. You wonder what would happen if you threw them all at once but you " +
                "never do.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.B;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            // subscribe to reload and projectile events
            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
        }

        public override void DisableEffect(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            // unsubscribe from events
            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody != null && hitRigidbody.aiActor != null)
            {
                PlayerController player = this.Owner;
                
                if (!player.CurrentItem.IsCurrentlyActive)
                {
                    // reduces cooldown based on damage cooldown values
                    if (player.CurrentItem.CurrentDamageCooldown > 0)
                    {
                        float currentCooldownValue = player.CurrentItem.CurrentDamageCooldown;
                        float reducedCooldownValue = currentCooldownValue * (1f - (TranscendenceCooldownReductionRatio * tickrate));
                        player.CurrentItem.CurrentDamageCooldown = reducedCooldownValue;
                    }
                    else if (player.CurrentItem.CurrentTimeCooldown > 0)
                    {
                        float currentCooldownValue = player.CurrentItem.CurrentTimeCooldown;
                        float reducedCooldownValue = currentCooldownValue * (1f - TranscendenceCooldownReductionRatio);
                        player.CurrentItem.CurrentTimeCooldown = reducedCooldownValue;
                    }
                }
            }
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null || enemy.aiActor != null)
                {
                    PlayerController player = this.Owner;
                    if (!player.CurrentItem.IsCurrentlyActive)
                    {
                        // reduces cooldown based on damage cooldown values
                        if (player.CurrentItem.CurrentDamageCooldown > 0)
                        {
                            float currentCooldownValue = player.CurrentItem.CurrentDamageCooldown;
                            float reducedCooldownValue = currentCooldownValue * (1f - TranscendenceCooldownReductionRatio);
                            player.CurrentItem.CurrentDamageCooldown = reducedCooldownValue;
                        }
                        else if (player.CurrentItem.CurrentTimeCooldown > 0)
                        {
                            float currentCooldownValue = player.CurrentItem.CurrentTimeCooldown;
                            float reducedCooldownValue = currentCooldownValue * (1f - TranscendenceCooldownReductionRatio);
                            player.CurrentItem.CurrentTimeCooldown = reducedCooldownValue;
                        }
                    }
                }
            };
        }
    }
}
