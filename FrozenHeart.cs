using Alexandria;
using Alexandria.ItemAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// range around player that decreases enemy fire rate
// not complete

namespace LOLItems
{
    internal class FrozenHeart : BasicStatPickup
    {
        // stats pool for item
        private static int ArmorStat = 1;

        private static float WintersCaressCrippleRatio = 0.8f;
        private static float WintersCaressRange = 10f;

        public static void Init()
        {
            string itemName = "Frozen Heart";
            string resourceName = "LOLItems/Resources/passive_item_sprites/frozen_heart_item_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<FrozenHeart>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Warning: Do not thaw!";
            string longDesc = "idk";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            item.ArmorToGainOnInitialPickup = ArmorStat;

            item.quality = PickupObject.ItemQuality.A;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Thornmail");

        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Thornmail");

        }
    }
}