using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.passive_items
{
    internal class FatedAshes : PassiveItem
    {
        public static string ItemName = "Fated Ashes";

        private static float InflameDamagePerSecond = 3f;
        private static float InflameDuration = 3f;

        private static Gun phoenix = PickupObjectDatabase.GetById((int)Items.Phoenix) as Gun;
        private static GameActorFireEffect InflameBurnEffect = new GameActorFireEffect
        {
            duration = InflameDuration,
            DamagePerSecondToEnemies = InflameDamagePerSecond,
            effectIdentifier = "inflame_burn",
            ignitesGoops = false,
            FlameVfx = phoenix.DefaultModule.projectiles[0].fireEffect.FlameVfx
        };

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/fated_ashes_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<FatedAshes>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "It's getting warm...";
            string longDesc = "Dealing damage burns enemies.\n\n" +
                "A vase storing the ashes of the guilty. Their guilt marks their fate for hell and, in turn, causes their ashes to burn up occasionally.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.D;

            item.UsesCustomCost = true;
            item.CustomCost = 20;

            item.AddToSubShop(ItemBuilder.ShopType.Goopton);
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.PostProcessBeamTick -= OnPostProcessProjectile;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.BUILDS_INTO_LIANDRYS_TORMENT))
                {
                    Owner.RemovePassiveItem(ID);

                    LootEngine.SpawnCurrency(Owner.specRigidbody.UnitCenter, this.PurchasePrice);
                }
            }

            base.Update();
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy == null) return;
                if (enemy.healthHaver == null) return;
                if (enemy.aiActor != null)
                {
                    enemy.aiActor.ApplyEffect(InflameBurnEffect);
                }
                else if (enemy.GetComponentInParent<AIActor>() != null)
                {
                    enemy.GetComponentInParent<AIActor>().ApplyEffect(InflameBurnEffect);
                }
            };
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody == null) return;
            if (hitRigidbody.healthHaver == null) return;
            if (hitRigidbody.aiActor != null)
            {
                hitRigidbody.aiActor.ApplyEffect(InflameBurnEffect);
            }
            else if (hitRigidbody.GetComponentInParent<AIActor>() != null)
            {
                hitRigidbody.GetComponentInParent<AIActor>().ApplyEffect(InflameBurnEffect);
            }
        }
    }
}
