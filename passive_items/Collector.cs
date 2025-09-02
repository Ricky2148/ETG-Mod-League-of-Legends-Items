using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;

namespace LOLItems
{
    internal class Collector : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.1f;
        private static int DeathGoldStat = 1;

        private static float ExecuteThreshold = 0.05f;

        public static void Init()
        {
            string itemName = "The Collector";
            string resourceName = "LOLItems/Resources/passive_item_sprites/the_collector_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Collector>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"death and taxes\"";
            string longDesc = "A weapon that once belonged to a legendary pirate. It now rests in your hands " +
                "and lends you a desire for gold. An orange sounds good right about now.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
            player.OnKilledEnemyContext += DeathGoldDrop;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;
            player.OnKilledEnemyContext -= DeathGoldDrop;
        }

        // executes enemies below 5% health
        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody != null || hitRigidbody.aiActor != null)
            {
                float currentHealthPercentage = hitRigidbody.aiActor.healthHaver.GetCurrentHealthPercentage();
                if (currentHealthPercentage <= ExecuteThreshold)
                {
                    // applies additional damage instance equal to their max health value
                    hitRigidbody.aiActor.healthHaver.ApplyDamage(
                        hitRigidbody.aiActor.healthHaver.GetMaxHealth(),
                        Vector2.zero,
                        "the_collector_death_execute",
                        CoreDamageTypes.None,
                        DamageCategory.Unstoppable,
                        false
                    );
                }
            };
        }

        // executes enemies below 5% health
        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                float currentHealthPercentage = enemy.healthHaver.GetCurrentHealthPercentage();
                if (currentHealthPercentage <= ExecuteThreshold)
                {
                    // applies additional damage instance equal to their max health value
                    enemy.healthHaver.ApplyDamage(
                        enemy.healthHaver.GetMaxHealth(),
                        Vector2.zero,
                        "the_collector_death_execute",
                        CoreDamageTypes.None,
                        DamageCategory.Unstoppable,
                        false
                    );
                }
            };
        }

        // drops extra gold on enemy death
        private void DeathGoldDrop(PlayerController player, HealthHaver enemy)
        {
            enemy.healthHaver.OnDeath += (obj) =>
            {
                if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                {
                    LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, DeathGoldStat * 10);
                }
                else
                {
                    LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, DeathGoldStat);
                }
            };
        }
    }
}
