using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

//increase max ammo, increase max clip size with the stacks, at max stacks, increase damage a little
// only thing that needs changing is the sprite update when upgrading to Muramana

namespace LOLItems
{
    public class Manamune : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.05f;
        private static float ManaflowIncreaseMax = 0.5f;
        private static float ManaflowIncrementValue = 0.05f;
        private static float ManaflowIncrementKillReq = 25f;
        private static float MuramanaShockBaseDamage = 5f;
        private float CurrentManaflowKillCount = 0f;
        private int ManaflowStackCount = 0;

        public static void Init()
        {
            string itemName = "Manamune";
            string resourceName = "LOLItems/Resources/passive_item_sprites/manamune_pixelart_sprite_small";
            string upgradeResourceName = "LOLItems/Resources/passive_item_sprites/muramana_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Manamune>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            tk2dBaseSprite sprite = item.sprite;
            tk2dSpriteCollectionData collection = sprite.collection;

            SpriteBuilder.AddSpriteToCollection(upgradeResourceName, collection);

            string shortDesc = "from the Greatest Swordsmith";
            // maybe add effect explanation?
            string longDesc = "Created by the Greatest Swordsmith, Masamune, this sword increases the wielder's" +
                " capacity for battle.\n\nLegends hint at the blade's true strength being sealed away.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.B;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.OnKilledEnemy += ManaflowStack;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.OnKilledEnemy -= ManaflowStack;
            //player.PostProcessProjectile -= MuramanaShock;
        }

        private void ManaflowStack(PlayerController player)
        {
            CurrentManaflowKillCount++;
            // when kill count reaches threshold, reset count and increase stack count and update stats
            if (CurrentManaflowKillCount >= ManaflowIncrementKillReq)
            {
                CurrentManaflowKillCount = 0f;
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier);
                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier);
                ManaflowStackCount++;
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AdditionalClipCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.AmmoCapacityMultiplier, 1f + ManaflowIncrementValue * ManaflowStackCount, StatModifier.ModifyMethod.MULTIPLICATIVE);
                player.stats.RecalculateStats(player, false, false);
                // when stack count reaches max, upgrade to Muramana
                if (ManaflowStackCount * ManaflowIncrementValue >= ManaflowIncreaseMax) UpgradeToMuramana(player);
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
            player.OnKilledEnemy -= ManaflowStack;
            player.RemovePassiveItem(this.PickupObjectId);

            PassiveItem muramana = PickupObjectDatabase.GetByName("Muramana") as PassiveItem;
            if (muramana != null)
            {
                player.AcquirePassiveItem(muramana);
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
