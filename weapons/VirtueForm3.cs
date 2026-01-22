using Alexandria.BreakableAPI;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.SoundAPI;
using BepInEx;
using Dungeonator;
using Gungeon;
using LOLItems.custom_class_data;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LOLItems.weapons
{
    internal class VirtueForm3 : AdvancedGunBehavior
    {
        public static string internalName = "VirtueForm3";
        public static int ID;
        public static string realName = "Virtue";

        private PlayerController currentOwner;

        private static int ammoStat = 750;
        private static float reloadDuration = 0f;
        private static float fireRateStat = 0.35f;
        private static int spreadAngle = 0;

        private static float zealSpeedInc = 1.1f;

        public GameObject prefabToAttachToPlayer;
        private GameObject instanceWings;
        private tk2dSprite instanceWingsSprite;

        private bool m_isCurrentlyActive;
        private bool m_hiddenForAll;

        private static float projectileDamageStat = 15f;
        private static float projectileSpeedStat = 65f;
        private static float projectileRangeStat = 20f;
        private static float projectileForceStat = 8f;

        private static List<string> VirtueFiringSFXList = new List<string>
        {
            "virtue_atk_sfx1",
            "virtue_atk_sfx2",
            "virtue_atk_sfx3",
            "virtue_atk_sfx4",
        };

        private static List<string> VirtueWaveSFXList = new List<string>
        {
            "virtue_wave_sfx1",
            "virtue_wave_sfx2",
            "virtue_wave_sfx3",
            "virtue_wave_sfx4",
            "virtue_wave_sfx5",
        };

        public static void Add()
        {
            string FULLNAME = realName;
            string SPRITENAME = "virtue_form3";
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<VirtueForm3>();
            gun.SetShortDescription("\"Drown in holy fire!\"");
            gun.SetLongDescription("Permanently have max zealous stacks. Fires waves every attack. Gains increased movespeed. Gain wings.\n\n" +
                "Virtue. Definition: a quality considered morally good.\n\nA blade of celestial creation that are capable of burning evil. " +
                "The original wielder of this weapon was said to have tested whether one was virtuous by slashing at their neck. If they were truly virtuous, then the blade would cause them no harm. " +
                "\n\nI have absolved myself of all mortal sin in order to serve judgement to the unworthy. " +
                "\n\n\"To be human is to be imperfect, but I am not human.\"\n");

            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);

            gun.SetAnimationFPS(gun.shootAnimation, 30);
            gun.SetAnimationFPS(gun.alternateShootAnimation, 30);

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun, true, false);
            gun.muzzleFlashEffects = null; //(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).muzzleFlashEffects;

            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null);
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null);

            gun.DefaultModule.angleVariance = spreadAngle;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.gunClass = GunClass.RIFLE;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat;
            gun.DefaultModule.numberOfShotsInClip = ammoStat;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.TwoHanded;

            gun.carryPixelOffset += new IntVector2(13, 2); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(-11, -16); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(-13, 11); //offset when aiming up

            gun.barrelOffset.transform.localPosition += new Vector3(56 / 16f, 52 / 16f);

            gun.gunScreenShake.magnitude = 0f;

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles[0] = projectile;

            projectile.hitEffects.HasProjectileDeathVFX = true;
            projectile.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById((int)Items.Bow) as Gun).DefaultModule.chargeProjectiles[1].Projectile.hitEffects.enemy.effects[0].effects[0].effect;
            projectile.hitEffects.deathAny = (PickupObjectDatabase.GetById((int)Items.Bow) as Gun).DefaultModule.chargeProjectiles[1].Projectile.hitEffects.enemy;
            projectile.hitEffects.deathEnemy = (PickupObjectDatabase.GetById((int)Items.Bow) as Gun).DefaultModule.chargeProjectiles[1].Projectile.hitEffects.enemy;
            projectile.hitEffects.enemy = (PickupObjectDatabase.GetById((int)Items.Bow) as Gun).DefaultModule.chargeProjectiles[1].Projectile.hitEffects.enemy;
            projectile.hitEffects.tileMapHorizontal = null;
            projectile.hitEffects.tileMapVertical = null;

            projectile.objectImpactEventName = "virtue2";
            projectile.enemyImpactEventName = "virtue5";

            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            projectile.baseData.damage = projectileDamageStat;
            projectile.baseData.speed = projectileSpeedStat;
            projectile.baseData.range = projectileRangeStat;
            projectile.baseData.force = projectileForceStat;
            projectile.transform.parent = gun.barrelOffset;
            projectile.shouldRotate = true;

            projectile.SetProjectileSpriteRight("virtue_yellow_small_projectile_001", 19, 7, true, tk2dBaseSprite.Anchor.MiddleCenter, 17, 6);

            EasyTrailBullet projTrail = projectile.gameObject.AddComponent<EasyTrailBullet>();
            projTrail.TrailPos = projectile.transform.position;
            projTrail.StartWidth = 0.25f;
            projTrail.EndWidth = 0f;
            projTrail.LifeTime = 0.15f; //How long the trail lingers
            // BaseColor sets an overall color for the trail. Start and End Colors are subtractive to it. 
            projTrail.BaseColor = ExtendedColours.paleYellow; //Set to white if you don't want to interfere with Start/End Colors.
            projTrail.StartColor = ExtendedColours.paleYellow;
            projTrail.EndColor = Color.white; //Custom Orange example using r/g/b values.

            /*List<string> projectileSpriteNames = new List<string>
            {
                "virtue_yellow_projectile_001",
                "virtue_yellow_projectile_002",
                "virtue_yellow_projectile_003",
                "virtue_yellow_projectile_004",
            };
            int projectileFPS = 8;
            List<IntVector2> projectileSizes = new List<IntVector2>
            {
                new IntVector2(20, 6), //1
                new IntVector2(20, 6), //2
                new IntVector2(20, 6), //3
                new IntVector2(20, 6), //4
            };
            List<bool> projectileLighteneds = new List<bool>
            {
                true,
                true,
                true,
                true,
            };
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>
            {
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
            };
            List<bool> projectileAnchorsChangeColiders = new List<bool>
            {
                false,
                false,
                false,
                false,
            };
            List<bool> projectilefixesScales = new List<bool>
            {
                false,
                false,
                false,
                false,
            };
            List<Vector3?> projectileManualOffsets = new List<Vector3?>
            {
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
            };
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?>
            {
                new IntVector2(16, 5), //1
                new IntVector2(16, 5), //2
                new IntVector2(16, 5), //3
                new IntVector2(16, 5), //4
            };
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?>
            {
                null,
                null,
                null,
                null,
            };
            List<Projectile> projectileOverrideProjectilesToCopyFrom = new List<Projectile>
            {
                null,
                null,
                null,
                null,
            };
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);
            */
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("virtue_form3_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/virtue_form3_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/virtue_form3_ammo_empty");

            gun.gameObject.Attach<VirtueForm3AmmoDisplay>();

            gun.Volley.ModulesAreTiers = true;
            ProjectileModule mod1 = gun.DefaultModule;
            ProjectileModule mod2 = ProjectileModule.CreateClone(mod1, false);
            gun.Volley.projectiles.Add(mod2);

            Projectile wave = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            //gun.DefaultModule.projectiles.Add(wave);
            gun.Volley.projectiles[1].projectiles[0] = wave;

            wave.hitEffects.HasProjectileDeathVFX = true;
            wave.hitEffects.deathAny = null;
            wave.hitEffects.deathEnemy = null;
            wave.hitEffects.enemy = null;
            wave.hitEffects.tileMapHorizontal = null;
            wave.hitEffects.tileMapVertical = null;

            //wave.objectImpactEventName = ;
            //wave.enemyImpactEventName = ;

            wave.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(wave.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(wave);

            wave.baseData.damage = projectileDamageStat * 0.5f;
            wave.baseData.speed = projectileSpeedStat;
            wave.baseData.range = projectileRangeStat;
            wave.baseData.force = 0;
            wave.transform.parent = gun.barrelOffset;
            wave.shouldRotate = true;

            wave.PenetratesInternalWalls = true;
            wave.pierceMinorBreakables = true;
            wave.damagesWalls = false;

            TrueWallPiercingRounds wallPiercingRounds = wave.gameObject.GetOrAddComponent<TrueWallPiercingRounds>();

            //wave.BulletScriptSettings.surviveRigidbodyCollisions = true;
            //wave.BulletScriptSettings.surviveTileCollisions = true;

            PierceProjModifier pierce = wave.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = 999;
            pierce.penetratesBreakables = true;

            //wave.sprite.color = Color.cyan;
            //wave.AdditionalScaleMultiplier = 5f;

            wave.SetProjectileSpriteRight("virtue_yellow_medium_projectile_001", 14, 64, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 52);

            var sprite = wave.sprite.gameObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                //Plugin.Log("sprite not null");
                sprite.usesOverrideMaterial = true;
                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.8f);
            }
            else
            {
                //Plugin.Log("sprite is null");
            }

            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 0;
            gun.clipsToLaunchOnReload = 0;

            gun.ShouldBeExcludedFromShops = true;

            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;

            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.MovementSpeed, zealSpeedInc, StatModifier.ModifyMethod.MULTIPLICATIVE);

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:virtueform3",
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "macho_brace",
                "scouter",
                "broccoli",
                "life_orb"
            };
            AdvancedSynergyEntry ase = CustomSynergies.Add("Exp. Share", mandatoryConsoleIDs, optionalConsoleIDs, true);*/
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            BraveUtility.Swap(ref this.gun.shootAnimation, ref this.gun.alternateShootAnimation);

            //HelpfulMethods.PlayRandomSFX(gun.gameObject, VirtueFiringSFXList);

            base.OnPostFired(player, gun);
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            if (projectile != null && projectile.Owner != null)
            {
                HelpfulMethods.PlayRandomSFX(this.gun.gameObject, VirtueWaveSFXList);

                currentOwner.StartCoroutine(FireWaveDelayed(projectile));
            }

            base.PostProcessProjectile(projectile);
        }

        private System.Collections.IEnumerator FireWaveDelayed(Projectile projectile)
        {
            if (projectile == null || projectile.Owner == null || !projectile)
            {
                Plugin.Log("fail 1");
                yield break;
            }

            yield return new WaitForSeconds(0.001f);

            Vector2 direction = projectile.LastVelocity.normalized;

            //Projectile wave = UnityEngine.Object.Instantiate(projectile.gameObject).GetComponent<Projectile>();

            Projectile wave = UnityEngine.Object.Instantiate(this.gun.Volley.projectiles[1].projectiles[0].gameObject).GetComponent<Projectile>();

            /*wave.baseData.damage = projectile.baseData.damage * 0.5f;
            wave.baseData.force = 0;

            wave.sprite.color = Color.cyan;
            wave.AdditionalScaleMultiplier = 5f;*/

            wave.Owner = projectile.Owner;
            wave.Shooter = projectile.Shooter;

            wave.transform.position = currentOwner.CurrentGun.barrelOffset.position + new Vector3(direction.x, direction.y);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            wave.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            wave.SendInDirection(direction, true, true);
        }

        public override void OnInitializedWithOwner(GameActor actor)
        {
            currentOwner = actor as PlayerController;

            currentOwner.OnIsRolling += OnRollFrame;

            TriggerFlight();

            Plugin.Log($"picked up {realName}");

            base.OnInitializedWithOwner(actor);
        }

        public override void OnDropped()
        {
            currentOwner.OnIsRolling -= OnRollFrame;

            StopFlight();

            currentOwner = null;

            Plugin.Log($"dropped {realName}");

            base.OnDropped();
        }

        private void TriggerFlight()
        {
            Plugin.Log("trigger flight");
            if (!Dungeon.IsGenerating && currentOwner && currentOwner.sprite && currentOwner.sprite.GetComponent<tk2dSpriteAttachPoint>())
            {
                //Plugin.Log("flight work");

                m_isCurrentlyActive = true;
                currentOwner.AdditionalCanDodgeRollWhileFlying.SetOverride("Feather", true);
                currentOwner.SetIsFlying(value: true, "DivineAscent");

                var waxWings = PickupObjectDatabase.GetById((int)Items.WaxWings).gameObject.GetComponent<WingsItem>();

                instanceWings = currentOwner.RegisterAttachedObject(waxWings.prefabToAttachToPlayer, "jetpack", 0.1f);
                instanceWingsSprite = instanceWings.GetComponent<tk2dSprite>();

                if (!instanceWingsSprite)
                {
                    instanceWingsSprite = instanceWings.GetComponentInChildren<tk2dSprite>();
                }

                //instanceWingsSprite.transform.localPosition = waxWings.GetLocalOffsetForCharacter(currentOwner.characterIdentity).ToVector3ZUp();
            }
        }

        private void StopFlight()
        {
            //Plugin.Log("Stop flight");

            m_isCurrentlyActive = false;
            currentOwner.AdditionalCanDodgeRollWhileFlying.SetOverride("Feather", false);

            //Plugin.Log("Stop flight 2");

            currentOwner.SetIsFlying(value: false, "DivineAscent");
            currentOwner.DeregisterAttachedObject(instanceWings);
            instanceWingsSprite = null;

            //Plugin.Log("Stop flight 3");
        }

        protected override void Update()
        {
            base.Update();
            if (!(currentOwner != null) || !this.PickedUp || !(this.Owner != null))
            {
                return;
            }
            if (m_isCurrentlyActive)
            {
                if (currentOwner.IsFalling)
                {
                    m_hiddenForAll = true;
                    instanceWingsSprite.renderer.enabled = false;
                }
                else
                {
                    if (m_hiddenForAll)
                    {
                        m_hiddenForAll = false;
                        instanceWingsSprite.renderer.enabled = true;
                    }
                    string text = "white_wing" + currentOwner.GetBaseAnimationSuffix(false);
                    if (!instanceWingsSprite.spriteAnimator.IsPlaying(text) && (!currentOwner.IsDodgeRolling))
                    {
                        instanceWingsSprite.spriteAnimator.Play(text);
                    }
                    if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
                    {
                        StopFlight();
                    }
                }
            }
            else if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
            {
                //Plugin.Log("update trigger flight");
                TriggerFlight();
            }
        }

        private void OnRollFrame(PlayerController player)
        {
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
            {
                return;
            }
        }
    }

    internal class VirtueForm3AmmoDisplay : CustomAmmoDisplay
    {
        private Gun _gun;
        private VirtueForm3 _virtueform3;
        private PlayerController _owner;

        private void Start()
        {
            this._gun = base.GetComponent<Gun>();
            this._virtueform3 = this._gun.GetComponent<VirtueForm3>();
            this._owner = this._gun.CurrentOwner as PlayerController;
        }

        public override bool DoCustomAmmoDisplay(GameUIAmmoController uic)
        {
            if (!this._owner || !this._virtueform3)
            {
                return false;
            }
            uic.GunAmmoCountLabel.Text = $"[color #FFD700]TRANSCENDENT[/color]\n{this._owner.VanillaAmmoDisplay()}";
            return true;
        }
    }
}
