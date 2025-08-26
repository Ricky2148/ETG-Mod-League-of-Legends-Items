using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Alexandria;

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
            string resourceName = "LOLItems/Resources/passive_item_sprites/guardian_angel_item_sprite";

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
            Plugin.Log($"Player picked up Guardian Angel");

            player.healthHaver.OnPreDeath += Rebirth;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Guardian Angel");

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
            player.healthHaver.TriggerInvulnerabilityPeriod(4f);
            player.healthHaver.ForceSetCurrentHealth(player.healthHaver.GetMaxHealth() / 2);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.OnPreDeath -= Rebirth;

            Color originalColor = player.sprite.color;

            player.sprite.color = Color.yellow;

            AkSoundEngine.PostEvent("guardian_angel_passive_SFX", GameManager.Instance.gameObject);

            // animations for the revive: animation of the player's health being restored
            // and the player being invulnerable for a short time
            // including sound effects and visual effects
            // during the invulnerability period, enemies be frozen in time???

            yield return new WaitForSeconds(4f);

            player.sprite.color = originalColor;

            player.ForceBlank();
            player.CurrentInputState = PlayerInputState.AllInput;
            player.healthHaver.PreventAllDamage = false;
        }
    }
}
