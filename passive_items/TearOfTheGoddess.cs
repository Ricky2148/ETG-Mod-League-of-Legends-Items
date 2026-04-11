using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// REWRITE DESCRIPTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

namespace LOLItems.passive_items
{
    internal class TearOfTheGoddess : PassiveItem
    {
        public static string ItemName = "Tear of the Goddess";

        private static float ManaflowIncreaseMax = 0.5f;
        private static float ManaflowIncrementValue = 0.05f;
        private static float ManaflowIncrementKillReq = 50f;
        public float CurrentManaflowKillCount = 0f;
        public int ManaflowStackCount = 0;

        public bool ManaflowMaxed = false;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/tear_of_the_goddess_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<TearOfTheGoddess>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*NOT A REAL TEAR*";
            string longDesc = "Increases max ammo multiplier and clip size multiplier every few kills. After enough kills, stops increasing stats. Stacks carry over to Manamune.\n\n" +
                "\"Few are fortunate enough to come across the tears of the very goddess who created our world... " +
                "but those who are lucky enough to acquire a Tear of the Goddess are said to be destined for greatness.\"";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.D;

            item.UsesCustomCost = false;
            //item.CustomCost = 20;

            item.AddToSubShop(ItemBuilder.ShopType.Goopton);
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            if (ManaflowStackCount * ManaflowIncrementValue <= ManaflowIncreaseMax)
            {
                player.OnAnyEnemyReceivedDamage += ManaflowStack;
            }
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.OnAnyEnemyReceivedDamage -= ManaflowStack;
            }
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
                    if (ManaflowStackCount * ManaflowIncrementValue >= ManaflowIncreaseMax)
                    {
                        Owner.OnAnyEnemyReceivedDamage -= ManaflowStack;
                        ManaflowMaxed = true;
                    }
                }
            }
        }
    }
}
