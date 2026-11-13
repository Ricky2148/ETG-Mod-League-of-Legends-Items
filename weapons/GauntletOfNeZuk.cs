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
using Alexandria.Misc;


namespace LOLItems.weapons
{
    internal class GauntletOfNeZuk : AdvancedGunBehavior
    {
        public static string internalName = "gauntlet_of_nezuk"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Gauntlet of Ne'Zuk";

        private static int ammoStat = 1000;
        private static float fireRateStat = 0.6f;
        private static float tierOneChargeTime = 0.5f;
        private static float tierTwoChargeTime = 1.5f;
        private static int spreadAngle = 0;

        private int risingSpellForceStacks = 0;
        private float risingSpellForceFireRatePerStack = 0.15f;
        private int risingSpellForceStackCap = 5;
        private Coroutine risingSpellForceDecayCoroutine;

        private static float projectileDamageStat = 10f;
        private static float projectileSpeedStat = 35f;
        private static float projectileRangeStat = 12f;
        private static float projectileForceStat = 5f;

        private static float chargeProjectileDamageStat = projectileDamageStat * 2.0f;
        private static float chargeProjectileSpeedStat = projectileSpeedStat;
        private static float chargeProjectileRangeStat = projectileRangeStat * 1.25f;
        private static float chargeProjectileForceStat = projectileForceStat * 1.5f;

        private static float chargeProjectile2DamageStat = projectileDamageStat * 4.0f;
        private static float chargeProjectile2SpeedStat = projectileSpeedStat * 0.75f;
        private static float chargeProjectile2RangeStat = 250f;
        private static float chargeProjectile2ForceStat = 0f;

        private static string projOneIdentifierName = "auto_attack_projectile";
        private static string projTwoIdentifierName = "q_projectile";
        private static string projThreeIdentifierName = "ult_projectile";

        public static void Add()
        {
            string FULLNAME = realName; //Full name of your gun 
            string SPRITENAME = "basicchargegun"; //The name that prefixes your sprite files
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName); //Renames the default internal name to your custom internal name
            gun.gameObject.AddComponent<GauntletOfNeZuk>(); //AddComponent<[ClassName]>
            gun.SetShortDescription("I made a gun!");  //The description that pops up when you pick up the gun.
            gun.SetLongDescription("This gun has the potential to become any other gun!  Way better than that other Unfinished Gun."); //The full description in the Ammonomicon.
            /* SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.  Your "..._idle_001" is often a good example.  
             * A copy of the sprite used must be in your "sprites/Ammonomicon Encounter Icon Collection/" folder.
             * The variable at the end assigns a default FPS to all other animations. */
            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);
            /* You can also manually assign the FPS of indivisual animations, below are some examples.
             * Note that if your animation takes too long it might not get to finish, like if your reload animation takes longer than the act of reloading. */
            gun.SetAnimationFPS(gun.shootAnimation, 15);
            gun.SetAnimationFPS(gun.reloadAnimation, 15);
            /* You can also optionally add an intro animation that plays when picking up the gun by using the below line and also set the FPS the same as above. */
            //tk2dSpriteAnimationClip clip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_intro"); //by default uses sprites with the "_intro" suffix
            //gun.SetAnimationFPS(gun.introAnimation, 15);

            /* Animation settings for if you're using a gun that charges. */
            gun.SetAnimationFPS(gun.chargeAnimation, 10);
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 2;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).fps = 15;

            /* AddProjectileModuleFrom loads an existing projectile to use as a base, using GetById([ID]) reads in an existing vanilla gun to use.
             * ID 86 is the marine_sidearm which is a very basic bullet and good to use as a default.
             * ID 56 is the 38_special which is a good base for bullets that should point in the same direction as the gun is pointing.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.Heroine) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById((int)Items.Particulator) as Gun).muzzleFlashEffects; //Loads a muzzle flash based on gun ID names.
            /* gunSwitchGroup loads in the firing and reloading sound effects.
             * Use an existing ID if you want to copy another gun's firing and reloading sounds, otherwise use a custom gunSwitchGroup name then assign your sound effects manually.
             * List of default sound files https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./sound-list
             * Instructions on setting up custom sound files https://mtgmodders.gitbook.io/etg-modding-guide/misc/using-custom-sounds */
            //gun.gunSwitchGroup = (PickupObjectDatabase.GetById(56) as Gun).gunSwitchGroup; //Example using a vanilla gun's ID.
            /* OR */
            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}"; //Unique name for your gun's sound group. In this example it uses your console name but with an underscore.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null); //Play_WPN_Gun_Shot_01 is your weapon's base shot sound.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null); //Play_WPN_Gun_Reload_01 is your weapon's base reload sound.
            
            gun.DefaultModule.angleVariance = spreadAngle; //How far from where you're aiming that bullets can deviate. 0 equals perfect accuracy.
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged; //Sets the firing style of the gun.
            gun.gunClass = GunClass.CHARGE; // Sets the gun's class which is used by category based effects.
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random; //Sets how the gun handles multiple different projectiles
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = 0f;
            gun.DefaultModule.cooldownTime = fireRateStat; //Time between shots fired.  For Burst guns it's the time between each burst.
            gun.DefaultModule.numberOfShotsInClip = 9999;
            gun.SetBaseMaxAmmo(ammoStat);
            /* GunHandedness sets how the gun is held.
             * OneHanded and TwoHanded control how hands hold the gun.
             * AutoDetect will select between one and two handed based on the json/jtk2d.
             * HiddenOneHanded holds the gun with an invisible hand (think armcanons like the Megahand (Megabuster)).
             * NoHanded means the gun should not swing or aim at all, like the Crown of Guns which sits on the player's head. */
            gun.gunHandedness = GunHandedness.AutoDetect;
            /* carryPixelOffset sets the length and width away your character holds the gun. Values are subtle so use higher amounts like 10. */
            gun.carryPixelOffset += new IntVector2(0, 0);
            /* BarrelOffset sets the length and width away on the sprite where the barrel should end.
             * This is where the muzzle flash and projectile will appear. */
            gun.barrelOffset.transform.localPosition += new Vector3(0.75f, 0.2f);
            
            //gun.gunScreenShake.magnitude = 0.2f; //How much the gun shakes the screen when fired.

            //gun.preventRotation = true; //Prevents the gun from rotating with aim direction -> will always face directly right or left.
            //gun.InfiniteAmmo = true; //Gives a gun infinite ammo. By default infinite ammo guns can't crack walls leading to secret rooms.
            //gun.CanGainAmmo = false; //Prevents a gun from being able to pick up ammo boxes.
            //gun.CanReloadNoMatterAmmo = true; //Allows a gun to trigger reload events if it has a full clip or zero ammo.
            //gun.GainsRateOfFireAsContinueAttack = true; //Makes gun shoot faster the longer fire is held (e.g., Vulcan Cannon).
            //gun.RateOfFireMultiplierAdditionPerSecond = rampUpFactor; //Used in conjunction with GainsRateOfFireAsContinueAttack.
            //gun.Volley.ModulesAreTiers = true; //Treats each individual volley of a gun as a tier that can be selected using gun.CurrentStrengthTier


            //PROJECTILE BLOCK
            /* First line instantiates the projectile and uses an existing projectile to set default visuals and properties based on gun ID.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            Projectile projectile1 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            projectile1.name = projOneIdentifierName;
            /* If you want to use a charged projectile from a gun with multiple stages of shots, use a format like the below */
            //Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(613) as Gun).singleModule.chargeProjectiles[1].Projectile);
            /* The following line can be used to output the full details of the properties of a given gun or projectile for if you'd like to replicated certain aspects.
             * This example takes the projectile we just set up then outputs the details to your game folder under Enter the Gungeon\Resources\defaultLog.txt */
            //Alexandria.Misc.DebugUtility.LogPropertiesAndFields<Projectile>(projectile, $"Projectile Properties and Fields: ");
            gun.DefaultModule.projectiles[0] = projectile1; //Assigns the projectile to the gun.
            /* Adjust Impact Visuals */
            projectile1.hitEffects.alwaysUseMidair = true;  //Use end of range visual if it hits something
            projectile1.hitEffects.midairInheritsFlip = true; //Should impact be directional facing?
            projectile1.hitEffects.midairInheritsRotation = true; //Should the visual rotate with the gun's orientation?
            /* You can also copy individual properties using a format like this: */
            projectile1.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById(41) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            projectile1.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById(41) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

            /* The following block is needed so that cloned copies of your projectile have the same properties. */
            projectile1.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile1.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile1);

            /* Adjusting base properties */
            projectile1.baseData.damage = projectileDamageStat;
            projectile1.baseData.speed = projectileSpeedStat;
            projectile1.baseData.range = projectileRangeStat;
            projectile1.baseData.force = projectileForceStat;
            projectile1.transform.parent = gun.barrelOffset;
            projectile1.shouldRotate = true;
            projectile1.AdditionalScaleMultiplier = 1f;

            projectile1.SetProjectileSpriteRight("nezuk_charge1_projectile_001", 27, 9, true, tk2dBaseSprite.Anchor.MiddleCenter, 23, 7);

            // CHARGE PROJECTILE

            // The following settings are for if your gun uses additional projectiles when charged.
            // In general, you would assign properties in the same way as your "projectile" but by just using different names like "chargedprojectile" instead. 
            // You also would not assign the projectiles to the gun with DefaultModule since they will get called by the charging code.
            Projectile chargeProjectile1 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            chargeProjectile1.name = projTwoIdentifierName;
            chargeProjectile1.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeProjectile1.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeProjectile1);

            chargeProjectile1.baseData.damage = chargeProjectileDamageStat;
            chargeProjectile1.baseData.speed = chargeProjectileSpeedStat;
            chargeProjectile1.baseData.range = chargeProjectileRangeStat;
            chargeProjectile1.baseData.force = chargeProjectileForceStat;
            chargeProjectile1.transform.parent = gun.barrelOffset;
            chargeProjectile1.shouldRotate = true;
            chargeProjectile1.AdditionalScaleMultiplier = 1f;
            // Add shooting sounds to your pojectiles by incrementing Play_WPN_Gun_Shot_01.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_02", "Play_WPN_grasshopper_shot_01"); //Sound for second projectile.
            chargeProjectile1.SetProjectileSpriteRight("nezuk_charge2_projectile_001", 19, 15, true, tk2dBaseSprite.Anchor.MiddleCenter, 11, 5);

            // Sets up a third projectile to go with a third charge tier.
            Projectile chargeProjectile2 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            chargeProjectile2.name = projThreeIdentifierName;
            chargeProjectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeProjectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeProjectile2);
            chargeProjectile2.baseData.damage = chargeProjectile2DamageStat;
            chargeProjectile2.baseData.speed = chargeProjectile2SpeedStat;
            chargeProjectile2.baseData.range = chargeProjectile2RangeStat;
            chargeProjectile2.baseData.force = chargeProjectile2ForceStat;
            chargeProjectile2.transform.parent = gun.barrelOffset;
            chargeProjectile2.shouldRotate = true;
            chargeProjectile2.AdditionalScaleMultiplier = 3f;

            chargeProjectile2.gameObject.GetOrAddComponent<TrueWallPiercingRounds>();

            PierceProjModifier ultPierce = chargeProjectile2.gameObject.GetOrAddComponent<PierceProjModifier>();
            ultPierce.penetratesBreakables = true;
            ultPierce.penetration = 999;

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_03", "Play_WPN_bsg_shot_01"); //Sound for third projectile.

            //OPTIONAL ADDITIONAL PROPERTIES
            /* Properties default to whatever you copied your base gun from, but you an adjust them manually as needed. */

            // Ammo
            /* These two sections control what graphic is used on your ammo indicator.
             * A list of ammo types can be found here: https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./all-custom-ammo-types
             * The first entry takes one of the thirteen basic ammo types: 
             * SMALL_BULLET, MEDIUM_BULLET, BEAM, GRENADE, SHOTGUN, SMALL_BLASTER, MEDIUM_BLASTER, NAIL, MUSKETBALL, ARROW, MAGIC, BLUE_SHOTGUN, SKULL, FISH.*/
            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.FISH; 
            /* If the ammo type you want isn't one of those thirteen, then instead use CUSTOM followed by the customAmmoType you want.
             * If you want to make your own customAmmoType then instead use use CUSTOM and then the AddCustomAmmoType.
             * AddCustomAmmoType takes a name for the ammo, then paths to the EMBEDDED filled and emptied ammo sprites.
             * Instructions to embed a sprite https://mtgmodders.gitbook.io/etg-modding-guide/all-things-spriting/importing-a-sprite-to-visual-studios */
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = "samus";
            /* OR */
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Template Gun Orbs",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/tempgun_clipfull", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/tempgun_clipempty");
            /* If your gun uses special ammo for its final shot, use the below settings similar to the above */
            //gun.DefaultModule.finalAmmoType = GameUIAmmoType.AmmoType.FISH;
            //gun.DefaultModule.finalCustomAmmoType = "burning hand";

            // Casings and Clips
            /* Casings are the individual bullet shells and clips are the holders that are ejected from the gun.
             * Casings can be ejected on firing and reloading while clips can only be ejected on reload.
             * You can either use existing casings/clips from vanilla guns or add custom ones using a similar sprite import process as above with ammo.
             * Custom casings can also have their properties edited by adding more parameters to GenerateDebrisObject.*/
            //gun.shellCasing = (PickupObjectDatabase.GetById(15)as Gun).shellCasing; //Example using AK-47 casings.
            //gun.shellCasing = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/tempgun_clipempty").gameObject; //Example using a custom sprite as a casing.
            //gun.clipObject = (PickupObjectDatabase.GetById(15) as Gun).clipObject; //Example using AK-47 clips.
            //gun.shellsToLaunchOnFire = 1; //Number of shells to eject when shooting.
            //gun.shellsToLaunchOnReload = 0; //Number of shells to eject when reloading (revolvers for example).
            //gun.clipsToLaunchOnReload = 1; //Number of clips to eject when reloading.

            // Bullet Trail
            /*
            Tools.EasyTrailBullet trail = projectile.gameObject.AddComponent<Tools.EasyTrailBullet>();
            trail.TrailPos = projectile.transform.position;
            trail.StartWidth = 0.2f;
            trail.EndWidth = 0f;
            trail.LifeTime = 0.2f; //How long the trail lingers
            // BaseColor sets an overall color for the trail. Start and End Colors are subtractive to it. 
            trail.BaseColor = Color.white; //Set to white if you don't want to interfere with Start/End Colors.
            trail.StartColor = Color.red;
            trail.EndColor = new Color(1f, 0.6f, 0.2f); //Custom Orange example using r/g/b values.
            */

            // Homing
            //HomingModifier homing = projectile.gameObject.AddComponent<HomingModifier>();
            //homing.AngularVelocity = 600f;
            //homing.HomingRadius = 50f;

            // Bouncing
            //BounceProjModifier bounce = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
            //bounce.numberOfBounces = 2; //How many times can a projectile bounce?
            //bounce.damageMultiplierOnBounce = 1; //Should bounces do more/less damage?
            //bounce.bouncesTrackEnemies = true; //Should bounces aim at enemies?
            //bounce.TrackEnemyChance = 0.5f; //Odds that a bounce aims at an enemy.
            //bounce.bounceTrackRadius = 7; //How close does an enemy need to be for a bounce to track?

            // Pierce Enemies
            //PierceProjModifier pierce = projectile.gameObject.AddComponent<PierceProjModifier>();  //initialize piercing.
            //pierce.penetration = 3; //How many enemies it can pierce.
            //pierce.penetratesBreakables = true; //Are breakables included?

            //projectile.ignoreDamageCaps = false; //Damage caps exist for bosses to keep their HP from going down too fast. Set to True if you want to circumvent that.
            //projectile.BossDamageMultiplier = 1.5f; //Should bosses take more/less damage?
            //projectile.BlackPhantomDamageMultiplier = 1.5f; //Should Jammed enemies take more/less damage?
            //projectile.PlayerKnockbackForce = 2f; //Should the player get pushed back?
            //projectile.pierceMinorBreakables = true; //Can projectiles pierce through clutter around the room?
            //projectile.PenetratesInternalWalls = true; //Can projectiles pierce through the walls in the middle of a room?
            //projectile.collidesWithProjectiles = true; //Can projectiles hit enemy projectiles?
            //projectile.collidesWithEnemies = false; //Prevents a projectile from colliding with enemies.
            //projectile.BulletScriptSettings.surviveRigidbodyCollisions = true; //Lets a projectile continue existing after colliding with an enemy.
            //projectile.BulletScriptSettings.surviveTileCollisions = true; //Lets a projectile continue existing after colliding with a wall
            //projectile.collidesOnlyWithPlayerProjectiles = true; //If collidesWithProjectiles is true, makes the projectile only able to hit other player projectiles
            //projectile.onDestroyEventName = "Play_WPN_bsg_shot_01"; //An extra sound to play when the projectile is destroyed

            //mod.angleFromAim = 10f; //Degree offset from your aim angle the module fires. Useful for guns with multiple modules that fire in a spread.
            //mod.mirror = true; //If true, automatically fires a second projectile at negative angleFromAim. useful for symmetrical bursts.
            /* Prevents the module from contributing to whether your gun needs to reload.
             * Useful for synergy modules that fire additional shots but shouldn't interfere with the main module from reloading. */
            //mod.ignoredForReloadPurposes = true; 

            //Animate Projectile
            /*
            // Animating your projectile is similar to setting up the custom sprite except each parameter is set as a group.
            // Each of the parameter lists should have the same number of entries as frames you're using.

            // A list of filenames in the sprites/ProjectileCollection folder for each frame in the animation, extension not required.
            List<string> projectileSpriteNames = new List<string> { $"{SPRITENAME}_projectile_001", $"{SPRITENAME}_projectile_002", $"{SPRITENAME}_projectile_003", $"{SPRITENAME}_projectile_004", $"{SPRITENAME}_projectile_005" };
            // Animation FPS.
            int projectileFPS = 10;
            // Visual sprite size for each frame.  Sprite images will stretch to match these sizes.
            List<IntVector2> projectileSizes = new List<IntVector2> { new IntVector2(30, 12), new IntVector2(30, 12), new IntVector2(30, 12), new IntVector2(30, 12), new IntVector2(30, 12) };
            // Whether each frame should have a bit of glow.
            List<bool> projectileLighteneds = new List<bool> { true, true, true, true, true };
            // Sprite anchor list.
            List<tk2dBaseSprite.Anchor> projectileAnchors = new List<tk2dBaseSprite.Anchor>
                {tk2dBaseSprite.Anchor.MiddleCenter, tk2dBaseSprite.Anchor.MiddleCenter, tk2dBaseSprite.Anchor.MiddleCenter, tk2dBaseSprite.Anchor.MiddleCenter, tk2dBaseSprite.Anchor.MiddleCenter};
            // Whether or not the anchors should affect the hitboxees.
            List<bool> projectileAnchorsChangeColiders = new List<bool> { false, false, false, false, false };
            // Unknown, doesn't appear to matter so leave as false. 
            List<bool> projectilefixesScales = new List<bool> { false, false, false, false, false };
            // Manual Offsets for each sprite if needed.
            List<Vector3?> projectileManualOffsets = new List<Vector3?> { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
            // Override the projectile hitboxes on each frame.  Either null (same as visuals) or slightly smaller than the visuals is most common.
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?> { null, null, null, null, null };
            // Manually assign the projectile offsets.
            List<IntVector2?> projectileOverrideColliderOffsets = new List<IntVector2?> { null, null, null, null, null };
            // Copy another projectile each frame.
            List<Projectile> projectileOverrideProjectilesToCopyFrom = new List<Projectile> { null, null, null, null, null };
            // Your animations wrap mode. If you just want it to do a looping animation, leave it as Loop. Only useful for when adding multiple differing animations.
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            // Optionally, you can give your animations a clip name. Only useful for when adding multiple differing animations.
            //string projectileClipName = "projectileName"; 
            // Optionally, you can assign an animation as the default one that plays.  Only useful for when adding multiple differing animations.  If left as null then it will use the most recently added animation.
            //string projectileDefaultClipName = "projectileName"; 

            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);
            */

            // Sets the properties of charging including the time it takes to charge.
            ProjectileModule.ChargeProjectile chargeProj1 = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile1, //Assigns a projectile to the charge state
                ChargeTime = 0f, //How long to hold the trigger before getting to this charge state.  Set the first one to 0 to allow regular firing the base shot.
            };
            ProjectileModule.ChargeProjectile chargeProj2 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeProjectile1,
                ChargeTime = tierOneChargeTime,
                AmmoCost = 2,
            };
            ProjectileModule.ChargeProjectile chargeProj3 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeProjectile2,
                ChargeTime = tierTwoChargeTime, //Means this charge state will be reached after 3 seconds total, not 3 seconds since the previous state.
                AmmoCost = 10,
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> {chargeProj1, chargeProj2, chargeProj3}; //Assigns charged projectiles to the gun.
            

            gun.quality = PickupObject.ItemQuality.EXCLUDED; //Sets the gun's quality rank. Use "EXCLUDED" if the gun should not appear in chests.
            ETGMod.Databases.Items.Add(gun, false, "ANY");  //Adds your gun to the databse.
            //gun.AddToSubShop(ItemBuilder.ShopType.Trorc); //Select which sub shops during a run can carry the gun
            //gun.AddToSubShop(ItemBuilder.ShopType.Flynt);
            ID = gun.PickupObjectId; //Sets the Gun ID. 
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            base.OnPostFired(player, gun);
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            Plugin.Log($"projectile name: {projectile.name}");
            if (projectile.name == (projTwoIdentifierName + "(Clone)") || projectile.name == (projThreeIdentifierName + "(Clone)"))
            {
                Plugin.Log($"rising spell force stack on hit: {projectile.name}");
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
                        if (risingSpellForceStacks < risingSpellForceStackCap)
                        {
                            risingSpellForceStacks++;
                            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.RateOfFire);
                            ItemBuilder.AddCurrentGunStatModifier(gun, PlayerStats.StatType.RateOfFire, 1.0f + (risingSpellForceStacks * risingSpellForceFireRatePerStack), StatModifier.ModifyMethod.MULTIPLICATIVE);
                            Player.stats.RecalculateStatsWithoutRebuildingGunVolleys(Player);

                            //gun.spriteAnimator.OverrideTimeScale = 1f + (risingSpellForceStacks * risingSpellForceFireRatePerStack);

                            if (risingSpellForceDecayCoroutine != null)
                            {
                                StopCoroutine(risingSpellForceDecayCoroutine);
                            }
                            risingSpellForceDecayCoroutine = StartCoroutine(RisingSpellForceDecayCoroutine(Player, Player.CurrentGun));

                            base.Update();
                        }
                    }
                };
            }

            base.PostProcessProjectile(projectile);
        }

        public override void OnFinishAttack(PlayerController player, Gun gun)
        {
            /*
            if (risingSpellForceDecayCoroutine != null)
            {
                StopCoroutine(risingSpellForceDecayCoroutine);
            }
            risingSpellForceDecayCoroutine = StartCoroutine(RisingSpellForceDecayCoroutine(player, gun));
            */

            base.OnFinishAttack(player, gun);
        }

        private System.Collections.IEnumerator RisingSpellForceDecayCoroutine(PlayerController player, Gun gun)
        {
            yield return new WaitForSeconds(6f);
            risingSpellForceStacks = 0;
            ItemBuilder.RemoveCurrentGunStatModifier(gun, PlayerStats.StatType.RateOfFire);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            //gun.spriteAnimator.OverrideTimeScale = 1f;
        }
    }
}