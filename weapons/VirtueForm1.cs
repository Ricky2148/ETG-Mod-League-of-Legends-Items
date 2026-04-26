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
using UnityEngine;

namespace LOLItems.weapons
{
    internal class VirtueForm1 : AdvancedGunBehavior
    {
        public static string internalName = "VirtueForm1";
        public static int ID;
        public static string realName = "Virtue";

        private PlayerController currentOwner;

        private static int ammoStat = 250;
        private static float reloadDuration = 0f;
        private static float fireRateStat = 0.8f;
        private static int spreadAngle = 0;

        private Gun NextFormWeapon;
        private static GameObject AscensionIcon;

        public float DivineAscentExpTracker = 0f;
        //private int DivineAscentFormTracker = 0;
        /*private float[] DivineAscentThreshold =
        {
            500f,
            1000f
        };*/
        public float DivineAscentThreshold = 4000f;

        private static float projectileDamageStat = 8f;
        private static float projectileSpeedStat = 40f;
        private static float projectileRangeStat = 15f;
        private static float projectileForceStat = 8f;

        private static List<string> VirtueFiringSFXList = new List<string>
        {
            "virtue_atk_sfx1",
            "virtue_atk_sfx2",
            "virtue_atk_sfx3",
            "virtue_atk_sfx4",
        };

        private static List<string> VirtueAscensionSFXList = new List<string>
        {
            "virtue_ascension_sfx1",
            "virtue_ascension_sfx2",
            "virtue_ascension_sfx3",
        };

        public static void Add()
        {
            string FULLNAME = realName;
            string SPRITENAME = "virtue_form1";
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<VirtueForm1>();
            gun.SetShortDescription("\"Truth, guide my sword!\"");
            gun.SetLongDescription("Gain EXP per kill. Evolves after enough EXP.\n\n" +
                "Virtue. Definition: a quality considered morally good.\n\nA blade of celestial creation that are capable of burning evil. " +
                "The original wielder of this weapon was said to have tested whether one was virtuous by slashing at their neck. If they were truly virtuous, then the blade would cause them no harm. " +
                "\n\nShe was capable of igniting the blade with holy fire but it appears to require something deep within.\n");

            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);

            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.alternateShootAnimation, 12);
            //gun.SetAnimationFPS(gun.reloadAnimation, 10);

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
            gun.DefaultModule.numberOfShotsInClip = 9999;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.TwoHanded;

            gun.carryPixelOffset += new IntVector2(15, -1); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(-16, -15); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(-12, 16); //offset when aiming up

            gun.barrelOffset.transform.localPosition += new Vector3(56 / 16f, 52 / 16f);

            gun.gunScreenShake.magnitude = 0f;

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles[0] = projectile;

            projectile.hitEffects.HasProjectileDeathVFX = true;
            projectile.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical.effects[0].effects[0].effect;
            projectile.hitEffects.deathAny = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).DefaultModule.projectiles[0].hitEffects.deathAny;
            projectile.hitEffects.deathEnemy = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).DefaultModule.projectiles[0].hitEffects.deathEnemy;
            projectile.hitEffects.enemy = (PickupObjectDatabase.GetById((int)Items.MicrotransactionGun) as Gun).DefaultModule.projectiles[0].hitEffects.enemy;
            projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

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

            projectile.SetProjectileSpriteRight("virtue_green_projectile_straight_001", 19, 7, true, tk2dBaseSprite.Anchor.MiddleCenter, 17, 6);

            EasyTrailBullet projTrail = projectile.gameObject.AddComponent<EasyTrailBullet>();
            projTrail.TrailPos = projectile.transform.position;
            projTrail.StartWidth = 0.25f;
            projTrail.EndWidth = 0f;
            projTrail.LifeTime = 0.15f; //How long the trail lingers
            // BaseColor sets an overall color for the trail. Start and End Colors are subtractive to it. 
            projTrail.BaseColor = ExtendedColours.lime; //Set to white if you don't want to interfere with Start/End Colors.
            projTrail.StartColor = Color.green;
            projTrail.EndColor = Color.white; //Custom Orange example using r/g/b values.

            // A list of filenames in the sprites/ProjectileCollection folder for each frame in the animation, extension not required.
            /*List<string> projectileSpriteNames = new List<string>
            {
                "virtue_green_projectile_001",
                "virtue_green_projectile_002",
                "virtue_green_projectile_003",
                "virtue_green_projectile_004",
            };
            // Animation FPS.
            int projectileFPS = 8;
            // Visual sprite size for each frame.  Sprite images will stretch to match these sizes.
            List<IntVector2> projectileSizes = new List<IntVector2>
            {
                new IntVector2(20, 6), //1
                new IntVector2(20, 6), //2
                new IntVector2(20, 6), //3
                new IntVector2(20, 6), //4
            };
            // Whether each frame should have a bit of glow.
            List<bool> projectileLighteneds = new List<bool>
            {
                true,
                true,
                true,
                true,
            };
            // Sprite anchor list.
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>
            {
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
            };
            // Unknown, doesn't appear to matter so leave as false. 
            List<bool> projectilefixesScales = new List<bool>
            {
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
            };
            // Override the projectile hitboxes on each frame.  Either null (same as visuals) or slightly smaller than the visuals is most common.
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?>
            {
                new IntVector2(16, 5), //1
                new IntVector2(16, 5), //2
                new IntVector2(16, 5), //3
                new IntVector2(16, 5), //4
            };
            // Manually assign the projectile offsets.
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?>
            {
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
            };
            // Your animations wrap mode. If you just want it to do a looping animation, leave it as Loop. Only useful for when adding multiple differing animations.
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            // Optionally, you can give your animations a clip name. Only useful for when adding multiple differing animations.
            //string projectileClipName = "projectileName"; 
            // Optionally, you can assign an animation as the default one that plays.  Only useful for when adding multiple differing animations.  If left as null then it will use the most recently added animation.
            //string projectileDefaultClipName = "projectileName"; 

            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);
            */

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("virtue_form1_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/virtue_form1_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/virtue_form1_ammo_empty");

            gun.gameObject.Attach<VirtueForm1AmmoDisplay>();

            gun.shellsToLaunchOnFire = 0;
            gun.shellsToLaunchOnReload = 0;
            gun.clipsToLaunchOnReload = 0;

            gun.quality = PickupObject.ItemQuality.C;
            ETGMod.Databases.Items.Add(gun, false, "ANY");
            gun.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = gun.PickupObjectId;

            GunTools.TrimGunSprites(gun);

            AscensionIcon = SpriteBuilder.SpriteFromResource("LOLItems/Resources/one_off_sprites/virtue_ascension_icons/export_02", AscensionIcon);
            FakePrefab.MarkAsFakePrefab(AscensionIcon);
            AscensionIcon.SetActive(false);

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:virtueform1",
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

        public override void OnInitializedWithOwner(GameActor actor)
        {
            currentOwner = actor as PlayerController;
            currentOwner.OnAnyEnemyReceivedDamage += KillEnemyCount;

            Plugin.Log($"picked up {realName}");

            base.OnInitializedWithOwner(actor);
        }

        public override void OnDropped()
        {
            currentOwner.OnAnyEnemyReceivedDamage -= KillEnemyCount;

            Plugin.Log($"dropped {realName}");

            base.OnDropped();
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            BraveUtility.Swap(ref this.gun.shootAnimation, ref this.gun.alternateShootAnimation);

            HelpfulMethods.PlayRandomSFX(gun.gameObject, VirtueFiringSFXList);

            base.OnPostFired(player, gun);
        }

        private void KillEnemyCount(float damage, bool fatal, HealthHaver enemyHealth)
        {
            if (enemyHealth && fatal && enemyHealth.aiActor != null)
            {
                //Plugin.Log($"enemyHealth: {enemyHealth}");
                float expToGain = 0;
                if (enemyHealth.IsBoss || enemyHealth.IsSubboss)
                {
                    //Plugin.Log("is bosss");
                    expToGain = (enemyHealth.aiActor.healthHaver.GetMaxHealth() * 0.25f);
                }
                else
                {
                    expToGain = enemyHealth.aiActor.healthHaver.GetMaxHealth();
                }

                if (Player.HasSynergy(Synergy.EXP_SHARE_FORM_1))
                {
                    expToGain *= 2;
                }

                DivineAscentExpTracker += expToGain;
                //Plugin.Log($"Gained {expToGain} Divine Ascent EXP! Current EXP: {DivineAscentExpTracker}/{DivineAscentThreshold}");
                /*if (DivineAscentExpTracker >= DivineAscentThreshold[DivineAscentFormTracker] && DivineAscentFormTracker < DivineAscentThreshold.Length)
                {
                    TriggerAscent();
                    DivineAscentExpTracker = 0f;
                }*/
                if (DivineAscentExpTracker >= DivineAscentThreshold)
                {
                    TriggerAscent();
                }
            }
        }

        private void TriggerAscent()
        {
            //PlayerController player = this.Owner as PlayerController;
            //DivineAscentFormTracker++;

            currentOwner.OnAnyEnemyReceivedDamage -= KillEnemyCount;
            DivineAscentExpTracker = 0f;

            if (NextFormWeapon == null)
            {
                NextFormWeapon = PickupObjectDatabase.GetById((int)VirtueForm2.ID) as Gun;
            }

            if (NextFormWeapon != null || currentOwner != null)
            {
                HelpfulMethods.CustomNotification("Taste of Celestial Justice", "The unrighteous will burn!", AscensionIcon.GetComponent<tk2dBaseSprite>(), UINotificationController.NotificationColor.GOLD);
                //Plugin.Log("Upgrading Virtue1 to Virtue2");
                HelpfulMethods.PlayRandomSFX(this.gun.gameObject, VirtueAscensionSFXList);
                currentOwner.inventory.RemoveGunFromInventory(this.gun);
                //currentOwner.inventory.AddGunToInventory(NextFormWeapon, true);
                currentOwner.GiveItem("LOLItems:virtueform2");
            }

            /*
            switch (DivineAscentFormTracker)
            {
                case 1:
                    Plugin.Log($"Virtue has ascended to its 1st form! {DivineAscentFormTracker}");
                    
                    gun.CurrentStrengthTier = 1;

                    Plugin.Log($"current strength tier: {this.gun.CurrentStrengthTier}");

                    break;
                case 2:
                    Plugin.Log($"Virtue has ascended to its 2nd form! {DivineAscentFormTracker}");

                    currentOwner.OnAnyEnemyReceivedDamage -= KillEnemyCount;

                    break;
                default:
                    Plugin.Log("Shouldn't be here...");
                    break;
            }
            */
        }
    }

    internal class VirtueForm1AmmoDisplay : CustomAmmoDisplay
    {
        private Gun _gun;
        private VirtueForm1 _virtueform1;
        private PlayerController _owner;

        private void Start()
        {
            this._gun = base.GetComponent<Gun>();
            this._virtueform1 = this._gun.GetComponent<VirtueForm1>();
            this._owner = this._gun.CurrentOwner as PlayerController;
        }

        public override bool DoCustomAmmoDisplay(GameUIAmmoController uic)
        {
            if (!this._owner || !this._virtueform1)
            {
                return false;
            }
            uic.GunAmmoCountLabel.Text = $"[color #35D3AC]EXP: {this._virtueform1.DivineAscentExpTracker.RoundToNearest(0)} / {this._virtueform1.DivineAscentThreshold.RoundToNearest(0)}[/color]\n{this._owner.VanillaAmmoDisplay()}";
            return true;
        }
    }
}
