using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class LichBane : SpellbladePassiveItem
    {
        public static string ItemName = "Lich Bane";

        private static float spellbladeDmg = 40f;
        private static float spellbladeCooldown = 3f;
        private static string spellbladeDamageIdentifier = "lich_bane_spellblade_damage";

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/sheen_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<LichBane>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "lich's bane";
            string longDesc = "Grants Spellblade every few seconds. Spellblade: Empowers next bullet with some additional damage.\n\n" +
                "\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.S;

            item.activationDmgValue = spellbladeDmg;
            item.activationCooldownValue = spellbladeCooldown;
            item.damageIdentifier = spellbladeDamageIdentifier;

            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");
        }
    }
}
