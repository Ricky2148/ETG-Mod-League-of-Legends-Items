using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using Dungeonator;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static HealthHaver;

//health, active targets a circle zone that applies a heal to players and dmg to enemies, the effect casts down after a delay
// reticle center check doesn't work on controller

namespace LOLItems.active_items
{
    internal class Redemption : TargetedAttackPlayerItem
    {
        public static string ItemName = "Redemption";

        private static float HealthStat = 1f;

        private static float InterventionHealAmount = 0.5f;
        private static float InterventionDamageScale = 0.1f;
        private static float InterventionActivationRange = 10f;
        private static float InterventionEffectRadius = 5f;
        private static float InterventionCooldown = 1f; //90f
        private static float InterventionPerRoomCooldown = 10f;

        private static List<string> VFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/redemption/redemption_attack_001",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_002",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_003",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_004",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_005",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_006",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_007",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_008",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_009",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_010",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_011",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_012",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_013",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_014",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_015",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_016",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_017",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_018",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_019",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_020",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_021",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_022",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_023",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_024",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_025",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_026",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_027",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_028",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_029",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_030",
                "LOLItems/Resources/vfxs/redemption/redemption_attack_031",
            };

        private static GameObject EffectVFX;

        private static List<string> DamageEffectSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/redemption/redemption_damage_001",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_002",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_003",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_004",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_005",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_006",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_007",
                "LOLItems/Resources/vfxs/redemption/redemption_damage_008",
            };

        private static GameObject DamageEffectVFX;

        private static List<string> HealEffectSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/redemption/redemption_heal_001",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_002",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_003",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_004",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_005",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_006",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_007",
                "LOLItems/Resources/vfxs/redemption/redemption_heal_008",
            };

        private static GameObject HealEffectVFX;

        private static List<string> ReticleVFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/redemption/redemption_overheadicon_001",
            };

        private static GameObject ReticleVFX;

        private GameObject activeVFXObject;

        public bool ENLIGHTENEDBULLETSActivated = false;
        private static float ENLIGHTENEDBULLETSDamageStat = 1.15f;
        public bool TERRORTOTHEGUNDEADActivated = false;
        private static int TERRORTOTHEGUNDEADInterventionPerRoomCooldown = 5;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/redemption_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Redemption>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Deus Ex Machina";
            string longDesc = "+1 Heart\nActivates a circle where you target. The circle winds up for a short time, then harms enemies and heals players.\n\n" +
                "A magical pendant with the power to call upon a beam of light at will. It's purifying light harms those it deems evil and " +
                "heals those it deems good. The light's morals seem questionable if it deems the gungeoneers as good.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            EffectVFX = VFXBuilder.CreateVFX
            (
                "intervention_vfx",
                VFXSpritePath,
                8,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once,
                false
            );

            DamageEffectVFX = VFXBuilder.CreateVFX
            (
                "intervention_damage_hiteffect",
                DamageEffectSpritePath,
                8,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once,
                false
            );

            HealEffectVFX = VFXBuilder.CreateVFX
            (
                "intervention_heal_hiteffect",
                HealEffectSpritePath,
                8,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once,
                false
            );

            ReticleVFX = VFXBuilder.CreateVFX
            (
                "intervention_reticle_vfx",
                ReticleVFXSpritePath,
                1,
                new IntVector2(16, 13),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Loop,
                true
            );

            var sprite = EffectVFX.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = 0f; //-50f

                sprite.scale = new Vector3(1.1f, 1.1f, 1f);

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }

            sprite = DamageEffectVFX.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }

            sprite = HealEffectVFX.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }

            //ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1f);
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.PerRoom, InterventionPerRoomCooldown);
            item.consumable = false;

            item.minDistance = 0f;
            item.maxDistance = InterventionActivationRange;

            item.reticleQuad = PickupObjectDatabase.GetById(443).gameObject.GetComponent<TargetedAttackPlayerItem>().reticleQuad.InstantiateAndFakeprefab();

            tk2dBaseSprite vfxSprite = EffectVFX.GetComponent<tk2dBaseSprite>();
            int spriteId = vfxSprite.spriteId;
            tk2dSpriteCollectionData spriteCollection = vfxSprite.collection;

            tk2dSlicedSprite quad = item.reticleQuad.GetComponent<tk2dSlicedSprite>();
            //Plugin.Log($"spriteId: {spriteId}, spriteCollection: {spriteCollection}, vfxSprite: {vfxSprite}");

            quad.SetSprite(spriteCollection, spriteId);

            quad.scale = new Vector3(1.1f, 1.1f, 1f);
            //quad.transform.position += new Vector3(-5f, -5f);
            quad.dimensions = new Vector2(150, 143);

            //quad.CreateBoxCollider = false;
            //quad.UpdateCollider();

            //quad.ReshapeBounds(new Vector3(0f, 0f, 1f), new Vector3(5f, 5f, 1f));

            UnityEngine.Object.Destroy(item.reticleQuad.GetComponent<ReticleRiserEffect>());

            //item.reticleQuad.GetComponent<tk2dSprite>().SetSprite("redemption_overheadicon_001");
            item.doesStrike = false;
            item.doesGoop = false;
            item.DoScreenFlash = false;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void Update()
        {
            if (LastOwner != null)
            {
                if (LastOwner.HasSynergy(Synergy.ENLIGHTENED_BULLETS) && !ENLIGHTENEDBULLETSActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, ENLIGHTENEDBULLETSDamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    LastOwner.stats.RecalculateStatsWithoutRebuildingGunVolleys(LastOwner);

                    ENLIGHTENEDBULLETSActivated = true;
                }
                else if (!LastOwner.HasSynergy(Synergy.ENLIGHTENED_BULLETS) && ENLIGHTENEDBULLETSActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                    LastOwner.stats.RecalculateStatsWithoutRebuildingGunVolleys(LastOwner);

                    ENLIGHTENEDBULLETSActivated = false;
                }

                if (LastOwner.HasSynergy(Synergy.TERROR_TO_THE_GUNDEAD) && !TERRORTOTHEGUNDEADActivated)
                {
                    roomCooldown -= TERRORTOTHEGUNDEADInterventionPerRoomCooldown;
                    CurrentRoomCooldown -= TERRORTOTHEGUNDEADInterventionPerRoomCooldown;

                    TERRORTOTHEGUNDEADActivated = true;
                }
                else if (!LastOwner.HasSynergy(Synergy.TERROR_TO_THE_GUNDEAD) && TERRORTOTHEGUNDEADActivated)
                {
                    roomCooldown += TERRORTOTHEGUNDEADInterventionPerRoomCooldown;
                    CurrentRoomCooldown += TERRORTOTHEGUNDEADInterventionPerRoomCooldown;

                    TERRORTOTHEGUNDEADActivated = false;
                }
            }

            base.Update();
        }

        /*public override void DoEffect(PlayerController player)
        {
            IsCurrentlyActive = true;
            m_currentUser = player;
            GameObject reticleObject = (PickupObjectDatabase.GetById(462) as TargetedAttackPlayerItem).reticleQuad;
            m_extantReticleQuad = reticleObject.GetComponent<tk2dBaseSprite>();
            m_currentAngle = BraveMathCollege.Atan2Degrees(m_currentUser.unadjustedAimPoint.XY() - m_currentUser.CenterPosition);
            m_currentDistance = 5f;
            UpdateReticlePosition();
            spriteAnimator.Play("Activate");
        }*/

        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);

            AkSoundEngine.PostEvent("redemption_initial_activation_SFX", user.gameObject);
        }

        public override void DoActiveEffect(PlayerController player)
        {
            /*if (user && user.CurrentRoom != null)
            {
                Vector2 cachedPosition = m_extantReticleQuad.gameObject.transform.position + new Vector3(0, 0.25f);
                if (m_extantReticleQuad) { Destroy(m_extantReticleQuad.gameObject); }
                IsCurrentlyActive = true;
                AkSoundEngine.PostEvent("Play_OBJ_computer_boop_01", user.gameObject);

                Exploder.Explode(cachedPosition, new ExplosionData
                {
                    damage = 40f,
                    damageRadius = 5f,
                    doDamage = true,
                    doForce = true,
                    force = 25f,
                    debrisForce = 10f,
                    preventPlayerForce = true,
                    doScreenShake = true,
                    playDefaultSFX = true
                }, Vector2.zero);

                IsCurrentlyActive = false;
                user.DropActiveItem(this);
            }*/
            base.DoActiveEffect(player);

            tk2dBaseSprite cursor = this.m_extantReticleQuad;
            Vector2 overridePos = cursor.WorldCenter; //this sets the vector2 to the bottom left of the reticle sprite, not to the actual cursor

            StartCoroutine(DoInterventionEvent(player, overridePos));

            /*Exploder.Explode(overridePos, new ExplosionData
            {
                damageRadius = 3f,
                damageToPlayer = 0f,
                doDamage = true,
                damage = 20f,
                doDestroyProjectiles = true,
                doForce = true,
                debrisForce = 0f,
                preventPlayerForce = true,
                explosionDelay = 0f,
                usesComprehensiveDelay = false,
                doScreenShake = false,
                playDefaultSFX = true,
                breakSecretWalls = true,
                secretWallsRadius = 3,
                force = 2,
                forceUseThisRadius = true
            }, Vector2.zero, null, false, CoreDamageTypes.None, false);*/

            /*foreach (AIActor enemy in player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
            {
                if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                {
                    float distance = Vector2.Distance(cursor.WorldCenter, enemy.CenterPosition);
                    // scale damage with player damage modifiers
                    //float ShockwaveDamage = ShockwaveBaseDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

                    Plugin.Log($"cursor: {cursor.WorldCenter.ToString()}, enemy: {enemy.CenterPosition.ToString()}, distance: {distance}");

                    if (distance <= 6f)
                    {
                        float damageToDeal = enemy.healthHaver.GetMaxHealth() * InterventionDamageScale;

                        enemy.healthHaver.ApplyDamage(
                            damageToDeal,
                            Vector2.zero,
                            "Stridebreaker",
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                        );
                        //enemy.ApplyEffect(slowEffect, 1f, null);
                        //AkSoundEngine.PostEvent("stridebreaker_active_hit_SFX", player.gameObject);
                    }
                }
            }*/
        }

        private System.Collections.IEnumerator DoInterventionEvent(PlayerController player, Vector2 cursorPos)
        {
            //Vector2 initialCursorPos = cursorPos;

            Vector2 initialCursorPos = Vector2.zero;
            Vector3 positionOffset = Vector3.zero;

            if (BraveInput.GetInstanceForPlayer(player.PlayerIDX).IsKeyboardAndMouse())
            {
                initialCursorPos = player.unadjustedAimPoint.XY() - (player.CenterPosition - player.specRigidbody.UnitCenter);
                positionOffset = new Vector3(0.0f / 16f, 8.0f / 16f);
            }
            else
            {
                BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(m_currentUser.PlayerIDX);
                Vector2 vector3 = m_currentUser.CenterPosition + (Quaternion.Euler(0f, 0f, m_currentAngle) * Vector2.right).XY() * m_currentDistance;
                vector3 += instanceForPlayer.ActiveActions.Aim.Vector;
                m_currentAngle = BraveMathCollege.Atan2Degrees(vector3 - m_currentUser.CenterPosition);
                m_currentDistance = Vector2.Distance(vector3, m_currentUser.CenterPosition);
                m_currentDistance = Mathf.Min(m_currentDistance, maxDistance);
                vector3 = m_currentUser.CenterPosition + (Quaternion.Euler(0f, 0f, m_currentAngle) * Vector2.right).XY() * m_currentDistance;
                initialCursorPos = vector3;
            }
            if (initialCursorPos != Vector2.zero)
            {
                initialCursorPos = BraveMathCollege.ClampToBounds(initialCursorPos, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
            }

            activeVFXObject = UnityEngine.Object.Instantiate(EffectVFX, initialCursorPos, Quaternion.identity);

            activeVFXObject.GetComponent<tk2dSprite>().transform.localPosition += positionOffset;

            /*var sprite = activeVFXObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = 0f; //-50f

                sprite.scale = new Vector3(1.1f, 1.1f, 1f);

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }*/

            AkSoundEngine.PostEvent("redemption_effect_buildup_SFX", activeVFXObject.gameObject);

            yield return new WaitForSeconds(2.5f);

            AkSoundEngine.PostEvent("redemption_effect_landing_SFX", activeVFXObject.gameObject);

            List<AIActor> enemyList = player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (enemyList != null)
            {
                foreach (AIActor enemy in enemyList)
                {
                    if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                    {
                        float distance = Vector2.Distance(initialCursorPos, enemy.CenterPosition);
                        // scale damage with player damage modifiers
                        //float ShockwaveDamage = ShockwaveBaseDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

                        //Plugin.Log($"cursor: {initialCursorPos.ToString()}, enemy: {enemy.CenterPosition.ToString()}, distance: {distance}");

                        if (distance <= InterventionEffectRadius)
                        {
                            float damageToDeal = enemy.healthHaver.GetMaxHealth() * InterventionDamageScale;

                            enemy.healthHaver.ApplyDamage(
                                damageToDeal,
                                Vector2.zero,
                                "intervention",
                                CoreDamageTypes.None,
                                DamageCategory.Normal,
                                false
                            );

                            enemy.PlayEffectOnActor(DamageEffectVFX, new Vector3(23 / 16f, 18 / 16f, -2f), true, false, false);

                            AkSoundEngine.PostEvent("redemption_effect_damage_SFX", enemy.gameObject);

                            //enemy.ApplyEffect(slowEffect, 1f, null);
                            //AkSoundEngine.PostEvent("stridebreaker_active_hit_SFX", player.gameObject);
                        }
                    }
                }
            }

            if (GameManager.Instance.PrimaryPlayer != null)
            {
                PlayerController player1 = GameManager.Instance.PrimaryPlayer;

                //Plugin.Log($"cursor: {initialCursorPos.ToString()}, player1: {player1.CenterPosition.ToString()}, distance: {Vector2.Distance(initialCursorPos, player1.CenterPosition)}");

                if (Vector2.Distance(initialCursorPos, player1.CenterPosition) <= InterventionEffectRadius && player1.healthHaver.currentHealth < player1.healthHaver.GetMaxHealth())
                {
                    player1.healthHaver.ForceSetCurrentHealth(player1.healthHaver.currentHealth + InterventionHealAmount);

                    player1.PlayEffectOnActor(HealEffectVFX, new Vector3(23 / 16f, 18 / 16f, -2f), true, false, false);

                    AkSoundEngine.PostEvent("redemption_effect_heal_SFX", player1.gameObject);
                }
            }

            if (GameManager.Instance.SecondaryPlayer != null)
            {
                PlayerController player2 = GameManager.Instance.SecondaryPlayer;

                //Plugin.Log($"cursor: {initialCursorPos.ToString()}, player1: {player2.CenterPosition.ToString()}, distance: {Vector2.Distance(initialCursorPos, player2.CenterPosition)}");

                if (Vector2.Distance(initialCursorPos, player2.CenterPosition) <= InterventionEffectRadius && player2.healthHaver.currentHealth < player2.healthHaver.GetMaxHealth())
                {
                    player2.healthHaver.ForceSetCurrentHealth(player2.healthHaver.currentHealth + InterventionHealAmount);

                    player2.PlayEffectOnActor(HealEffectVFX, new Vector3(23 / 16f, 18 / 16f, -2f), true, false, false);

                    AkSoundEngine.PostEvent("redemption_effect_heal_SFX", player2.gameObject);
                }
            }

            yield return new WaitForSeconds(5f);
            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }
        }
    }
}
