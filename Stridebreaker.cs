using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using Alexandria;
using UnityEngine;
using Dungeonator;

//health, dmg, and fire rate, active attacks in a circle around player, slows enemies hit and deals set dmg

// active should be a vfx that does nothing and a range check that applies a slow and damage to enemies in a circle around the player
// maybe add the speed buff per enemy hit
// sound effect gets repeated and sounds really bad

namespace LOLItems
{
    internal class Stridebreaker : PlayerItem
    {
        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float RateOfFireStat = 1.1f;
        private static float HealthStat = 1f;
        private static float HealthToGive = 1f;

        private static float slowPercent = 0.3f;
        private static float slowDuration = 3f;
        private static float ShockwaveDamage = 10f;
        private static float ShockwaveRadius = 6f;
        private static float ShockwaveCooldown = 15f;

        public static void Init()
        {
            string itemName = "Stridebreaker";
            string resourceName = "LOLItems/Resources/passive_item_sprites/stridebreaker_item_sprite";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<Stridebreaker>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "\"No more cages!\"";
            string longDesc = "A set of chains that appears to have been used as a weapon. The chains feel cold to the touch " +
                "and seem to instill a feeling of rebellion within you.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, ShockwaveCooldown);
            item.consumable = false;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.A;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Stridebreaker");

            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() + HealthStat, HealthToGive);
            HealthToGive = 0f;
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of Stridebreaker");

            player.healthHaver.SetHealthMaximum(player.healthHaver.GetMaxHealth() - HealthStat);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            player.stats.RecalculateStats(player, false, false);
            return base.Drop(player);
        }

        /*
        public void BuildCustomSlashProjectile(PlayerController player)
        {
            Projectile baseProj = (PickupObjectDatabase.GetById(15) as Gun).DefaultModule.projectiles[0];

            Projectile slashProj = UnityEngine.Object.Instantiate(baseProj);

            slashProj.name = "Stridebreaker_SlashProjectile";
            slashProj.baseData.damage = ShockwaveDamage;
            slashProj.baseData.speed = 0f;
            slashProj.baseData.range = 0.1f;
            slashProj.pierceMinorBreakables = true;
            slashProj.shouldRotate = false;
            slashProj.collidesWithEnemies = true;
            slashProj.collidesWithPlayer = false;
            slashProj.specRigidbody.CollideWithOthers = true;
            slashProj.specRigidbody.CollideWithTileMap = false;
            slashProj.sprite.renderer.enabled = false;

            PierceProjModifier pierce = slashProj.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = 999;
            pierce.MaxBossImpacts = 1;
            pierce.penetratesBreakables = true;
            pierce.preventPenetrationOfActors = false;
            pierce.BeastModeLevel = PierceProjModifier.BeastModeStatus.NOT_BEAST_MODE;

            FakePrefab.MarkAsFakePrefab(slashProj.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(slashProj.gameObject);

            CustomSlashProjectile = slashProj;
        }
        */

        public override void DoEffect(PlayerController player)
        {
            AkSoundEngine.PostEvent("stridebreaker_active_SFX", player.gameObject);

            // sets vfx to slash effect of blasphemy (bullet character weapon)
            GameObject slashVFX = ((Gun)PickupObjectDatabase.GetById(417))
                .DefaultModule.projectiles[0]
                .hitEffects.tileMapHorizontal.effects[0]
                .effects[0].effect;

            if (slashVFX != null)
            {
                GameObject vfxInstance = UnityEngine.Object.Instantiate(slashVFX, player.CenterPosition, Quaternion.identity);
                vfxInstance.SetActive(true);

                var sprite = vfxInstance.GetComponent<tk2dSprite>();
                // tries to change sprite colors and opacity (doesn't work)
                if (sprite != null)
                {
                    sprite.HeightOffGround = 3f;
                    sprite.scale = new Vector3(8f, 8f, 1f);

                    sprite.color = new Color(0.6f, 0.6f, 0.6f);

                    sprite.UpdateZDepth();
                }

                var anim = vfxInstance.GetComponent<tk2dSpriteAnimator>();
                if (anim != null) anim.Play();
            }

            GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
            {
                duration = slowDuration,
                effectIdentifier = "stridebreaker_active_slow",
                resistanceType = EffectResistanceType.Freeze,
                AppliesOutlineTint = true,
                OutlineTintColor = Color.gray,
                SpeedMultiplier = slowPercent,
            };

            // checks for all enemies in the room that are in range, applies damage, slow effect, and plays sound
            foreach (AIActor enemy in player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
            {
                if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                {
                    float distance = Vector2.Distance(player.CenterPosition, enemy.CenterPosition);
                    if (distance <= ShockwaveRadius)
                    {
                        enemy.healthHaver.ApplyDamage(
                            ShockwaveDamage,
                            Vector2.zero,
                            "Stridebreaker",
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                        );
                        enemy.ApplyEffect(slowEffect, 1f, null);
                        AkSoundEngine.PostEvent("stridebreaker_active_hit_SFX", player.gameObject);
                    }
                }
            }
        }
    }
}
