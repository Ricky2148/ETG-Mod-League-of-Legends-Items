using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Alexandria;
using LOLItems.custom_class_data;

// fire rate, on hit: apply stack to enemies hit. At max stacks, enemy is charmed. Effect goes on cooldown
// while on cooldown, applying stacks is disabled
// look into balancing (lowering stack max, lowering tier)

namespace LOLItems.passive_items
{
    internal class Puppeteer : PassiveItem
    {
        public static string ItemName = "Puppeteer";

        private static float RateOfFireStat = 1.15f;
        private static float PullTheirStringsCharmDuration = 999f;
        private static float PullTheirStringsMaxStacks = 4f;
        private static float PullTheirStringsCooldown = 25f;
        private bool isOnCooldown = false;

        private Dictionary<AIActor, int> enemyCharmStacks = new Dictionary<AIActor, int>();
        private static GameActorCharmEffect CharmEffect = (PickupObjectDatabase.GetById(527)
            as BulletStatusEffectItem).CharmModifierEffect;

        public bool PLUS25CHARMActivated = false;
        private static float PLUS25CHARMPullTheirStringsMaxStacks = 3f;
        private static float PLUS25CHARMPullTheirStringsCooldown = 15f;
        public bool CHARMINGREINVIGORATIONActivated = false;

        private Coroutine itemCooldownCoroutine;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/puppeteer_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Puppeteer>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "dance, boy, dance!";
            string longDesc = "Increase fire rate\nEvery bullet applies a stack to enemies hit. Once any enemy reaches max stacks, charms them. Goes on a cooldown.\n\n" +
                "A marionette glove without its marionette. It allows you to control enemies but fills you with " +
                "an uneasy feeling. You could swear there's a creepy laugh in the gungeon's corridors now.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            CharmEffect.duration = PullTheirStringsCharmDuration;

            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
            player.OnUsedPlayerItem += OnCharmHornUsed;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.PostProcessBeamTick -= OnPostProcessProjectile;
                player.OnUsedPlayerItem -= OnCharmHornUsed;
            }

            if (enemyCharmStacks != null)
            {
                enemyCharmStacks.Clear();
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.PLUS25_CHARM) && !PLUS25CHARMActivated)
                {
                    PullTheirStringsMaxStacks = PLUS25CHARMPullTheirStringsMaxStacks;
                    PullTheirStringsCooldown = PLUS25CHARMPullTheirStringsCooldown;

                    PLUS25CHARMActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.PLUS25_CHARM) && PLUS25CHARMActivated)
                {
                    PullTheirStringsMaxStacks = 4f;
                    PullTheirStringsCooldown = 25f;

                    PLUS25CHARMActivated = false;
                }
            }

            base.Update();
        }

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            if (isOnCooldown) return;
            if (hitRigidbody == null) return;
            if (hitRigidbody.aiActor.GetEffect(CharmEffect.effectIdentifier) != null) return;
            if (hitRigidbody.aiActor != null)
            {
                PlayerController player = this.Owner;
                AIActor aiActor = hitRigidbody.aiActor;
                //Plugin.Log($"aiActor: {aiActor}, hitRigidBody: {hitRigidbody}, aiActor.aiActor: {aiActor.aiActor}");
                // simulate random chance to apply a stack upon hit
                //var rand = new System.Random();
                //int randomPool = 100;
                if (UnityEngine.Random.value <= 0.05f)
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
                    // if the hit enemy's stack count is at max stacks, trigger charm effect and cooldown
                    if (enemyCharmStacks[aiActor] >= PullTheirStringsMaxStacks)
                    {
                        enemyCharmStacks.Clear();
                        aiActor.ApplyEffect(CharmEffect);
                        itemCooldownCoroutine = StartCoroutine(StartPullTheirStringsCooldown(player));
                    }
                }
            }
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (isOnCooldown) return;
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                proj.OnHitEnemy += (proj, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.aiActor.GetEffect(CharmEffect.effectIdentifier) != null) return;
                    if (enemy.aiActor != null)
                    {
                        PlayerController player = this.Owner;
                        AIActor aiActor = enemy.aiActor;
                        //Plugin.Log($"aiActor: {aiActor}, enemy: {enemy}, aiActor.aiActor: {aiActor.aiActor}");
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
                            itemCooldownCoroutine = StartCoroutine(StartPullTheirStringsCooldown(player));
                        }
                    }
                };
            }
        }

        private void OnCharmHornUsed(PlayerController player, PlayerItem item)
        {
            if (player != null && item != null)
            {
                if (Owner.HasSynergy(Synergy.CHARMING_REINVIGORATION))
                {
                    if (item.PickupObjectId == (int)Items.CharmHorn)
                    {
                        StopCoroutine(itemCooldownCoroutine);
                        enemyCharmStacks.Clear();
                        isOnCooldown = false;
                    }
                }
            }
        }

        private System.Collections.IEnumerator StartPullTheirStringsCooldown(PlayerController player)
        {
            //Plugin.Log("start cooldown");

            isOnCooldown = true;
            yield return new WaitForSeconds(PullTheirStringsCooldown);
            isOnCooldown = false;
            enemyCharmStacks.Clear();

            //Plugin.Log("end cooldown");

            //tk2dBaseSprite s = this.sprite;
            //GameUIRoot.Instance.RegisterDefaultLabel(s.transform, new Vector3(s.GetBounds().max.x + 0f, s.GetBounds().min.y + 0f, 0f), $"{this.EncounterNameOrDisplayName} ready");
            //yield return new WaitForSeconds(1.5f);
            //GameUIRoot.Instance.DeregisterDefaultLabel(s.transform);
            yield break;
        }
    }
}