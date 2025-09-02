using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using UnityEngine;

namespace LOLItems
{
    internal class Shadowflame : PassiveItem
    {
        private static float DamageStat = 1.15f;
        private static float CinderbloomThreshold = 0.4f;
        private static float CinderbloomDamageAmp = 0.2f;

        public static void Init()
        {
            string itemName = "Shadowflame";
            string resourceName = "LOLItems/Resources/passive_item_sprites/shadowflame_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Shadowflame>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*hiring janitors*";
            // maybe add effect explanation?
            string longDesc = "A magical necklace that empowers you to finish off targets quicker. " +
                "You begin to feel like you like a character called \"Shadow\"? This cannot be good\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            
            item.quality = PickupObject.ItemQuality.A;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
        }

        public override void DisableEffect(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;
        }

        private void OnPostProcessProjectile (BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody != null && hitRigidbody.aiActor != null)
            {
                // checks if enemy's current health percentage meets threshold for extra damage
                float currentHealthPercent = hitRigidbody.aiActor.healthHaver.GetCurrentHealthPercentage();
                if (currentHealthPercent <= CinderbloomThreshold)
                {
                    // calculates additional extra damage to apply to enemy
                    float damageToDeal = beam.projectile.baseData.damage * CinderbloomDamageAmp * tickrate;
                    hitRigidbody.aiActor.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "shadowflame_low_health_crit_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false
                    );
                }
            }
        }

        private void OnPostProcessProjectile (Projectile proj, float f)
        {
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.aiActor != null)
                {
                    // checks if enemy's current health percentage meets threshold for extra damage
                    float currentHealthPercent = enemy.healthHaver.GetCurrentHealthPercentage();
                    if (currentHealthPercent <= CinderbloomThreshold)
                    {
                        // calculates additional extra damage to apply to enemy
                        float damageToDeal = projHit.baseData.damage * CinderbloomDamageAmp;
                        enemy.healthHaver.ApplyDamage(
                            damageToDeal,
                            Vector2.zero,
                            "shadowflame_low_health_crit_damage",
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                        );
                    }
                }
            };
        }
    }
}
