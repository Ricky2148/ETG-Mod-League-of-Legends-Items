using Alexandria.BreakableAPI;
using Alexandria.cAPI;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.SoundAPI;
using Alexandria.VisualAPI;
using BepInEx;
using Gungeon;
using LOLItems.custom_class_data;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalSparksDoer;

namespace LOLItems.weapons
{
    internal class SoulSpear : AdvancedGunBehavior
    {
        public static string internalName = "Soul Spear"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Soul Spear"; //The name that shows up in the Ammonomicon and the mod console.

        private static int ammoStat = 254;
        private static float reloadDuration = 0f;
        private static float fireRateStat = 0.6f;
        private static int spreadAngle = 2;

        private static float projectileDamageStat = 12f;
        private static float projectileSpeedStat = 60f; //60f;
        private static float projectileRangeStat = 20f;
        private static float projectileForceStat = 0f;

        private static float dashBaseDuration = 0.3f;
        private static float dashBaseSpeed = 20f;

        private Coroutine dashCoroutine;

        private static List<string> VFXSpritePath = new List<string>
        {
            "LOLItems/Resources/vfxs/vengencespear/vengencespear_vfx"
        };

        private static GameObject EffectVFX;

        private Dictionary<AIActor, List<GameObject>> activeVFXObjectList = new Dictionary<AIActor, List<GameObject>>();

        //private Dictionary<AIActor, int> enemyRendStacks = new Dictionary<AIActor, int>();
        private static float rendScale = 0.4f;

        //private bool isFiring = false;

        private static List<string> normalFiringSFXList = new List<string>
        {
            "vengencespear_dry_fire_sfx_001",
            "vengencespear_dry_fire_sfx_002",
            "vengencespear_dry_fire_sfx_003",
            "vengencespear_dry_fire_sfx_004",
            "vengencespear_dry_fire_sfx_005",
            "vengencespear_dry_fire_sfx_006",
            "vengencespear_dry_fire_sfx_007",
            "vengencespear_dry_fire_sfx_008",
        };

        private static List<string> rendSFXList = new List<string>
        {
            //"vengencespear_rend_sfx_001",
            //"vengencespear_rend_sfx_002",
            //"vengencespear_rend_sfx_003",
            //"vengencespear_rend_sfx_004",

            "rend1",
            "rend2",
            "rend3",
            "rend4",
            "rend5",
        };

        public bool THREEPRONGEDSPEARSActivated = false;
        private static float THREEPRONGEDSPEARSrendScaleInc = 0.4f;

        public static void Add()
        {
            string FULLNAME = "Soul Spear";
            string SPRITENAME = "vengencespear";
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<SoulSpear>();
            gun.SetShortDescription("\"Accept no contrition.\"");
            gun.SetLongDescription("These soul spears impale themselves onto the target and can be recalled out at will, inflicting even more suffering.\n\n" +
                "Every attacks forces the player to either stay still or dash. You are invulnerable during the dash.\n" +
                "Press reload to recall the spears and deal damage based on number of spears.\n\n" + 
                "A ghostly weapon originally wielded by the nightmare wraith, Kalista, a being with the sole purpose of hunting deceivers " +
                "and traitors. It was said that once you were made the focus of her wrath, there was nothing you could do but offer up your soul.\n");

            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);

            EffectVFX = VFXBuilder.CreateVFX
            (
                "soulspear_rend_vfx",
                VFXSpritePath,
                1,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Loop,
                true
            );

            gun.SetAnimationFPS(gun.shootAnimation, 17);

            //gun.SetAnimationFPS(gun.reloadAnimation, 12);

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items._38Special) as Gun, true, false);
            gun.muzzleFlashEffects = null;

            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}"; 
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null);
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null);
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_gun_finale_01", null);
            gun.DefaultModule.angleVariance = spreadAngle;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;

            gun.gunClass = GunClass.RIFLE;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat;
            gun.DefaultModule.numberOfShotsInClip = 9999;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.TwoHanded;

            gun.carryPixelOffset += new IntVector2(-14, 10);
            gun.carryPixelDownOffset += new IntVector2(18, 4); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(12, -22); // offset when aiming up

            gun.barrelOffset.transform.localPosition += new Vector3(32 / 16f, 5 / 16f);
            gun.gunScreenShake.magnitude = 0f;

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items._38Special) as Gun).DefaultModule.projectiles[0]);

            gun.DefaultModule.projectiles[0] = projectile;

            projectile.hitEffects.deathAny = (PickupObjectDatabase.GetById((int)Items.Bow) as Gun).DefaultModule.projectiles[0].hitEffects.deathAny;
            projectile.hitEffects.deathEnemy = null;
            projectile.hitEffects.enemy = null;
            projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

            projectile.objectImpactEventName = "vengencespear5";
            projectile.enemyImpactEventName = "vengencespear4";

            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            projectile.baseData.damage = projectileDamageStat;
            projectile.baseData.speed = projectileSpeedStat;
            projectile.baseData.range = projectileRangeStat;
            projectile.baseData.force = projectileForceStat;
            projectile.transform.parent = gun.barrelOffset;
            projectile.shouldRotate = true;

            projectile.ignoreDamageCaps = false;

            projectile.SetProjectileSpriteRight("vengencespear_projectile_spearonly", 34, 4, true, tk2dBaseSprite.Anchor.MiddleCenter, 32, 3);

            /*
            List<string> projectileSpriteNames = new List<string>
            {
                "vengencespear_projectile_001",
                "vengencespear_projectile_002",
                "vengencespear_projectile_003",
            };
            int projectileFPS = 8;
            List<IntVector2> projectileSizes = new List<IntVector2>
            {
                new IntVector2(34, 12),
                new IntVector2(34, 12),
                new IntVector2(34, 12),
            };
            List<bool> projectileLighteneds = new List<bool>
            {
                true,
                true,
                true,
            };
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>
            {
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
            };
            List<bool> projectileAnchorsChangeColiders = new List<bool>
            {
                false,
                false,
                false,
            };
            List<bool> projectilefixesScales = new List<bool>
            {
                false,
                false,
                false,
            };
            List<Vector3?> projectileManualOffsets = new List<Vector3?>
            {
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
            };
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?>
            {
                new IntVector2(32, 10),
                new IntVector2(32, 10),
                new IntVector2(32, 10),
            };
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?>
            {
                null,
                null,
                null,
            };
            List<Projectile> projectileOverrideProjectilesToCopyFrom = new List<Projectile>
            {
                null,
                null,
                null,
            };
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;

            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);
            */

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("soul_spear_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/vengencespear_ammo_full_001", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/vengencespear_ammo_empty_001");

            gun.shellCasing = null;
            gun.clipObject = null;
            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 0;
            gun.clipsToLaunchOnReload = 0;
            gun.reloadClipLaunchFrame = 0;

            EasyTrailBullet trail = projectile.gameObject.AddComponent<EasyTrailBullet>();
            trail.TrailPos = projectile.transform.position;
            trail.StartWidth = 0.15f;
            trail.EndWidth = 0f;
            trail.LifeTime = 0.1f;

            trail.BaseColor = Color.cyan;
            trail.StartColor = Color.white;
            trail.EndColor = Color.blue;

            gun.quality = PickupObject.ItemQuality.A;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = gun.PickupObjectId;

            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            var sprite = EffectVFX.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.usesOverrideMaterial = true;
                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.5f);
            }
        }

        protected override void Update()
        {
            if (Owner != null)
            {
                if (Player.HasSynergy(Synergy.THREE_PRONGED_SPEARS) && !THREEPRONGEDSPEARSActivated)
                {
                    rendScale += THREEPRONGEDSPEARSrendScaleInc;

                    THREEPRONGEDSPEARSActivated = true;
                }
                else if (!Player.HasSynergy(Synergy.THREE_PRONGED_SPEARS) && THREEPRONGEDSPEARSActivated)
                {
                    rendScale -= THREEPRONGEDSPEARSrendScaleInc;

                    THREEPRONGEDSPEARSActivated = false;
                }
            }

            base.Update();
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            HelpfulMethods.PlayRandomSFX(gun.gameObject, normalFiringSFXList);

            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = StartCoroutine(MartialPoiseDash(player));

            // if gun is not actively firing
            /*if (!isFiring)
            {
                isFiring = true;
                ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
                float statToMod = player.stats.GetStatValue(PlayerStats.StatType.MovementSpeed);
                ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, -statToMod, StatModifier.ModifyMethod.ADDITIVE);
                player.stats.RecalculateStats(player, true, false);
            }*/

            base.OnPostFired(player, gun);
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy == null) return;
                AIActor firstEnemy = null;
                if (enemy.aiActor != null) 
                {
                    firstEnemy = enemy.aiActor;
                    //Plugin.Log($"enemy.aiactor: {firstEnemy}");
                }
                else if (enemy.GetComponentInParent<AIActor>() != null)
                {
                    firstEnemy = enemy.GetComponentInParent<AIActor>();
                    //Plugin.Log($"enemy.parentAiactor: {firstEnemy}");
                }
                else
                {
                    //Plugin.Log($"no aiactor");
                    return;
                }
                if (enemy.healthHaver != null)
                {
                    /*if (fatal && enemyRendStacks.ContainsKey(enemy.aiActor))
                    {
                        enemyRendStacks.Remove(enemy.aiActor);
                    }

                    if (!enemyRendStacks.ContainsKey(enemy.aiActor))
                    {
                        enemyRendStacks.Add(enemy.aiActor, 1);
                        //enemy.aiActor.PlayEffectOnActor(EffectVFX, new Vector3(0 / 16f, 0 / 16f), true, false, false);

                    }
                    else
                    {
                        enemyRendStacks[enemy.aiActor] += 1;
                    }*/

                    if (fatal && activeVFXObjectList.ContainsKey(firstEnemy))
                    {
                        foreach (GameObject vfxObj in activeVFXObjectList[firstEnemy])
                        {
                            if (vfxObj != null)
                            {
                                Destroy(vfxObj);
                            }
                        }

                        activeVFXObjectList.Remove(firstEnemy);
                        return;
                    }

                    Vector3 idk = (enemy as SpeculativeRigidbody).UnitDimensions;
                    //Plugin.Log($"unit dimensions: {idk}");
                    float num = ((idk.x + idk.y) / 2);
                    //idk.x *= UnityEngine.Random.Range(-1.1f, 1.1f);
                    //idk.y *= UnityEngine.Random.Range(-1.1f, 1.1f);

                    Vector3 offset = new Vector3(26 / 16f, 0 / 16f);
                    offset += new Vector3(num * UnityEngine.Random.Range(-0.3f, 0.3f), num * UnityEngine.Random.Range(-0.3f, 0.3f));
                    //Plugin.Log($"offset (randomized): {offset}");

                    //var smth = enemy.aiActor.PlayEffectOnActor(EffectVFX, new Vector3(26 / 16f, 0 / 16f), true, false, true);
                    var smth = firstEnemy.PlayEffectOnActor(EffectVFX, offset, true, false, true);
                    var sprite = smth.GetComponent<tk2dSprite>();

                    if (sprite != null)
                    {
                        sprite.transform.rotation = projectile.transform.rotation;
                    }

                    if (!activeVFXObjectList.ContainsKey(firstEnemy))
                    {
                        //var smth = enemy.aiActor.PlayEffectOnActor(EffectVFX, new Vector3(0 / 16f, 0 / 16f), true, false, false);

                        activeVFXObjectList.Add(firstEnemy, new List<GameObject> { smth });
                        //enemy.aiActor.PlayEffectOnActor(EffectVFX, new Vector3(0 / 16f, 0 / 16f), true, false, false);
                    }
                    else
                    {
                        //var smth = enemy.aiActor.PlayEffectOnActor(EffectVFX, new Vector3(0 / 16f, 0 / 16f), true, false, false);

                        activeVFXObjectList[firstEnemy].Add(smth);
                    }
                }
            };

            base.PostProcessProjectile(projectile);
        }

        public override void OnReloadPressedSafe(PlayerController player, Gun gun, bool manualReload)
        {
            float rendDamagePerStack = gun.DefaultModule.projectiles[0].baseData.damage * rendScale * player.stats.GetStatValue(PlayerStats.StatType.Damage);
            //Plugin.Log($"rend damage per stack: {rendDamagePerStack}, projectile dmg: {gun.DefaultModule.projectiles[0].baseData.damage}, damage mult: {player.stats.GetStatValue(PlayerStats.StatType.Damage)}");
            /*foreach (KeyValuePair<AIActor, int> target in enemyRendStacks)
            {
                float damageToDeal = (target.Value + 1) * rendDamagePerStack;
                Plugin.Log($"rend stacks: {target.Value}, damage dealt: {damageToDeal}");

                target.Key.healthHaver.ApplyDamage(
                    damageToDeal,
                    Vector2.zero,
                    "soul_spear_rend_damage",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    false
                );
            }

            enemyRendStacks.Clear();
            */

            foreach (KeyValuePair<AIActor, List<GameObject>> target in activeVFXObjectList)
            {
                float damageToDeal = (target.Value.Count /* + 1 */) * rendDamagePerStack;
                //Plugin.Log($"rend stacks: {target.Value.Count}, damage dealt: {damageToDeal}");

                if (target.Key.healthHaver != null && target.Key.gameObject != null)
                {
                    HelpfulMethods.PlayRandomSFX(target.Key.gameObject, rendSFXList);

                    target.Key.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "soul_spear_rend_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        ignoreDamageCaps: true
                    );

                    Vector2 unitDimensions = target.Key.specRigidbody.HitboxPixelCollider.UnitDimensions;
                    Vector2 a = unitDimensions / 2f;

                    int num3 = Mathf.Min(target.Value.Count, 50) + 15;
                    Vector2 vector = target.Key.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
                    Vector2 vector2 = target.Key.specRigidbody.HitboxPixelCollider.UnitTopRight;
                    PixelCollider pixelCollider = target.Key.specRigidbody.GetPixelCollider(ColliderType.Ground);
                    if (pixelCollider != null && pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Manual)
                    {
                        vector = Vector2.Min(vector, pixelCollider.UnitBottomLeft);
                        vector2 = Vector2.Max(vector2, pixelCollider.UnitTopRight);
                    }
                    vector += Vector2.Min(a * 0.15f, new Vector2(0.25f, 0.25f));
                    vector2 -= Vector2.Min(a * 0.15f, new Vector2(0.25f, 0.25f));
                    vector2.y -= Mathf.Min(a.y * 0.1f, 0.1f);
                    //GlobalSparksDoer.DoRandomParticleBurst(num3, vector, vector2, Vector2.zero, 5f, 5f, 0.3f, 1, ExtendedColours.skyblue, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                    HelpfulMethods.DoRandomParticleBurst(num3, vector, vector2, 1f, 1f, 0.3f, 1, Color.cyan, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                    //anglevariance, magnitudevariance, startsize, startlifetime
                    //GlobalSparksDoer.DoRandomParticleBurst(Mathf.Max(target.Value.Count, 30), )
                }

                foreach (GameObject vfxObj in target.Value)
                {
                    if (vfxObj != null)
                    {
                        Destroy(vfxObj);
                    }
                }
            }

            activeVFXObjectList.Clear();
        }

        public override void OnFinishAttack(PlayerController player, Gun gun)
        {
            /*if (isFiring)
            {
                isFiring = false;
                ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
                ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                player.stats.RecalculateStats(player, true, false);
            }*/

            base.OnFinishAttack(player, gun);
        }

        public override void OnSwitchedAwayFromThisGun()
        {
            PlayerController player = Owner as PlayerController;
            //isFiring = false;

            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }

            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //player.stats.RecalculateStats(player, true, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            base.OnSwitchedAwayFromThisGun();
        }

        /*public System.Collections.IEnumerator MartialPoiseDash(PlayerController player)
        {
            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
            //player.stats.RecalculateStats(player, true, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            float statToMod = player.stats.GetStatValue(PlayerStats.StatType.MovementSpeed);
            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, -statToMod, StatModifier.ModifyMethod.ADDITIVE);
            //player.stats.RecalculateStats(player, true, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            //Plugin.Log($"modifier: {-statToMod}");

            float duration = dashBaseDuration / player.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
            //float adjSpeed = dashBaseSpeed * player.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
            float adjSpeed = dashBaseSpeed * (1 + ((player.stats.GetStatValue(PlayerStats.StatType.RateOfFire) - 1) * 0.5f));
            float elapsed = -BraveTime.DeltaTime;

            player.healthHaver.TriggerInvulnerabilityPeriod(duration);

            //Plugin.Log($"\n\n{player.m_playerCommandedDirection}\n{player.LastCommandedDirection}\n{player.m_activeActions.Move.Vector}");

            // need to check for player not inputting a direction, idk how yet tho lmao
            //Vector2 angle = player.m_lastNonzeroCommandedDirection.normalized;
            Vector2 angle = Vector2.zero;
            if (player.CurrentInputState != PlayerInputState.NoMovement)
            {
                angle = player.AdjustInputVector(player.m_activeActions.Move.Vector, BraveInput.MagnetAngles.movementCardinal, BraveInput.MagnetAngles.movementOrdinal);
                //Plugin.Log($"m_activeActions: {player.m_activeActions.Move.Vector}, angle: {angle}");
            }
            if (angle.magnitude > 1f)
            {
                angle.Normalize();
                //Plugin.Log($"normalized angle: {angle}");
            }

            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                player.specRigidbody.Velocity = angle * adjSpeed;
                yield return null;
            }

            yield return new WaitForSeconds(duration * 2);

            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //player.stats.RecalculateStats(player, true, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
            //Plugin.Log($"modifier: {1f}");
        }*/

        public System.Collections.IEnumerator MartialPoiseDash(PlayerController player)
        {
            Vector2 angle = Vector2.zero;
            //check and record player direction input
            if (player.CurrentInputState != PlayerInputState.NoMovement)
            {
                angle = player.AdjustInputVector(player.m_activeActions.Move.Vector, BraveInput.MagnetAngles.movementCardinal, BraveInput.MagnetAngles.movementOrdinal);
                //if no direction input, no dash
                if (angle.magnitude <= 0)
                {
                    //Plugin.Log("break");
                    ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
                    player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
                    yield break;
                }
            }

            //Plugin.Log("dash");

            //makes player unable to move while being dashed in direction, gives invulerability frames
            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            float statToMod = player.stats.GetStatValue(PlayerStats.StatType.MovementSpeed);
            //Plugin.Log($"statToMod: {statToMod}, base stat: {player.stats.GetBaseStatValue(PlayerStats.StatType.MovementSpeed)}");
            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, -statToMod, StatModifier.ModifyMethod.ADDITIVE);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            float duration = dashBaseDuration / player.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
            float adjSpeed = dashBaseSpeed * (1 + ((player.stats.GetStatValue(PlayerStats.StatType.RateOfFire) - 1) * 0.5f));
            //Plugin.Log($"adjSpeed: {adjSpeed}");
            adjSpeed *= ((7f + ((statToMod - 7f) * 0.5f)) / 7f);
            //Plugin.Log($"final adjSpeed: {adjSpeed}");
            float elapsed = -BraveTime.DeltaTime;

            player.healthHaver.TriggerInvulnerabilityPeriod(duration);

            //colored outline for invul frames
            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(51f * 0.3f, 255f * 0.3f, 153f * 0.3f));
            }

            if (angle.magnitude > 1f)
            {
                angle.Normalize();
            }

            while (elapsed < duration)
            {
                if (player.IsFalling)
                {
                    if (mat)
                    {
                        mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
                    }

                    ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
                    ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

                    yield break;
                }

                elapsed += BraveTime.DeltaTime;
                player.specRigidbody.Velocity = angle * adjSpeed;
                yield return null;
            }

            //reset outline color
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }

            //resets movementspeed and gives player back control
            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed);
            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
        }
    }
}
