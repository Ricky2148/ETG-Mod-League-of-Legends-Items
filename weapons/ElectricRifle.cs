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

// might want to change the headshot mechanic to be stacking on hit instead of being a final projectile. any clip size modifiers make it not 5 shots then 1 headshot
// balance this: what rarity dps etc.
// new synergy of shock rifle recharging

namespace LOLItems.weapons
{
    internal class ElectricRifle : AdvancedGunBehavior
    {
        public static string internalName = "Electric Rifle"; //Internal name of the gun as used by console commands
        public static int ID; //The Gun ID stored by the game.  Can be used by other functions to call your custom gun.
        public static string realName = "Zaunite Rifle"; //The name that shows up in the Ammonomicon and the mod console.

        private static int ammoStat = 1000; // seems to be a limit of 1000 on ammo gun can have on pickup for some reason
        private static int clipStat = 70;
        private static float reloadDuration = 1.0f;
        private static float fireRateStat = 0.67f;
        private static int spreadAngle = 5;

        private static float projectileDamageStat = 3.5f;
        private static float projectileSpeedStat = 40f;
        private static float projectileRangeStat = 15f;
        private static float projectileForceStat = 6f;

        private static int burstCount = 7; //7
        private static float burstFireInterval = 0.02f;
        private static float adjustedFireRate = fireRateStat - (burstCount * burstFireInterval);

        private static List<string> normalFiringSFXList = new List<string>
        {
            "shockSMG_fire_001",
            "shockSMG_fire_002",
            "shockSMG_fire_003",
            "shockSMG_fire_004",
            "shockSMG_fire_005",
            "shockSMG_fire_006"
        };

        private bool initialShotInBurst = true;
        private int ammoInClipTracker = 0;

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
            string SPRITENAME = "shockSMG"; //The name that prefixes your sprite files
            internalName = $"LOLItems:{internalName.ToID()}";
            Gun gun = ETGMod.Databases.Items.NewGun(FULLNAME, SPRITENAME);
            Game.Items.Rename($"outdated_gun_mods:{FULLNAME.ToID()}", internalName); //Renames the default internal name to your custom internal name
            gun.gameObject.AddComponent<ElectricRifle>(); //AddComponent<[ClassName]>
            gun.SetShortDescription("\"I am Lightning!\"");  //The description that pops up when you pick up the gun.
            gun.SetLongDescription("A small compact rifle that appears to run on electricity rather than ammunition. Another weapon of shockingly " +
                "amazing engineering despite its appearances. It tends to shock you occasionally, which is a bit annoying.\n"); //The full description in the Ammonomicon.
            /* SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.  Your "..._idle_001" is often a good example.  
             * A copy of the sprite used must be in your "sprites/Ammonomicon Encounter Icon Collection/" folder.
             * The variable at the end assigns a default FPS to all other animations. */
            gun.SetupSprite(null, $"{SPRITENAME}_idle_001", 8);
            /* You can also manually assign the FPS of indivisual animations, below are some examples.
             * Note that if your animation takes too long it might not get to finish, like if your reload animation takes longer than the act of reloading. */
            gun.SetAnimationFPS(gun.shootAnimation, 43);
            //gun.SetAnimationFPS(gun.criticalFireAnimation, 5);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
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
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById((int)Items._38Special) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById((int)Items.RogueSpecial) as Gun).muzzleFlashEffects; //Loads a muzzle flash based on gun ID names.
            /* gunSwitchGroup loads in the firing and reloading sound effects.
             * Use an existing ID if you want to copy another gun's firing and reloading sounds, otherwise use a custom gunSwitchGroup name then assign your sound effects manually.
             * List of default sound files https://mtgmodders.gitbook.io/etg-modding-guide/various-lists-of-ids-sounds-etc./sound-list
             * Instructions on setting up custom sound files https://mtgmodders.gitbook.io/etg-modding-guide/misc/using-custom-sounds */
            //gun.gunSwitchGroup = (PickupObjectDatabase.GetById(56) as Gun).gunSwitchGroup; //Example using a vanilla gun's ID.
            /* OR */
            gun.gunSwitchGroup = $"LOLItems_{FULLNAME.ToID()}"; //Unique name for your gun's sound group. In this example it uses your console name but with an underscore.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", null); //Play_WPN_Gun_Shot_01 is your weapon's base shot sound.
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_WPN_plasmacell_reload_01"); //Play_WPN_Gun_Reload_01 is your weapon's base reload sound.
            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_gun_finale_01", null);
            gun.DefaultModule.angleVariance = spreadAngle; //How far from where you're aiming that bullets can deviate. 0 equals perfect accuracy.
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Burst; //Sets the firing style of the gun.
            /* Optional settings for Burst style guns. */
            gun.DefaultModule.burstShotCount = burstCount; //Number of shots per burst.
            gun.DefaultModule.burstCooldownTime = burstFireInterval; //Time in between shots during a burst.
            
            gun.gunClass = GunClass.RIFLE; // Sets the gun's class which is used by category based effects.
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random; //Sets how the gun handles multiple different projectiles
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = reloadDuration;
            gun.DefaultModule.cooldownTime = adjustedFireRate; //Time between shots fired.  For Burst guns it's the time between each burst.
            gun.DefaultModule.numberOfShotsInClip = clipStat;
            gun.SetBaseMaxAmmo(ammoStat);
            /* GunHandedness sets how the gun is held.
             * OneHanded and TwoHanded control how hands hold the gun.
             * AutoDetect will select between one and two handed based on the json/jtk2d.
             * HiddenOneHanded holds the gun with an invisible hand (think armcanons like the Megahand (Megabuster)).
             * NoHanded means the gun should not swing or aim at all, like the Crown of Guns which sits on the player's head. */
            gun.gunHandedness = GunHandedness.OneHanded;
            /* carryPixelOffset sets the length and width away your character holds the gun. Values are subtle so use higher amounts like 10. */
            gun.carryPixelOffset += new IntVector2(0, 2); //offset when holding gun vertically
            gun.carryPixelDownOffset += new IntVector2(0, 0); //offset when aiming down
            gun.carryPixelUpOffset += new IntVector2(0, 0); //offset when aiming up
            /* BarrelOffset sets the length and width away on the sprite where the barrel should end.
             * This is where the muzzle flash and projectile will appear. */
            gun.barrelOffset.transform.localPosition += new Vector3(8 / 16f, 2 / 16f);
            gun.gunScreenShake.magnitude = 0.15f; //How much the gun shakes the screen when fired.

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
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById((int)Items._38Special) as Gun).DefaultModule.projectiles[0]);
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
            projectile.hitEffects.HasProjectileDeathVFX = true;
            projectile.hitEffects.deathAny = (PickupObjectDatabase.GetById((int)Items.ShockRifle) as Gun).DefaultModule.projectiles[0].hitEffects.deathAny;
            projectile.hitEffects.deathEnemy = (PickupObjectDatabase.GetById((int)Items.ShockRifle) as Gun).DefaultModule.projectiles[0].hitEffects.deathEnemy;
            projectile.hitEffects.enemy = (PickupObjectDatabase.GetById((int)Items.ShockRifle) as Gun).DefaultModule.projectiles[0].hitEffects.enemy;
            projectile.hitEffects.tileMapHorizontal = (PickupObjectDatabase.GetById((int)Items.ShockRifle) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapHorizontal;
            projectile.hitEffects.tileMapVertical = (PickupObjectDatabase.GetById((int)Items.ShockRifle) as Gun).DefaultModule.projectiles[0].hitEffects.tileMapVertical;

            projectile.objectImpactEventName = "shockSMG1"; //starlet, zapper, plasmarifle, energy
            projectile.enemyImpactEventName = "shockSMG2";

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
            projectile.SetProjectileSpriteRight("shockSMG_projectile_001", 16, 7, true, tk2dBaseSprite.Anchor.MiddleCenter, 14, 5); //Note that your sprite will stretch to match the visual dimensions

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
            gun.DefaultModule.customAmmoType = "green_beam";


            /* OR */
            //gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.customAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("electric_rifle_ammo", 
                //"LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/electric_rifle_clipfull", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/electric_rifle_clipempty");
                //"LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/shockSMG_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/shockSMG_ammo_empty");
            /* If your gun uses special ammo for its final shot, use the below settings similar to the above */
            //gun.DefaultModule.finalAmmoType = GameUIAmmoType.AmmoType.CUSTOM;
            //gun.DefaultModule.finalCustomAmmoType = CustomClipAmmoTypeToolbox.AddCustomAmmoType("hextech_peacemaker_ammo", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_peacemaker_ammo_full", "LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_peacemaker_ammo_empty");

            // Casings and Clips
            /* Casings are the individual bullet shells and clips are the holders that are ejected from the gun.
             * Casings can be ejected on firing and reloading while clips can only be ejected on reload.
             * You can either use existing casings/clips from vanilla guns or add custom ones using a similar sprite import process as above with ammo.
             * Custom casings can also have their properties edited by adding more parameters to GenerateDebrisObject.*/
            gun.shellCasing = (PickupObjectDatabase.GetById((int)Items.M1)as Gun).shellCasing; //Example using AK-47 casings.
            //gun.shellCasing = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/hextech_casing").gameObject; //Example using a custom sprite as a casing.
            //gun.clipObject = (PickupObjectDatabase.GetById((int)Items.M1) as Gun).clipObject; //Example using AK-47 clips.
            gun.clipObject = BreakableAPIToolbox.GenerateDebrisObject("LOLItems/Resources/weapon_sprites/CustomGunAmmoTypes/shockSMG_clip_001", DebrisBounceCount: 1).gameObject;

            gun.shellsToLaunchOnFire = 0; //Number of shells to eject when shooting.
            gun.shellsToLaunchOnReload = 0; //Number of shells to eject when reloading (revolvers for example).
            gun.clipsToLaunchOnReload = 1; //Number of clips to eject when reloading.
            gun.reloadClipLaunchFrame = 4;

            // Bullet Trail
            
            EasyTrailBullet trail = projectile.gameObject.AddComponent<EasyTrailBullet>();
            trail.TrailPos = projectile.transform.position;
            trail.StartWidth = 0.2f;
            trail.EndWidth = 0f;
            trail.LifeTime = 0.1f; //How long the trail lingers
            // BaseColor sets an overall color for the trail. Start and End Colors are subtractive to it. 
            trail.BaseColor = ExtendedColours.lime; //Set to white if you don't want to interfere with Start/End Colors.
            trail.StartColor = Color.white;
            trail.EndColor = Color.green; //Custom Orange example using r/g/b values.

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

            // Animating your projectile is similar to setting up the custom sprite except each parameter is set as a group.
            // Each of the parameter lists should have the same number of entries as frames you're using.

            // A list of filenames in the sprites/ProjectileCollection folder for each frame in the animation, extension not required.
            List<string> projectileSpriteNames = new List<string> 
            {
                "shockSMG_projectile_001",
                "shockSMG_projectile_002",
                "shockSMG_projectile_003",
                "shockSMG_projectile_004",
                "shockSMG_projectile_005",
                "shockSMG_projectile_006",
                "shockSMG_projectile_007",
                "shockSMG_projectile_008",
                "shockSMG_projectile_009",
                "shockSMG_projectile_010",
                "shockSMG_projectile_011",
                "shockSMG_projectile_012"
            };
            // Animation FPS.
            int projectileFPS = 30;
            // Visual sprite size for each frame.  Sprite images will stretch to match these sizes.
            List<IntVector2> projectileSizes = new List<IntVector2> 
            {
                new IntVector2(16, 7), //1
                new IntVector2(16, 7), //2
                new IntVector2(16, 7), //3
                new IntVector2(16, 7), //4
                new IntVector2(16, 7), //5 
                new IntVector2(16, 7), //6
                new IntVector2(16, 7), //7
                new IntVector2(16, 7), //8
                new IntVector2(16, 7), //9
                new IntVector2(16, 7), //10
                new IntVector2(16, 7), //11
                new IntVector2(16, 7) //12
            };
            // Whether each frame should have a bit of glow.
            List<bool> projectileLighteneds = new List<bool> 
            {
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true
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
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter,
                tk2dBaseSprite.Anchor.MiddleCenter
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
                false, 
                false, 
                false, 
                false 
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
                false,
                false,
                false,
                false
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
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero
            };
            // Override the projectile hitboxes on each frame.  Either null (same as visuals) or slightly smaller than the visuals is most common.
            List<IntVector2?> projectileOverrideColliderSizes = new List<IntVector2?> 
            {
                new IntVector2(14, 5), //1
                new IntVector2(14, 5), //2
                new IntVector2(14, 5), //3
                new IntVector2(14, 5), //4
                new IntVector2(14, 5), //5 
                new IntVector2(14, 5), //6
                new IntVector2(14, 5), //7
                new IntVector2(14, 5), //8
                new IntVector2(14, 5), //9
                new IntVector2(14, 5), //10
                new IntVector2(14, 5), //11
                new IntVector2(14, 5) //12
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
                null,
                null,
                null,
                null
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
                null,
                null,
                null,
                null
            };
            // Your animations wrap mode. If you just want it to do a looping animation, leave it as Loop. Only useful for when adding multiple differing animations.
            tk2dSpriteAnimationClip.WrapMode ProjectileWrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            // Optionally, you can give your animations a clip name. Only useful for when adding multiple differing animations.
            //string projectileClipName = "projectileName"; 
            // Optionally, you can assign an animation as the default one that plays.  Only useful for when adding multiple differing animations.  If left as null then it will use the most recently added animation.
            //string projectileDefaultClipName = "projectileName"; 

            projectile.AddAnimationToProjectile(projectileSpriteNames, projectileFPS, projectileSizes, projectileLighteneds, projectileAnchors, projectileAnchorsChangeColiders, projectilefixesScales,
                                                projectileManualOffsets, projectileOverrideColliderSizes, projectileOverrideColliderOffsets, projectileOverrideProjectilesToCopyFrom, ProjectileWrapMode);



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

            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_02", null);

            gun.quality = PickupObject.ItemQuality.B; //Sets the gun's quality rank. Use "EXCLUDED" if the gun should not appear in chests.
            ETGMod.Databases.Items.Add(gun, false, "ANY");  //Adds your gun to the databse.
            //gun.AddToSubShop(ItemBuilder.ShopType.Trorc); //Select which sub shops during a run can carry the gun
            //gun.AddToSubShop(ItemBuilder.ShopType.Flynt);
            ID = gun.PickupObjectId; //Sets the Gun ID. 


            List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:electric_rifle",
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "battery_bullets",
                //"shock_rounds",
                //"thunderclap",
                "shock_rifle",
                //"laser_lotus",
            };
            AdvancedSynergyEntry ase = CustomSynergies.Add("Passive Charge", mandatoryConsoleIDs, optionalConsoleIDs, true);

            CustomSynergyType customSynergy = ETGModCompatibility.ExtendEnum<CustomSynergyType>("LOLItems", "Passive Charge");

            ase.bonusSynergies = [customSynergy];

            AmmoRegenSynergyProcessor arsp = gun.gameObject.AddComponent<AmmoRegenSynergyProcessor>();
            arsp.RequiredSynergy = customSynergy;
            arsp.AmmoPerSecond = 2f;
            arsp.PreventGainWhileFiring = true;
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            /*
            // initialShotInBurst = true only on first bullet fired
            if (initialShotInBurst)
            {
                Plugin.Log($"Before Initial bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}", "#F1C232");
                AkSoundEngine.PostEvent("vineboom", gun.gameObject);
                //gun.DefaultModule.ammoCost = 0;
                gun.CurrentAmmo -= 1;
                //ammoInClipTracker = gun.ClipShotsRemaining;
                initialShotInBurst = false;
                //gun.m_customAmmunitions[0].ShotsRemaining;
                Plugin.Log($"After Initial bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}");
            }
            // this section is only true for all bullets except the last bullet fired
            if (gun.m_midBurstFire && !initialShotInBurst)
            {
                Plugin.Log($"Before Middle bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}");
                initialShotInBurst = false;
                //gun.ClipShotsRemaining = ammoInClipTracker;
                Plugin.Log($"After Middle bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}");
            }
            // this section is only true for the last bullet fired
            else
            {
                Plugin.Log($"Before Final bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}");
                initialShotInBurst = true;
                //gun.DefaultModule.ammoCost = 1;
                //gun.ClipShotsRemaining = ammoInClipTracker;
                Plugin.Log($"After Final bullet, gun.ClipShotsRemaining: {gun.ClipShotsRemaining}");
            }
            */

            if ( gun != null && initialShotInBurst)
            {
                initialShotInBurst = false;
                //AkSoundEngine.PostEvent("vineboom", gun.gameObject);
                HelpfulMethods.PlayRandomSFX(gun.gameObject, normalFiringSFXList);
                //Plugin.Log("initial fire");
            }
            else if (!gun.m_midBurstFire)
            {
                //initialShotInBurst = true;
                //Plugin.Log("reset midburstfire");
                ResetInitialFire(player);
            }
            //Plugin.Log($"objectImpactEventName: {gun.projectile.objectImpactEventName}, enemyImpactEventName: {gun.projectile.enemyImpactEventName}");
            //Plugin.Log($"canattack: {player.m_CanAttack}, dodgerollstate: {player.m_dodgeRollState}, isdodgerolling: {player.IsDodgeRolling}");
            base.OnPostFired(player, gun);
        }

        public override void OnInitializedWithOwner(GameActor actor)
        {
            PlayerController player = this.Owner as PlayerController;
            player.OnPreDodgeRoll += ResetInitialFire;
            base.OnInitializedWithOwner(actor);
        }

        private void ResetInitialFire(PlayerController player)
        {
            initialShotInBurst = true;
            //Plugin.Log("reset initial fire");
        }

        public override void OnFinishAttack(PlayerController player, Gun gun)
        {
            //AkSoundEngine.PostEvent("vineboom", gun.gameObject);
            //initialShotInBurst = true;
            //Plugin.Log("reset onfinishattack");
            ResetInitialFire(player);
            base.OnFinishAttack(player, gun);
        }

        public override void OnBurstContinued(PlayerController player, Gun gun)
        {
            //AkSoundEngine.PostEvent("vineboom", gun.gameObject);
            //initialShotInBurst = true;
            //Plugin.Log("reset onburstcontinued");
            ResetInitialFire(player);
            base.OnBurstContinued(player, gun);
        }

        public override void OnSwitchedToThisGun()
        {
            //initialShotInBurst = true;
            //Plugin.Log("reset onswitchedtothisgun");
            PlayerController player = this.Owner as PlayerController;
            ResetInitialFire(player);
            base.OnSwitchedToThisGun();
        }

        public override void OnReload(PlayerController player, Gun gun)
        {
            //ammoInClipTracker = clipStat;
            //initialShotInBurst = true;
            //Plugin.Log("reset onreload");
            ResetInitialFire(player);
            base.OnReload(player, gun);
        }
    }
}