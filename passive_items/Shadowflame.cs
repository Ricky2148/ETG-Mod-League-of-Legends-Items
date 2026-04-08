using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class Shadowflame : PassiveItem
    {
        public static string ItemName = "Shadowflame";

        private static float DamageStat = 1.15f;
        private static float CinderbloomThreshold = 0.5f;
        private static float CinderbloomDamageAmp = 0.5f;

        public bool HELLSSHADOWSActivated = false;
        private static float HELLSSHADOWSCinderbloomDamageAmpInc = 0.25f;
        public bool SOLARFLAMEActivated = false;
        private static float SOLARFLAMECinderbloomThresholdInc = 0.25f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/shadowflame_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Shadowflame>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*hiring janitors*";
            // maybe add effect explanation?
            string longDesc = "Deal increased damage to low health enemies.\n\n" +
                "A magical necklace that empowers you to finish off targets quicker. " +
                "You begin to feel like you like a character called \"Shadow\"? This cannot be good\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            
            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
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

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.PostProcessBeamTick -= OnPostProcessProjectile;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.HELLS_SHADOWS) && !HELLSSHADOWSActivated)
                {
                    CinderbloomDamageAmp += HELLSSHADOWSCinderbloomDamageAmpInc;

                    HELLSSHADOWSActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.HELLS_SHADOWS) && HELLSSHADOWSActivated)
                {
                    CinderbloomDamageAmp = 0.4f;

                    HELLSSHADOWSActivated = false;
                }

                if (Owner.HasSynergy(Synergy.SOLAR_FLAME) && !SOLARFLAMEActivated)
                {
                    CinderbloomThreshold += SOLARFLAMECinderbloomThresholdInc;

                    SOLARFLAMEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.SOLAR_FLAME) && SOLARFLAMEActivated)
                {
                    CinderbloomThreshold = 0.4f;

                    SOLARFLAMEActivated = false;
                }
            }

            base.Update();
        }

        private void OnPostProcessProjectile (BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody == null) return;
            AIActor firstEnemy = null;
            if (hitRigidbody.aiActor != null)
            {
                firstEnemy = hitRigidbody.aiActor;
            }
            else if (hitRigidbody.GetComponentInParent<AIActor>() != null)
            {
                firstEnemy = hitRigidbody.GetComponentInParent<AIActor>();
            }
            else
            {
                return;
            }
            if (hitRigidbody.healthHaver != null)
            {
                // checks if enemy's current health percentage meets threshold for extra damage
                float currentHealthPercent = firstEnemy.healthHaver.GetCurrentHealthPercentage();
                if (currentHealthPercent <= CinderbloomThreshold)
                {
                    // calculates additional extra damage to apply to enemy
                    float damageToDeal = beam.projectile.baseData.damage * CinderbloomDamageAmp * tickrate;
                    firstEnemy.healthHaver.ApplyDamage(
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
                if (enemy == null) return;
                if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                if (enemy.healthHaver != null)
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
