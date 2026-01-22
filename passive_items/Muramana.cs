using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using LOLItems.custom_class_data;
using LOLItems.passive_items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    public class Muramana : PassiveItem
    {
        public static string ItemName = "Muramana";

        // stats pool for item
        private static float DamageStat = 1.2f;
        private static float ClipAndAmmoIncrease = 1.5f;
        private static float MuramanaShockBaseDamage = 5f;
        private static float MuramanaShockScale = 0.5f;

        public bool BLADEOFTHEONIActivated = false;
        private static float BLADEOFTHEONIDamageStat = 1.5f;
        public bool ITHASTOBETHISWAYActivated = false;
        private static float ITHASTOBETHISWAYClipAndAmmoIncrease = 2.0f;
        public bool JETSTREAMSAMActivated = false;
        private static float JETSTREAMSAMMovementSpeedInc = 2.0f;
        private static float JETSTREAMSAMMuramanaShockBaseDamageInc = 5f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/muramana_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Muramana>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "The Peak of Swordsmithing";
            string longDesc = "Increase damage, max ammo, and clip size\nEvery bullet deals additional damage. This damage increases based on your max ammo multiplier and clip size multiplier.\n\n" +
                "A blade forged by Masamune and wielded by the worthy, the Manamune's true " +
                "power has been unlocked and empowers your every attack.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalClipCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AmmoCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.EXCLUDED;
            item.ShouldBeExcludedFromShops = true;

            ID = item.PickupObjectId;

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:muramana",
                "LOLItems:manaflow_fully_stacked"
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                ""
            };
            CustomSynergies.Add("Manamune Upgraded To Muramana", mandatoryConsoleIDs, null, true);
            */
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            //HelpfulMethods.CustomNotification("Manamune Upgraded to Muramana", "", this.sprite, UINotificationController.NotificationColor.GOLD);

            player.PostProcessProjectile += MuramanaShock;
            player.PostProcessBeamTick += MuramanaShock;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= MuramanaShock;
                player.PostProcessBeamTick -= MuramanaShock;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.BLADE_OF_THE_ONI_MURAMANA) && !BLADEOFTHEONIActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, BLADEOFTHEONIDamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    BLADEOFTHEONIActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.BLADE_OF_THE_ONI_MURAMANA) && BLADEOFTHEONIActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    BLADEOFTHEONIActivated = false;
                }

                if (Owner.HasSynergy(Synergy.IT_HAS_TO_BE_THIS_WAY) && !ITHASTOBETHISWAYActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);

                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, ITHASTOBETHISWAYClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, ITHASTOBETHISWAYClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    ITHASTOBETHISWAYActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.IT_HAS_TO_BE_THIS_WAY) && ITHASTOBETHISWAYActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);

                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, ClipAndAmmoIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    ITHASTOBETHISWAYActivated = false;
                }

                if (Owner.HasSynergy(Synergy.JETSTREAM_SAM) && !JETSTREAMSAMActivated)
                {
                    //ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, JETSTREAMSAMMovementSpeedInc, StatModifier.ModifyMethod.ADDITIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    MuramanaShockBaseDamage += JETSTREAMSAMMuramanaShockBaseDamageInc;

                    JETSTREAMSAMActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.JETSTREAM_SAM) && JETSTREAMSAMActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    MuramanaShockBaseDamage = 5f;

                    JETSTREAMSAMActivated = false;
                }
            }

            base.Update();
        }

        private void MuramanaShock(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (beam.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
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
                //scales the damage based on player's clip size and ammo size 
                float clipSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier) - 1f) * MuramanaShockScale);
                float ammoSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier) - 1f) * MuramanaShockScale);
                float MuramanaShockDamageMultiplier = Mathf.Max(1f, 1f + clipSizeStat + ammoSizeStat);
                // scale damage down by tickrate
                float damageToDeal = Mathf.Max(1f, MuramanaShockBaseDamage * MuramanaShockDamageMultiplier) * tickrate;
                // calculates additional extra damage to apply to enemy
                firstEnemy.healthHaver.ApplyDamage(
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
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                if (proj.Owner is not PlayerController player) return;
                if (player.CurrentGun is not Gun gun) return;
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                    if (enemy.healthHaver != null)
                    {
                        //scales the damage based on player's clip size and ammo size stats
                        float clipSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier) - 1f) * MuramanaShockScale);
                        float ammoSizeStat = Mathf.Max(0f, (player.stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier) - 1f) * MuramanaShockScale);
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
}
