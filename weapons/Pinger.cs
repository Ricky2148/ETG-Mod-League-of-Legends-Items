using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using Gungeon;
using LOLItems.custom_class_data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.weapons
{
    public class EnemyTiltedTracker
    {
        public Coroutine timerCoroutine;
        public GameObject activeVFXObject;

        public EnemyTiltedTracker (Coroutine corou, GameObject obj)
        {
            timerCoroutine = corou;
            activeVFXObject = obj;
        }
    }

    internal class Pinger : AdvancedGunBehavior
    {
        public static string internalName = "Pinger_LOLItems"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Pinger"; //The name that shows up in the Ammonomicon and the mod console.

        private static int ammoStat = 200;
        private static float reloadDuration = 0f;
        private static float fireRateStat = 0.4f;
        private static int spreadAngle = 0;

        private static float projectileDamageStat = 12f;
        private static float projectileSpeedStat = 35f;
        private static float projectileRangeStat = 20f;
        private static float projectileForceStat = 15f;

        private static float TiltedDuration = 10f;
        private static Color flatColorOverride = ExtendedColours.maroon; //new Color(0.5f, 0f, 0f, 0.75f);
        public GameObject OverheadVFX = (PickupObjectDatabase.GetById((int)Items.EnragingPhoto) as RagePassiveItem).OverheadVFX;
        //private GameObject instanceVFX;

        private static AIActorBuffEffect TiltedEffect = new AIActorBuffEffect
        {
            effectIdentifier = "pinger_tilted_effect",
            SpeedMultiplier = 2.0f,
            //CooldownMultiplier = 0.1f,
            HealthMultiplier = 0.5f,
            KeepHealthPercentage = true,
        };

        private Dictionary<AIActor, EnemyTiltedTracker> enemyTiltedTrackerList = new Dictionary<AIActor, EnemyTiltedTracker>();

        private static List<string> PingerFiringSFXList = new List<string>()
        {
            
        };

        public static void Add()
        {
            string FULLNAME = realName;
            string SPRITENAME = "pinger";
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName);
            gun.gameObject.AddComponent<Pinger>();
            gun.SetShortDescription("idk");
            gun.SetLongDescription("idk");

            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);

            gun.SetAnimationFPS(gun.shootAnimation, 15);

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun, true, false);

            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}";
            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null);
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null);

            gun.DefaultModule.angleVariance = spreadAngle;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.gunClass = GunClass.SILLY;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat;
            gun.DefaultModule.numberOfShotsInClip = ammoStat;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.TwoHanded;

            gun.carryPixelOffset += new IntVector2(0, 0); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(0, 0); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(0, 0); //offset when aiming up

            gun.barrelOffset.transform.localPosition += new Vector3(0 / 16f, 0 / 16f);
            gun.gunScreenShake.magnitude = 0f;

            gun.DefaultModule.projectiles.Clear();

            //custom hit effect setup
            VFXPool pool2 = new VFXPool();
            pool2.type = VFXPoolType.Single;

            VFXComplex customHitEffect = HelpfulMethods.CreateVFXComplex("pinger_hiteffect",
                new List<string>()
                {
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_001",
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_002",
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_003",
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_004",
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_005",
                    "LOLItems/Resources/hit_effects/pinger/pinger_hit_006",
                },
                18, //FPS
                new IntVector2(16, 16), //Dimensions
                tk2dBaseSprite.Anchor.MiddleCenter, //Anchor
                false, //Uses a Z height off the ground
                0, //The Z height, if used
                false,
                VFXAlignment.Fixed
                );

            pool2.effects = new VFXComplex[]
            {
                customHitEffect,
            };

            #region Projectile Setup
            //proj 1: all in
            Projectile projectile1 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile1);

            projectile1.hitEffects.HasProjectileDeathVFX = true;
            projectile1.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile1.hitEffects.deathAny = pool2;
            projectile1.hitEffects.deathEnemy = null;
            projectile1.hitEffects.enemy = null;
            projectile1.hitEffects.tileMapHorizontal = pool2;
            projectile1.hitEffects.tileMapVertical = pool2;
            
            
            projectile1.objectImpactEventName = "plasmarifle";
            projectile1.enemyImpactEventName = "plasmarifle"; 
            

            projectile1.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile1.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile1);

            projectile1.baseData.damage = projectileDamageStat;
            projectile1.baseData.speed = projectileSpeedStat;
            projectile1.baseData.range = projectileRangeStat;
            projectile1.baseData.force = projectileForceStat; //Knockback strength
            projectile1.transform.parent = gun.barrelOffset;

            projectile1.SetProjectileSpriteRight("pinger_projectile_all_in_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 2: assist me
            Projectile projectile2 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile2);

            projectile2.hitEffects.HasProjectileDeathVFX = true;
            projectile2.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile2.hitEffects.deathAny = pool2;
            projectile2.hitEffects.deathEnemy = null;
            projectile2.hitEffects.enemy = null;
            projectile2.hitEffects.tileMapHorizontal = pool2;
            projectile2.hitEffects.tileMapVertical = pool2;


            projectile2.objectImpactEventName = "plasmarifle";
            projectile2.enemyImpactEventName = "plasmarifle";



            projectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile2);

            projectile2.baseData.damage = projectileDamageStat;
            projectile2.baseData.speed = projectileSpeedStat;
            projectile2.baseData.range = projectileRangeStat;
            projectile2.baseData.force = projectileForceStat; //Knockback strength
            projectile2.transform.parent = gun.barrelOffset;

            projectile2.SetProjectileSpriteRight("pinger_projectile_assist_me_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 3: bait
            Projectile projectile3 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile3);

            projectile3.hitEffects.HasProjectileDeathVFX = true;
            projectile3.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile3.hitEffects.deathAny = pool2;
            projectile3.hitEffects.deathEnemy = null;
            projectile3.hitEffects.enemy = null;
            projectile3.hitEffects.tileMapHorizontal = pool2;
            projectile3.hitEffects.tileMapVertical = pool2;


            projectile3.objectImpactEventName = "plasmarifle";
            projectile3.enemyImpactEventName = "plasmarifle";


            projectile3.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile3.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile3);

            projectile3.baseData.damage = projectileDamageStat;
            projectile3.baseData.speed = projectileSpeedStat;
            projectile3.baseData.range = projectileRangeStat;
            projectile3.baseData.force = projectileForceStat; //Knockback strength
            projectile3.transform.parent = gun.barrelOffset;

            projectile3.SetProjectileSpriteRight("pinger_projectile_bait_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 4: caution
            Projectile projectile4 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile4);

            projectile4.hitEffects.HasProjectileDeathVFX = true;
            projectile4.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile4.hitEffects.deathAny = pool2;
            projectile4.hitEffects.deathEnemy = null;
            projectile4.hitEffects.enemy = null;
            projectile4.hitEffects.tileMapHorizontal = pool2;
            projectile4.hitEffects.tileMapVertical = pool2;


            projectile4.objectImpactEventName = "plasmarifle";
            projectile4.enemyImpactEventName = "plasmarifle";


            projectile4.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile4.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile4);

            projectile4.baseData.damage = projectileDamageStat;
            projectile4.baseData.speed = projectileSpeedStat;
            projectile4.baseData.range = projectileRangeStat;
            projectile4.baseData.force = projectileForceStat; //Knockback strength
            projectile4.transform.parent = gun.barrelOffset;

            projectile4.SetProjectileSpriteRight("pinger_projectile_caution_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 5: defend
            Projectile projectile5 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile5);

            projectile5.hitEffects.HasProjectileDeathVFX = true;
            projectile5.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile5.hitEffects.deathAny = pool2;
            projectile5.hitEffects.deathEnemy = null;
            projectile5.hitEffects.enemy = null;
            projectile5.hitEffects.tileMapHorizontal = pool2;
            projectile5.hitEffects.tileMapVertical = pool2;


            projectile5.objectImpactEventName = "plasmarifle";
            projectile5.enemyImpactEventName = "plasmarifle";


            projectile5.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile5.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile5);

            projectile5.baseData.damage = projectileDamageStat;
            projectile5.baseData.speed = projectileSpeedStat;
            projectile5.baseData.range = projectileRangeStat;
            projectile5.baseData.force = projectileForceStat; //Knockback strength
            projectile5.transform.parent = gun.barrelOffset;

            projectile5.SetProjectileSpriteRight("pinger_projectile_defend_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 6: enemy missing
            Projectile projectile6 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile6);

            projectile6.hitEffects.HasProjectileDeathVFX = true;
            projectile6.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile6.hitEffects.deathAny = pool2;
            projectile6.hitEffects.deathEnemy = null;
            projectile6.hitEffects.enemy = null;
            projectile6.hitEffects.tileMapHorizontal = pool2;
            projectile6.hitEffects.tileMapVertical = pool2;


            projectile6.objectImpactEventName = "plasmarifle";
            projectile6.enemyImpactEventName = "plasmarifle";


            projectile6.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile6.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile6);

            projectile6.baseData.damage = projectileDamageStat;
            projectile6.baseData.speed = projectileSpeedStat;
            projectile6.baseData.range = projectileRangeStat;
            projectile6.baseData.force = projectileForceStat; //Knockback strength
            projectile6.transform.parent = gun.barrelOffset;

            projectile6.SetProjectileSpriteRight("pinger_projectile_enemy_missing_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 7: enemy vision
            Projectile projectile7 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile7);

            projectile7.hitEffects.HasProjectileDeathVFX = true;
            projectile7.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile7.hitEffects.deathAny = pool2;
            projectile7.hitEffects.deathEnemy = null;
            projectile7.hitEffects.enemy = null;
            projectile7.hitEffects.tileMapHorizontal = pool2;
            projectile7.hitEffects.tileMapVertical = pool2;


            projectile7.objectImpactEventName = "plasmarifle";
            projectile7.enemyImpactEventName = "plasmarifle";


            projectile7.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile7.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile7);

            projectile7.baseData.damage = projectileDamageStat;
            projectile7.baseData.speed = projectileSpeedStat;
            projectile7.baseData.range = projectileRangeStat;
            projectile7.baseData.force = projectileForceStat; //Knockback strength
            projectile7.transform.parent = gun.barrelOffset;

            projectile7.SetProjectileSpriteRight("pinger_projectile_enemy_vision_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 8: generic
            Projectile projectile8 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile8);

            projectile8.hitEffects.HasProjectileDeathVFX = true;
            projectile8.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile8.hitEffects.deathAny = pool2;
            projectile8.hitEffects.deathEnemy = null;
            projectile8.hitEffects.enemy = null;
            projectile8.hitEffects.tileMapHorizontal = pool2;
            projectile8.hitEffects.tileMapVertical = pool2;


            projectile8.objectImpactEventName = "plasmarifle";
            projectile8.enemyImpactEventName = "plasmarifle";


            projectile8.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile8.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile8);

            projectile8.baseData.damage = projectileDamageStat;
            projectile8.baseData.speed = projectileSpeedStat;
            projectile8.baseData.range = projectileRangeStat;
            projectile8.baseData.force = projectileForceStat; //Knockback strength
            projectile8.transform.parent = gun.barrelOffset;

            projectile8.SetProjectileSpriteRight("pinger_projectile_generic_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 9: need vision
            Projectile projectile9 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile9);

            projectile9.hitEffects.HasProjectileDeathVFX = true;
            projectile9.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile9.hitEffects.deathAny = pool2;
            projectile9.hitEffects.deathEnemy = null;
            projectile9.hitEffects.enemy = null;
            projectile9.hitEffects.tileMapHorizontal = pool2;
            projectile9.hitEffects.tileMapVertical = pool2;


            projectile9.objectImpactEventName = "plasmarifle";
            projectile9.enemyImpactEventName = "plasmarifle";


            projectile9.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile9.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile9);

            projectile9.baseData.damage = projectileDamageStat;
            projectile9.baseData.speed = projectileSpeedStat;
            projectile9.baseData.range = projectileRangeStat;
            projectile9.baseData.force = projectileForceStat; //Knockback strength
            projectile9.transform.parent = gun.barrelOffset;

            projectile9.SetProjectileSpriteRight("pinger_projectile_need_vision_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 10: on my way
            Projectile projectile10 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile10);

            projectile10.hitEffects.HasProjectileDeathVFX = true;
            projectile10.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile10.hitEffects.deathAny = pool2;
            projectile10.hitEffects.deathEnemy = null;
            projectile10.hitEffects.enemy = null;
            projectile10.hitEffects.tileMapHorizontal = pool2;
            projectile10.hitEffects.tileMapVertical = pool2;


            projectile10.objectImpactEventName = "plasmarifle";
            projectile10.enemyImpactEventName = "plasmarifle";


            projectile10.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile10.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile10);

            projectile10.baseData.damage = projectileDamageStat;
            projectile10.baseData.speed = projectileSpeedStat;
            projectile10.baseData.range = projectileRangeStat;
            projectile10.baseData.force = projectileForceStat; //Knockback strength
            projectile10.transform.parent = gun.barrelOffset;

            projectile10.SetProjectileSpriteRight("pinger_projectile_on_my_way_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 11: push
            Projectile projectile11 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile11);

            projectile11.hitEffects.HasProjectileDeathVFX = true;
            projectile11.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile11.hitEffects.deathAny = pool2;
            projectile11.hitEffects.deathEnemy = null;
            projectile11.hitEffects.enemy = null;
            projectile11.hitEffects.tileMapHorizontal = pool2;
            projectile11.hitEffects.tileMapVertical = pool2;


            projectile11.objectImpactEventName = "plasmarifle";
            projectile11.enemyImpactEventName = "plasmarifle";


            projectile11.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile11.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile11);

            projectile11.baseData.damage = projectileDamageStat;
            projectile11.baseData.speed = projectileSpeedStat;
            projectile11.baseData.range = projectileRangeStat;
            projectile11.baseData.force = projectileForceStat; //Knockback strength
            projectile11.transform.parent = gun.barrelOffset;

            projectile11.SetProjectileSpriteRight("pinger_projectile_push_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 12: retreat
            Projectile projectile12 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile12);

            projectile12.hitEffects.HasProjectileDeathVFX = true;
            projectile12.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile12.hitEffects.deathAny = pool2;
            projectile12.hitEffects.deathEnemy = null;
            projectile12.hitEffects.enemy = null;
            projectile12.hitEffects.tileMapHorizontal = pool2;
            projectile12.hitEffects.tileMapVertical = pool2;


            projectile12.objectImpactEventName = "plasmarifle";
            projectile12.enemyImpactEventName = "plasmarifle";


            projectile12.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile12.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile12);

            projectile12.baseData.damage = projectileDamageStat;
            projectile12.baseData.speed = projectileSpeedStat;
            projectile12.baseData.range = projectileRangeStat;
            projectile12.baseData.force = projectileForceStat; //Knockback strength
            projectile12.transform.parent = gun.barrelOffset;

            projectile12.SetProjectileSpriteRight("pinger_projectile_retreat_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 13: target
            Projectile projectile13 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile13);

            projectile13.hitEffects.HasProjectileDeathVFX = true;
            projectile13.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile13.hitEffects.deathAny = pool2;
            projectile13.hitEffects.deathEnemy = null;
            projectile13.hitEffects.enemy = null;
            projectile13.hitEffects.tileMapHorizontal = pool2;
            projectile13.hitEffects.tileMapVertical = pool2;


            projectile13.objectImpactEventName = "plasmarifle";
            projectile13.enemyImpactEventName = "plasmarifle";


            projectile13.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile13.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile13);

            projectile13.baseData.damage = projectileDamageStat;
            projectile13.baseData.speed = projectileSpeedStat;
            projectile13.baseData.range = projectileRangeStat;
            projectile13.baseData.force = projectileForceStat; //Knockback strength
            projectile13.transform.parent = gun.barrelOffset;

            projectile13.SetProjectileSpriteRight("pinger_projectile_target_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            //proj 14: vision cleared
            Projectile projectile14 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            gun.DefaultModule.projectiles.Add(projectile14);

            projectile14.hitEffects.HasProjectileDeathVFX = true;
            projectile14.hitEffects.overrideMidairDeathVFX = pool2.effects[0].effects[0].effect;
            projectile14.hitEffects.deathAny = pool2;
            projectile14.hitEffects.deathEnemy = null;
            projectile14.hitEffects.enemy = null;
            projectile14.hitEffects.tileMapHorizontal = pool2;
            projectile14.hitEffects.tileMapVertical = pool2;


            projectile14.objectImpactEventName = "plasmarifle";
            projectile14.enemyImpactEventName = "plasmarifle";


            projectile14.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile14.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile14);

            projectile14.baseData.damage = projectileDamageStat;
            projectile14.baseData.speed = projectileSpeedStat;
            projectile14.baseData.range = projectileRangeStat;
            projectile14.baseData.force = projectileForceStat; //Knockback strength
            projectile14.transform.parent = gun.barrelOffset;

            projectile14.SetProjectileSpriteRight("pinger_projectile_vision_cleared_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions
            #endregion Projectile Setup



            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("pinger_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/pinger_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/pinger_ammo_empty");

            gun.shellCasing = null;
            gun.clipObject = null;

            //custom muzzle effect setup
            VFXPool pool = new VFXPool();
            pool.type = VFXPoolType.Single;

            VFXComplex muzzleFlash = HelpfulMethods.CreateVFXComplex("pinger_muzzleflash",
                new List<string>()
                {
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_001",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_002",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_003",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_004",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_005",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_006",
                    "LOLItems/Resources/muzzle_flashes/pinger/pinger_muzzleflare_007",
                },
                18, //FPS
                new IntVector2(41, 28), //Dimensions
                tk2dBaseSprite.Anchor.MiddleLeft,
                false, //Uses a Z height off the ground
                0, //The Z height, if used
                false,
                VFXAlignment.Fixed
                );

            pool.effects = new VFXComplex[]
            {
                muzzleFlash,
            };
            gun.muzzleFlashEffects = pool;

            gun.shellsToLaunchOnFire = 0; //Number of shells to eject when shooting.
            gun.shellsToLaunchOnReload = 0; //Number of shells to eject when reloading (revolvers for example).
            gun.clipsToLaunchOnReload = 0; //Number of clips to eject when reloading.
            gun.reloadClipLaunchFrame = 0;

            gun.quality = PickupObject.ItemQuality.D;
            ETGMod.Databases.Items.Add(gun, false, "ANY");  //Adds your gun to the databse.
            ID = gun.PickupObjectId;
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            HelpfulMethods.PlayRandomSFX(gun.gameObject, PingerFiringSFXList);

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
                }
                else if (enemy.GetComponentInParent<AIActor>() != null)
                {
                    firstEnemy = enemy.GetComponentInParent<AIActor>();
                }
                else
                {
                    return;
                }
                if (enemy.healthHaver != null)
                {
                    /*if (m_isRaged)
                    {
                        if ((bool)OverheadVFX && !instanceVFX)
                        {
                            instanceVFX = firstEnemy.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);
                        }
                        m_elapsed = 0f;
                    }
                    else
                    {
                        obj.StartCoroutine(HandleRage());
                    }*/
                    //firstEnemy.StartCoroutine(HandleTilt(firstEnemy, TiltedDuration));

                    if (firstEnemy.healthHaver.IsBoss || firstEnemy.healthHaver.IsSubboss)
                    {
                        Plugin.Log($"is boss/subboss");
                        return;
                    }

                    if (!enemyTiltedTrackerList.ContainsKey(firstEnemy))
                    {
                        Plugin.Log($"not in list");
                        enemyTiltedTrackerList.Add(firstEnemy, new EnemyTiltedTracker
                        (
                            firstEnemy.StartCoroutine(HandleTilt(firstEnemy, TiltedDuration)),
                            firstEnemy.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true)
                        ));
                    }
                    else
                    {
                        Plugin.Log($"already in list");
                        //firstEnemy.StopCoroutine(enemyTiltedTrackerList[firstEnemy].timerCoroutine);
                        //firstEnemy.RemoveEffect(TiltedEffect);

                    }
                }
            };

            base.PostProcessProjectile(projectile);
        }

        private IEnumerator HandleTilt(AIActor enemy, float duration)
        {
            //instanceVFX = enemy.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);

            TiltedEffect.SpeedMultiplier = UnityEngine.Random.Range(0.2f, 2.0f);
            TiltedEffect.HealthMultiplier = UnityEngine.Random.Range(0.6f, 1.5f);

            //Plugin.Log($"speed: {TiltedEffect.SpeedMultiplier}, health: {TiltedEffect.HealthMultiplier}");

            enemy.ApplyEffect(TiltedEffect);
            /*
            switch (UnityEngine.Random.Range(0, 5))
            {
                case < 1:
                    break;
                case < 2:
                    break;
            }
            */
            float elapsed = 0f;
            float particleCounter = 0f;
            Color ogColor = enemy.sprite.color;
            //enemy.sprite.color = Color.Lerp(ogColor, flatColorOverride, 0.5f);
            enemy.RegisterOverrideColor(flatColorOverride, TiltedEffect.effectIdentifier);
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                //m_player.baseFlatColorOverride = flatColorOverride.WithAlpha(Mathf.Lerp(flatColorOverride.a, 0f, Mathf.Clamp01(m_elapsed - (duration - 1f))));
                //enemy.sprite.color = flatColorOverride.WithAlpha(Mathf.Lerp(flatColorOverride.a, 0f, Mathf.Clamp01(elapsed - (duration - 1f))));
                //enemy.sprite.color = Color.Lerp(ogColor, flatColorOverride, Mathf.Clamp01(elapsed - (duration - 1f)));
                if (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.LOW && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)enemy)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (particleCounter > 1f)
                    {
                        int num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, enemy.sprite.WorldBottomLeft.ToVector3ZisY(), enemy.sprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 90f, 0.5f, 0.25f, 1f, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }
                yield return null;
            }
            //enemy.sprite.color = ogColor;
            enemy.DeregisterOverrideColor(TiltedEffect.effectIdentifier);
            enemyTiltedTrackerList[enemy].activeVFXObject.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
            enemy.RemoveEffect(TiltedEffect);
            enemyTiltedTrackerList.Remove(enemy);
        }
    }
}
