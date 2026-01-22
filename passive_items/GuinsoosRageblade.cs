using Alexandria.ItemAPI;
using Alexandria.Misc;
using JetBrains.Annotations;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

// add a way for phantomhit extra projectiles to be modified by other items like scatter shot or helix bullets
// i don't think the % offset for the delay is accounting for additional
// rate of fire increases from other items
// copy projectile before delay and send it out after to prevent error

namespace LOLItems.passive_items
{
    public class GuinsoosRageblade : PassiveItem
    {
        public static string ItemName = "Guinsoo's Rageblade";

        // stats pool for item
        private int phantomHitCount = 0; // Counter for the number of phantom hits
        private int phantomHitCap = 3;

        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.2f;

        const int AK47_ID = 15;
        //GameObject ak47ProjPrefab = (PickupObjectDatabase.GetById(AK47_ID) as Gun).DefaultModule.projectiles[0].gameObject;

        private static Gun phoenix = PickupObjectDatabase.GetById((int)Items.Phoenix) as Gun;
        private static GameActorFireEffect BLADESOFCHAOSBurnEffect = new GameActorFireEffect
        {
            duration = 5,
            DamagePerSecondToEnemies = 10,
            effectIdentifier = "blades_of_chaos_synergy_burn_effect",
            ignitesGoops = false,
            FlameVfx = phoenix.DefaultModule.projectiles[0].fireEffect.FlameVfx
        };

        public bool POSEIGUNSWRATHActivated = false;
        private static float POSEIGUNSWRATHProjSpeedStat = 1.5f;
        private static float POSEIGUNSWRATHReloadSpeedStat = 0.6f;
        public bool TRIPLEDELUXEActivated = false;
        private static int TRIPLEDELUXEphantomHitCap = 2;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/guinsoos_rageblade_pixelart_sprite_small";
            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<GuinsoosRageblade>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*not affiliated with Kratos*";
            string longDesc = "Increase damage and fire rate\nEvery 3rd bullet fires an additional copy of that bullet.\n\n" +
                "Forged in the foulest depths of the Void. These blades increase one's capacity for " +
                "rage and destruction. Perhaps you should not wield them.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            /*
            implement double hit passive = every 3rd shot fires 2 bullets
            count with each bullet fired, 3rd bullet fired will fire 2 bullets
            maybe implement atkspd stacking passive
            */

            item.quality = PickupObject.ItemQuality.S;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            //player.OnReloadedGun += OnGunReloaded;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
            }
            phantomHitCount = 0; // Reset the count when the item is dropped
            //player.OnReloadedGun -= OnGunReloaded;
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.POSEIGUNS_WRATH) && !POSEIGUNSWRATHActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.ProjectileSpeed, POSEIGUNSWRATHProjSpeedStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.ReloadSpeed, POSEIGUNSWRATHReloadSpeedStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    POSEIGUNSWRATHActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.POSEIGUNS_WRATH) && POSEIGUNSWRATHActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.ProjectileSpeed);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.ReloadSpeed);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    POSEIGUNSWRATHActivated = false;
                }
                if (Owner.HasSynergy(Synergy.TRIPLE_DELUXE) && !TRIPLEDELUXEActivated)
                {
                    phantomHitCap = TRIPLEDELUXEphantomHitCap;

                    TRIPLEDELUXEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.TRIPLE_DELUXE) && TRIPLEDELUXEActivated)
                {
                    phantomHitCap = 3;

                    TRIPLEDELUXEActivated = false;
                }
            }

            base.Update();
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            //ensures only the player's own projectiles are counted
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                phantomHitCount++;
                if (phantomHitCount >= phantomHitCap)
                {
                    PlayerController player = proj.Owner as PlayerController;
                    phantomHitCount = 0;
                    player.StartCoroutine(FirePhantomProjectileDelayed(proj));
                }
            }
        }

        /*
        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            phantomHitCount = 0; // Reset the count when the gun is reloaded
        }
        */

        private System.Collections.IEnumerator FirePhantomProjectileDelayed(Projectile proj)
        {
            if (proj == null || proj.Owner == null)
            {
                Plugin.Log("fail 1");
                yield break;
            }

            if (!proj)
            {
                Plugin.Log("fail 2");
                yield break;
            }
            PlayerController player = proj.Owner as PlayerController;

            GameActor ogOwner = proj.Owner;
            SpeculativeRigidbody ogShooter = proj.Shooter;
            Color ogSpriteColor = ExtendedColours.brown;
            if (proj.sprite != null)
            {
                ogSpriteColor = proj.sprite.color;
            }
            else
            {
                //Plugin.Log("proj.sprite = null at ogSpriteColor");
            }
            //Vector3 ogPosition = player.CurrentGun.barrelOffset.position;

            // wait 1ms for projectiles to have their data be properly updated to be properly copied from
            yield return new WaitForSeconds(0.001f);

            Vector2 ogDirection = proj.LastVelocity.normalized;

            ProjectileData newData = new ProjectileData();
            newData.damage = proj.baseData.damage;
            newData.force = proj.baseData.force;
            newData.speed = proj.baseData.speed;
            newData.range = proj.baseData.range;

            // Calculate the delay based on the gun's rate of fire
            float baseDelayRatio = 0.3f;
            float gunRateOfFire = player.CurrentGun.DefaultModule.cooldownTime;
            float delay = Mathf.Max(gunRateOfFire * baseDelayRatio / player.stats.GetStatValue(PlayerStats.StatType.RateOfFire), 0.01f);
            delay = Mathf.Ceil(delay * 100f) / 100f;

            yield return new WaitForSeconds(delay - 0.001f);

            if (proj == null)
            {
                //Plugin.Log("proj == null");
                yield break;
            }
            // create a phantom projectile based on the exact stats of the original projectile
            Projectile phantomHit = UnityEngine.Object.Instantiate(proj.gameObject).GetComponent<Projectile>();

            phantomHit.baseData = newData;
            phantomHit.Owner = ogOwner;
            phantomHit.Shooter = ogShooter;

            /*if (player.HasSynergy(Synergy.ONHIT_SYNERGY_WITH_KRAKENSLAYER))
            {
                foreach (PassiveItem item in player.passiveItems)
                {
                    if (item.PickupObjectId == KrakenSlayer.ID)
                    {
                        KrakenSlayer krakenSlayer = item.GetComponent<KrakenSlayer>();
                        krakenSlayer.bringItDownCount++;
                        if (krakenSlayer.bringItDownCount >= 3)
                        {
                            if (proj.sprite != null)
                            {
                                proj.sprite.color = Color.Lerp(proj.sprite.color, Color.cyan, 0.7f);
                            }
                            HelpfulMethods.PlayRandomSFX(proj.gameObject, krakenSlayer.sfxList);
                            proj.OnHitEnemy += (projHit, enemy, fatal) =>
                            {
                                if (enemy == null) return;
                                if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                                if (enemy.healthHaver != null)
                                {
                                    // scales damage based on enemy's missing health percentage
                                    float percentDamageIncrease = 0.75f * (1.0f - enemy.healthHaver.GetCurrentHealthPercentage());
                                    // GetFloorPriceMod works instead of explicitly stating it like GetFloorDamageScale
                                    float damageToDeal = krakenSlayer.bringItDownDamage * (1.0f + percentDamageIncrease) * HelpfulMethods.GetFloorPriceMod();
                                    // damage is 1/4 against bosses and sub-bosses
                                    if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                                    {
                                        //damageToDeal *= 0.25f;
                                    }

                                    // calculates additional extra damage to apply to enemy
                                    enemy.healthHaver.ApplyDamage(
                                        damageToDeal,
                                        Vector2.zero,
                                        "kraken_slayer_bring_it_down_damage",
                                        CoreDamageTypes.None,
                                        DamageCategory.Normal,
                                        false
                                    );
                                }
                            };
                            krakenSlayer.bringItDownCount = 0;
                        }
                    }
                }
            }*/

            player.DoPostProcessProjectile(phantomHit);
            phantomHitCount--;

            //phantomHit.OnHitEnemy += proj.OnHitEnemy;

            if (proj.sprite != null)
            { 
                phantomHit.sprite.color = Color.Lerp(ogSpriteColor, ExtendedColours.carrionRed, 0.7f);
            }
            else
            {
                //Plugin.Log("proj.sprite = null at Color.Lerp");
            }

            if (player.HasSynergy(Synergy.BLADES_OF_CHAOS))
            {
                phantomHit.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.healthHaver == null) return;
                    if (enemy.aiActor != null)
                    {
                        enemy.aiActor.ApplyEffect(BLADESOFCHAOSBurnEffect);
                    }
                    else if (enemy.GetComponentInParent<AIActor>() != null)
                    {
                        enemy.GetComponentInParent<AIActor>().ApplyEffect(BLADESOFCHAOSBurnEffect);
                    }
                };
            }

            // Set the position and rotation of the phantom projectile
            phantomHit.transform.position = player.CurrentGun.barrelOffset.position;
            //Vector2 direction = proj.LastVelocity.normalized;
            float angle = Mathf.Atan2(ogDirection.y, ogDirection.x) * Mathf.Rad2Deg;

            // Set the projectile's rotation to match the direction
            phantomHit.transform.rotation = Quaternion.Euler(0, 0, angle);

            phantomHit.SendInDirection(ogDirection, true, true);
            

            // experimental new code to setup basic proj that looks like original proj
            /*Vector2 direction = proj.LastVelocity.normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Projectile newProj = SpawnManager.SpawnProjectile(
                prefab: ak47ProjPrefab,
                position: player.CurrentGun.barrelOffset.position,
                rotation: Quaternion.Euler(0f, 0f, angle)
            ).GetComponent<Projectile>();
            
            if ((bool)newProj.sprite && (bool)proj.sprite)
            {
                newProj.shouldRotate = proj.shouldRotate;
                newProj.shouldFlipHorizontally = proj.shouldFlipHorizontally;
                newProj.shouldFlipVertically = proj.shouldFlipVertically;
                newProj.sprite.SetSprite(proj.sprite.collection, proj.sprite.spriteId);
                Vector2 vector = newProj.transform.position.XY() - newProj.sprite.WorldCenter;
                newProj.transform.position += vector.ToVector3ZUp();
                newProj.specRigidbody.Reinitialize();
            }

            ProjectileData newData = new ProjectileData();
            newData.damage = proj.baseData.damage;
            newData.speed = proj.baseData.speed;
            newData.range = proj.baseData.range;
            newData.force = proj.baseData.force;
            newProj.baseData = newData;
            newProj.Shooter = proj.Shooter;
            newProj.Owner = proj.Owner;
            newProj.sprite.color = Color.Lerp(proj.sprite.color, ExtendedColours.carrionRed, 0.7f);
            */
        }
    }
}
