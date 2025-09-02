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
    internal class ZhonyasHourglass : PlayerItem
    {
        // stats pool for item
        private static float ArmorStat = 2.0f;
        private bool hasGainedArmor = false;
        private static float StasisDuration = 2.5f;
        private static float StasisCooldown = 120f;

        public static void Init()
        {
            string itemName = "Zhonya's Hourglass";
            string resourceName = "LOLItems/Resources/active_item_sprites/zhonyas_hourglass_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ZhonyasHourglass>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Borrowed Time";
            string longDesc = "A sand stopwatch that allows the user to suspend their life for a few moments. " +
                "It's believed that a pharaoh used it to reminisce his last moments during his empire's fall.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, StasisCooldown);
            item.consumable = false;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.A;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            if (!hasGainedArmor) player.healthHaver.Armor += ArmorStat;
            hasGainedArmor = true;
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void DoEffect(PlayerController player)
        {
            player.StartCoroutine(StasisCoroutine(player));
        }

        // upon activation, player enters invul then forces blank effect after invul
        private System.Collections.IEnumerator StasisCoroutine(PlayerController player)
        {
            player.healthHaver.TriggerInvulnerabilityPeriod(StasisDuration);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.PreventAllDamage = true;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;

            // find a better color later
            player.sprite.color = ExtendedColours.honeyYellow;
            player.CurrentGun.sprite.color = ExtendedColours.honeyYellow;

            AkSoundEngine.PostEvent("zhonyas_hourglass_activation_SFX", GameManager.Instance.gameObject);

            yield return new WaitForSeconds(StasisDuration);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;

            player.ForceBlank();
            player.CurrentInputState = PlayerInputState.AllInput;

            // wait an extra 0.25 seconds to prevent player from immediate collision damage
            yield return new WaitForSeconds(0.25f);
            player.healthHaver.PreventAllDamage = false;

            AkSoundEngine.PostEvent("zhonyas_hourglass_ending_SFX", GameManager.Instance.gameObject);
        }
    }
}
