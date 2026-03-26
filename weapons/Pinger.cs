using Alexandria.BreakableAPI;
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
using UnityEngine.Experimental.UIElements;

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
        private static float reloadDuration = 2.0f;
        private static float fireRateStat = 0.4f;
        private static int spreadAngle = 0;

        private static float projectileDamageStat = 12f;
        private static float projectileSpeedStat = 25f;
        private static float projectileRangeStat = 12f;
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
            "mouseclick_SFX_01"
        };

        private static List<GameObject> KeyboardPiecesList = new List<GameObject>
        {
            BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_black_1", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 2).gameObject,
            BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_black_2", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 2).gameObject,
            //BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_black_3", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 3).gameObject,
            //BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_black_4", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 3).gameObject,
            BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_white_1", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 2).gameObject,
            BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_white_2", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 2).gameObject,
            //BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_white_3", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 3).gameObject,
            //BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/ProjectileCollection/key_white_4", AngularVelocity: 180, AngularVelocityVariance: 450, DebrisBounceCount: 3).gameObject,
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
            gun.SetAnimationFPS(gun.reloadAnimation, 15);

            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun, true, false);

            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}";
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "mouseclick_SFX_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "keyboard_smashing_SFX");

            gun.DefaultModule.angleVariance = spreadAngle;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.gunClass = GunClass.SILLY;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat;
            gun.DefaultModule.numberOfShotsInClip = 20;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.gunHandedness = GunHandedness.TwoHanded;

            gun.carryPixelOffset += new IntVector2(13, -1); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(-20, -8); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(-8, 12); //offset when aiming up

            gun.barrelOffset.transform.localPosition += new Vector3(6 / 16f, 10 / 16f);
            gun.gunScreenShake.magnitude = 0f;

            gun.DefaultModule.projectiles.Clear();

            //custom hit effect setup
            VFXPool pool2 = new VFXPool();
            pool2.type = VFXPoolType.Single;

            /*VFXComplex customHitEffect = HelpfulMethods.CreateVFXComplex("pinger_hiteffect",
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
                );*/

            VFXComplex customHitEffect = HelpfulMethods.CreateVFXComplex("pinger_hiteffect",
                new List<string>()
                {
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_001",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_002",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_003",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_004",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_005",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_006",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_007",
                    "LOLItems/Resources/hit_effects/pinger_rainbow/pinger_hit_008",
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
            
            
            //projectile1.objectImpactEventName = "all_in_ping_1";
            //projectile1.enemyImpactEventName = "all_in_ping_1";
            projectile1.onDestroyEventName = "Play_WPN_all_in_ping_1_impact_01";



            projectile1.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile1.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile1);

            projectile1.baseData.damage = projectileDamageStat;
            projectile1.baseData.speed = projectileSpeedStat;
            projectile1.baseData.range = projectileRangeStat;
            projectile1.baseData.force = projectileForceStat; //Knockback strength
            projectile1.transform.parent = gun.barrelOffset;

            projectile1.SetProjectileSpriteRight("pinger_projectile_all_in_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail1 = projectile1.gameObject.AddComponent<EasyTrailBullet>();
            trail1.TrailPos = projectile1.transform.position;
            trail1.StartWidth = 0.15f;
            trail1.EndWidth = 0f;
            trail1.LifeTime = 0.1f;

            trail1.BaseColor    = new Color(255 / 255f, 234 / 255f, 201 / 255f);
            trail1.StartColor   = new Color(255 / 255f, 234 / 255f, 201 / 255f);
            trail1.EndColor     = new Color(255 / 255f, 234 / 255f, 201 / 255f);*/

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


            //projectile2.objectImpactEventName = "assist_me_ping_2";
            //projectile2.enemyImpactEventName = "assist_me_ping_2";
            projectile2.onDestroyEventName = "Play_WPN_assist_me_ping_2_impact_01";



            projectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile2);

            projectile2.baseData.damage = projectileDamageStat;
            projectile2.baseData.speed = projectileSpeedStat;
            projectile2.baseData.range = projectileRangeStat;
            projectile2.baseData.force = projectileForceStat; //Knockback strength
            projectile2.transform.parent = gun.barrelOffset;

            projectile2.SetProjectileSpriteRight("pinger_projectile_assist_me_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail2 = projectile2.gameObject.AddComponent<EasyTrailBullet>();
            trail2.TrailPos = projectile1.transform.position;
            trail2.StartWidth = 0.15f;
            trail2.EndWidth = 0f;
            trail2.LifeTime = 0.1f;

            trail2.BaseColor    = new Color(148 / 255f, 255 / 255f, 210 / 255f);
            trail2.StartColor   = new Color(148 / 255f, 255 / 255f, 210 / 255f);
            trail2.EndColor     = new Color(148 / 255f, 255 / 255f, 210 / 255f);*/

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


            //projectile3.objectImpactEventName = "bait_ping_3";
            //projectile3.enemyImpactEventName = "bait_ping_3";
            projectile3.onDestroyEventName = "Play_WPN_bait_ping_3_impact_01";


            projectile3.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile3.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile3);

            projectile3.baseData.damage = projectileDamageStat;
            projectile3.baseData.speed = projectileSpeedStat;
            projectile3.baseData.range = projectileRangeStat;
            projectile3.baseData.force = projectileForceStat; //Knockback strength
            projectile3.transform.parent = gun.barrelOffset;

            projectile3.SetProjectileSpriteRight("pinger_projectile_bait_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail3 = projectile3.gameObject.AddComponent<EasyTrailBullet>();
            trail3.TrailPos = projectile1.transform.position;
            trail3.StartWidth = 0.15f;
            trail3.EndWidth = 0f;
            trail3.LifeTime = 0.1f;

            trail3.BaseColor    = new Color(255 / 255f, 250 / 255f, 199 / 255f);
            trail3.StartColor   = new Color(255 / 255f, 250 / 255f, 199 / 255f);
            trail3.EndColor     = new Color(255 / 255f, 250 / 255f, 199 / 255f);*/

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


            //projectile4.objectImpactEventName = "caution_ping_4";
            //projectile4.enemyImpactEventName = "caution_ping_4";
            projectile4.onDestroyEventName = "Play_WPN_caution_ping_4_impact_01";


            projectile4.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile4.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile4);

            projectile4.baseData.damage = projectileDamageStat;
            projectile4.baseData.speed = projectileSpeedStat;
            projectile4.baseData.range = projectileRangeStat;
            projectile4.baseData.force = projectileForceStat; //Knockback strength
            projectile4.transform.parent = gun.barrelOffset;

            projectile4.SetProjectileSpriteRight("pinger_projectile_caution_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail4 = projectile4.gameObject.AddComponent<EasyTrailBullet>();
            trail4.TrailPos = projectile1.transform.position;
            trail4.StartWidth = 0.15f;
            trail4.EndWidth = 0f;
            trail4.LifeTime = 0.1f;

            trail4.BaseColor    = new Color(255 / 255f, 231 / 255f, 158 / 255f);
            trail4.StartColor   = new Color(255 / 255f, 231 / 255f, 158 / 255f);
            trail4.EndColor     = new Color(255 / 255f, 231 / 255f, 158 / 255f);*/

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


            //projectile5.objectImpactEventName = "defend_ping_5";
            //projectile5.enemyImpactEventName = "defend_ping_5";
            projectile5.onDestroyEventName = "Play_WPN_defend_ping_5_impact_01";


            projectile5.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile5.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile5);

            projectile5.baseData.damage = projectileDamageStat;
            projectile5.baseData.speed = projectileSpeedStat;
            projectile5.baseData.range = projectileRangeStat;
            projectile5.baseData.force = projectileForceStat; //Knockback strength
            projectile5.transform.parent = gun.barrelOffset;

            projectile5.SetProjectileSpriteRight("pinger_projectile_defend_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail5 = projectile5.gameObject.AddComponent<EasyTrailBullet>();
            trail5.TrailPos = projectile1.transform.position;
            trail5.StartWidth = 0.15f;
            trail5.EndWidth = 0f;
            trail5.LifeTime = 0.1f;

            trail5.BaseColor    = new Color(204 / 255f, 255 / 255f, 224 / 255f);
            trail5.StartColor   = new Color(204 / 255f, 255 / 255f, 224 / 255f);
            trail5.EndColor     = new Color(204 / 255f, 255 / 255f, 224 / 255f);*/

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


            //projectile6.objectImpactEventName = "enemy_missing_ping_6";
            //projectile6.enemyImpactEventName = "enemy_missing_ping_6";
            projectile6.onDestroyEventName = "Play_WPN_enemy_missing_ping_6_impact_01";


            projectile6.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile6.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile6);

            projectile6.baseData.damage = projectileDamageStat;
            projectile6.baseData.speed = projectileSpeedStat;
            projectile6.baseData.range = projectileRangeStat;
            projectile6.baseData.force = projectileForceStat; //Knockback strength
            projectile6.transform.parent = gun.barrelOffset;

            projectile6.SetProjectileSpriteRight("pinger_projectile_enemy_missing_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail6 = projectile6.gameObject.AddComponent<EasyTrailBullet>();
            trail6.TrailPos = projectile1.transform.position;
            trail6.StartWidth = 0.15f;
            trail6.EndWidth = 0f;
            trail6.LifeTime = 0.1f;

            trail6.BaseColor    = new Color(220 / 255f, 255 / 255f, 145 / 255f);
            trail6.StartColor   = new Color(220 / 255f, 255 / 255f, 145 / 255f);
            trail6.EndColor     = new Color(220 / 255f, 255 / 255f, 145 / 255f);*/

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


            //projectile7.objectImpactEventName = "enemy_vision_ping_7";
            //projectile7.enemyImpactEventName = "enemy_vision_ping_7";
            projectile7.onDestroyEventName = "Play_WPN_enemy_vision_ping_7_impact_01";


            projectile7.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile7.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile7);

            projectile7.baseData.damage = projectileDamageStat;
            projectile7.baseData.speed = projectileSpeedStat;
            projectile7.baseData.range = projectileRangeStat;
            projectile7.baseData.force = projectileForceStat; //Knockback strength
            projectile7.transform.parent = gun.barrelOffset;

            projectile7.SetProjectileSpriteRight("pinger_projectile_enemy_vision_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail7 = projectile7.gameObject.AddComponent<EasyTrailBullet>();
            trail7.TrailPos = projectile1.transform.position;
            trail7.StartWidth = 0.15f;
            trail7.EndWidth = 0f;
            trail7.LifeTime = 0.1f;

            trail7.BaseColor    = new Color(255 / 255f, 161 / 255f, 251 / 255f);
            trail7.StartColor   = new Color(255 / 255f, 161 / 255f, 251 / 255f);
            trail7.EndColor     = new Color(255 / 255f, 161 / 255f, 251 / 255f);*/

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


            //projectile8.objectImpactEventName = "generic_ping_8";
            //projectile8.enemyImpactEventName = "generic_ping_8";
            projectile8.onDestroyEventName = "Play_WPN_generic_ping_8_impact_01";


            projectile8.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile8.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile8);

            projectile8.baseData.damage = projectileDamageStat;
            projectile8.baseData.speed = projectileSpeedStat;
            projectile8.baseData.range = projectileRangeStat;
            projectile8.baseData.force = projectileForceStat; //Knockback strength
            projectile8.transform.parent = gun.barrelOffset;

            projectile8.SetProjectileSpriteRight("pinger_projectile_generic_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail8 = projectile8.gameObject.AddComponent<EasyTrailBullet>();
            trail8.TrailPos = projectile1.transform.position;
            trail8.StartWidth = 0.15f;
            trail8.EndWidth = 0f;
            trail8.LifeTime = 0.1f;

            trail8.BaseColor    = new Color(186 / 255f, 247 / 255f, 255 / 255f);
            trail8.StartColor   = new Color(186 / 255f, 247 / 255f, 255 / 255f);
            trail8.EndColor     = new Color(186 / 255f, 247 / 255f, 255 / 255f);*/

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


            //projectile9.objectImpactEventName = "need_vision_ping_9";
            //projectile9.enemyImpactEventName = "need_vision_ping_9";
            projectile9.onDestroyEventName = "Play_WPN_need_vision_ping_9_impact_01";


            projectile9.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile9.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile9);

            projectile9.baseData.damage = projectileDamageStat;
            projectile9.baseData.speed = projectileSpeedStat;
            projectile9.baseData.range = projectileRangeStat;
            projectile9.baseData.force = projectileForceStat; //Knockback strength
            projectile9.transform.parent = gun.barrelOffset;

            projectile9.SetProjectileSpriteRight("pinger_projectile_need_vision_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail9 = projectile9.gameObject.AddComponent<EasyTrailBullet>();
            trail9.TrailPos = projectile1.transform.position;
            trail9.StartWidth = 0.15f;
            trail9.EndWidth = 0f;
            trail9.LifeTime = 0.1f;

            trail9.BaseColor    = new Color(172 / 255f, 255 / 255f, 143 / 255f);
            trail9.StartColor   = new Color(172 / 255f, 255 / 255f, 143 / 255f);
            trail9.EndColor     = new Color(172 / 255f, 255 / 255f, 143 / 255f);*/

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


            //projectile10.objectImpactEventName = "on_my_way_ping_10";
            //projectile10.enemyImpactEventName = "on_my_way_ping_10";
            projectile10.onDestroyEventName = "Play_WPN_on_my_way_ping_10_impact_01";


            projectile10.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile10.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile10);

            projectile10.baseData.damage = projectileDamageStat;
            projectile10.baseData.speed = projectileSpeedStat;
            projectile10.baseData.range = projectileRangeStat;
            projectile10.baseData.force = projectileForceStat; //Knockback strength
            projectile10.transform.parent = gun.barrelOffset;

            projectile10.SetProjectileSpriteRight("pinger_projectile_on_my_way_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail10 = projectile10.gameObject.AddComponent<EasyTrailBullet>();
            trail10.TrailPos = projectile1.transform.position;
            trail10.StartWidth = 0.15f;
            trail10.EndWidth = 0f;
            trail10.LifeTime = 0.1f;

            trail10.BaseColor   = new Color(150 / 255f, 211 / 255f, 255 / 255f);
            trail10.StartColor  = new Color(150 / 255f, 211 / 255f, 255 / 255f);
            trail10.EndColor    = new Color(150 / 255f, 211 / 255f, 255 / 255f);*/

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


            //projectile11.objectImpactEventName = "push_ping_11";
            //projectile11.enemyImpactEventName = "push_ping_11";
            projectile11.onDestroyEventName = "Play_WPN_push_ping_11_impact_01";


            projectile11.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile11.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile11);

            projectile11.baseData.damage = projectileDamageStat;
            projectile11.baseData.speed = projectileSpeedStat;
            projectile11.baseData.range = projectileRangeStat;
            projectile11.baseData.force = projectileForceStat; //Knockback strength
            projectile11.transform.parent = gun.barrelOffset;

            projectile11.SetProjectileSpriteRight("pinger_projectile_push_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail11 = projectile11.gameObject.AddComponent<EasyTrailBullet>();
            trail11.TrailPos = projectile1.transform.position;
            trail11.StartWidth = 0.15f;
            trail11.EndWidth = 0f;
            trail11.LifeTime = 0.1f;

            trail11.BaseColor   = new Color(204 / 255f, 255 / 255f, 246 / 255f);
            trail11.StartColor  = new Color(204 / 255f, 255 / 255f, 246 / 255f);
            trail11.EndColor    = new Color(204 / 255f, 255 / 255f, 246 / 255f);*/

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


            //projectile12.objectImpactEventName = "retreat_ping_12";
            //projectile12.enemyImpactEventName = "retreat_ping_12";
            projectile12.onDestroyEventName = "Play_WPN_retreat_ping_12_impact_01";


            projectile12.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile12.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile12);

            projectile12.baseData.damage = projectileDamageStat;
            projectile12.baseData.speed = projectileSpeedStat;
            projectile12.baseData.range = projectileRangeStat;
            projectile12.baseData.force = projectileForceStat; //Knockback strength
            projectile12.transform.parent = gun.barrelOffset;

            projectile12.SetProjectileSpriteRight("pinger_projectile_retreat_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail12 = projectile12.gameObject.AddComponent<EasyTrailBullet>();
            trail12.TrailPos = projectile1.transform.position;
            trail12.StartWidth = 0.15f;
            trail12.EndWidth = 0f;
            trail12.LifeTime = 0.1f;

            trail12.BaseColor   = new Color(255 / 255f, 166 / 255f, 214 / 255f);
            trail12.StartColor  = new Color(255 / 255f, 166 / 255f, 214 / 255f);
            trail12.EndColor    = new Color(255 / 255f, 166 / 255f, 214 / 255f);*/

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


            //projectile13.objectImpactEventName = "target_ping_13";
            //projectile13.enemyImpactEventName = "target_ping_13";
            projectile13.onDestroyEventName = "Play_WPN_target_ping_13_impact_01";


            projectile13.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile13.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile13);

            projectile13.baseData.damage = projectileDamageStat;
            projectile13.baseData.speed = projectileSpeedStat;
            projectile13.baseData.range = projectileRangeStat;
            projectile13.baseData.force = projectileForceStat; //Knockback strength
            projectile13.transform.parent = gun.barrelOffset;

            projectile13.SetProjectileSpriteRight("pinger_projectile_target_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail13 = projectile13.gameObject.AddComponent<EasyTrailBullet>();
            trail13.TrailPos = projectile1.transform.position;
            trail13.StartWidth = 0.15f;
            trail13.EndWidth = 0f;
            trail13.LifeTime = 0.1f;

            trail13.BaseColor   = new Color(255 / 255f, 143 / 255f, 160 / 255f);
            trail13.StartColor  = new Color(255 / 255f, 143 / 255f, 160 / 255f);
            trail13.EndColor    = new Color(255 / 255f, 143 / 255f, 160 / 255f);*/

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


            //projectile14.objectImpactEventName = "vision_cleared_ping_14";
            //projectile14.enemyImpactEventName = "vision_cleared_ping_14";
            projectile14.onDestroyEventName = "Play_WPN_vision_cleared_ping_14_impact_01";


            projectile14.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile14.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile14);

            projectile14.baseData.damage = projectileDamageStat;
            projectile14.baseData.speed = projectileSpeedStat;
            projectile14.baseData.range = projectileRangeStat;
            projectile14.baseData.force = projectileForceStat; //Knockback strength
            projectile14.transform.parent = gun.barrelOffset;

            projectile14.SetProjectileSpriteRight("pinger_projectile_vision_cleared_001", 16, 16, true, tk2dBaseSprite.Anchor.MiddleCenter, 12, 12); //Note that your sprite will stretch to match the visual dimensions

            /*EasyTrailBullet trail14 = projectile14.gameObject.AddComponent<EasyTrailBullet>();
            trail14.TrailPos = projectile1.transform.position;
            trail14.StartWidth = 0.15f;
            trail14.EndWidth = 0f;
            trail14.LifeTime = 0.1f;

            trail14.BaseColor   = new Color(204 / 255f, 153 / 255f, 255 / 255f);
            trail14.StartColor  = new Color(204 / 255f, 153 / 255f, 255 / 255f);
            trail14.EndColor    = new Color(204 / 255f, 153 / 255f, 255 / 255f);*/

            #endregion Projectile Setup

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("pinger_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/pinger_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/pinger_ammo_empty");

            //custom muzzle effect setup
            VFXPool pool = new VFXPool();
            pool.type = VFXPoolType.Single;

            VFXComplex muzzleFlash = HelpfulMethods.CreateVFXComplex("pinger_muzzleflash",
                new List<string>()
                {
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_001",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_002",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_003",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_004",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_005",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_006",
                    "LOLItems/Resources/muzzle_flashes/pinger_rainbow/pinger_muzzleflare_007",
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

            gun.m_casingLaunchAttachPoint.localPosition = new Vector3(20f / 16f, 17f / 16f, 0.0f);

            gun.shellCasing = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/white_dot", AngularVelocity: 540, AngularVelocityVariance: 180, DebrisBounceCount: 3).gameObject;
            gun.clipObject = null;

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
            //HelpfulMethods.PlayRandomSFX(gun.gameObject, PingerFiringSFXList);

            base.OnPostFired(player, gun);
        }

        public override void OnReload(PlayerController player, Gun gun)
        {
            //player.StartCoroutine(ReloadShellEjectCoroutine(gun));

            SpawnKeyboardPiecesAtPosition(gun.CasingLaunchPoint, gun.m_transform, gun.gunAngle, gun.m_owner, gun.barrelOffset, gun.m_localAimPoint);

            base.OnReload(player, gun);
        }

        private IEnumerator ReloadShellEjectCoroutine(Gun gun)
        {
            /*float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;

                yield return null;
            }*/

            //Plugin.Log($"casing Launch Point: {gun.CasingLaunchPoint}, casing Launch Attach Point: {gun.m_casingLaunchAttachPoint.localPosition}");

            for (int i = 0; i < 15; i++)
            {
                gun.SpawnShellCasingAtPosition(gun.CasingLaunchPoint);
            }

            yield return new WaitForSeconds(0.7f);

            for (int i = 0; i < 15; i++)
            {
                gun.SpawnShellCasingAtPosition(gun.CasingLaunchPoint);
            }

            yield return new WaitForSeconds(0.7f);

            for (int i = 0; i < 15; i++)
            {
                gun.SpawnShellCasingAtPosition(gun.CasingLaunchPoint);
            }
        }

        private void SpawnKeyboardPiecesAtPosition(Vector3 position, Transform m_transform, float gunAngle, GameActor m_owner, Transform barrelOffset, Vector2 m_localAimPoint)
        {
            GameObject casingToSpawn = gun.shellCasing;

            for (int i = 0; i < 15; i++)
            {
                switch (UnityEngine.Random.value)
                {
                    //case < 0.125f:
                        //casingToSpawn = KeyboardPiecesList[0];
                        //break;
                    case < 0.250f:
                        casingToSpawn = KeyboardPiecesList[0];
                        break;
                    //case < 0.375f:
                        //casingToSpawn = KeyboardPiecesList[2];
                        //break;
                    case < 0.500f:
                        casingToSpawn = KeyboardPiecesList[1];
                        break;
                    //case < 0.625f:
                        //casingToSpawn = KeyboardPiecesList[4];
                        //break;
                    case < 0.750f:
                        casingToSpawn = KeyboardPiecesList[2];
                        break;
                    //case < 0.875f:
                        //casingToSpawn = KeyboardPiecesList[6];
                        //break;
                    default:
                        casingToSpawn = KeyboardPiecesList[3];
                        break;
                }

                GameObject gameObject = SpawnManager.SpawnDebris(casingToSpawn, position.WithZ(m_transform.position.z), Quaternion.Euler(0f, 0f, gunAngle));
                ShellCasing component = gameObject.GetComponent<ShellCasing>();
                if (component != null)
                {
                    component.Trigger();
                }
                DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
                if (!(component2 != null))
                {
                    return;
                }
                int num = ((component2.transform.right.x > 0f) ? 1 : (-1));
                Vector3 vector = Vector3.up * (UnityEngine.Random.value * 1.5f + 1f) + -1.5f * Vector3.right * num * (UnityEngine.Random.value + 1.5f);
                Vector3 startingForce = new Vector3(vector.x, 0f, vector.y);
                if (m_owner is PlayerController)
                {
                    PlayerController playerController = m_owner as PlayerController;
                    if (playerController.CurrentRoom != null && playerController.CurrentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.CATACOMBS_BRIDGE_ROOM)
                    {
                        startingForce = (vector.x * (float)num * -1f * (barrelOffset.position.XY() - m_localAimPoint).normalized).ToVector3ZUp(vector.y);
                    }
                }
                float y = m_owner.transform.position.y;
                float num2 = position.y - m_owner.transform.position.y + 0.2f;
                float num3 = component2.transform.position.y - y + UnityEngine.Random.value * 0.5f;
                component2.additionalHeightBoost = num2 - num3;
                if (gunAngle > 25f && gunAngle < 155f)
                {
                    component2.additionalHeightBoost += -0.25f;
                }
                else
                {
                    component2.additionalHeightBoost += 0.25f;
                }
                component2.Trigger(startingForce, num3);
            }
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
