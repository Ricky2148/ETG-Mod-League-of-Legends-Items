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
    public class Muramana : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.2f;
        private static float ClipAndAmmoIncrease = 1.5f;
        private static float MuramanaShockBaseDamage = 5f;

        public static void Init()
        {
            string itemName = "Muramana";
            string resourceName = "LOLItems/Resources/passive_item_sprites/muramana_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Muramana>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "The Peak of Swordsmithing";
            string longDesc = "A blade forged by Masamune and wielded by the worthy, the Manamune's true " +
                "power has been unlocked and empowers your every attack.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalClipCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AmmoCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.EXCLUDED;
            item.ShouldBeExcludedFromShops = true;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += MuramanaShock;
            player.PostProcessBeamTick += MuramanaShock;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile -= MuramanaShock;
            player.PostProcessBeamTick -= MuramanaShock;
        }

        private void MuramanaShock(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (beam.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
            if (hitRigidbody!= null && hitRigidbody.aiActor != null)
            {
                //scales the damage based on player's clip size and ammo size 
                float clipSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier) - 1f) / 5);
                float ammoSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier) - 1f) / 5);
                float MuramanaShockDamageMultiplier = Mathf.Max(1f, 1f + clipSizeStat + ammoSizeStat);
                // scale damage down by tickrate
                float damageToDeal = Mathf.Max(1f, MuramanaShockBaseDamage * MuramanaShockDamageMultiplier) * tickrate;
                // calculates additional extra damage to apply to enemy
                hitRigidbody.aiActor.healthHaver.ApplyDamage(
                    damageToDeal,
                    Vector2.zero,
                    "muramana_shock_damage",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    false
                );
            }
        }

        private void MuramanaShock(Projectile proj, float f)
        {
            if (proj.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.healthHaver != null)
                {
                    //scales the damage based on player's clip size and ammo size stats
                    float clipSizeStat = Mathf.Max(0f ,(player.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier) - 1f) / 5);
                    float ammoSizeStat = Mathf.Max(0f ,(player.stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier) - 1f) / 5);
                    float MuramanaShockDamageMultiplier = Mathf.Max(1f, 1f + clipSizeStat + ammoSizeStat);
                    float damageToDeal = Mathf.Max(1f, MuramanaShockBaseDamage * MuramanaShockDamageMultiplier);
                    // calculates additional extra damage to apply to enemy
                    enemy.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "muramana_shock_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false
                    );
                }
            };
        }
    }
}
