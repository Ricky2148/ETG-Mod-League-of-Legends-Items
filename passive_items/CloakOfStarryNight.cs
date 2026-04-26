using Alexandria;
using Alexandria.ItemAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static tk2dSpriteCollectionDefinition;

// add vfx and sfx when proc occurs

namespace LOLItems.passive_items
{
    internal class CloakOfStarryNight : OnPreDamagedPassiveItem
    {
        public static string ItemName = "Cloak of Starry Night";

        private static float HealthStat = 1f;
        private static int ArmorStat = 0;

        private static float preDamageProcChance = 0.2f;
        private static float synergyProcChance = 0.3f;

        public bool HEAVENANDEARTHCOMBINEDActivated = false;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/cloak_of_starry_night_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<CloakOfStarryNight>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Limitless as the Stars";
            string longDesc = "Sometimes prevents the player from taking damage.\n\n" +
                "This magical cloak imbues your body with great toughness and durability like that of a star.\n";

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
            
            item.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = item.PickupObjectId;

            // synergy with Shield of Molten Stone
            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:shield_of_molten_stone",
                "LOLItems:cloak_of_starry_night"
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                ""
            };
            CustomSynergies.Add("Heaven and Earth Combined", mandatoryConsoleIDs, null, true);*/
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            // updates stats for both items if synergy is active on pickup
            /*if (player.PlayerHasActiveSynergy("Heaven and Earth Combined"))
            {
                this.procChance = synergyProcChance;
                Plugin.Log("Cloak of Starry Night's Synergy activated");

                foreach (PassiveItem item in player.passiveItems)
                {
                    if (item.PickupObjectId == ShieldOfMoltenStone.ID && item != null)
                    {
                        item.GetComponent<ShieldOfMoltenStone>().setProcChance(synergyProcChance);
                    }
                }
            }*/
        }

        // updates stats for both items if synergy is active on drop or destroy
        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            /*this.procChance = preDamageProcChance;
            if (player != null)
            {
                foreach (PassiveItem item in player.passiveItems)
                {
                    if (item.PickupObjectId == ShieldOfMoltenStone.ID && item != null)
                    {
                        item.GetComponent<ShieldOfMoltenStone>().setProcChance(preDamageProcChance);
                    }
                }
            }*/
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.HEAVEN_AND_EARTH_COMBINED) && !HEAVENANDEARTHCOMBINEDActivated)
                {
                    procChance = synergyProcChance;
                    foreach (PassiveItem item in Owner.passiveItems)
                    {
                        if (item.PickupObjectId == ShieldOfMoltenStone.ID && item != null)
                        {
                            item.GetComponent<ShieldOfMoltenStone>().setProcChance(synergyProcChance);
                            //Plugin.Log($"Cloak: {procChance}, Shield: {item.GetComponent<ShieldOfMoltenStone>().procChance}");
                        }
                    }

                    HEAVENANDEARTHCOMBINEDActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.HEAVEN_AND_EARTH_COMBINED) && HEAVENANDEARTHCOMBINEDActivated)
                {
                    procChance = preDamageProcChance;
                    foreach (PassiveItem item in Owner.passiveItems)
                    {
                        if (item.PickupObjectId == ShieldOfMoltenStone.ID && item != null)
                        {
                            item.GetComponent<ShieldOfMoltenStone>().setProcChance(preDamageProcChance);
                            //Plugin.Log($"Cloak: {procChance}, Shield: {item.GetComponent<ShieldOfMoltenStone>().procChance}");
                        }
                    }

                    HEAVENANDEARTHCOMBINEDActivated = false;
                }
            }

            base.Update();
        }
    }
}