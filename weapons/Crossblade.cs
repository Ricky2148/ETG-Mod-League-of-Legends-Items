using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;
using BepInEx;
using System.Collections.Generic;
using LOLItems.custom_class_data;
using Dungeonator;

namespace LOLItems.weapons
{
    internal class Crossblade : AdvancedGunBehavior
    {
        public static string internalName = "Crossblade";
        public static int ID; 
        public static string realName = "Crossblade"; 

        private static int ammoStat = 250;
        private static float reloadDuration = 0f;
        private static float fireRateStat = 0.6f;
        private static int spreadAngle = 5;

        private static int ricochetRange = 5;
        private static int ricochetCount = 8;
        private static float ricochetDamageScale = 0.4f;
        private static float ricochetSpeedScale = 0.5f;

        private static float projectileDamageStat = 10f;
        private static float projectileSpeedStat = 20f;
        private static float projectileRangeStat = 25f;
        private static float projectileForceStat = 8f;

        private static List<string> CrossbladeFiringSFXList = new List<string>
        {
            //"boomerangblade_fire_sfx_001",
            //"boomerangblade_fire_sfx_002",
            //"boomerangblade_fire_sfx_003",
            "boomerangblade_fire_sfx_004",
            "boomerangblade_fire_sfx_005",
            "boomerangblade_fire_sfx_006",
            "boomerangblade_fire_sfx_007",
        };

        public bool BOUNCEMAXXINGActivated;

        public static void Add()
        {
            string FULLNAME = realName;
            string SPRITENAME = "boomerangblade";
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<Crossblade>();
            gun.SetShortDescription("\"Catch!\"");
            gun.SetLongDescription("A special cross-shaped boomerang blade said to be from a famous desert mercenary. As if the boomerang itself " +
                "is determined to finish the job, it will try to attack more enemies if possible.\n");

            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);

            gun.SetAnimationFPS(gun.shootAnimation, 18);
            //gun.SetAnimationFPS(gun.reloadAnimation, 20);

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun, true, false);
            gun.muzzleFlashEffects = null; //(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).muzzleFlashEffects;

            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null); 
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null); 

            gun.DefaultModule.angleVariance = spreadAngle;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.gunClass = GunClass.FULLAUTO; 
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random; 
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat; 
            gun.DefaultModule.numberOfShotsInClip = 9999;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.OneHanded;

            gun.carryPixelOffset += new IntVector2(0, 0);

            gun.barrelOffset.transform.localPosition += new Vector3(4 / 16f, 10 / 16f);

            gun.gunScreenShake.magnitude = 0f;

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles[0] = projectile;

            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            projectile.baseData.damage = projectileDamageStat;
            projectile.baseData.speed = projectileSpeedStat;
            projectile.baseData.range = projectileRangeStat;
            projectile.baseData.force = projectileForceStat;
            projectile.transform.parent = gun.barrelOffset;
            projectile.shouldRotate = true;

            RicochetProjectileModule ricochetModule = projectile.gameObject.AddComponent<RicochetProjectileModule>();
            ricochetModule.ricochetDamageScale = ricochetDamageScale;
            ricochetModule.ricochetSpeedScale = ricochetSpeedScale;
            ricochetModule.ricochetRange = ricochetRange;

            PierceProjModifier pierce = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = ricochetCount - 1;
            pierce.penetratesBreakables = false;

            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 0;
            gun.clipsToLaunchOnReload = 0;

            projectile.hitEffects.HasProjectileDeathVFX = true;
            projectile.hitEffects.deathAny = (PickupObjectDatabase.GetById((int)Items.DartGun) as Gun).DefaultModule.projectiles[0].hitEffects.deathAny;
            projectile.hitEffects.deathEnemy = null;
            projectile.hitEffects.enemy = null;
            projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById((int)Items.DartGun) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById((int)Items.DartGun) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

            projectile.objectImpactEventName = "shockSMG3";
            projectile.enemyImpactEventName = "boomerangblade1";

            projectile.SetProjectileSpriteRight("boomerangblade_projectile_001", 22, 22, true, tk2dBaseSprite.Anchor.MiddleCenter, 16, 16);

            // A list of filenames in the sprites/ProjectileCollection folder for each frame in the animation, extension not required.
            List<string> projectileSpriteNames = new List<string>
            {
                "boomerangblade_projectile_001",
                "boomerangblade_projectile_002",
                "boomerangblade_projectile_003",
                "boomerangblade_projectile_004",
                "boomerangblade_projectile_005",
                "boomerangblade_projectile_006",
                "boomerangblade_projectile_007",
                "boomerangblade_projectile_008",
            };
            // Animation FPS.
            int projectileFPS = 16;
            // Visual sprite size for each frame.  Sprite images will stretch to match these sizes.
            List<IntVector2> projectileSizes = new List<IntVector2>
            {
                new IntVector2(22, 22), //1
                new IntVector2(22, 22), //2
                new IntVector2(22, 22), //3
                new IntVector2(22, 22), //4
                new IntVector2(22, 22), //5 
                new IntVector2(22, 22), //6
                new IntVector2(22, 22), //7
                new IntVector2(22, 22), //8
            };
            // Whether each frame should have a bit of glow.
            List<bool> projectileLighteneds = new List<bool>
            {
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
            };
            // Sprite anchor list.
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>
            {
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
            };
            // Whether or not the anchors should affect the hitboxees.
            List<bool> projectileAnchorsChangeColiders = new List<bool>
            {
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
            };
            // Unknown, doesn't appear to matter so leave as false. 
            List<bool> projectilefixesScales = new List<bool>
            {
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
            };
            // Manual Offsets for each sprite if needed.
            List<Vector3?> projectileManualOffsets = new List<Vector3?>
            {
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
            };
            // Override the projectile hitboxes on each frame.  Either null (same as visuals) or slightly smaller than the visuals is most common.
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?>
            {
                new IntVector2(16, 16), //1
                new IntVector2(16, 16), //2
                new IntVector2(16, 16), //3
                new IntVector2(16, 16), //4
                new IntVector2(16, 16), //5 
                new IntVector2(16, 16), //6
                new IntVector2(16, 16), //7
                new IntVector2(16, 16), //8
            };
            // Manually assign the projectile offsets.
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?>
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
            };
            // Copy another projectile each frame.
            List<Projectile> projectileOverrideProjectilesToCopyFrom = new List<Projectile>
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
            };
            // Your animations wrap mode. If you just want it to do a looping animation, leave it as Loop. Only useful for when adding multiple differing animations.
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            // Optionally, you can give your animations a clip name. Only useful for when adding multiple differing animations.
            //string projectileClipName = "projectileName"; 
            // Optionally, you can assign an animation as the default one that plays.  Only useful for when adding multiple differing animations.  If left as null then it will use the most recently added animation.
            //string projectileDefaultClipName = "projectileName"; 

            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);


            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("crossblade_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/boomerangblade_ammo_full_001", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/boomerangblade_ammo_empty_001");

            gun.quality = PickupObject.ItemQuality.C;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:crossblade",
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "bouncy_bullets",
                "boomerang"
            };
            AdvancedSynergyEntry ase = CustomSynergies.Add("Bouncemaxxing", mandatoryConsoleIDs, optionalConsoleIDs, true);*/
        }

        public override void OnInitializedWithOwner(GameActor actor)
        {
            base.OnInitializedWithOwner(actor);
            Plugin.Log($"Player picked up {internalName}");

            /*if (Player.PlayerHasActiveSynergy("Bouncemaxxing"))
            {
                gun.DefaultModule.projectiles[0].gameObject.GetComponent<PierceProjModifier>().penetration = ricochetCount + 2;
            }*/
        }

        public override void OnDropped()
        {
            base.OnDropped();
        }

        protected override void Update()
        {
            if (Owner != null)
            {
                if (Player.HasSynergy(Synergy.BOUNCEMAXXING) && !BOUNCEMAXXINGActivated)
                {
                    gun.DefaultModule.projectiles[0].gameObject.GetComponent<PierceProjModifier>().penetration = ricochetCount + 2;

                    BOUNCEMAXXINGActivated = true;
                }
                else if (!Player.HasSynergy(Synergy.BOUNCEMAXXING) && BOUNCEMAXXINGActivated)
                {
                    gun.DefaultModule.projectiles[0].gameObject.GetComponent<PierceProjModifier>().penetration = ricochetCount;

                    BOUNCEMAXXINGActivated = false;
                }
            }

            base.Update();
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            /*
            if (projectile != null && projectile.Owner != null)
            {
                projectile.OnHitEnemy += HandleHitEnemy;
            }
            */
            //HelpfulMethods.PlayRandomSFX(projectile.gameObject, CrossbladeFiringSFXList);

            base.PostProcessProjectile(projectile);
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            HelpfulMethods.PlayRandomSFX(gun.gameObject, CrossbladeFiringSFXList);

            base.OnPostFired(player, gun);
        }

        /*
        private void HandleHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy != null && enemy.aiActor != null)
            {
                var ricochetModule = proj.GetComponent<RicochetProjectileModule>();
                if (proj.GetComponent<PierceProjModifier>() != null && proj.GetComponent<PierceProjModifier>().penetration > 0 && enemy != null && enemy.aiActor != null)
                {
                    //var dir = UnityEngine.Random.insideUnitCircle;
                    if (enemy.aiActor.ParentRoom != null && enemy.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All) != null)
                    {
                        var t = enemy.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).FindAll(x => x != null && x != enemy.aiActor && x.HasBeenEngaged && x.healthHaver != null && x.healthHaver.IsVulnerable);
                        if (t.Count > 0)
                        {
                            dir = BraveUtility.RandomElement(t.ToArray()).CenterPosition - proj.specRigidbody.UnitCenter;
                        }

                        AIActor closest = null;
                        float closestDistSq = ricochetRange * ricochetRange;
                        var t = enemy.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).FindAll(x => x != null && x != enemy.aiActor && x.HasBeenEngaged && x.healthHaver != null && x.healthHaver.IsVulnerable);

                        foreach (AIActor target in t)
                        {
                            if (proj.GetComponent<RicochetProjectileModule>().visited.Contains(target) || !target.IsNormalEnemy) 
                                continue;

                            float distSq = (target.CenterPosition - enemy.aiActor.CenterPosition).sqrMagnitude;
                            if (distSq < closestDistSq)
                            {
                                closest = target;
                                closestDistSq = distSq;
                            }
                        }

                        if (closest != null)
                        {
                            var dir = closest.CenterPosition - proj.specRigidbody.UnitCenter;
                            proj.SendInDirection(dir, false);
                            ricochetModule.visited.Add(enemy.aiActor);
                            //Plugin.Log($"Crossblade ricochet to {closest.GetActorName()}");
                        }
                        else
                        {
                            Plugin.Log($"Crossblade found no ricochet targets");
                            proj.ForceDestruction();
                        }
                    }
                    //proj.SendInDirection(dir, false);
                }
            }
        }
        */
    }
}
