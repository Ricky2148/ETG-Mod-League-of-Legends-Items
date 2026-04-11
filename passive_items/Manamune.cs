using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using LOLItems.passive_items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

//increase max ammo, increase max clip size with the stacks, at max stacks, increase damage a little
// only thing that needs changing is the sprite update when upgrading to Muramana

// doesn't give the player muramana after having gotten it once before, this is true even over the course of multiple runs as long as the game is the same loaded instance

namespace LOLItems.passive_items
{
    public class Manamune : PassiveItem
    {
        public static string ItemName = "Manamune";

        // stats pool for item
        private static float DamageStat = 1.05f;
        private static float ManaflowIncreaseMax = 0.5f;
        private static float ManaflowIncrementValue = 0.05f;
        private static float ManaflowIncrementKillReq = 25f;
        private static float MuramanaShockBaseDamage = 5f;
        private float CurrentManaflowKillCount = 0f;
        private int ManaflowStackCount = 0;

        public bool BLADEOFTHEONIActivated = false;
        private static float BLADEOFTHEONIDamageStat = 1.25f;

        public static int ID;

        private PassiveItem muramanaItem;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/manamune_pixelart_sprite_small";
            //string upgradeResourceName = "LOLItems/Resources/passive_item_sprites/muramana_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Manamune>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //tk2dBaseSprite sprite = item.sprite;
            //tk2dSpriteCollectionData collection = sprite.collection;

            //SpriteBuilder.AddSpriteToCollection(upgradeResourceName, collection);

            string shortDesc = "from the Greatest Swordsmith";
            // maybe add effect explanation?
            string longDesc = "Slightly increase damage\nIncreases max ammo multiplier and clip size multiplier every few kills. Evolves after enough kills.\n\n" +
                "Created by the Greatest Swordsmith, Masamune, this sword increases the wielder's" +
                " capacity for battle.\n\nLegends hint at the blade's true strength being sealed away.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.B;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            //player.OnKilledEnemy += ManaflowStack;
            if (ManaflowStackCount * ManaflowIncrementValue <= ManaflowIncreaseMax)
            {
                player.OnAnyEnemyReceivedDamage += ManaflowStack;
            }
               
            /*foreach (PassiveItem item in player.passiveItems)
            {
                if (item.PickupObjectId == TearOfTheGoddess.ID && item != null)
                {
                    if (item.GetComponent<TearOfTheGoddess>().ManaflowMaxed)
                    {
                        UpgradeToMuramana(Owner);
                    }
                    else
                    {
                        ManaflowStackCount = item.GetComponent<TearOfTheGoddess>().ManaflowStackCount;
                        CurrentManaflowKillCount = item.GetComponent<TearOfTheGoddess>().CurrentManaflowKillCount * (3f/5f);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        Plugin.Log($"tear: {item.GetComponent<TearOfTheGoddess>().ManaflowStackCount}, {item.GetComponent<TearOfTheGoddess>().CurrentManaflowKillCount}" +
                            $"\nmanamune: {ManaflowStackCount}, {CurrentManaflowKillCount}");
                    }
                    player.RemovePassiveItem(TearOfTheGoddess.ID);
                }
            }*/
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                //player.OnKilledEnemy -= ManaflowStack;
                player.OnAnyEnemyReceivedDamage -= ManaflowStack;
                //player.PostProcessProjectile -= MuramanaShock;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.BUILDS_INTO_MANAMUNE))
                {
                    foreach (PassiveItem item in Owner.passiveItems)
                    {
                        if (item.PickupObjectId == TearOfTheGoddess.ID && item != null)
                        {
                            if (item.GetComponent<TearOfTheGoddess>().ManaflowMaxed)
                            {
                                UpgradeToMuramana(Owner);
                            }
                            else
                            {
                                ManaflowStackCount = item.GetComponent<TearOfTheGoddess>().ManaflowStackCount;
                                CurrentManaflowKillCount = item.GetComponent<TearOfTheGoddess>().CurrentManaflowKillCount * (1f / 2f);
                                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                                //Plugin.Log($"tear: {item.GetComponent<TearOfTheGoddess>().ManaflowStackCount}, {item.GetComponent<TearOfTheGoddess>().CurrentManaflowKillCount}" nmanamune: {ManaflowStackCount}, {CurrentManaflowKillCount}");

                                LootEngine.SpawnCurrency(Owner.specRigidbody.UnitCenter, item.PurchasePrice);
                            }
                        }
                    }
                    Owner.RemovePassiveItem(TearOfTheGoddess.ID);
                }

                if (Owner != null)
                {
                    if (Owner.HasSynergy(Synergy.BLADE_OF_THE_ONI_MANAMUNE) && !BLADEOFTHEONIActivated)
                    {
                        ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, BLADEOFTHEONIDamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                        BLADEOFTHEONIActivated = true;
                    }
                    else if (!Owner.HasSynergy(Synergy.BLADE_OF_THE_ONI_MANAMUNE) && BLADEOFTHEONIActivated)
                    {
                        ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                        BLADEOFTHEONIActivated = false;
                    }
                }
            }

            base.Update();
        }

        private void ManaflowStack(float damage, bool fatal, HealthHaver enemyHealth)
        {
            if (enemyHealth.aiActor && enemyHealth && fatal)
            {
                if (enemyHealth.aiActor.IsNormalEnemy && (enemyHealth.IsBoss || enemyHealth.IsSubboss))
                {
                    CurrentManaflowKillCount += 5;
                    //Plugin.Log($"is boss: {enemyHealth.IsBoss}, is sub boss: {enemyHealth.IsSubboss}");
                }
                else
                {
                    CurrentManaflowKillCount++;
                    //Plugin.Log($"is normal enemy");
                }
                //CurrentManaflowKillCount++;
                // when kill count reaches threshold, reset count and increase stack count and update stats
                if (CurrentManaflowKillCount >= ManaflowIncrementKillReq)
                {
                    CurrentManaflowKillCount -= ManaflowIncrementKillReq;
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                    ManaflowStackCount++;
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    //Owner.stats.RecalculateStats(Owner, false, false);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);
                    // when stack count reaches max, upgrade to Muramana
                    //Plugin.Log($"manaflow stack count: {ManaflowStackCount}, manaflow increment value: {ManaflowIncrementValue}, ({ManaflowStackCount} * {ManaflowIncrementValue} = {ManaflowStackCount * ManaflowIncrementValue}) >= manaflow increase max: {ManaflowIncreaseMax}");
                    if (ManaflowStackCount * ManaflowIncrementValue >= ManaflowIncreaseMax) UpgradeToMuramana(Owner);
                }
            }
        }

        private void UpgradeToMuramana(PlayerController player)
        {
            // tries to change the manamune item to muramana without removing item from player
            /*
            player.OnKilledEnemy -= ManaflowStack; //unsubscribe from the ManaflowStack event
            // change item cosmetics
            this.SetName("Muramana");
            this.SetShortDescription("The Peak of Swordsmithing");
            this.SetLongDescription("A blade forged by Masamune and wielded by the worthy, the Manamune's true " +
                "power has been unlocked.");
            // something to change sprite of item
            this.sprite.SetSprite("muramana_item_sprite");

            player.PostProcessProjectile += MuramanaShock; // subscribe to the MuramanaShock event

            Plugin.Log("Manamune has been upgraded to Muramana");
            */

            // tries to remove manamune from player and give muramana
            //player.OnKilledEnemy -= ManaflowStack;
            player.OnAnyEnemyReceivedDamage -= ManaflowStack;
            ManaflowStackCount = 0;
            CurrentManaflowKillCount = 0;
            //player.RemovePassiveItem(this.PickupObjectId);

            if (muramanaItem == null) muramanaItem = PickupObjectDatabase.GetByName("Muramana") as PassiveItem;
            //PassiveItem muramanasynergy = PickupObjectDatabase.GetByName("MuramanaSynergyActivation") as PassiveItem;
            if (muramanaItem != null)
            {
                //player.AcquirePassiveItem(muramanasynergy);
                //player.AcquirePassiveItem(muramana);

                HelpfulMethods.CustomNotification("Manamune Upgraded to Muramana", "", muramanaItem.sprite, UINotificationController.NotificationColor.PURPLE);

                player.RemovePassiveItem(this.PickupObjectId);
                //player.AcquirePassiveItem(PickupObjectDatabase.GetById(Muramana.ID) as PassiveItem);
                player.GiveItem("LOLItems:muramana");

                //player.GiveItem("LOLItems:manaflow_fully_stacked");
                //player.RemovePassiveItem(MuramanaSynergyActivation.ID);
                //player.PostProcessProjectile += MuramanaShock;
                Plugin.Log("Manamune has been upgraded to Muramana");
            }
            else
            {
                Plugin.Log("Muramana not found in the database!");
            }
        }

        // unneeded since muramana is a separate item
        /*private void MuramanaShock(Projectile proj, float f)
        {
            if (proj.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy != null && enemy.healthHaver != null)
                {
                    //scales the damage based on player's clip size and ammo size stats
                    float clipSizeStat = Mathf.Max(0f ,(player.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier) - 1f) / 5);
                    float ammoSizeStat = Mathf.Max(0f ,(player.stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier) - 1f) / 5);
                    float MuramanaShockDamageMultiplier = Mathf.Max(1f, 1f + clipSizeStat + ammoSizeStat);
                    float damageToDeal = Mathf.Max(1f, MuramanaShockBaseDamage * MuramanaShockDamageMultiplier);
                    enemy.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "muramana_shock_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false
                    );
                }
            };
        }*/
    }
}
