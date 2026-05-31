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

//add vfx and sfx during revive duration

namespace LOLItems.active_items
{
    internal class ZhonyasHourglass : PlayerItem
    {
        public static string ItemName = "Zhonya's Hourglass";

        // stats pool for item
        //private static float DamageStat = 1.05f;
        private static float ArmorStat = 2.0f;
        private bool hasGainedArmor;
        private static float StasisDuration = 2.5f;
        private static float StasisCooldown = 120f;

        public bool CHAOSCONTROLActivated = false;
        private static float CHAOSCONTROLStasisCooldown = 45f;
        public bool SEVENSECONDSREMAINActivated = false;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/zhonyas_hourglass_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ZhonyasHourglass>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Borrowed Time";
            string longDesc = "Enter stasis, where you're invulnerable but also can't do anything for a duration, then activates a blank.\n\n" +
                "A sand stopwatch that allows the user to suspend their life for a few moments. " +
                "It's believed that a pharaoh used it to reminisce his last moments during his empire's fall.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, StasisCooldown);
            item.consumable = false;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.hasGainedArmor = false;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.A;
            item.AddToSubShop(ItemBuilder.ShopType.OldRed);
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            if (!m_pickedUpThisRun) player.healthHaver.Armor += ArmorStat;

            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            base.Pickup(player);
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
                if (LastOwner.HasSynergy(Synergy.CHAOS_CONTROL) && !CHAOSCONTROLActivated)
                {
                    timeCooldown -= CHAOSCONTROLStasisCooldown;
                    CurrentTimeCooldown -= CHAOSCONTROLStasisCooldown;

                    CHAOSCONTROLActivated = true;
                }
                else if (!LastOwner.HasSynergy(Synergy.CHAOS_CONTROL) && CHAOSCONTROLActivated)
                {
                    timeCooldown += CHAOSCONTROLStasisCooldown;
                    CurrentTimeCooldown += CHAOSCONTROLStasisCooldown;

                    CHAOSCONTROLActivated = false;
                }
            }

            base.Update();
        }

        public override void DoEffect(PlayerController player)
        {
            player.StartCoroutine(StasisCoroutine(player));
        }

        // upon activation, player enters invul then forces blank effect after invul
        private System.Collections.IEnumerator StasisCoroutine(PlayerController player)
        {
            player.healthHaver.TriggerInvulnerabilityPeriod(StasisDuration + 0.1f);

            if (!player.HasSynergy(Synergy.SEVEN_SECONDS_REMAIN)) player.CurrentInputState = PlayerInputState.NoInput;
            
            player.healthHaver.PreventAllDamage = true;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;
            //float ogFpsScale = player.aiAnimator.FpsScale;

            // find a better color later
            player.sprite.color = ExtendedColours.honeyYellow;
            player.CurrentGun.sprite.color = ExtendedColours.honeyYellow;

            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(255f * 0.7f, 180f * 0.7f, 18f * 0.7f));
            }

            //player.aiAnimator.FpsScale = 0f;
            //player.spriteAnimator.OverrideTimeScale = 0f;

            //player.TriggerInvulnerableFrames(StasisDuration + 0.1f);
            //player.CurrentInputState = PlayerInputState.NoInput;

            AkSoundEngine.PostEvent("zhonyas_hourglass_activation_SFX", GameManager.Instance.gameObject);

            /*
            Vector2 unitDimensions = player.specRigidbody.HitboxPixelCollider.UnitDimensions;
            Vector2 a = unitDimensions / 2f;

            Vector2 vector = player.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
            Vector2 vector2 = player.specRigidbody.HitboxPixelCollider.UnitTopRight;
            vector += Vector2.Min(a * 0.15f, new Vector2(0.25f, 0.25f));
            vector2 -= Vector2.Min(a * 0.15f, new Vector2(0.25f, 0.25f));
            vector2.y -= Mathf.Min(a.y * 0.1f, 0.1f);

            HelpfulMethods.DoRandomParticleBurst(15, vector, vector2, 1f, 0.7f, 0.4f, 2, ExtendedColours.honeyYellow, GlobalSparksDoer.SparksType.SOLID_SPARKLES);
            */

            yield return new WaitForSeconds(StasisDuration);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;

            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }

            //player.aiAnimator.FpsScale = ogFpsScale;
            //player.spriteAnimator.OverrideTimeScale = 1f;

            player.ForceBlank();

            if (!player.HasSynergy(Synergy.SEVEN_SECONDS_REMAIN)) player.CurrentInputState = PlayerInputState.AllInput;

            AkSoundEngine.PostEvent("zhonyas_hourglass_ending_SFX", GameManager.Instance.gameObject);
            
            // wait an extra 0.25 seconds to prevent player from immediate collision damage
            yield return new WaitForSeconds(0.25f);
            player.healthHaver.PreventAllDamage = false;
        }
    }
}