using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class TrinityForce : SpellbladePassiveItem
    {
        public static string ItemName = "Trinity Force";

        private static float spellbladeDmg = 20f;
        private static float spellbladeCooldown = 3f;
        private static string spellbladeDamageIdentifier = "trinity_force_spellblade_damage";

        private static float DamageStat = 1.1f;
        private static float RateOfFireStat = 1.1f;
        private static float HealthStat = 1f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/trinity_force_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<TrinityForce>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*not affiliated with Zelda*";
            string longDesc = "+1 Heart, Increase damage and fire rate\nGrants Spellblade every few seconds. Spellblade: Empowers next bullet with high additional damage.\n\n" +
                "The pinnacle of balance. A charm of legend rumored to belong to a far away land. You feel stronger with it, especially your Power, Wisdom, and Courage.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.A;

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
