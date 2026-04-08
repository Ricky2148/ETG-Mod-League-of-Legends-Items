using Alexandria;
using Alexandria.ItemAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//still needs a custom sfx

namespace LOLItems.passive_items
{
    internal class ShieldOfMoltenStone : OnPreDamagedPassiveItem
    {
        public static string ItemName = "Shield of Molten Stone";

        private static float HealthStat = 1f;
        private static int ArmorStat = 0;

        private static float preDamageProcChance = 0.2f;
        private static float synergyProcChance = 0.3f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/shield_of_molten_stone_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ShieldOfMoltenStone>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Immovable as the Earth";
            string longDesc = "Sometimes prevents the player from taking damage.\n\n" +
                "This magical shield imbues your body with heavy defense and resilience like that of the earth.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);
            //item.ArmorToGainOnInitialPickup = ArmorStat;

            item.quality = PickupObject.ItemQuality.B;

            item.procChance = preDamageProcChance;
            item.triggersInvulnerability = true;
            item.effectDuration = 1f;
            item.triggersOutline = true;
            item.triggersGlow = true;
            item.outlineColor = new Color(138f * 0.7f, 43f * 0.7f, 226f * 0.7f);

            item.playsSFX = true;
            string[] sfxList = { "carefree_melody_SFX" };
            item.updateSFXList(sfxList);
            ID = item.PickupObjectId;
        }

        // updates stats for both items if synergy is active on pickup
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            if (!player.HasSynergy(Synergy.HEAVEN_AND_EARTH_COMBINED))
            {
                procChance = preDamageProcChance;
            }

            /*if (player.PlayerHasActiveSynergy("Heaven and Earth Combined"))
            {
                this.procChance = synergyProcChance;
                Plugin.Log("Shield of Molten Stone's Synergy activated");

                foreach (PassiveItem item in player.passiveItems)
                {
                    if (item.PickupObjectId == CloakOfStarryNight.ID && item != null)
                    {
                        item.GetComponent<CloakOfStarryNight>().setProcChance(synergyProcChance);
                    }
                }
            }*/
        }

        // updates stats for both items if synergy is active on drop
        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            /*this.procChance = preDamageProcChance;

            if (player != null)
            {
                foreach (PassiveItem item in player.passiveItems)
                {
                    if (item.PickupObjectId == CloakOfStarryNight.ID && item != null)
                    {
                        item.GetComponent<CloakOfStarryNight>().setProcChance(preDamageProcChance);
                    }
                }
            }*/
        }
    }
}
