using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;

namespace LOLItems.passive_items
{
    internal class HorizonFocus : PassiveItem
    {
        public static string ItemName = "Horizon Focus";

        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float HyperShotMaxDistance = 15f;
        private static float HyperShotMinDistance = 3f;
        private static float HyperShotMaxDamageInc = 0.25f;

        public bool AMPLIFIEDLENSActivated = false;
        private static float AMPLIFIEDLENSHyperShotMaxDamageInc = 0.25f;
        public bool FUTURISTICCOMPATIBILITYActivated = false;
        private static float FUTURISTICCOMPATIBILITYHyperShotMaxDamageInc = 0.50f;
        public bool GUARANTEEDHITIFITHITSActivated = false;
        private static float GUARANTEEDHITIFITHITSHyperShotMaxDamageInc = 0.75f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/horizon_focus_pixelart_sprite";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<HorizonFocus>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "*not camping btw*";
            string longDesc = "Increase damage\nBullets deal increased damage the farther away you are from the target.\n\n" +
                "A futuristic gauntlet that seems to improve your aim at far ranges, allowing " +
                "your long range attacks to deal increased damage. There seems to be a signature: Ja-c- -nd Vi--or\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

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
                if (Owner.HasSynergy(Synergy.AMPLIFIED_LENS) && !AMPLIFIEDLENSActivated)
                {
                    HyperShotMaxDamageInc += AMPLIFIEDLENSHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    AMPLIFIEDLENSActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.AMPLIFIED_LENS) && AMPLIFIEDLENSActivated)
                {
                    HyperShotMaxDamageInc -= AMPLIFIEDLENSHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    AMPLIFIEDLENSActivated = false;
                }
                if (Owner.HasSynergy(Synergy.FUTURISTIC_COMPATIBILITY) && !FUTURISTICCOMPATIBILITYActivated)
                {
                    HyperShotMaxDamageInc += FUTURISTICCOMPATIBILITYHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    FUTURISTICCOMPATIBILITYActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.FUTURISTIC_COMPATIBILITY) && FUTURISTICCOMPATIBILITYActivated)
                {
                    HyperShotMaxDamageInc -= FUTURISTICCOMPATIBILITYHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    FUTURISTICCOMPATIBILITYActivated = false;
                }
                if (Owner.HasSynergy(Synergy.GUARANTEED_HIT_IF_IT_HITS) && !GUARANTEEDHITIFITHITSActivated)
                {
                    HyperShotMaxDamageInc += GUARANTEEDHITIFITHITSHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    GUARANTEEDHITIFITHITSActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.GUARANTEED_HIT_IF_IT_HITS) && GUARANTEEDHITIFITHITSActivated)
                {
                    HyperShotMaxDamageInc -= GUARANTEEDHITIFITHITSHyperShotMaxDamageInc;
                    //Plugin.Log($"{HyperShotMaxDamageInc}");

                    GUARANTEEDHITIFITHITSActivated = false;
                }
            }

            base.Update();
        }

        // scales damage based on distance to target
        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
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
                if (beam && beam.Owner is PlayerController player)
                {
                    float distanceToTarget = Vector2.Distance(player.transform.position, firstEnemy.transform.position);
                    if (distanceToTarget <= HyperShotMinDistance)
                    {
                        return;
                    }
                    // checks if distance to target is higher than max distance to limit damage multiplier
                    float damageMultiplier = (Mathf.Min(distanceToTarget, HyperShotMaxDistance) / HyperShotMaxDistance) * HyperShotMaxDamageInc;
                    // calculates additional extra damage to apply to enemy
                    float damageToDeal = beam.projectile.baseData.damage * damageMultiplier * tickrate;
                    firstEnemy.healthHaver.ApplyDamage(
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
        }

        // scales damage based on distance to target
        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj && proj.Owner is PlayerController player)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                    if (enemy.healthHaver != null)
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
                        //Plugin.Log($"{damageToDeal}");
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
                    }
                };
            }
        }
    }
}
