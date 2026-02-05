using Alexandria;
using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using LOLItems.passive_items;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// increase rarity, nerf execute gold amount with rng chance instead of guarantee
// increase execute threshold maybe

namespace LOLItems.passive_items
{
    internal class Collector : PassiveItem
    {
        public static string ItemName = "The Collector";

        // stats pool for item
        private static float DamageStat = 1.1f;
        private static int DeathGoldStat = 1;
        private static float DeathGoldChance = 0.30f;

        private static float ExecuteThreshold = 0.15f;

        public bool RETURNONINVESTMENTActivated = false;
        private static int RETURNONINVESTMENTDeathGoldStat = 2;
        public bool STROKEOFLUCKActivated = false;
        private static int STROKEOFLUCKGoldMultiplier = 2;
        public bool ANOFFERINGActivated = false;
        private static int ANOFFERINGGoldMultiplier = 3;
        public bool BETTERRNGActivated = false;
        private static float BETTERRNGDeathGoldChance = 0.75f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/the_collector_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Collector>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"death and taxes\"";
            string longDesc = "Increase damage\nBullets execute enemies at low health. Each kill has a chance to give an extra casing.\n\n" +
                "A weapon that once belonged to a legendary pirate. It now rests in your hands " +
                "and lends you a desire for gold. An orange sounds good right about now.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
            player.OnKilledEnemyContext += DeathGoldDrop;
            player.OnUsedPlayerItem += DarumaGoldDrop;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.PostProcessBeamTick -= OnPostProcessProjectile;
                player.OnKilledEnemyContext -= DeathGoldDrop;
                player.OnUsedPlayerItem -= DarumaGoldDrop;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.RETURN_ON_INVESTMENT) && !RETURNONINVESTMENTActivated)
                {
                    DeathGoldStat = RETURNONINVESTMENTDeathGoldStat;
                    //Plugin.Log($"{DeathGoldStat}");

                    RETURNONINVESTMENTActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.RETURN_ON_INVESTMENT) && RETURNONINVESTMENTActivated)
                {
                    DeathGoldStat = 1;
                    //Plugin.Log($"{DeathGoldStat}");

                    RETURNONINVESTMENTActivated = false;
                }

                if (Owner.HasSynergy(Synergy.BETTER_RNG) && !BETTERRNGActivated)
                {
                    DeathGoldChance = BETTERRNGDeathGoldChance;
                    //Plugin.Log("synergy 4 activate");

                    BETTERRNGActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.BETTER_RNG)&& BETTERRNGActivated)
                {
                    DeathGoldChance = 0.3f;

                    BETTERRNGActivated = false;
                }
            }

            base.Update();
        }

        // executes enemies below 5% health
        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (hitRigidbody == null) return;
            AIActor firstEnemy = null;
            if (hitRigidbody.aiActor != null)
            {
                firstEnemy = hitRigidbody.aiActor;
            }
            else if (hitRigidbody.GetComponentInParent<AIActor>() != null)
            {   
                firstEnemy = hitRigidbody.GetComponentInParent<AIActor>();
            }
            else
            {
                return;
            }
            if (hitRigidbody.healthHaver != null)
            {
                float currentHealthPercentage = firstEnemy.healthHaver.GetCurrentHealthPercentage();
                if (currentHealthPercentage <= ExecuteThreshold)
                {
                    // applies additional damage instance equal to their max health value
                    firstEnemy.healthHaver.ApplyDamage(
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
                if (enemy == null) return;
                if (enemy.aiActor == null || enemy.GetComponentInParent<AIActor>() == null) return;
                if (enemy.healthHaver != null)
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
                }
            };
        }

        // drops extra gold on enemy death
        private void DeathGoldDrop(PlayerController player, HealthHaver enemy)
        {
            enemy.healthHaver.OnDeath += (obj) =>
            {
                if (player.HasSynergy(Synergy.STROKE_OF_LUCK))
                {
                    foreach (PlayerItem item in player.activeItems)
                    {
                        if (item.PickupObjectId == (int)Items.FortunesFavor && item != null && item.IsCurrentlyActive)
                        {
                            //Plugin.Log($"synergy 2 fortunes favor work");
                            if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                            {
                                LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, STROKEOFLUCKGoldMultiplier * DeathGoldStat * 10);
                            }
                            else
                            {
                                LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, STROKEOFLUCKGoldMultiplier * DeathGoldStat);
                            }
                            return;
                        }
                    }
                }

                if (UnityEngine.Random.value < DeathGoldChance)
                {
                    //Plugin.Log($"randVal: {randVal}, goldChance: {DeathGoldChance}");
                    if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                    {
                        LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, DeathGoldStat * 10);
                    }
                    else
                    {
                        LootEngine.SpawnCurrency(enemy.specRigidbody.UnitCenter, DeathGoldStat);
                    }
                }
            };
        }

        private void DarumaGoldDrop(PlayerController player, PlayerItem item)
        {
            //Plugin.Log("daruma attempt");
            if (player != null && item != null)
            {
                if (Owner.HasSynergy(Synergy.AN_OFFERING))
                {
                    if (item.PickupObjectId == (int)Items.Daruma)
                    {
                        //Plugin.Log($"synergy 3 daruma work");
                        if (UnityEngine.Random.value < DeathGoldChance)
                        {
                            LootEngine.SpawnCurrency(Owner.specRigidbody.UnitCenter, ANOFFERINGGoldMultiplier * DeathGoldStat);
                        }
                    }
                }
            }
        }
    }
}
