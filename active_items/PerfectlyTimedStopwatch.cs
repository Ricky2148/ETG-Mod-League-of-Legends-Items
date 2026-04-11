using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace LOLItems.active_items
{
    internal class PerfectlyTimedStopwatch : PlayerItem
    {
        public static string ItemName = "Perfectly Timed Stopwatch";

        private static float StasisDuration = 2.5f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/perfectly_timed_stopwatch_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<PerfectlyTimedStopwatch>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"yea, I got time.\"";
            string longDesc = "Enter a one-time stasis, where you're invulnerable but also can't do anything for a duration, then activates a blank.\n\n" +
                "A stopwatch that is stuck at 2:37:56. Activating it will affect time itself but it'll shatter instantly since it's been stuck for so long.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.None, 0);
            item.consumable = true;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.D;

            item.UsesCustomCost = true;
            item.CustomCost = 20;

            item.AddToSubShop(ItemBuilder.ShopType.OldRed);
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void Update()
        {
            if (LastOwner != null)
            {
                if (LastOwner.HasSynergy(Synergy.BUILDS_INTO_ZHONYAS_HOURGLASS))
                {
                    LastOwner.RemoveActiveItem(ID);

                    LootEngine.SpawnCurrency(LastOwner.specRigidbody.UnitCenter, this.PurchasePrice);
                }
            }

            base.Update();
        }

        public override void DoEffect(PlayerController player)
        {
            player.StartCoroutine(StasisCoroutine(player));
        }

        private System.Collections.IEnumerator StasisCoroutine(PlayerController player)
        {
            player.healthHaver.TriggerInvulnerabilityPeriod(StasisDuration + 0.1f);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.PreventAllDamage = true;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;
            //float ogFpsScale = player.aiAnimator.FpsScale;

            // find a better color later
            player.sprite.color = ExtendedColours.honeyYellow;
            player.CurrentGun.sprite.color = ExtendedColours.honeyYellow;
            //player.aiAnimator.FpsScale = 0f;
            //player.spriteAnimator.OverrideTimeScale = 0f;

            //player.TriggerInvulnerableFrames(StasisDuration + 0.1f);
            //player.CurrentInputState = PlayerInputState.NoInput;

            AkSoundEngine.PostEvent("zhonyas_hourglass_activation_SFX", GameManager.Instance.gameObject);

            yield return new WaitForSeconds(StasisDuration);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;
            //player.aiAnimator.FpsScale = ogFpsScale;
            //player.spriteAnimator.OverrideTimeScale = 1f;

            player.ForceBlank();
            player.CurrentInputState = PlayerInputState.AllInput;

            // wait an extra 0.25 seconds to prevent player from immediate collision damage
            yield return new WaitForSeconds(0.25f);
            player.healthHaver.PreventAllDamage = false;

            AkSoundEngine.PostEvent("zhonyas_hourglass_ending_SFX", GameManager.Instance.gameObject);
        }
    }
}
