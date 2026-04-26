using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;
using LOLItems.custom_class_data;

namespace LOLItems.passive_items
{
    internal class Sheen : SpellbladePassiveItem
    {
        public static string ItemName = "Sheen";

        private static float spellbladeDmg = 10f;
        private static float spellbladeCooldown = 3f;
        private static string spellbladeDamageIdentifier = "sheen_spellblade_damage";

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/sheen_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Sheen>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "who makes a sword out of ice?";
            string longDesc = "Grants Spellblade every few seconds. Spellblade: Empowers next bullet with some additional damage.\n\n" +
                "A sword made out of ice... It's been magically enchanted to mend itself when shattered, but since it's made of ice, it always shatters...\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.D;

            item.activationDmgValue = spellbladeDmg;
            item.activationCooldownValue = spellbladeCooldown;
            item.damageIdentifier = spellbladeDamageIdentifier;

            item.UsesCustomCost = true;
            item.CustomCost = 20;

            ID = item.PickupObjectId;
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

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.BUILDS_INTO_TRINITY_FORCE) || Owner.HasSynergy(Synergy.BUILDS_INTO_DIVINE_SUNDERER) || Owner.HasSynergy(Synergy.BUILDS_INTO_ESSENCE_REAVER) || Owner.HasSynergy(Synergy.BUILDS_INTO_LICH_BANE))
                {
                    Owner.RemovePassiveItem(ID);

                    LootEngine.SpawnCurrency(Owner.specRigidbody.UnitCenter, this.PurchasePrice);
                }
            }

            base.Update();
        }

        /*public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.OnReloadedGun += OnGunReloaded;
            player.PostProcessProjectile += OnPostProcessProjectile;
            shouldApplySpellblade = true;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.OnReloadedGun -= OnGunReloaded;
                player.PostProcessProjectile -= OnPostProcessProjectile;
            }

            shouldApplySpellblade = false;
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (isOnCooldown)
                {
                    float now = BraveTime.ScaledTimeSinceStartup;
                    //Plugin.Log($"curTime: {now}");
                    if (now < CooldownTimer)
                    {
                        base.Update();
                        return;
                    }
                    Plugin.Log($"cooldown ended: curTime: {BraveTime.ScaledTimeSinceStartup}");
                    isOnCooldown = false;
                }
            }

            base.Update();
        }

        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            if (isOnCooldown) return;
            shouldApplySpellblade = true;
            Plugin.Log($"sheen activated: {shouldApplySpellblade}");
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            //Plugin.Log($"spellblade proc: {shouldApplySpellblade}");
            if (proj.Shooter == proj.Owner.specRigidbody && shouldApplySpellblade)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (!shouldApplySpellblade) return;
                    if (enemy == null) return;
                    if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                    if (enemy.healthHaver != null)
                    {
                        enemy.healthHaver.ApplyDamage(
                            spellbladeDmg,
                            Vector2.zero,
                            "sheen_spellblade_damage",
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                        );
                    }
                    Plugin.Log($"cooldown started, shouldApplySpellblade: {shouldApplySpellblade}, curTime: {BraveTime.ScaledTimeSinceStartup}, expected cooldown end time: {BraveTime.ScaledTimeSinceStartup + spellbladeCooldown}");
                    shouldApplySpellblade = false;
                    isOnCooldown = true;
                    CooldownTimer = BraveTime.ScaledTimeSinceStartup + spellbladeCooldown;
                };
            }
        }*/
    }
}
