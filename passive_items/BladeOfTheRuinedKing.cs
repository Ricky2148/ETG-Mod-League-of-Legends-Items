using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static tk2dSpriteCollectionDefinition;

//fix interaction with beam weapons = they dont work at all lmao

namespace LOLItems
{
    internal class BladeOfTheRuinedKing : PassiveItem
    {
        // stats pool for item
        private bool shouldApplySlow = false;
        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.2f;
        private static float PercentCurrentHealthStat = 0.12f;
        private static float slowPercent = 0.5f;
        private static float slowDuration = 3f;

        public static void Init()
        {
            string itemName = "Blade of the Ruined King";
            string resourceName = "LOLItems/Resources/passive_item_sprites/blade_of_the_ruined_king_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<BladeOfTheRuinedKing>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"The mist devours all!\"";
            string longDesc = "No price is too great.\n" +
                "No atrocity beyond my reach.\n" +
                "For her, I will do anything.\n" +
                "\n- King of Camavor\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.S;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            // subscribe to reload and projectile events
            player.OnReloadedGun += OnGunReloaded;
            shouldApplySlow = true;
            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
        }

        public override void DisableEffect(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            // unsubscribe from events
            player.OnReloadedGun -= OnGunReloaded;
            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;
            shouldApplySlow = false;
        }

        // when gun is reloaded, set the flag to apply slow effect
        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            shouldApplySlow = true;
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody != null && hitRigidbody.aiActor != null)
            {
                float currentHealth = hitRigidbody.aiActor.healthHaver.GetCurrentHealth();
                // calculates additional extra damage to apply to enemy
                float damageToDeal = Mathf.Max(1f, currentHealth * PercentCurrentHealthStat) * tickrate;
                // damage is 1/4 against bosses and sub-bosses
                if (hitRigidbody.aiActor.healthHaver.IsBoss || hitRigidbody.aiActor.healthHaver.IsSubboss)
                {
                    damageToDeal *= 0.25f;
                }
                hitRigidbody.aiActor.healthHaver.ApplyDamage(
                    damageToDeal,
                    Vector2.zero,
                    "botrk_current_health_damage",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    false
                );
            }
        }

        private void OnPostProcessProjectile (Projectile proj, float f)
        {
            // first bullet of clip slows via checking shouldApplySlow flag
            if (shouldApplySlow)
            {
                ApplySlowEffect(proj);
                shouldApplySlow = false;
            }
            //Apply 12% of current health damage to enemies hit by the projectile
            //Seems to be an interaction with bosses where they only take the bonus damage every few frames
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.aiActor != null)
                {
                    float currentHealth = enemy.healthHaver.GetCurrentHealth();
                    // calculates additional extra damage to apply to enemy
                    float damageToDeal = Mathf.Max(1f, currentHealth * PercentCurrentHealthStat);
                    // damage is 1/4 against bosses and sub-bosses
                    if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                    {
                        damageToDeal *= 0.25f;
                    }
                    enemy.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "botrk_current_health_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false
                    );
                }
            };
        }

        // This method applies a slow effect to the projectile and its target
        private void ApplySlowEffect(Projectile projectile)
        {
            GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
            {
                duration = slowDuration,
                effectIdentifier = "botrk_slow",
                resistanceType = EffectResistanceType.Freeze,
                AppliesOutlineTint = true,
                OutlineTintColor = Color.cyan,
                SpeedMultiplier = slowPercent,
            };

            projectile.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.aiActor != null)
                {
                    enemy.aiActor.ApplyEffect(slowEffect);
                }
            };
        }
    }
}
