using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.VisualAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// needs vfx and sfx work
// tune the damage scale on enemies and bosses
// vfx should be a white spark ball looking thing, on effect application, the vfx is thrown above the enemies head, it starts small, then grows larger with more damage
// maybe start changing colors, would want it to instantly change to red when it would detonate to execute the target. Every time you refresh the duration, it just increases the duration without refreshing the vfx or vfx's loop

namespace LOLItems.passive_items
{
    public class EnemyTheBombTracker
    {
        public float storedDamage;
        public Coroutine timerCoroutine;
        public GameObject activeVFXObject;
    }

    internal class DetonationOrb : PassiveItem
    {
        public static string ItemName = "Detonation Orb";

        private static float DamageStat = 1.2f;
        private static float TheBombDmgScale = 0.20f;
        private static float TheBombDuration = 3f;

        private Dictionary<AIActor, float> enemyTheBombDmgStored = new Dictionary<AIActor, float>();

        private Dictionary<AIActor, Coroutine> enemyTheBombCoroutine = new Dictionary<AIActor, Coroutine>();

        /*private static List<string> IdleVFXSpritePath = new List<string>
        {
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_001",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_002",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_003",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_004",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_005",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_006",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_007",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_008",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_009",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_010",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_011",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_012",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_013",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_014",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_015",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_016",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_017",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_018",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_019",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_020",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_021",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_022",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_023",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_024",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_025",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_026",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_027",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_028",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_029",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_030",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_031",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_032",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_033",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_034",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_035",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_036",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_037",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_038",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_039",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle_040",
        };*/

        private static List<string> IdleVFXSpritePath = "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_idle".GetResourceFrames(40);

        private static GameObject IdleEffectVFX;

        private static List<string> ExplodeVFXSpritePath = new List<string>
        {
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_001",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_002",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_003",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_004",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_005",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_006",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_007",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_008",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_009",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_010",
            "LOLItems/Resources/vfxs/detOrb_effect/detOrbFX_explode_011",
        };

        private static GameObject ExplodeEffectVFX;

        private Dictionary<AIActor, GameObject> activeVFXObjectList = new Dictionary<AIActor, GameObject>();

        public static Vector3 vfxOffset = new Vector3(0 / 16f, 2 / 16f, 0);

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/detonation_orb_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<DetonationOrb>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "idk";
            string longDesc = "idk";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            IdleEffectVFX = VFXBuilder.CreateVFX
            (
                "the_bomb_idle_vfx",
                IdleVFXSpritePath,
                16,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Loop,
                true
            );

            VFXAnchorModule anchor1 = IdleEffectVFX.GetOrAddComponent<VFXAnchorModule>();

            ExplodeEffectVFX = VFXBuilder.CreateVFX
            (
                "the_bomb_explode_vfx",
                ExplodeVFXSpritePath,
                16,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once,
                true
            );

            VFXAnchorModule anchor2 = ExplodeEffectVFX.GetOrAddComponent<VFXAnchorModule>();

            item.quality = PickupObject.ItemQuality.B;
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

            if (enemyTheBombDmgStored != null)
            {
                enemyTheBombDmgStored.Clear();
            }
            if (enemyTheBombCoroutine != null)
            {
                enemyTheBombCoroutine.Clear();
            }
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody == null) return;
            AIActor target = null;
            if (hitRigidbody.aiActor != null)
            {
                target = hitRigidbody.aiActor;
                //Plugin.Log($"enemy.aiActor: {target}");
            }
            else if (hitRigidbody.GetComponentInParent<AIActor>() != null)
            {
                target = hitRigidbody.GetComponentInParent<AIActor>();
                //Plugin.Log($"enemy.parentActor: {target}");
            }
            else
            {
                //Plugin.Log("target = null");
                return;
            }
            if (hitRigidbody.healthHaver != null && hitRigidbody.healthHaver.IsAlive)
            {
                float dmgToStore = beam.Gun.DefaultModule.projectiles[0].baseData.damage * TheBombDmgScale * tickrate;
                if (hitRigidbody.healthHaver.IsBoss || hitRigidbody.healthHaver.IsSubboss)
                {
                    dmgToStore *= 0.25f;
                }
                if (!enemyTheBombDmgStored.ContainsKey(target))
                {
                    enemyTheBombDmgStored.Add(target, dmgToStore);
                    enemyTheBombCoroutine.Add(target, null);
                }
                else
                {
                    enemyTheBombDmgStored[target] += dmgToStore;
                }

                //Plugin.Log($"enemyTheBombDmgStored: {enemyTheBombDmgStored[hitRigidbody.aiActor]}, enemy hp: {hitRigidbody.aiActor.healthHaver.GetCurrentHealth()}");

                // if the hit enemy's stack count is at max stacks, trigger charm effect and cooldown
                if (enemyTheBombDmgStored[target] >= target.healthHaver.GetCurrentHealth() && target.healthHaver.GetCurrentHealth() != 0)
                {
                    DetonateTheBomb(target);

                    /*enemy.aiActor.healthHaver.ApplyDamage(
                        enemyTheBombDmgStored[enemy.aiActor],
                        Vector2.zero,
                        "the_bomb_detonation_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        ignoreInvulnerabilityFrames: true,
                        hitPixelCollider: null,
                        ignoreDamageCaps: true
                    );

                    enemyTheBombDmgStored.Remove(enemy.aiActor);
                    */
                }
                else
                {
                    if (enemyTheBombCoroutine[target] != null)
                    {
                        StopCoroutine(enemyTheBombCoroutine[target]);
                    }
                    enemyTheBombCoroutine[target] = StartCoroutine(TheBombCooldown(target));
                }
            }
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    //Plugin.Log($"enemy curhealth: {enemy.healthHaver.GetCurrentHealth()}, enemy isAlive: {enemy.healthHaver.IsAlive}");

                    if (enemy == null) return;
                    AIActor target = null;
                    if (enemy.aiActor != null)
                    {
                        target = enemy.aiActor;
                        //Plugin.Log($"enemy.aiActor: {target}");
                    }
                    else if (enemy.GetComponentInParent<AIActor>() != null)
                    {
                        target = enemy.GetComponentInParent<AIActor>();
                        //Plugin.Log($"enemy.parentActor: {target}");
                    }
                    else
                    {
                        //Plugin.Log("target = null");
                        return;
                    }
                    if (enemy.healthHaver != null && enemy.healthHaver.IsAlive)
                    {
                        float dmgToStore = projHit.baseData.damage * TheBombDmgScale;
                        if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                        {
                            dmgToStore *= 0.25f;
                        }

                        //if dmgTrackingList doesn't contain the target, add the target and damage to dmgTrackerList and add target and null to timeTrackerList
                        if (!enemyTheBombDmgStored.ContainsKey(target))
                        {
                            enemyTheBombDmgStored.Add(target, dmgToStore);
                            enemyTheBombCoroutine.Add(target, null);
                            GameObject vfxObject = UnityEngine.Object.Instantiate(IdleEffectVFX, target.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset, Quaternion.identity);
                            var sprite = vfxObject.GetComponent<tk2dSprite>();

                            if (sprite != null)
                            {
                                
                            }

                            vfxObject.GetComponent<VFXAnchorModule>().anchorAIActor = target;
                            vfxObject.GetComponent<VFXAnchorModule>().offset = vfxOffset + new Vector3(0, target.specRigidbody.HitboxPixelCollider.UnitDimensions.y);

                            Plugin.Log($"hitboxpixelcollider: {target.specRigidbody.HitboxPixelCollider.UnitDimensions}");

                            activeVFXObjectList.Add(target, vfxObject);
                        }
                        // if dmgTrackingList does contain the target, add dmgToStore to damage stored in dmgTrackerList
                        else
                        {
                            enemyTheBombDmgStored[target] += dmgToStore;
                        }

                        Plugin.Log($"enemyTheBombDmgStored: {enemyTheBombDmgStored[target]}, enemy hp: {target.healthHaver.GetCurrentHealth()}");

                        //detonate damage if dmg stored is greater than target's current health
                        if (enemyTheBombDmgStored[target] >= target.healthHaver.GetCurrentHealth() && target.healthHaver.GetCurrentHealth() != 0)
                        {
                            DetonateTheBomb(target);

                            /*enemy.aiActor.healthHaver.ApplyDamage(
                                enemyTheBombDmgStored[enemy.aiActor],
                                Vector2.zero,
                                "the_bomb_detonation_damage",
                                CoreDamageTypes.None,
                                DamageCategory.Normal,
                                ignoreInvulnerabilityFrames: true,
                                hitPixelCollider: null,
                                ignoreDamageCaps: true
                            );

                            enemyTheBombDmgStored.Remove(enemy.aiActor);
                            */
                        }
                        // if dmg stored is not greater than enemy health, start timer and reset this timer with more applications
                        else
                        {
                            if (enemyTheBombCoroutine[target] != null)
                            {
                                StopCoroutine(enemyTheBombCoroutine[target]);
                            }
                            enemyTheBombCoroutine[target] = StartCoroutine(TheBombCooldown(target));
                        }
                    }
                };
            }
        }

        private System.Collections.IEnumerator TheBombCooldown(AIActor enemyActor) 
        {
            Plugin.Log("bomb cooldown start");

            yield return new WaitForSeconds(TheBombDuration);

            DetonateTheBomb(enemyActor);

            //StopCoroutine(enemyTheBombCoroutine[enemyActor]);
        }

        private void DetonateTheBomb(AIActor enemyActor)
        {
            Plugin.Log("bomb detonated");

            /*
            if (activeVFXObjectList[enemyActor] != null)
            {
                Destroy(activeVFXObjectList[enemyActor]);
            }
            activeVFXObjectList[enemyActor] = enemyActor.PlayEffectOnActor(ExplodeEffectVFX, new Vector3(0 / 16f, 0 / 16f, -2f), true, false, false);
            */

            if (activeVFXObjectList[enemyActor] != null)
            {
                Destroy(activeVFXObjectList[enemyActor]);
                activeVFXObjectList.Remove(enemyActor);
            }

            UnityEngine.Object.Instantiate(ExplodeEffectVFX, enemyActor.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset + new Vector3(0, enemyActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y), Quaternion.identity);

            enemyActor.healthHaver.ApplyDamage(
                enemyTheBombDmgStored[enemyActor],
                Vector2.zero,
                "the_bomb_detonation_damage",
                CoreDamageTypes.None,
                DamageCategory.Normal,
                ignoreInvulnerabilityFrames: true,
                hitPixelCollider: null,
                ignoreDamageCaps: true
            );

            StopCoroutine(enemyTheBombCoroutine[enemyActor]);
            enemyTheBombCoroutine.Remove(enemyActor);
            enemyTheBombDmgStored.Remove(enemyActor);

            //Plugin.Log($"dmg storage: {enemyTheBombDmgStored}, coroutine storage: {enemyTheBombCoroutine}");
        }
    }
}
