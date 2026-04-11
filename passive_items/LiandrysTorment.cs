using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//health, dmg, every bullet applies a burn effect that deals dmg over time, DOT is %max health of enemy (scales on bosses and mini bosses)
// this applies the regular burn effect: damage, duration, and vfx all seem to match
// look into changing the color of the fire

namespace LOLItems.passive_items
{
    internal class LiandrysTorment : PassiveItem
    {
        public static string ItemName = "Liandry's Torment";

        // stats pool for item
        private static float DamageStat = 1.15f;
        private static float HealthStat = 1f;
        private static float TormentBaseDamage = 5f;
        private static float TormentPercentHealthDamage = 0.15f;
        private static float TormentDuration = 3f;

        // tracks enemies affected by the torment burn effect and their coroutines
        //private Dictionary<AIActor, Coroutine> TormentAffectedEnemies = new Dictionary<AIActor, Coroutine>();
        private static Gun phoenix = PickupObjectDatabase.GetById(99) as Gun;
        private static GameActorFireEffect TormentBurnEffect = new GameActorFireEffect
        {
            duration = TormentDuration,
            DamagePerSecondToEnemies = 0f,
            effectIdentifier = "liandrys_torment_burn",
            ignitesGoops = false,
            FlameVfx = phoenix.DefaultModule.projectiles[0].fireEffect.FlameVfx,
        };

        public bool BURNINGVENGENCEActivated = false;
        private static float BURNINGVENGENCETormentDurationInc = 3f;
        public bool BLAZINGUNIVERSEActivated = false;
        private static float BLAZINGUNIVERSETormentDamageIncScale = 2.0f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/liandrys_torment_pixelart_sprite_small";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<LiandrysTorment>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "Cursed Mask";
            string longDesc = "+1 Heart\nDealing damage burns enemies for %max health damage.\n\n" +
                "Once belonged to a theatre company and used as a prop in their most infamous act. " +
                "Rumors claim that each run of the act needed new actors since one actor always died mysteriously. " +
                "\nSomething tells you that this mask was connected to these incidents.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.B;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
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
                if (Owner.HasSynergy(Synergy.BURNING_VENGENCE) && !BURNINGVENGENCEActivated)
                {
                    TormentBurnEffect.duration = TormentDuration + BURNINGVENGENCETormentDurationInc;

                    BURNINGVENGENCEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.BURNING_VENGENCE) && BURNINGVENGENCEActivated)
                {
                    TormentBurnEffect.duration = TormentDuration;

                    BURNINGVENGENCEActivated = false;
                }
                if (Owner.HasSynergy(Synergy.BLAZING_UNIVERSE) && !BLAZINGUNIVERSEActivated)
                {
                    TormentBaseDamage *= BLAZINGUNIVERSETormentDamageIncScale;
                    TormentPercentHealthDamage *= BLAZINGUNIVERSETormentDamageIncScale;

                    BLAZINGUNIVERSEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.BLAZING_UNIVERSE) && BLAZINGUNIVERSEActivated)
                {
                    TormentBaseDamage = 5f;
                    TormentPercentHealthDamage = 0.10f;
                    BLAZINGUNIVERSEActivated = false;
                }
            }

            base.Update();
        }

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
                // sets burn effect's damage based on the enemy's max health
                TormentBurnEffect.DamagePerSecondToEnemies = (firstEnemy.healthHaver.GetMaxHealth() * TormentPercentHealthDamage) + TormentBaseDamage;
                if (firstEnemy.healthHaver.IsBoss || firstEnemy.healthHaver.IsSubboss)
                {
                    TormentBurnEffect.DamagePerSecondToEnemies *= 0.25f; // Reduce damage for bosses and minibosses
                }

                firstEnemy.ApplyEffect(TormentBurnEffect);
            }
        }

        private void OnPostProcessProjectile (Projectile proj, float f)
        {
            proj.OnHitEnemy += (projHit, enemy, fatal) =>
            {
                if (enemy == null) return;
                AIActor target = null;
                if (enemy.aiActor != null)
                {
                    target = enemy.aiActor;
                    //Plugin.Log($"enemy.aiActor: {target}");
                }
                else if (enemy.GetComponentInParent<AIActor>() != null)
                {
                    target = enemy.GetComponentInParent<AIActor>();
                    //Plugin.Log($"enemy.parentActor: {target}");
                }
                else
                {
                    //Plugin.Log("target = null");
                    return;
                }
                if (enemy.healthHaver != null)
                {

                    // sets burn effect's damage based on the enemy's max health
                    TormentBurnEffect.DamagePerSecondToEnemies = (target.healthHaver.GetMaxHealth() * TormentPercentHealthDamage) + TormentBaseDamage;
                    if (target.healthHaver.IsBoss || target.healthHaver.IsSubboss)
                    {
                        TormentBurnEffect.DamagePerSecondToEnemies *= 0.25f; // Reduce damage for bosses and minibosses
                    }

                    target.ApplyEffect(TormentBurnEffect);

                    /*
                    if (!TormentAffectedEnemies.ContainsKey(aiActor))
                    {
                        Coroutine coroutine = aiActor.StartCoroutine(ApplyTormentEffect(aiActor));
                        TormentAffectedEnemies[aiActor] = coroutine;
                    }
                    else
                    {
                        aiActor.StopCoroutine(TormentAffectedEnemies[aiActor]);
                        Coroutine coroutine = aiActor.StartCoroutine(ApplyTormentEffect(aiActor));
                        TormentAffectedEnemies[aiActor] = coroutine;
                    }
                    */
                }
            };
        }

        // outdated logic for manually applying the damage ticks
        /*private System.Collections.IEnumerator ApplyTormentEffect (AIActor enemy)
        {
            float elapsed = 0f;
            float dotTickInterval = 0.5f;

            yield return new WaitForSeconds(0.25f);

            while (elapsed < TormentDuration && enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsAlive)
            {
                float tormentDamageToDeal = enemy.healthHaver.GetMaxHealth() * TormentPercentHealthDamage * dotTickInterval * 0;
                if(enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                {
                    tormentDamageToDeal *= 0.25f;
                }
                enemy.healthHaver.ApplyDamage(
                    tormentDamageToDeal,
                    Vector2.zero,
                    "liandrys_max_health_burn_tick",
                    CoreDamageTypes.None,
                    DamageCategory.Normal,
                    false,
                    null,
                    false
                );
                Plugin.Log($"Applying Liandry's torment damage: {tormentDamageToDeal} to {enemy.aiActor.ActorName}");
                Plugin.Log($"Elapsed time: {elapsed} seconds");
                
                yield return new WaitForSeconds(dotTickInterval);
                elapsed += dotTickInterval;
            }
            TormentAffectedEnemies.Remove(enemy);
        }*/
    }
}
