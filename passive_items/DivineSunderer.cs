using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class DivineSunderer : SpellbladePassiveItem
    {
        public static string ItemName = "Divine Sunderer";

        private static float spellbladeDmg = 15f;
        private static float spellbladeCooldown = 3f;
        private static string spellbladeDamageIdentifier = "divine_sunderer_force_spellblade_damage";

        private static float HealthStat = 1f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/sheen_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<DivineSunderer>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "anti-tank";
            string longDesc = "Grants Spellblade every few seconds. Spellblade: Empowers next bullet with some additional damage.\n\n" +
                "\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.B;

            item.activationDmgValue = spellbladeDmg;
            item.activationCooldownValue = spellbladeCooldown;
            item.damageIdentifier = spellbladeDamageIdentifier;

            item.activationDealsPercentDamage = true;
            item.percentDamageRatio = 0.2f;

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
