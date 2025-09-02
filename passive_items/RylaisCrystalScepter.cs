using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class RylaisCrystalScepter : PassiveItem
    {
        private static float HealthStat = 1f;
        private static float RimefrostSlowPercent = 0.7f;
        private static float RimefrostSlowDuration = 1f;

        public static void Init()
        {
            string itemName = "Rylai's Crystal Scepter";
            string resourceName = "LOLItems/Resources/passive_item_sprites/rylais_crystal_scepter_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<RylaisCrystalScepter>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "A Cold One";
            string longDesc = "A magic scepter with a bright blue crystal. The crystal is freezing to the touch, " +
                "but you kinda don't care. Things start to feel less important as you hold this scepter.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = ItemQuality.B;
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
            player.PostProcessProjectile -= ApplyRimefrostEffect;
            player.PostProcessBeamTick -= ApplyRimefrostEffect;
        }

        private void ApplyRimefrostEffect(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody != null && hitRigidbody.aiActor != null)
            {
                GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
                {
                    duration = RimefrostSlowDuration,
                    effectIdentifier = "rimefrost_slow",
                    resistanceType = EffectResistanceType.Freeze,
                    AppliesOutlineTint = true,
                    OutlineTintColor = Color.cyan,
                    SpeedMultiplier = RimefrostSlowPercent,
                };
                hitRigidbody.aiActor.ApplyEffect(slowEffect);
            }
        }

        private void ApplyRimefrostEffect(Projectile proj, float f)
        {
            GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
            {
                duration = RimefrostSlowDuration,
                effectIdentifier = "rimefrost_slow",
                resistanceType = EffectResistanceType.Freeze,
                AppliesOutlineTint = true,
                OutlineTintColor = Color.cyan,
                SpeedMultiplier = RimefrostSlowPercent,
            };

            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.aiActor != null)
                {
                    enemy.aiActor.ApplyEffect(slowEffect);
                }
            };
        }
    }
}
