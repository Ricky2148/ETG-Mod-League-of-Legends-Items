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
    internal class RylaisCrystalScepter : PassiveItem
    {
        public static string ItemName = "Rylai's Crystal Scepter";

        private static float HealthStat = 1f;
        private static float RimefrostSlowPercent = 0.7f;
        private static float RimefrostSlowDuration = 3.5f;

        private static GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
        {
            duration = RimefrostSlowDuration,
            effectIdentifier = "rimefrost_slow",
            resistanceType = EffectResistanceType.None,
            AppliesOutlineTint = true,
            OutlineTintColor = Color.cyan,
            SpeedMultiplier = RimefrostSlowPercent,
        };

        public bool ICEIIActivated = false;
        private static float ICEIIRimefrostSlowPercent = 0.4f;
        public bool WITCHCRAFTActivated = false;
        private static float WITCHCRAFTDamageStat = 1.1f;
        private static float WITCHCRAFTRimefrostSlowDurationMultiplier = 2f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/rylais_crystal_scepter_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<RylaisCrystalScepter>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "A Cold One";
            string longDesc = "+1 Heart\nDealing damage slows enemies hit.\n\n" +
                "A magic scepter with a bright blue crystal. The crystal is freezing to the touch, " +
                "but you kinda don't care. Things start to feel less important as you hold this scepter.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = ItemQuality.C;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            player.PostProcessProjectile += ApplyRimefrostEffect;
            player.PostProcessBeamTick += ApplyRimefrostEffect;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= ApplyRimefrostEffect;
                player.PostProcessBeamTick -= ApplyRimefrostEffect;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.ICE_II) && !ICEIIActivated)
                {
                    slowEffect.SpeedMultiplier = ICEIIRimefrostSlowPercent;

                    ICEIIActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ICE_II) && ICEIIActivated)
                {
                    slowEffect.SpeedMultiplier = RimefrostSlowPercent;

                    ICEIIActivated = false;
                }

                if (Owner.HasSynergy(Synergy.WITCHCRAFT) && !WITCHCRAFTActivated)
                {
                    slowEffect.duration = RimefrostSlowDuration * WITCHCRAFTRimefrostSlowDurationMultiplier;

                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, WITCHCRAFTDamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    WITCHCRAFTActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.WITCHCRAFT) && WITCHCRAFTActivated)
                {
                    slowEffect.duration = RimefrostSlowDuration;

                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    WITCHCRAFTActivated = false;
                }
            }

            base.Update();
        }

        private void ApplyRimefrostEffect(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody == null) return;
            if (hitRigidbody.healthHaver == null) return;
            if (hitRigidbody.aiActor != null)
            {
                hitRigidbody.aiActor.ApplyEffect(slowEffect);
            }
            else if (hitRigidbody.GetComponentInParent<AIActor>() != null)
            {
                hitRigidbody.GetComponentInParent<AIActor>().ApplyEffect(slowEffect);
            }
        }

        private void ApplyRimefrostEffect(Projectile proj, float f)
        {
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.healthHaver == null) return;
                    if (enemy.aiActor != null)
                    {
                        enemy.aiActor.ApplyEffect(slowEffect);
                    }
                    else if (enemy.GetComponentInParent<AIActor>() != null)
                    {
                        enemy.GetComponentInParent<AIActor>().ApplyEffect(slowEffect);
                    }
                };
            }
        }
    }
}
