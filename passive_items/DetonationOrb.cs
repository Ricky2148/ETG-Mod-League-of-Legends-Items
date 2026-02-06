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

// vfx should be a white spark ball looking thing, on effect application, the vfx is thrown above the enemies head, it starts small, then grows larger with more damage
// maybe start changing colors, would want it to instantly change to red when it would detonate to execute the target. Every time you refresh the duration, it just increases the duration without refreshing the vfx or vfx's loop
// might have to rewrite the entire logic, fuck me man

namespace LOLItems.passive_items
{
    public class EnemyTheBombTracker
    {
        public float storedDamage;
        public Coroutine timerCoroutine;
        public GameObject activeVFXObject;

        public EnemyTheBombTracker (float dmg, Coroutine corou, GameObject obj)
        {
            storedDamage = dmg;
            timerCoroutine = corou;
            activeVFXObject = obj;
        }
    }

    internal class DetonationOrb : PassiveItem
    {
        public static string ItemName = "Detonation Orb";

        private static float DamageStat = 1.2f;
        private static float TheBombDmgScale = 0.25f;
        private static float TheBombDuration = 3f;

        public bool LIFEANDDEATHActivated = false;
        private static float LIFEANDDEATHTheBombDmgScaleInc = 0.15f;
        public bool OVERCHARGEDActivated = false;
        private static float OVERCHARGEDTheBombDmgScaleInc = 0.20f;

        private Dictionary<AIActor, EnemyTheBombTracker> enemyTheBombTrackerList = new Dictionary<AIActor, EnemyTheBombTracker> ();

        //private Dictionary<AIActor, float> enemyTheBombDmgStored = new Dictionary<AIActor, float>();

        //private Dictionary<AIActor, Coroutine> enemyTheBombCoroutine = new Dictionary<AIActor, Coroutine>();

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
        //potential sfx:Play_WPN_bountyhunterarm_charge_03, Play_WPN_raidenlaser_shot_01, m_WPN_thor_charge_01

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

        //private Dictionary<AIActor, GameObject> activeVFXObjectList = new Dictionary<AIActor, GameObject>();

        public static Vector3 vfxOffset = new Vector3(-1 / 16f, 6 / 16f, 0);

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/detonation_orb_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<DetonationOrb>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Bites the Dust";
            string longDesc = "Dealing damage to an enemy stores some of that damage. Damage accumulates with more damage and detonates after 3 seconds of no damage. " +
                "Will immediately detonate if stored damage is enough to kill.\n\n" +
                "A magical orb imbued with the power of a lightning spark. Any who are harmed by its wielder will be subject to further damage from lightning.";

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
                emissivePower: 1.5f,
                emissiveColour: Color.blue,
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
                false
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

            /*if (enemyTheBombDmgStored != null)
            {
                enemyTheBombDmgStored.Clear();
            }
            if (enemyTheBombCoroutine != null)
            {
                enemyTheBombCoroutine.Clear();
            }
            if (activeVFXObjectList != null)
            {
                activeVFXObjectList.Clear();
            }*/
            if (enemyTheBombTrackerList != null)
            {
                enemyTheBombTrackerList.Clear();
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.LIFE_AND_DEATH) && !LIFEANDDEATHActivated)
                {
                    TheBombDmgScale += LIFEANDDEATHTheBombDmgScaleInc;

                    LIFEANDDEATHActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.LIFE_AND_DEATH) && LIFEANDDEATHActivated)
                {
                    TheBombDmgScale -= LIFEANDDEATHTheBombDmgScaleInc;

                    LIFEANDDEATHActivated = false;
                }

                if (Owner.HasSynergy(Synergy.OVERCHARGED) && !OVERCHARGEDActivated)
                {
                    TheBombDmgScale += OVERCHARGEDTheBombDmgScaleInc;

                    OVERCHARGEDActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.OVERCHARGED) && OVERCHARGEDActivated)
                {
                    TheBombDmgScale -= OVERCHARGEDTheBombDmgScaleInc;

                    OVERCHARGEDActivated = false;
                }
            }
            base.Update();
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
                    //dmgToStore *= 0.25f;
                }

                //experimental
                if (!enemyTheBombTrackerList.ContainsKey(target))
                {
                    GameObject vfxObject = UnityEngine.Object.Instantiate(IdleEffectVFX, target.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset, Quaternion.identity);
                    var sprite = vfxObject.GetComponent<tk2dSprite>();

                    if (sprite != null)
                    {

                    }

                    vfxObject.GetComponent<VFXAnchorModule>().anchorAIActor = target;
                    vfxObject.GetComponent<VFXAnchorModule>().offset = vfxOffset + new Vector3(0, target.specRigidbody.HitboxPixelCollider.UnitDimensions.y);

                    //Plugin.Log($"hitboxpixelcollider: {target.specRigidbody.HitboxPixelCollider.UnitDimensions}");

                    //activeVFXObjectList.Add(target, vfxObject);

                    enemyTheBombTrackerList.Add(target, new EnemyTheBombTracker(dmgToStore, null, vfxObject));

                    if (target.healthHaver.gameObject.GetComponent<HealthHaverOnPreDeathActionModule>() == null)
                    {
                        Plugin.Log($"{target.healthHaver}");
                        HealthHaverOnPreDeathActionModule onPreDeathModule = new HealthHaverOnPreDeathActionModule();
                        onPreDeathModule.targetAIActor = target;
                        onPreDeathModule.explosionVFX = ExplodeEffectVFX;
                        onPreDeathModule.vfxOffset = vfxOffset;
                        onPreDeathModule.theBombTracker = enemyTheBombTrackerList[target];

                        target.healthHaver.gameObject.AddComponent(onPreDeathModule);
                    }

                    AkSoundEngine.PostEvent("detOrb_SFX_loop_002", target.gameObject);
                }
                else
                {
                    enemyTheBombTrackerList[target].storedDamage += dmgToStore;
                }

                //Plugin.Log($"enemyTheBombTracker storedDamage: {enemyTheBombTrackerList[target].storedDamage}, enemy hp: {target.healthHaver.GetCurrentHealth()}");

                if (enemyTheBombTrackerList[target].storedDamage >= target.healthHaver.GetCurrentHealth())
                {
                    if (enemyTheBombTrackerList[target].timerCoroutine != null)
                    {
                        StopCoroutine(enemyTheBombTrackerList[target].timerCoroutine);
                    }
                    DetonateTheBomb(target);
                }
                else
                {
                    if (enemyTheBombTrackerList[target].timerCoroutine != null)
                    {
                        StopCoroutine(enemyTheBombTrackerList[target].timerCoroutine);
                    }
                    enemyTheBombTrackerList[target].timerCoroutine = StartCoroutine(TheBombCooldown(target));
                }
            }

            /*if (hitRigidbody.healthHaver != null && hitRigidbody.healthHaver.IsAlive)
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
                }
                else
                {
                    if (enemyTheBombCoroutine[target] != null)
                    {
                        StopCoroutine(enemyTheBombCoroutine[target]);
                    }
                    enemyTheBombCoroutine[target] = StartCoroutine(TheBombCooldown(target));
                }
            }*/
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

                    //Plugin.Log($"pre fatal: {fatal}, {enemyTheBombTrackerList.ContainsKey(target)}");
                    if (fatal && enemyTheBombTrackerList.ContainsKey(target))
                    {
                        if (enemyTheBombTrackerList[target].timerCoroutine != null)
                        {
                            StopCoroutine(enemyTheBombTrackerList[target].timerCoroutine);
                        }
                        DetonateTheBomb(target);
                    }

                    if (enemy.healthHaver != null && enemy.healthHaver.IsAlive)
                    {
                        float dmgToStore = projHit.baseData.damage * TheBombDmgScale;
                        if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                        {
                            //dmgToStore *= 0.25f;
                        }

                        //Plugin.Log($"in module: {enemyTheBombTrackerList[target]}, {enemyTheBombTrackerList.ContainsKey(target)}");
                        //experimental
                        if (!enemyTheBombTrackerList.ContainsKey(target))
                        {
                            GameObject vfxObject = UnityEngine.Object.Instantiate(IdleEffectVFX, target.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset, Quaternion.identity);
                            var sprite = vfxObject.GetComponent<tk2dSprite>();

                            if (sprite != null)
                            {

                            }

                            vfxObject.GetComponent<VFXAnchorModule>().anchorAIActor = target;
                            vfxObject.GetComponent<VFXAnchorModule>().offset = vfxOffset + new Vector3(0, target.specRigidbody.HitboxPixelCollider.UnitDimensions.y);

                            //Plugin.Log($"hitboxpixelcollider: {target.specRigidbody.HitboxPixelCollider.UnitDimensions}");

                            //activeVFXObjectList.Add(target, vfxObject);

                            enemyTheBombTrackerList.Add(target, new EnemyTheBombTracker(dmgToStore, null, vfxObject));
                            
                            if (target.healthHaver.gameObject.GetComponent<HealthHaverOnPreDeathActionModule>() == null)
                            {
                                Plugin.Log($"{target.healthHaver}");
                                HealthHaverOnPreDeathActionModule onPreDeathModule = new HealthHaverOnPreDeathActionModule();
                                onPreDeathModule.targetAIActor = target;
                                onPreDeathModule.explosionVFX = ExplodeEffectVFX;
                                onPreDeathModule.vfxOffset = vfxOffset;
                                onPreDeathModule.theBombTracker = enemyTheBombTrackerList[target];
                                
                                target.healthHaver.gameObject.AddComponent(onPreDeathModule);
                            }

                            AkSoundEngine.PostEvent("detOrb_SFX_loop_002", target.gameObject);
                        }
                        else
                        {
                            enemyTheBombTrackerList[target].storedDamage += dmgToStore;
                        }

                        //Plugin.Log($"enemyTheBombTracker storedDamage: {enemyTheBombTrackerList[target].storedDamage}, enemy hp: {target.healthHaver.GetCurrentHealth()}");

                        if (enemyTheBombTrackerList[target].storedDamage >= target.healthHaver.GetCurrentHealth())
                        {
                            if (enemyTheBombTrackerList[target].timerCoroutine != null)
                            {
                                StopCoroutine(enemyTheBombTrackerList[target].timerCoroutine);
                            }
                            DetonateTheBomb(target);
                        }
                        else
                        {
                            if (enemyTheBombTrackerList[target].timerCoroutine != null)
                            {
                                StopCoroutine(enemyTheBombTrackerList[target].timerCoroutine);
                            }
                            enemyTheBombTrackerList[target].timerCoroutine = StartCoroutine(TheBombCooldown(target));
                            //should probably do it with target.StartCoroutine
                        }


                        //if dmgTrackingList doesn't contain the target, add the target and damage to dmgTrackerList and add target and null to timeTrackerList
                        /*if (!enemyTheBombDmgStored.ContainsKey(target))
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
                        }*/

                        /*Plugin.Log($"enemyTheBombDmgStored: {enemyTheBombDmgStored[target]}, enemy hp: {target.healthHaver.GetCurrentHealth()}");

                        //detonate damage if dmg stored is greater than target's current health
                        if (enemyTheBombDmgStored[target] >= target.healthHaver.GetCurrentHealth() && target.healthHaver.GetCurrentHealth() != 0)
                        {
                            DetonateTheBomb(target);
                        }
                        // if dmg stored is not greater than enemy health, start timer and reset this timer with more applications
                        else
                        {
                            if (enemyTheBombCoroutine[target] != null)
                            {
                                StopCoroutine(enemyTheBombCoroutine[target]);
                            }
                            enemyTheBombCoroutine[target] = StartCoroutine(TheBombCooldown(target));
                        }*/
                    }
                };
            }
        }

        private System.Collections.IEnumerator TheBombCooldown(AIActor enemyActor) 
        {
            //Plugin.Log($"bomb cooldown start: {enemyActor}");

            yield return new WaitForSeconds(TheBombDuration);

            // issue comes when enemy is null here
            Plugin.Log($"bomb cooldown end: {enemyActor}");
            DetonateTheBomb(enemyActor);

            /*if (enemyActor.healthHaver.IsAlive)
            {
                DetonateTheBomb(enemyActor);
            }
            else
            {
                StopCoroutine(enemyTheBombTrackerList[enemyActor].timerCoroutine);

                if (enemyTheBombTrackerList[enemyActor].activeVFXObject != null)
                {
                    Destroy(enemyTheBombTrackerList[enemyActor].activeVFXObject);
                }

                enemyTheBombTrackerList.Remove(enemyActor);
            }*/

            //StopCoroutine(enemyTheBombCoroutine[enemyActor]);
        }

        private void DetonateTheBomb(AIActor enemyActor)
        {
            //Plugin.Log("bomb detonated");

            /*
            if (activeVFXObjectList[enemyActor] != null)
            {
                Destroy(activeVFXObjectList[enemyActor]);
            }
            activeVFXObjectList[enemyActor] = enemyActor.PlayEffectOnActor(ExplodeEffectVFX, new Vector3(0 / 16f, 0 / 16f, -2f), true, false, false);
            */
            if (enemyActor == null)
            {
                Plugin.Log($"{enemyActor}");
            }

            AkSoundEngine.PostEvent("detOrb_SFX_loop_002" + "_stop", enemyActor.gameObject);

            if (enemyActor.healthHaver.IsAlive)
            {
                UnityEngine.Object.Instantiate(ExplodeEffectVFX, enemyActor.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset + new Vector3(0, enemyActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y), Quaternion.identity);

                AkSoundEngine.PostEvent("detOrb_SFX_explosion_001", enemyActor.gameObject);

                enemyActor.healthHaver.ApplyDamage(
                    enemyTheBombTrackerList[enemyActor].storedDamage,
                    Vector2.zero,
                    "the_bomb_detonation_damage",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    ignoreInvulnerabilityFrames: true,
                    hitPixelCollider: null,
                    ignoreDamageCaps: true
                );
            }

            if (enemyTheBombTrackerList[enemyActor].timerCoroutine != null)
            {
                StopCoroutine(enemyTheBombTrackerList[enemyActor].timerCoroutine);
            }

            if (enemyTheBombTrackerList[enemyActor].activeVFXObject != null)
            {
                Destroy(enemyTheBombTrackerList[enemyActor].activeVFXObject);
            }

            enemyTheBombTrackerList.Remove(enemyActor);

            /*if (activeVFXObjectList[enemyActor] != null)
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
            enemyTheBombDmgStored.Remove(enemyActor);*/

            //Plugin.Log($"dmg storage: {enemyTheBombDmgStored}, coroutine storage: {enemyTheBombCoroutine}");
        }
    }
}
