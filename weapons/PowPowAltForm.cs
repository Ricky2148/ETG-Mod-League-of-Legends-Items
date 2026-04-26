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
using HutongGames.PlayMaker.Actions;

// gotta figure out how to make the sprites .json files without taking 15 centuries
// retry making the explosions larger

namespace LOLItems.weapons
{
    internal class PowPowAltForm : AdvancedGunBehavior
    {
        public static string internalName = "Pow-Pow_AltForm"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Fishbones"; //The actual name of the gun that shows up ingame.

        private static float rampUpIncCap = 5f;
        private static float rampUpIncPerSecond = 0.5f;
        private static int ammoStat = 600;
        private static float reloadDuration = 1.6f;
        private static float fireRateStat = 0.3f * 1.25f;
        private static int spreadAngle = 5;

        private float currentRampUpIncAmount = 0f;
        private int rampUpStacks = 0;
        private float rampUpIncPerStack = 0.01f;
        private int rampUpStackCap = 20; // max fire rate is fireRateStat - (rampUpStackCap * rampUpIncPerStack)
        private Coroutine rampDownCoroutine;

        private static float projectileDamageStat = 6f;
        private static float projectileSpeedStat = 50f;
        private static float projectileRangeStat = 35f;
        private static float projectileForceStat = 15f;

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
            string FULLNAME = realName; //Full name of your gun 
            string SPRITENAME = "fishbones"; //The name that prefixes your sprite files
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName); //Renames the default internal name to your custom internal name
            gun.gameObject.AddComponent<PowPowAltForm>(); //AddComponent<[ClassName]>
            gun.SetShortDescription("Jinx's Fishbones");  //The description that pops up when you pick up the gun.
            gun.SetLongDescription("idk man"); //The full description in the Ammonomicon.
            /* SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.  Your "..._idle_001" is often a good example.  
             * A copy of the sprite used must be in your "sprites/Ammonomicon Encounter Icon Collection/" folder.
             * The variable at the end assigns a default FPS to all other animations. */
            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 1);
            /* You can also manually assign the FPS of indivisual animations, below are some examples.
             * Note that if your animation takes too long it might not get to finish, like if your reload animation takes longer than the act of reloading. */
            gun.SetAnimationFPS(gun.shootAnimation, 27);
            gun.SetAnimationFPS(gun.reloadAnimation, 20);

            /* You can also optionally add an intro animation that plays when picking up the gun by using the below line and also set the FPS the same as above. */

            //tk2dSpriteAnimationClip introClip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_intro"); //by default uses sprites with the "_intro" suffix
            //gun.SetAnimationFPS(gun.introAnimation, 15);

            //tk2dSpriteAnimationClip outroClip = gun.spriteAnimator.GetClipByName($"{SPRITENAME}_outro"); //by default uses sprites with the "_outro" suffix
            //gun.SetAnimationFPS(gun.finalShootAnimation, 15);

            /* Animation settings for if you're using a gun that charges. */
            //gun.SetAnimationFPS(gun.chargeAnimation, 10);
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 2;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).fps = 3;

            //Animation settings for if you're using a gun with a looping firing animation.
            //gun.usesContinuousFireAnimation = true; //Does continuous fire loop an animation, such as a minigun which spins up then loops the full spin animation?
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            //gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).loopStart = 1;  //frame to start looping (starts at zero)

            /* AddProjectileModuleFrom loads an existing projectile to use as a base, using GetById([ID]) reads in an existing vanilla gun to use.
             * ID 86 is the marine_sidearm which is a very basic bullet and good to use as a default.
             * ID 56 is the 38_special which is a good base for bullets that should point in the same direction as the gun is pointing.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(16) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(2) as Gun).muzzleFlashEffects; //Loads a muzzle flash based on gun ID names.

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
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic; //Sets the firing style of the gun.
            /* Optional settings for Burst style guns. */
            //gun.DefaultModule.burstShotCount = 3; //Number of shots per burst.
            //gun.DefaultModule.burstCooldownTime = 0.1f; //Time in between shots during a burst.
            gun.gunClass = GunClass.FULLAUTO; // Sets the gun's class which is used by category based effects.
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random; //Sets how the gun handles multiple different projectiles
            gun.DefaultModule.ammoCost = 3;
            gun.reloadTime = 0f;
            gun.DefaultModule.cooldownTime = fireRateStat; //Time between shots fired.  For Burst guns it's the time between each burst.
            gun.DefaultModule.numberOfShotsInClip = 9999;
            gun.SetBaseMaxAmmo(ammoStat);

            gun.DefaultModule.angleFromAim = 0f; //Degree offset from your aim angle the module fires. Useful for guns with multiple modules that fire in a spread.

            /* GunHandedness sets how the gun is held.
             * OneHanded and TwoHanded control how hands hold the gun.
             * AutoDetect will select between one and two handed based on the json/jtk2d.
             * HiddenOneHanded holds the gun with an invisible hand (think armcanons like the Megahand (Megabuster)).
             * NoHanded means the gun should not swing or aim at all, like the Crown of Guns which sits on the player's head. */
            gun.gunHandedness = GunHandedness.OneHanded;

            /* carryPixelOffset sets the length and width away your character holds the gun. Values are subtle so use higher amounts like 10. */
            gun.carryPixelOffset += new IntVector2(2, 2); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(0, 0); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(0, 0); // offset when aiming up

            /* BarrelOffset sets the length and width away on the sprite where the barrel should end.
             * This is where the muzzle flash and projectile will appear. */
            gun.barrelOffset.transform.localPosition += new Vector3(17 / 16f, -2 / 16f);

            gun.gunScreenShake.magnitude = 0.3f; //How much the gun shakes the screen when fired.

            //gun.preventRotation = true; //Prevents the gun from rotating with aim direction -> will always face directly right or left.
            //gun.InfiniteAmmo = true; //Gives a gun infinite ammo. By default infinite ammo guns can't crack walls leading to secret rooms.
            //gun.CanGainAmmo = false; //Prevents a gun from being able to pick up ammo boxes.
            //gun.CanReloadNoMatterAmmo = true; //Allows a gun to trigger reload events if it has a full clip or zero ammo.
            gun.GainsRateOfFireAsContinueAttack = false; //Makes gun shoot faster the longer fire is held (e.g., Vulcan Cannon).
            //gun.RateOfFireMultiplierAdditionPerSecond = rampUpFactor; //Used in conjunction with GainsRateOfFireAsContinueAttack.
            //gun.Volley.ModulesAreTiers = true; //Treats each individual volley of a gun as a tier that can be selected using gun.CurrentStrengthTier


            //PROJECTILE BLOCK

            /* First line instantiates the projectile and uses an existing projectile to set default visuals and properties based on gun ID.
             * Full list of IDs and names can be found here https://raw.githubusercontent.com/ModTheGungeon/ETGMod/master/Assembly-CSharp.Base.mm/Content/gungeon_id_map/items.txt
             * List of visual effects https://enterthegungeon.wiki.gg/wiki/Weapon_Visual_Effects */
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);

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
            //projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById(41) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            //projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById(41) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

            /* The following block is needed so that cloned copies of your projectile have the same properties. */
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            /* Adjusting base properties */
            projectile.baseData.damage = 0;
            projectile.baseData.speed = projectileSpeedStat * 0.75f;
            projectile.baseData.range = projectileRangeStat;
            projectile.baseData.force = projectileForceStat; //Knockback strength

            projectile.transform.parent = gun.barrelOffset;

            projectile.AdditionalScaleMultiplier = 1f;

            // additional projectile properties
            projectile.GetComponent<RobotechProjectile>().searchRadius = 0f;
            projectile.GetComponent<RobotechProjectile>().angularAcceleration = 0f;
            //Destroy(projectile.GetComponent<RobotechProjectile>());

            ExplosionData explosion = projectile.GetComponent<ExplosiveModifier>().explosionData = new ExplosionData();
            explosion.doDamage = true;
            explosion.doForce = false;
            explosion.damage = projectileDamageStat * 2;
            //explosion.damageRadius = 5f;
            explosion.damageRadius = 1f;
            explosion.pushRadius = 1f;
            explosion.forceUseThisRadius = false;
            explosion.breakSecretWalls = false;
            explosion.forcePreventSecretWallDamage = false;
            explosion.doDestroyProjectiles = false;
            explosion.doScreenShake = true;
            
            /*
            var sprite = explosion.effect.GetComponent<tk2dSprite>();
            if (sprite != null)
            {
                sprite.scale = new Vector3(3f, 3f, 1f);
                sprite.UpdateZDepth();
            }
            */


            // this changes the entire gun to be the ghost lamp??????????
            //projectile.GetComponent<Explosion>().radius = 5f;

            //projectile.shouldRotate = true; //If the projectile should rotate to match the direction it was shot and you don't want your default projectile's setting
            //projectile.AdditionalScaleMultiplier = 1f; //Further modify the projectile's size

            /* Optionally sets a custom Projectile Sprite if you don't want to use the default.
             * The first value is the sprite name in sprites\ProjectileCollection without the extension.
             * tk2dBaseSprite.Anchor.MiddleCenter controls where the sprite is anchored. MiddleCenter will work in most cases.
             * The first set of numbers is visual dimensions of the sprite while the last set of numbers is the hitbox.  Generally the hitbox should be a little smaller than the visuals. */
            //projectile.SetProjectileSpriteRight($"{SPRITEPATH}_projectile_001", 20, 8, true, tk2dBaseSprite.Anchor.MiddleCenter, 18, 6); //Note that your sprite will stretch to match the visual dimensions

            //OPTIONAL ADDITIONAL PROPERTIES
            /* Properties default to whatever you copied your base gun from, but you an adjust them manually as needed. */

            // Ammo
            /* These two sections control what graphic is used on your ammo indicator.
             * A list of ammo types can be found here: https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./all-custom-ammo-types
             * The first entry takes one of the thirteen basic ammo types: 
             * SMALL_BULLET, MEDIUM_BULLET, BEAM, GRENADE, SHOTGUN, SMALL_BLASTER, MEDIUM_BLASTER, NAIL, MUSKETBALL, ARROW, MAGIC, BLUE_SHOTGUN, SKULL, FISH.*/

            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = "rocket";
            /* If the ammo type you want isn't one of those thirteen, then instead use CUSTOM followed by the customAmmoType you want.
             * If you want to make your own customAmmoType then instead use use CUSTOM and then the AddCustomAmmoType.
             * AddCustomAmmoType takes a name for the ammo, then paths to the EMBEDDED filled and emptied ammo sprites.
             * Instructions to embed a sprite https://mtgmodders.gitbook.io/etg-modding-guide/all-things-spriting/importing-a-sprite-to-visual-studios */
            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = "samus";
            /* OR */

            gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("Template Gun Orbs",
                "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/fishbones_clipfull_nooutline", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/fishbones_clipempty_nooutline");
            /* If your gun uses special ammo for its final shot, use the below settings similar to the above */
            //gun.DefaultModule.finalAmmoType = GameUIAmmoType.AmmoType.FISH;
            //gun.DefaultModule.finalCustomAmmoType = "burning hand";

            // Casings and Clips
            /* Casings are the individual bullet shells and clips are the holders that are ejected from the gun.
             * Casings can be ejected on firing and reloading while clips can only be ejected on reload.
             * You can either use existing casings/clips from vanilla guns or add custom ones using a similar sprite import process as above with ammo.
             * Custom casings can also have their properties edited by adding more parameters to GenerateDebrisObject.*/
            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing; //Example using AK-47 casings.
            //gun.shellCasing = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/tempgun_clipempty").gameObject; //Example using a custom sprite as a casing.
            gun.clipObject = (PickupObjectDatabase.GetById(15) as Gun).clipObject; //Example using AK-47 clips.
            gun.shellsToLaunchOnFire = 0; //Number of shells to eject when shooting.
            gun.shellsToLaunchOnReload = 0; //Number of shells to eject when reloading (revolvers for example).
            gun.clipsToLaunchOnReload = 0; //Number of clips to eject when reloading.



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

            gun.quality = PickupObject.ItemQuality.EXCLUDED; //Sets the gun's quality rank. Use "EXCLUDED" if the gun should not appear in chests.
            ETGMod.Databases.Items.Add(gun, false, "ANY");  //Adds your gun to the databse.
            //gun.AddToSubShop(ItemBuilder.ShopType.Trorc); //Select which sub shops during a run can carry the gun
            //gun.AddToSubShop(ItemBuilder.ShopType.Flynt);
            ID = gun.PickupObjectId; //Sets the Gun ID. 
        }
    }
}