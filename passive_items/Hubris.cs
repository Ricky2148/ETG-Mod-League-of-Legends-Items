using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;

//dmg increase, dmg increase increases per kill, scaling infinitely, but very slowly

namespace LOLItems
{
    internal class Hubris : PassiveItem
    {
        // stats pool for item
        private int eminenceCount = 0;
        private float eminenceDamageIncrease = 0.002f;

        public static void Init()
        {
            string itemName = "Hubris";
            string resourceName = "LOLItems/Resources/passive_item_sprites/hubris_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Hubris>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "A symbol of victory";
            string longDesc = "A congratulatory laurel wreath gifted to the victor. With each triumph, one's strength increases. " +
                "Legends speak of a statue that manifests once you reach the pinnacle of victory.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.A;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.OnKilledEnemy += KillEnemyCount;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.OnKilledEnemy -= KillEnemyCount;
        }

        // removes current damage modifier, increments damage increase count, and adds new damage modifier
        private void KillEnemyCount(PlayerController player)
        {
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            eminenceCount++;
            float damageIncrease = eminenceCount * eminenceDamageIncrease;
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1.0f + damageIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
            player.stats.RecalculateStats(player, false, false);
        }
    }
}
