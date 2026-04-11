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
using Alexandria.VisualAPI;

// need to make the gun's idle animation not its item sprite or the UI sprite somehow
// figure out hit effects and spread vfx
// why did i make this do 15 dmg per shot?

namespace LOLItems.weapons
{
    internal class PrayerBeads : AdvancedGunBehavior
    {
        public static string internalName = "Prayer Beads"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Aion Er'na"; //The name that shows up in the Ammonomicon and the mod console.

        private static int ammoStat = 300;
        private static float reloadDuration = 0.5f;
        private static float fireRateStat = 0.5f;
        private static int spreadAngle = 5;

        private static float projectileDamageStat = 15f;
        private static float projectileSpeedStat = 70f;
        private static float projectileRangeStat = 20f;
        private static float projectileForceStat = 10f;

        private static float projSpreadRange = 5f;

        private static List<string> normalFiringSFXList = new List<string>
        {
            "prayerbeads_fire_sfx1",
            "prayerbeads_fire_sfx2",
            "prayerbeads_fire_sfx3",
            "prayerbeads_fire_sfx4",
            "prayerbeads_fire_sfx5"
        };


        public static void Add()
        {
            /* This expanded template is meant to document through many of the most common settings in making a gun.
             * Not every setting is covered so if there's an effect you're looking for that isn't listed, try using the the Autocorrect function to see if it's there anyway.
             * When in doubt, try searching GitHub for similar mods that might have already used something similar for you to compare to. */

            //GUN BLOCK

            /* NewGun(x,y) works where "x" is the full name of your gun and y is the prefix most of your sprite files use. 
             * Rename(a,b) works where "a" is what the game names your gun internally which uses lower case and underscores.  Here it would be "outdated_gun_mods:template_gun".
             * "b" is how you're renaming the gun to show up in the mod console.
             * The default here is to use your mod's prefix then shortname so in this example it would come out as "twp:template_gun". */
            string FULLNAME = "Aion Er'na"; //Full name of your gun 
            string SPRITENAME = "prayerbeads"; //The name that prefixes your sprite files
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName); //Renames the default internal name to your custom internal name
            gun.gameObject.AddComponent<PrayerBeads>(); //AddComponent<[ClassName]>
            gun.SetShortDescription("\"Relentless fortitude!\n");  //The description that pops up when you pick up the gun.
            gun.SetLongDescription("A legendary Kinkou relic. The Kinkou were known for their abilities to interact with both the physical world and the spiritual world. " +
                "This set of prayer beads can be sent out with simple hand motions, almost as if there's someone else helping you control them.\n"); //The full description in the Ammonomicon.
            /* SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.  Your "..._idle_001" is often a good example.  
             * A copy of the sprite used must be in your "sprites/Ammonomicon Encounter Icon Collection/" folder.
             * The variable at the end assigns a default FPS to all other animations. */

            //gun.SetupSprite(null, "prayerbeads_icon_001", 1);
            gun.SetupSprite(null, $"prayerbeads_idle_001", 10);

            /*
            gun.alternateIdleAnimation = gun.UpdateAnimation("alternate_idle", null, true);

            if (gun.alternateIdleAnimation != null)
            {
                BraveUtility.Swap(ref gun.idleAnimation, ref gun.alternateIdleAnimation);
            }
            else
            {
                Plugin.Log("alt idle is null");
            }
            */

            //gun.UpdateAnimation("idle", null, true);

            /* You can also manually assign the FPS of indivisual animations, below are some examples.
             * Note that if your animation takes too long it might not get to finish, like if your reload animation takes longer than the act of reloading. */
            gun.SetAnimationFPS(gun.shootAnimation, 34);
            //gun.SetAnimationFPS(gun.criticalFireAnimation, 5);
            //gun.SetAnimationFPS(gun.reloadAnimation, 13);
            /* You can also optionally add an intro animation that plays when picking up the gun by using the below line and also set the FPS the same as above. */
            //tk2dSpriteAnimationClip clip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_intro"); //by default uses sprites with the "_intro" suffix
            //gun.SetAnimationFPS(gun.introAnimation, 15);

            /* Animation settings for if you're using a gun that charges. */
            //gun.SetAnimationFPS(gun.chargeAnimation, 10);
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 2;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).fps = 3;

            /* Animation settings for if you're using a gun with a looping firing animation. */
            //gun.usesContinuousFireAnimation = true; //Does continuous fire loop an animation, such as a minigun which spins up then loops the full spin animation?
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).loopStart = 1;  //frame to start looping (starts at zero)

            /* AddProjectileModuleFrom loads an existing projectile to use as a base, using GetById([ID]) reads in an existing vanilla gun to use.
             * ID 86 is the marine_sidearm which is a very basic bullet and good to use as a default.
             * ID 56 is the 38_special which is a good base for bullets that should point in the same direction as the gun is pointing.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun, true, false);
            //gun.muzzleFlashEffects = (PickupObjectDatabase.GetById((int)Items.Awp) as Gun).muzzleFlashEffects; //Loads a muzzle flash based on gun ID names.
            gun.muzzleFlashEffects = null;
            /* gunSwitchGroup loads in the firing and reloading sound effects.
             * Use an existing ID if you want to copy another gun's firing and reloading sounds, otherwise use a custom gunSwitchGroup name then assign your sound effects manually.
             * List of default sound files https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./sound-list
             * Instructions on setting up custom sound files https://mtgmodders.gitbook.io/etg-modding-guide/misc/using-custom-sounds */
            //gun.gunSwitchGroup = (PickupObjectDatabase.GetById(56) as Gun).gunSwitchGroup; //Example using a vanilla gun's ID.
            /* OR */
            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}"; //Unique name for your gun's sound group. In this example it uses your console name but with an underscore.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null); //Play_WPN_Gun_Shot_01 is your weapon's base shot sound.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", null); //Play_WPN_Gun_Reload_01 is your weapon's base reload sound.
            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_gun_finale_01", null);
            gun.DefaultModule.angleVariance = spreadAngle; //How far from where you're aiming that bullets can deviate. 0 equals perfect accuracy.
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic; //Sets the firing style of the gun.
            /* Optional settings for Burst style guns. */
            //gun.DefaultModule.burstShotCount = 3; //Number of shots per burst.
            //gun.DefaultModule.burstCooldownTime = 0.1f; //Time in between shots during a burst.
            gun.gunClass = GunClass.FULLAUTO; // Sets the gun's class which is used by category based effects.
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random; //Sets how the gun handles multiple different projectiles
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = 0f; //reloadDuration;
            gun.DefaultModule.cooldownTime = fireRateStat; //Time between shots fired.  For Burst guns it's the time between each burst.
            gun.DefaultModule.numberOfShotsInClip = ammoStat; //ammoStat;
            gun.SetBaseMaxAmmo(ammoStat);
            /* GunHandedness sets how the gun is held.
             * OneHanded and TwoHanded control how hands hold the gun.
             * AutoDetect will select between one and two handed based on the json/jtk2d.
             * HiddenOneHanded holds the gun with an invisible hand (think armcanons like the Megahand (Megabuster)).
             * NoHanded means the gun should not swing or aim at all, like the Crown of Guns which sits on the player's head. */
            gun.gunHandedness = GunHandedness.TwoHanded;
            /* carryPixelOffset sets the length and width away your character holds the gun. Values are subtle so use higher amounts like 10. */
            gun.carryPixelOffset += new IntVector2(11, 2); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(-16, -16); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(-4, 10); //offset when aiming up
            /* BarrelOffset sets the length and width away on the sprite where the barrel should end.
             * This is where the muzzle flash and projectile will appear. */
            gun.barrelOffset.transform.localPosition += new Vector3(8 / 16f, 0 / 16f);
            gun.gunScreenShake.magnitude = 0f; //How much the gun shakes the screen when fired.

            //gun.preventRotation = true; //Prevents the gun from rotating with aim direction -> will always face directly right or left.
            //gun.InfiniteAmmo = true; //Gives a gun infinite ammo. By default infinite ammo guns can't crack walls leading to secret rooms.
            //gun.CanGainAmmo = false; //Prevents a gun from being able to pick up ammo boxes.
            //gun.CanReloadNoMatterAmmo = true; //Allows a gun to trigger reload events if it has a full clip or zero ammo.
            //gun.GainsRateOfFireAsContinueAttack = true; //Makes gun shoot faster the longer fire is held (e.g., Vulcan Cannon).
            //gun.RateOfFireMultiplierAdditionPerSecond = rampUpFactor; //Used in conjunction with GainsRateOfFireAsContinueAttack.
            //gun.Volley.ModulesAreTiers = true; //Treats each individual volley of a gun as a tier that can be selected using gun.CurrentStrengthTier
            
            //unsure what this actually does
            //gun.weaponPanelSpriteOverride = ;


            //PROJECTILE BLOCK

            /* First line instantiates the projectile and uses an existing projectile to set default visuals and properties based on gun ID.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items.MarineSidearm) as Gun).DefaultModule.projectiles[0]);
            /* If you want to use a charged projectile from a gun with multiple stages of shots, use a format like the below */
            //Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(613) as Gun).singleModule.chargeProjectiles[1].Projectile);
            /* The following line can be used to output the full details of the properties of a given gun or projectile for if you'd like to replicated certain aspects.
             * This example takes the projectile we just set up then outputs the details to your game folder under Enter the Gungeon\Resources\defaultLog.txt */
            //Alexandria.Misc.DebugUtility.LogPropertiesAndFields<Projectile>(projectile, $"Projectile Properties and Fields: ");
            gun.DefaultModule.projectiles[0] = projectile; //Assigns the projectile to the gun.
            /* Adjust Impact Visuals */
            //projectile.hitEffects.alwaysUseMidair = true;  //Use end of range visual if it hits something
            //projectile.hitEffects.midairInheritsFlip = true; //Should impact be directional facing?
            //projectile.hitEffects.midairInheritsRotation = true; //Should the visual rotate with the gun's orientation?
            /* You can also copy individual properties using a format like this: */
            projectile.hitEffects.HasProjectileDeathVFX = false;
            projectile.hitEffects.deathAny = null;
            projectile.hitEffects.deathEnemy = null;
            projectile.hitEffects.enemy = (PickupObjectDatabase.GetById((int)Items.LuxinCannon) as Gun).DefaultModule.finalProjectile.hitEffects.enemy;
            projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById((int)Items.LuxinCannon) as Gun).DefaultModule.finalProjectile.hitEffects.tileMapHorizontal;
            projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById((int)Items.LuxinCannon) as Gun).DefaultModule.finalProjectile.hitEffects.tileMapVertical;

            //projectile.objectImpactEventName = "plasmarifle"; //starlet, zapper, plasmarifle, energy
            //projectile.enemyImpactEventName = "plasmarifle";

            /* The following block is needed so that cloned copies of your projectile have the same properties. */
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            /* Adjusting base properties */
            projectile.baseData.damage = projectileDamageStat;
            projectile.baseData.speed = projectileSpeedStat;
            projectile.baseData.range = projectileRangeStat;
            projectile.baseData.force = projectileForceStat; //Knockback strength
            projectile.transform.parent = gun.barrelOffset;
            projectile.shouldRotate = true; //If the projectile should rotate to match the direction it was shot and you don't want your default projectile's setting
            //projectile.AdditionalScaleMultiplier = 1f; //Further modify the projectile's size

            /* Optionally sets a custom Projectile Sprite if you don't want to use the default.
             * The first value is the sprite name in sprites\ProjectileCollection without the extension.
             * tk2dBaseSprite.Anchor.MiddleCenter controls where the sprite is anchored. MiddleCenter will work in most cases.
             * The first set of numbers is visual dimensions of the sprite while the last set of numbers is the hitbox.  Generally the hitbox should be a little smaller than the visuals. */
            projectile.SetProjectileSpriteRight("prayerbeads_projectile_003", 13, 9, true, tk2dBaseSprite.Anchor.MiddleCenter, 11, 7); //Note that your sprite will stretch to match the visual dimensions

            /*
            Projectile headshot = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            headshot.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(headshot.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(headshot);

            headshot.baseData.damage = projectileDamageStat * headshotDamageScale;
            headshot.baseData.speed = projectileSpeedStat * headshotDamageScale * 1.0f;
            headshot.baseData.range = projectileRangeStat * headshotDamageScale;
            headshot.baseData.force = projectileForceStat * headshotDamageScale; //Knockback strength
            headshot.transform.parent = gun.barrelOffset;
            headshot.shouldRotate = true;

            headshot.SetProjectileSpriteRight("hextech_peacemaker_projectile_glow", 11, 7, true, tk2dBaseSprite.Anchor.MiddleCenter, 9, 5);
            headshot.AdditionalScaleMultiplier = 2f;

            PierceProjModifier headshotPierce = headshot.gameObject.GetOrAddComponent<PierceProjModifier>();
            headshotPierce.penetration = projectilePierceStat * 2;
            headshotPierce.penetratesBreakables = true;

            gun.DefaultModule.finalProjectile = headshot;
            gun.DefaultModule.numberOfFinalProjectiles = 1;
            gun.DefaultModule.usesOptionalFinalProjectile = true;
            */

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
            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = "Rifle";


            /* OR */
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("prayerbeads_ammo",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/prayerbeads_ammo_full_001", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/prayerbeads_ammo_empty_001");
            /* If your gun uses special ammo for its final shot, use the below settings similar to the above */
            //gun.DefaultModule.finalAmmoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.finalCustomAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("hextech_peacemaker_ammo",
            //    "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_peacemaker_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_peacemaker_ammo_empty");

            // Casings and Clips
            /* Casings are the individual bullet shells and clips are the holders that are ejected from the gun.
             * Casings can be ejected on firing and reloading while clips can only be ejected on reload.
             * You can either use existing casings/clips from vanilla guns or add custom ones using a similar sprite import process as above with ammo.
             * Custom casings can also have their properties edited by adding more parameters to GenerateDebrisObject.*/
            //gun.shellCasing = (PickupObjectDatabase.GetById((int)Items.M1)as Gun).shellCasing; //Example using AK-47 casings.
            gun.shellCasing = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_casing").gameObject; //Example using a custom sprite as a casing.
            //gun.clipObject = (PickupObjectDatabase.GetById((int)Items.M1) as Gun).clipObject; //Example using AK-47 clips.
            gun.clipObject = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_clip").gameObject;

            gun.shellsToLaunchOnFire = 0; //Number of shells to eject when shooting.
            gun.shellsToLaunchOnReload = 0; //Number of shells to eject when reloading (revolvers for example).
            gun.clipsToLaunchOnReload = 0; //Number of clips to eject when reloading.
            gun.reloadClipLaunchFrame = 0;

            // Bullet Trail
            EasyTrailBullet trail = projectile.gameObject.AddComponent<EasyTrailBullet>();
            trail.TrailPos = projectile.transform.position;
            trail.StartWidth = 0.3f;
            trail.EndWidth = 0f;
            trail.LifeTime = 0.1f; //How long the trail lingers
            // BaseColor sets an overall color for the trail. Start and End Colors are subtractive to it. 
            trail.BaseColor = new Color(173 / 255f, 180 / 255f, 255 / 255f); //Set to white if you don't want to interfere with Start/End Colors.
            trail.StartColor = ExtendedColours.pastelPurple;
            trail.EndColor = Color.white; //Custom Orange example using r/g/b values.

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

            //Pierce Enemies
            //PierceProjModifier pierce = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();  //initialize piercing.
            //pierce.penetration = projectilePierceStat; //How many enemies it can pierce.
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

            /* Hungry allows bullets to consume enemy bullets, getting larger and gaining effects like damage when it happens. */
            //HungryProjectileModifier eat = projectile.gameObject.AddComponent<HungryProjectileModifier>(); //Initilaize the property.
            //eat.HungryRadius = 5f; //How far away the bullet can eat other bullets from.
            //eat.DamagePercentGainPerSnack = 0.5f; //Increases damage based on how many bullets eaten.
            //eat.MaximumBulletsEaten = 3; //Limit how many bullets each shot can eat.
            //eat.MaxMultiplier = 3f; //Limit how high the damage increase can get.

            //On-Hit Effects
            /* bleed */
            //projectile.AppliesBleed = true;
            //projectile.BleedApplyChance = 0.5f;
            /* Charm */
            //projectile.AppliesCharm = true;
            //projectile.CharmApplyChance = 0.5f;
            /* Cheese */
            //projectile.AppliesCheese = true;
            //projectile.CheeseApplyChance = 0.5f;
            /* Fire */
            //projectile.AppliesFire = true;
            //projectile.FireApplyChance = 0.5f;
            /* freeze */
            //projectile.AppliesFreeze = true;
            //projectile.FreezeApplyChance = 0.5f;
            /* poison */
            //projectile.AppliesPoison = true;
            //projectile.PoisonApplyChance = 0.5f;
            /* stun */
            //projectile.AppliesStun = true;
            //projectile.StunApplyChance = 0.5f;
            //projectile.AppliedStunDuration = 2f;

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


            // CHARGE PROJECTILE
            /*
            // The following settings are for if your gun uses additional projectiles when charged.
            // In general, you would assign properties in the same way as your "projectile" but by just using different names like "chargedprojectile" instead. 
            // You also would not assign the projectiles to the gun with DefaultModule since they will get called by the charging code.
            Projectile chargeprojectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(274) as Gun).DefaultModule.projectiles[0]); //Initialize chargedprojectile.
            chargeprojectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeprojectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeprojectile);
            chargeprojectile.baseData.damage = 30f;
            chargeprojectile.baseData.speed = 25f;
            chargeprojectile.baseData.range = 100f;
            chargeprojectile.baseData.force = 5f;
            chargeprojectile.transform.parent = gun.barrelOffset;
            chargeprojectile.AdditionalScaleMultiplier = 2f;
            // Add shooting sounds to your pojectiles by incrementing Play_WPN_Gun_Shot_01.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_02", "Play_WPN_grasshopper_shot_01"); //Sound for second projectile.

            // Sets up a third projectile to go with a third charge tier.
            Projectile chargeprojectile2 = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(39) as Gun).DefaultModule.projectiles[0]); //Initialize chargedprojectile.
            chargeprojectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(chargeprojectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(chargeprojectile2);
            chargeprojectile2.baseData.damage = 70f;
            chargeprojectile2.baseData.speed = 25f;
            chargeprojectile2.baseData.range = 100f;
            chargeprojectile2.baseData.force = 10f;
            chargeprojectile2.transform.parent = gun.barrelOffset;
            chargeprojectile2.AdditionalScaleMultiplier = 3f;
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_03", "Play_WPN_bsg_shot_01"); //Sound for third projectile.
            
            

            // Sets the properties of charging including the time it takes to charge.
            ProjectileModule.ChargeProjectile chargeProj1 = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile, //Assigns a projectile to the charge state
                ChargeTime = 0f, //How long to hold the trigger before getting to this charge state.  Set the first one to 0 to allow regular firing the base shot.
            };
            ProjectileModule.ChargeProjectile chargeProj2 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeprojectile,
                ChargeTime = 1f,
            };
            ProjectileModule.ChargeProjectile chargeProj3 = new ProjectileModule.ChargeProjectile
            {
                Projectile = chargeprojectile2,
                ChargeTime = 3f, //Means this charge state will be reached after 3 seconds total, not 3 seconds since the previous state.
            };
            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> {chargeProj1, chargeProj2, chargeProj3}; //Assigns charged projectiles to the gun.
            */

            // custom vfx creation
            List<string> spreadVFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_01",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_02",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_03",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_04",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_05",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_06",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_07",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_08",
                "LOLItems/Resources/vfxs/test_vfx/test_vfx_09"
            };

            GameObject spreadVFXPrefab = VFXBuilder.CreateVFX
            (
                "prayer_beads_spread_vfx",
                spreadVFXSpritePath,
                10,
                new IntVector2(5, 5),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1, 
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Once, 
                false
            );

            /*
            GameObject yourObject = ...;
            tk2dSprite oldSprite = yourObject.GetComponent<tk2dSprite>();
            int oldSpriteId = oldSprite.spriteId;
            var oldSpriteCollection = oldSprite.spriteqCollection;
            UnityEngine.Object.Destroy(oldSprite);
            tk2dTiledSprite newSprite -yourObject.AddComponent<tk2dTiledSprite>();
            newSprite.SetSprite(oldSpriteCollection, oldSpriteId);
            */

            // add spread module
            ComplexProjectileModifier shockRounds = PickupObjectDatabase.GetById((int)Items.ShockRounds) as ComplexProjectileModifier;
            CustomLightningChainEnemiesModifierAOE chain = projectile.gameObject.GetOrAddComponent<CustomLightningChainEnemiesModifierAOE>();
            chain.LinkVFXPrefab = shockRounds.ChainLightningVFX;
            chain.PlaysSFX = true;
            string[] sfxList = {
                    "statikk_shiv_lightning_SFX_1",
                    "statikk_shiv_lightning_SFX_2",
                    "statikk_shiv_lightning_SFX_3",
                    "statikk_shiv_lightning_SFX_4",
                    "statikk_shiv_lightning_SFX_5"
                };
            chain.updateSFXList(sfxList);
            chain.DamagesEnemies = true;
            chain.usesStaticDamageStat = false;
            chain.damageScale = 0.3f;
            chain.damageTypes = CoreDamageTypes.Magic;
            chain.maximumLinkDistance = projSpreadRange;
            //chain.damagePerHit = projectileDamageStat * 0.3f;
            chain.UsesDispersalParticles = false;
            chain.DispersalDensity = 5f;
            chain.DispersalMaxCoherency = 0.7f;
            chain.DispersalMinCoherency = 0.3f;

            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_02", null);

            gun.quality = PickupObject.ItemQuality.B; //Sets the gun's quality rank. Use "EXCLUDED" if the gun should not appear in chests.
            ETGMod.Databases.Items.Add(gun, false, "ANY");  //Adds your gun to the databse.
            //gun.AddToSubShop(ItemBuilder.ShopType.Trorc); //Select which sub shops during a run can carry the gun
            //gun.AddToSubShop(ItemBuilder.ShopType.Flynt);
            gun.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = gun.PickupObjectId; //Sets the Gun ID. 
        }

        public override void OnDropped()
        {
            //base.gun.SetupSprite(null, $"prayerbeads_icon_001", 1);

            //BraveUtility.Swap(ref this.gun.idleAnimation, ref this.gun.alternateIdleAnimation);

            base.OnDropped();
        }

        public override void OnInitializedWithOwner(GameActor actor)
        {
            //PlayerController player = this.Owner as PlayerController;

            //base.gun.SetupSprite(null, $"prayerbeads_idle_001", 10);

            //BraveUtility.Swap(ref this.gun.idleAnimation, ref this.gun.alternateIdleAnimation);

            base.OnInitializedWithOwner(actor);
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            //PlayerController player = projectile.Owner as PlayerController;
            /*
            if (projectile.GetCachedBaseDamage == projectileDamageStat)
            {
                //Plugin.Log("regular sound");
                HelpfulMethods.PlayRandomSFX(player.gameObject, normalFiringSFXList);
            }
            else if (projectile.GetCachedBaseDamage == projectileDamageStat * headshotDamageScale)
            {
                //Plugin.Log("headshot sound");
                HelpfulMethods.PlayRandomSFX(player.gameObject, headshotSFXList);
            }
            else
            {
                Plugin.Log("fuck");
            }
            */
            //HelpfulMethods.PlayRandomSFX(projectile.Owner.gameObject, normalFiringSFXList);
            //projectile.OnHitEnemy += HandleHitEnemy;
            base.PostProcessProjectile(projectile);
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            HelpfulMethods.PlayRandomSFX(gun.gameObject, normalFiringSFXList);

            base.OnPostFired(player, gun);
        }

        /* Spread module
        private void HandleHitEnemy(Projectile proj, SpeculativeRigidbody enemyRigidbody, bool fatal)
        {
            if (enemyRigidbody == null || enemyRigidbody.aiActor == null) return;

            AIActor firstEnemy = enemyRigidbody.aiActor;

            // Build chain starting from the first hit enemy
            List<AIActor> chain = ChainEnemies(firstEnemy.CenterPosition);

            //if (PlaysSFX) PlayLightningSFX(firstEnemy);
            if (PlaysSFX) HelpfulMethods.PlayRandomSFX(firstEnemy.gameObject, sfxPath);

            AkSoundEngine.PostEvent("vineboom", proj.gameObject);

            if (chain.Count > 0)
            {
                // No need to: Include the initial enemy as the first link
                // chain.Insert(0, firstEnemy);
                UpdateLinkChain(chain);
            }
        }

        private List<AIActor> ChainEnemies(Vector2 startPos)
        {
            List<AIActor> chain = new List<AIActor>();

            float closestDistSq = maximumLinkDistance * maximumLinkDistance;

            // this forloop loops to find the closest target out of EVERY SINGLE enemy in the room.
            foreach (AIActor enemy in StaticReferenceManager.AllEnemies)
            {
                if (enemy == null || !enemy.healthHaver || enemy.healthHaver.IsDead)
                    continue;

                if (!enemy.IsNormalEnemy || !enemy.HasBeenEngaged)
                    continue;

                // sets distSq = the distance between targetEnemy and currentPos ^ 2
                float distSq = (enemy.CenterPosition - startPos).sqrMagnitude;
                // if the distance between them is less than the max distance
                if (distSq < closestDistSq)
                {
                    // adds targetEnemy to chain list
                    chain.Add(enemy);
                }
            }

            return chain;
        }

        /*
        private List<AIActor> ChainEnemies(Vector2 startPos)
        {
            List<AIActor> chain = new List<AIActor>();
            HashSet<AIActor> visited = new HashSet<AIActor>();

            Vector2 currentPos = startPos;

            for (int i = 0; i < maxLinkCount; i++)
            {
                AIActor closest = null;
                float closestDistSq = maximumLinkDistance * maximumLinkDistance;

                // this forloop loops to find the closest target out of EVERY SINGLE enemy in the room.
                foreach (AIActor enemy in StaticReferenceManager.AllEnemies)
                {
                    if (enemy == null || !enemy.healthHaver || enemy.healthHaver.IsDead)
                        continue;

                    if (!enemy.IsNormalEnemy || !enemy.HasBeenEngaged || visited.Contains(enemy))
                        continue;

                    // sets distSq = the distance between targetEnemy and currentPos ^ 2
                    float distSq = (enemy.CenterPosition - currentPos).sqrMagnitude;
                    // if the distance between them is less than the max distance
                    if (distSq < closestDistSq)
                    {
                        // sets closest to targetEnemy and closestDistSq to distSq
                        closest = enemy;
                        closestDistSq = distSq;
                        // could modify to make it stop looping once it finds any target within the max distance
                    }
                }

                // if a closest enemy is found, add them to the chain list and the exclusion list
                if (closest != null)
                {
                    chain.Add(closest);
                    visited.Add(closest);
                    //currentPos = closest.CenterPosition; // no need to change position since its not chaining to and from each target
                }
                else
                {
                    break; // no more valid enemies in range
                }
            }

            return chain;
        }

        private void UpdateLinkChain(List<AIActor> chain)
        {
            Vector2 prevPos = base.projectile.specRigidbody.UnitCenter;

            foreach (AIActor enemy in chain)
            {
                Vector2 endPos = enemy.specRigidbody.UnitCenter;

                // Spawn VFX link segment
                GameObject vfxObj = SpawnManager.SpawnVFX(LinkVFXPrefab, true);
                tk2dTiledSprite link = vfxObj.GetComponent<tk2dTiledSprite>();

                link.transform.position = prevPos;

                Vector2 delta = endPos - prevPos;
                float angle = BraveMathCollege.Atan2Degrees(delta.normalized);
                int length = Mathf.RoundToInt(delta.magnitude / 0.0625f);

                link.dimensions = new Vector2(length, link.dimensions.y);
                link.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                link.UpdateZDepth();

                Destroy(vfxObj, 0.25f);

                // Damage enemy (with cooldown so they don’t get hit every frame)
                if (!m_damagedEnemies.Contains(enemy))
                {
                    enemy.healthHaver.ApplyDamage(damagePerHit, Vector2.zero, "Cultivation of Spirit", damageTypes);
                    if (UsesDispersalParticles)
                    {
                        //DoDispersalParticles(prevPos, endPos);
                    }
                    GameManager.Instance.StartCoroutine(HandleDamageCooldown(enemy));
                }

                // No need to: Move chain forward
                //prevPos = endPos;
            }
        }

        
        private IEnumerator HandleDamageCooldown(AIActor damagedTarget)
        {
            m_damagedEnemies.Add(damagedTarget);
            yield return new WaitForSeconds(damageCooldown);
            m_damagedEnemies.Remove(damagedTarget);
        }

        private void ClearLink()
        {
            if (m_extantLink != null)
            {
                SpawnManager.Despawn(m_extantLink.gameObject);
                m_extantLink = null;
            }
        }
        */

        public override void OnReload(PlayerController player, Gun gun)
        {
            /*
            Plugin.Log("fuck");
            float updatedSpreadDamageStat = projectileDamageStat * player.stats.GetStatValue(PlayerStats.StatType.Damage) * 0.3f;
            this.projectile.gameObject.GetComponent<CustomLightningChainEnemiesModifierAOE>().damagePerHit = updatedSpreadDamageStat;
            Plugin.Log($"projectileDamageStat: {projectileDamageStat}, player's damage stat: {player.stats.GetStatValue(PlayerStats.StatType.Damage)}, " +
                $"updated spread damage stat: {this.projectile.gameObject.GetComponent<CustomLightningChainEnemiesModifierAOE>().damagePerHit}, {updatedSpreadDamageStat}");
            */

            BraveUtility.Swap(ref this.gun.idleAnimation, ref this.gun.alternateIdleAnimation);

            base.OnReload(player, gun);
        }
    }
}