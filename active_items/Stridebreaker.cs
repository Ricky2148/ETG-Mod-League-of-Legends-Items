using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//health, dmg, and fire rate, active attacks in a circle around player, slows enemies hit and deals set dmg

// active should be a vfx that does nothing and a range check that applies a slow and damage to enemies in a circle around the player
// maybe add the speed buff per enemy hit

namespace LOLItems.active_items
{
    internal class Stridebreaker : PlayerItem
    {
        public static string ItemName = "Stridebreaker";

        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float RateOfFireStat = 1.1f;
        private static float HealthStat = 1f;

        private static float slowPercent = 0.3f;
        private static float slowDuration = 3f;
        private static float ShockwaveBaseDamage = 10f;
        private static float ShockwaveRadius = 6f;
        private static float ShockwaveCooldown = 15f; //15f

        private static GameObject slashVFX = ((Gun)PickupObjectDatabase.GetById(417))
                .DefaultModule.projectiles[0]
                .hitEffects.tileMapHorizontal.effects[0]
                .effects[0].effect;

        private static GameActorSpeedEffect slowEffect = new GameActorSpeedEffect
        {
            duration = slowDuration,
            effectIdentifier = "stridebreaker_active_slow",
            resistanceType = EffectResistanceType.Freeze,
            AppliesOutlineTint = true,
            OutlineTintColor = Color.gray,
            SpeedMultiplier = slowPercent,
        };

        private static List<string> VFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_001",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_002",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_003",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_004",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_005",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_006",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_007",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_008",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_009",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_010",
                "LOLItems/Resources/vfxs/breakingshockwave/breakingshockwave_011"
            };

        private static GameObject EffectVFX;

        private GameObject activeVFXObject;

        public bool DEMACIANTRAITORActivated = false;
        private static float DEMACIANTRAITORShockwaveDamageMultiplier = 2f;
        private static float DEMACIANTRAITORslowPercent = 0.05f;
        private static float DEMACIANTRAITORslowDuration = 8f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/stridebreaker_pixelart_sprite_small";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<Stridebreaker>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "\"No more cages!\"";
            string longDesc = "+1 Heart, Increase damage and fire rate\nSlashes around you, deals damage, enemies hit are slowed.\n\n" +
                "A set of chains that appears to have been used as a weapon. The chains feel cold to the touch " +
                "and seem to instill a feeling of rebellion within you.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, ShockwaveCooldown);
            item.consumable = false;

            EffectVFX = VFXBuilder.CreateVFX
            (
                "breakingshockwave",
                VFXSpritePath,
                30,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once,
                true
            );

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Health);
            //player.stats.RecalculateStats(player, false, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
            
            return base.Drop(player);
        }

        public override void Update()
        {
            if (LastOwner.HasSynergy(Synergy.DEMACIAN_TRAITOR) && !DEMACIANTRAITORActivated)
            {
                slowEffect.duration = DEMACIANTRAITORslowDuration;
                //slowEffect.SpeedMultiplier = DEMACIANTRAITORslowPercent;

                DEMACIANTRAITORActivated = true;
            }
            else if (!LastOwner.HasSynergy(Synergy.DEMACIAN_TRAITOR) && DEMACIANTRAITORActivated)
            {
                slowEffect.duration = slowDuration;
                //slowEffect.SpeedMultiplier = slowPercent;

                DEMACIANTRAITORActivated = false;
            }

            base.Update();
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

            /*if (slashVFX != null)
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
            }*/

            // seperation line LMAO

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            //activeVFXObject = player.PlayEffectOnActor(EffectVFX, new Vector3(32 / 16f, 21 / 16f, -2f), true, false, false);
            activeVFXObject = UnityEngine.Object.Instantiate(EffectVFX, player.CenterPosition, Quaternion.identity);

            var sprite = activeVFXObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = 0f; //-50f

                sprite.scale = new Vector3(3.1f, 3.1f, 1f);
                
                sprite.UpdateZDepth();

                //sprite.usesOverrideMaterial = true;

                //sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                //sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }

            float ShockwaveDamage = ShockwaveBaseDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

            if (player.HasSynergy(Synergy.DEMACIAN_TRAITOR))
            {
                ShockwaveDamage *= DEMACIANTRAITORShockwaveDamageMultiplier;
            }

            // checks for all enemies in the room that are in range, applies damage, slow effect, and plays sound
            List<AIActor> enemyList = player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (enemyList != null)
            {
                foreach (AIActor enemy in enemyList)
                {
                    if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                    {
                        float distance = Vector2.Distance(player.CenterPosition, enemy.CenterPosition);
                        // scale damage with player damage modifiers
                        //float ShockwaveDamage = ShockwaveBaseDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

                        //Plugin.Log($"player: {player.CenterPosition}, enemy: {enemy.CenterPosition}, distance: {distance}");

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
}
