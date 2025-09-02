using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Alexandria;
using LOLItems.custom_class_data;

//add vfx and sfx during revive duration

namespace LOLItems
{
    internal class GuardianAngel : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.25f;
        private static int ArmorStat = 2;
        private bool hasRevived = false;
        public static void Init()
        {
            string itemName = "Guardian Angel";
            string resourceName = "LOLItems/Resources/passive_item_sprites/guardian_angel_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<GuardianAngel>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"Heroes never die!\"";
            string longDesc = "A blade imbued with the hope of a cult who believed in rebirth. You almost feel " +
                "like you could defy even death with it in hand.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.ArmorToGainOnInitialPickup = ArmorStat;

            item.quality = PickupObject.ItemQuality.S;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.healthHaver.OnPreDeath += Rebirth;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.healthHaver.OnPreDeath -= Rebirth;
        }

        private void Rebirth(Vector2 DeathPositon)
        {
            if (!hasRevived && this.Owner is PlayerController player)
            {
                hasRevived = true;
                player.StartCoroutine(ReviveCoroutine(player));
            }
        }

        // revives the player with half health and invulnerability for a short time, activates a blank after invul
        private System.Collections.IEnumerator ReviveCoroutine(PlayerController player)
        {
            // makes player character invulnerable, reset health, take no inputs from player, and remove revive effect
            player.healthHaver.TriggerInvulnerabilityPeriod(4f);
            player.healthHaver.ForceSetCurrentHealth(player.healthHaver.GetMaxHealth() / 2);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.OnPreDeath -= Rebirth;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;

            player.sprite.color = ExtendedColours.paleYellow;
            player.CurrentGun.sprite.color = ExtendedColours.paleYellow;

            AkSoundEngine.PostEvent("guardian_angel_passive_SFX", GameManager.Instance.gameObject);

            // animations for the revive: animation of the player's health being restored
            // and the player being invulnerable for a short time
            // including sound effects and visual effects
            // during the invulnerability period, enemies be frozen in time???

            yield return new WaitForSeconds(4f);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;

            // trigger blank to push away enemies and clear bullets, restore input, and remove invulerability
            player.ForceBlank();
            player.CurrentInputState = PlayerInputState.AllInput;
            player.healthHaver.PreventAllDamage = false;
        }
    }
}
