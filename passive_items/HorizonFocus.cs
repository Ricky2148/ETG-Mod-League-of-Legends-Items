using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;

namespace LOLItems
{
    internal class HorizonFocus : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float HyperShotMaxDistance = 15f;
        private static float HyperShotMinDistance = 3f;
        private static float HyperShotMaxDamageInc = 0.25f;

        public static void Init()
        {
            string itemName = "Horizon Focus";
            string resourceName = "LOLItems/Resources/passive_item_sprites/horizon_focus_pixelart_sprite";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<HorizonFocus>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "*not camping btw*";
            string longDesc = "A futuristic gauntlet that seems to improve your aim at far ranges, allowing " +
                "your long range attacks to deal increased damage. There seems to be a signature: Ja-c- -nd Vi--or\n";
            
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
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");
            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;
        }

        // scales damage based on distance to target
        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (beam && beam.Owner is PlayerController player)
            {
                float distanceToTarget = Vector2.Distance(player.transform.position, hitRigidbody.aiActor.transform.position);
                if (distanceToTarget <= HyperShotMinDistance)
                {
                    return;
                }
                // checks if distance to target is higher than max distance to limit damage multiplier
                float damageMultiplier = (Mathf.Min(distanceToTarget, HyperShotMaxDistance) / HyperShotMaxDistance) * HyperShotMaxDamageInc;
                // calculates additional extra damage to apply to enemy
                float damageToDeal = beam.projectile.baseData.damage * damageMultiplier * tickrate;
                hitRigidbody.aiActor.healthHaver.ApplyDamage(
                    damageToDeal,
                    Vector2.zero,
                    "horizon_focus_hypershot_amp",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    false,
                    null,
                    false
                );
            }
        }

        // scales damage based on distance to target
        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj && proj.Owner is PlayerController player)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    float distanceToTarget = Vector2.Distance(player.transform.position, projHit.transform.position);
                    if (distanceToTarget <= HyperShotMinDistance)
                    {
                        return;
                    }
                    // checks if distance to target is higher than max distance to limit damage multiplier
                    float damageMultiplier = (Mathf.Min(distanceToTarget, HyperShotMaxDistance) / HyperShotMaxDistance) * HyperShotMaxDamageInc;
                    // calculates additional extra damage to apply to enemy
                    float damageToDeal = projHit.baseData.damage * damageMultiplier;
                    enemy.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "horizon_focus_hypershot_amp",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false,
                        null,
                        false
                    );
                };
            }
        }
    }
}
