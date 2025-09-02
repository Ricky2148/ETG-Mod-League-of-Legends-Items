using Alexandria;
using Alexandria.ItemAPI;
using Dungeonator;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// range around player that decreases enemy fire rate
// not complete

namespace LOLItems
{
    internal class FrozenHeart : AuraItem
    {
        // stats pool for item
        private static int ArmorStat = 1;

        private static float WintersCaressCrippleRatio = 0.8f;
        private static float WintersCaressRange = 8f;

        private static GameActorCrippleEffect WintersCaressCrippleEffect = new GameActorCrippleEffect
        {
            duration = 1f,
            effectIdentifier = "frozen_heart_cripple_effect",
            resistanceType = EffectResistanceType.None,
            AppliesOutlineTint = true,
            OutlineTintColor = ExtendedColours.skyblue
        };

        public static void Init()
        {
            string itemName = "Frozen Heart-LOLItems";
            string resourceName = "LOLItems/Resources/passive_item_sprites/frozen_heart_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<FrozenHeart>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Warning: Do not thaw!";
            string longDesc = "Emits a chilling air that causes those nearby to have cold hands. It might just be " +
                "a disguised AC unit. They keep complaining that they would've killed you if it weren't for their " +
                "cold hands.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            item.ArmorToGainOnInitialPickup = ArmorStat;
            item.AuraRadius = WintersCaressRange;
            item.DamagePerSecond = 0f;

            item.quality = PickupObject.ItemQuality.B;

            item.SetName("Frozen Heart");
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

        public override void DoAura()
        {
            if (m_extantAuraVFX == null)
            {
            }
            didDamageEnemies = false;
            if (AuraAction == null)
            {
                AuraAction = delegate (AIActor actor, float dist)
                {
                    WintersCaressCrippleEffect.CrippleAmount = WintersCaressCrippleRatio;
                    WintersCaressCrippleEffect.CrippleDuration = 0.5f;
                    actor.ApplyEffect(WintersCaressCrippleEffect);
                };
            }
            if (m_owner != null && m_owner.CurrentRoom != null)
            {
                m_owner.CurrentRoom.ApplyActionToNearbyEnemies(m_owner.CenterPosition, ModifiedAuraRadius, AuraAction);
            }
            if (didDamageEnemies)
            {
                m_owner.DidUnstealthyAction();
            }
        }
    }
}