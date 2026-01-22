using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Dungeonator;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//add vfx and sfx during revive duration
// sometimes can get hit immediately after revive ends, add extra iframes?
// has a bug where the player gets stuck in last animation after invul ends, bug gets fixed by simply dodge rolling

namespace LOLItems.passive_items
{
    internal class GuardianAngel : PassiveItem
    {
        public static string ItemName = "Guardian Angel";

        // stats pool for item
        private static float DamageStat = 1.25f;
        private static int ArmorStat = 2;
        private bool hasRevived = false;

        private static float DIVINEJUDGEMENTRange = 5f;
        private static float DIVINEJUDGEMENTDamage = 30f;
        public bool WHYWONTYOUDIEActivated;
        private static float WHYWONTYOUDIEHealthStat = 1f;
        public bool WHYWONTYOUDIEFirstActivation = true;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/guardian_angel_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<GuardianAngel>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"Heroes never die!\"";
            string longDesc = "+2 Armor, Increase damage\nRevives you once on death.\n\n" +
                "A blade imbued with the hope of a cult who believed in rebirth. You almost feel " +
                "like you could defy even death with it in hand.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.ArmorToGainOnInitialPickup = ArmorStat;

            item.quality = PickupObject.ItemQuality.S;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.healthHaver.OnPreDeath += Rebirth;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.healthHaver.OnPreDeath -= Rebirth;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.WHY_WONT_YOU_DIE) && !WHYWONTYOUDIEActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Health, WHYWONTYOUDIEHealthStat);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);
                    if (WHYWONTYOUDIEFirstActivation)
                    {
                        Owner.healthHaver.ApplyHealing(1f);

                        WHYWONTYOUDIEFirstActivation = false;
                    }
                    //Plugin.Log($"why wont you die activated, healthStat: {Owner.stats.GetStatValue(PlayerStats.StatType.Health)}");

                    WHYWONTYOUDIEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.WHY_WONT_YOU_DIE) && WHYWONTYOUDIEActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Health);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    WHYWONTYOUDIEActivated = false;
                }
            }

            base.Update();
        }

        private void Rebirth(Vector2 DeathPositon)
        {
            if (!hasRevived && this.Owner is PlayerController player)
            {
                hasRevived = true;
                player.StartCoroutine(ReviveCoroutine(player));
            }
        }

        // revives the player with half health and invulnerability for a short time, activates a blank after invul
        private System.Collections.IEnumerator ReviveCoroutine(PlayerController player)
        {
            // makes player character invulnerable, reset health, take no inputs from player, and remove revive effect
            player.healthHaver.TriggerInvulnerabilityPeriod(4.1f);
            //player.TriggerInvulnerableFrames(4.1f);
            player.healthHaver.ForceSetCurrentHealth(player.healthHaver.GetMaxHealth() / 2);
            player.CurrentInputState = PlayerInputState.NoInput;
            player.healthHaver.OnPreDeath -= Rebirth;

            Color originalPlayerColor = player.sprite.color;
            Color originalGunColor = player.CurrentGun.sprite.color;

            player.sprite.color = ExtendedColours.paleYellow;
            player.CurrentGun.sprite.color = ExtendedColours.paleYellow;

            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(242f * 0.7f, 238f * 0.7f, 148f * 0.7f));
            }

            AkSoundEngine.PostEvent("guardian_angel_passive_SFX", GameManager.Instance.gameObject);

            // animations for the revive: animation of the player's health being restored
            // and the player being invulnerable for a short time
            // including sound effects and visual effects
            // during the invulnerability period, enemies be frozen in time???

            yield return new WaitForSeconds(4f);

            player.sprite.color = originalPlayerColor;
            player.CurrentGun.sprite.color = originalGunColor;

            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }

            // trigger blank to push away enemies and clear bullets, restore input, and remove invulerability
            player.ForceBlank();

            if (player.HasSynergy(Synergy.DIVINE_JUDGEMENT))
            {
                DoDivineJudgement(player);
            }

            player.CurrentInputState = PlayerInputState.AllInput;
            player.healthHaver.PreventAllDamage = false;
        }

        private void DoDivineJudgement(PlayerController player)
        {
            if (player.CurrentRoom == null) return;

            List<AIActor> enemyList = player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (enemyList != null)
            {
                foreach (AIActor enemy in enemyList)
                {
                    if (enemy != null && enemy.healthHaver != null && enemy.healthHaver.IsVulnerable)
                    {
                        float dist = Vector2.Distance(player.CenterPosition, enemy.CenterPosition);
                        float damageToDeal = DIVINEJUDGEMENTDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

                        if (dist <= DIVINEJUDGEMENTRange)
                        {
                            enemy.healthHaver.ApplyDamage(
                                damageToDeal,
                                Vector2.zero,
                                "thornmail_blank_damage",
                                CoreDamageTypes.None,
                                DamageCategory.Normal,
                                false
                            );
                        }
                    }
                }
            }
        }
    }
}
