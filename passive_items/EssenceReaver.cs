using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class EssenceReaver : SpellbladePassiveItem
    {
        public static string ItemName = "Essence Reaver";

        private static float spellbladeDmg = 15f;
        private static float spellbladeCooldown = 3f;
        private static string spellbladeDamageIdentifier = "essence_reaver_spellblade_damage";

        private static float ammoRestorePercent = 0.01f;

        private static float DamageStat = 1.15f;

        public static int ID;
        
        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/essence_reaver_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<EssenceReaver>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "reaves essence";
            string longDesc = "Increase damage\nGrants Spellblade every few seconds. Spellblade: Empowers next bullet with additional damage scaling with your stats. Restores a small amount of ammo each time spellblade is used.\n\n" +
                "This magical reaver is made of self-forming ice, like Sheen. However, the higher quality magic imbued in the weapon overflows when the blade shatters, restoring slight amounts of ammunition to your weapons before reforming the blade.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;

            item.activationDmgValue = spellbladeDmg;
            item.activationCooldownValue = spellbladeCooldown;
            item.damageIdentifier = spellbladeDamageIdentifier;

            item.baseDamageScalesWithPlayerStats = true;
            item.damageStatScaleRatio = 1f;

            //item.OnSpellbladeProc += SpellbladeAmmoRestore;

            ID = item.PickupObjectId;
        }

        public new void Start()
        {
            this.OnSpellbladeProc += SpellbladeAmmoRestore;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            //OnSpellbladeProc += SpellbladeAmmoRestore;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");
        }

        private static void SpellbladeAmmoRestore(PlayerController player)
        {
            Gun gun = player.inventory.CurrentGun;
            if (!gun.InfiniteAmmo && gun.CanGainAmmo)
            {
                int ammoToGain = Mathf.CeilToInt((float)gun.AdjustedMaxAmmo * ammoRestorePercent);
                Plugin.Log($"ammo to gain: {ammoToGain}");
                gun.GainAmmo(ammoToGain);
            }
        }
    }
}
