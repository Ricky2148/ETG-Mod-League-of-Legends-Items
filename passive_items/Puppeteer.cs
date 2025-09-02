using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Alexandria;

// fire rate, on hit: apply stack to enemies hit. At max stacks, enemy is charmed. Effect goes on cooldown
// while on cooldown, applying stacks is disabled
// complete

namespace LOLItems.passive_items
{
    internal class Puppeteer : PassiveItem
    {
        private static float RateOfFireStat = 1.15f;
        private static float PullTheirStringsCharmDuration = 10f;
        private static float PullTheirStringsCooldown = 25f;
        private static float PullTheirStringsMaxStacks = 5f;
        private bool isOnCooldown = false;

        private Dictionary<AIActor, int> enemyCharmStacks = new Dictionary<AIActor, int>();
        private static GameActorCharmEffect CharmEffect = (PickupObjectDatabase.GetById(527)
            as BulletStatusEffectItem).CharmModifierEffect;

        public static void Init()
        {
            string itemName = "Puppeteer";
            string resourceName = "LOLItems/Resources/passive_item_sprites/puppeteer_pixelart_sprite";

            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<Puppeteer>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "dance, boy, dance!";
            string longDesc = "A marionette glove without its marionette. It allows you to control enemies but fills you with " +
                "an uneasy feeling. You could swear there's a creepy laugh in the gungeon's corridors now.\n";
            
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            
            CharmEffect.duration = PullTheirStringsCharmDuration;

            item.quality = PickupObject.ItemQuality.A;
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
            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.PostProcessBeamTick -= OnPostProcessProjectile;

            enemyCharmStacks.Clear();
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (isOnCooldown) return;
            if (hitRigidbody != null && hitRigidbody.aiActor != null)
            {
                PlayerController player = this.Owner;
                AIActor aiActor = hitRigidbody.aiActor;
                // simulate random chance to apply a stack upon hit
                var rand = new System.Random();
                int randomPool = 100;
                if (rand.Next(randomPool) == 0)
                {
                    // increase stack count if enemy is already in dictionary
                    if (!enemyCharmStacks.ContainsKey(aiActor))
                    {
                        enemyCharmStacks.Add(aiActor, 1);
                    }
                    // if not, add them to dictionary with 1 stack
                    else
                    {
                        enemyCharmStacks[aiActor] += 1;
                    }
                }

                // if the hit enemy's stack count is at max stacks, trigger charm effect and cooldown
                if (enemyCharmStacks[aiActor] >= PullTheirStringsMaxStacks)
                {
                    enemyCharmStacks.Clear();
                    aiActor.ApplyEffect(CharmEffect);
                    StartCoroutine(StartPullTheirStringsCooldown(player));
                }
            }
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (isOnCooldown) return;
            proj.OnHitEnemy += (proj, enemy, fatal) =>
            {
                if (enemy != null || enemy.aiActor != null)
                {
                    PlayerController player = this.Owner;
                    AIActor aiActor = enemy.aiActor;
                    // increase stack count if enemy is already in dictionary
                    if (!enemyCharmStacks.ContainsKey(aiActor))
                    {
                        enemyCharmStacks.Add(aiActor, 1);
                    }
                    // if not, add them to dictionary with 1 stack
                    else
                    {
                        enemyCharmStacks[aiActor] += 1;
                    }
                    // if the hit enemy's stack count is at max stacks, trigger charm effect and cooldown
                    if (enemyCharmStacks[aiActor] >= PullTheirStringsMaxStacks)
                    {
                        enemyCharmStacks.Clear();
                        aiActor.ApplyEffect(CharmEffect);
                        StartCoroutine(StartPullTheirStringsCooldown(player));
                    }
                }
            };
        }

        private System.Collections.IEnumerator StartPullTheirStringsCooldown(PlayerController player)
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(PullTheirStringsCooldown);
            isOnCooldown = false;
            enemyCharmStacks.Clear();
            yield break;
        }
    }
}
