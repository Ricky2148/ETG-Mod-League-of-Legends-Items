using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using Alexandria;
using UnityEngine;
using Dungeonator;

//health, active targets a circle zone that applies a heal to players and dmg to enemies, the effect casts down after a delay
// NOT COMPLETE

namespace LOLItems
{
    internal class Redemption : TargetedAttackPlayerItem
    {
        private static float HealthStat = 1f;

        private static float InterventionPercentMaxHealth = 10f;
        private static float InterventionHealAmount = 0.5f;
        private static float InterventionActivationRange = 10f;
        private static float InterventionEffectRadius = 5f;
        private static float InterventionCooldown = 90f;

        public static void Init()
        {
            string itemName = "Redemption";
            string resourceName = "LOLItems/Resources/active_item_sprites/redemption_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Redemption>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Break their stride.";
            string longDesc = "Increases health, damage, and fire rate. Active attacks in a circle around the player, slowing enemies hit and dealing set damage.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, InterventionCooldown);
            item.consumable = false;

            item.minDistance = 0f;
            item.maxDistance = InterventionActivationRange;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.A;
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

        public override void DoEffect(PlayerController player)
        {
            IsCurrentlyActive = true;
            m_currentUser = player;
            GameObject reticleObject = (PickupObjectDatabase.GetById(462) as TargetedAttackPlayerItem).reticleQuad;
            m_extantReticleQuad = reticleObject.GetComponent<tk2dBaseSprite>();
            m_currentAngle = BraveMathCollege.Atan2Degrees(m_currentUser.unadjustedAimPoint.XY() - m_currentUser.CenterPosition);
            m_currentDistance = 5f;
            UpdateReticlePosition();
            spriteAnimator.Play("Activate");
        }

        public override void DoActiveEffect(PlayerController user)
        {
            if (user && user.CurrentRoom != null)
            {
                Vector2 cachedPosition = m_extantReticleQuad.gameObject.transform.position + new Vector3(0, 0.25f);
                if (m_extantReticleQuad) { Destroy(m_extantReticleQuad.gameObject); }
                IsCurrentlyActive = true;
                AkSoundEngine.PostEvent("Play_OBJ_computer_boop_01", user.gameObject);

                Exploder.Explode(cachedPosition, new ExplosionData
                {
                    damage = 40f,
                    damageRadius = 5f,
                    doDamage = true,
                    doForce = true,
                    force = 25f,
                    debrisForce = 10f,
                    preventPlayerForce = true,
                    doScreenShake = true,
                    playDefaultSFX = true
                }, Vector2.zero);

                IsCurrentlyActive = false;
                user.DropActiveItem(this);
            }
        }
    }
}
