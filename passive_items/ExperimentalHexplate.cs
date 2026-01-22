using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;
using Alexandria.Misc;
using LOLItems.custom_class_data;

//health, dmg, and fire rate, extra dmg and fire rate when using an item

namespace LOLItems.passive_items
{
    internal class ExperimentalHexplate : PassiveItem
    {
        public static string ItemName = "Experimental Hexplate";

        // stats pool for item
        private static float DamageStat = 1.1f;
        private static float RateOfFireStat = 1.1f;
        private static float HealthStat = 1f;
        private static float OverdriveDuration = 8f;
        private static float OverdriveCooldown = 30f;
        private static float OverdriveRateOfFireStat = 1.5f;
        private static float overdriveMovementSpeedStat = 1.25f;
        private bool isOverdriveActive = false;
        public bool isOnCooldown = false;

        private int cigaretteUses = 0;
        private static float DamageStatPerCigUse = 0.05f;

        public bool SPEEDBLITZActivated = false;
        private static float SPEEDBLITZMovementSpeedStat = 1.5f;
        public bool FILLERUPActivated = false;
        private static float FILLERUPRateOfFireStat = 2f;

        public static int ID;
        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/experimental_hexplate_pixelart_sprite";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<ExperimentalHexplate>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Ethically Questionable";
            // maybe add effect explanation?
            string longDesc = "+1 Heart, Increase damage and fire rate\nActivating an item increases your fire rate and movespeed for a few seconds. Goes on a cooldown.\n\n" +
                "This strange piece of armor appears to be mechanically equipped to help the user " +
                "enhance their physical abilities. There's an extra mechanism on the armor, but you can't figure " +
                "out what the trigger is.\n\nIt never passed testing phase for a reason.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.OnUsedPlayerItem += OnPlayerItemUsed;
        }
        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.OnUsedPlayerItem -= OnPlayerItemUsed;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.SPEED_BLITZ) && !SPEEDBLITZActivated)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, SPEEDBLITZMovementSpeedStat, StatModifier.ModifyMethod.ADDITIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    SPEEDBLITZActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.SPEED_BLITZ) && SPEEDBLITZActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    SPEEDBLITZActivated = false;
                }
                //Plugin.Log($"{Owner.HasSynergy(Synergy.FILLER_UP)}, {FILLERUPActivated}, {Owner.CurrentGun.PickupObjectId == ((int)Items.Gungine)}");
                if (Owner.HasSynergy(Synergy.FILLER_UP) && !FILLERUPActivated && Owner.CurrentGun.PickupObjectId == (int)Items.Gungine)
                {
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, FILLERUPRateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    FILLERUPActivated = true;
                }
                else if (Owner.HasSynergy(Synergy.FILLER_UP) && Owner.CurrentGun.PickupObjectId != (int)Items.Gungine)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
                    ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    if (isOverdriveActive)
                    {
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, OverdriveRateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    }
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    FILLERUPActivated = false;
                }
                else if (!Owner.HasSynergy(Synergy.FILLER_UP) && FILLERUPActivated)
                {
                    ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
                    Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);

                    FILLERUPActivated = false;
                }
            }

            base.Update();
        }

        private void OnPlayerItemUsed(PlayerController player, PlayerItem item)
        {
            if (player.HasSynergy(Synergy.THAT_GOOD_SHIT))
            {
                if (item.PickupObjectId == (int)Items.Cigarettes)
                {
                    cigaretteUses++;
                    ItemBuilder.RemovePassiveStatModifier(item, PlayerStats.StatType.Damage);
                    ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, (cigaretteUses * DamageStatPerCigUse), StatModifier.ModifyMethod.MULTIPLICATIVE);
                    player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);
                }
            }
            if (!isOnCooldown) StartCoroutine(ApplyOverdriveBuff(player));
        }

        // upon item use, apply overdrive buff, track cooldown, and track duration
        private System.Collections.IEnumerator ApplyOverdriveBuff(PlayerController player)
        {
            isOverdriveActive = true;
            isOnCooldown = true;

            // applies additional stat buff for overdrive
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, OverdriveRateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, overdriveMovementSpeedStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //player.stats.RecalculateStats(player, false, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f * 0.7f, 128f * 0.7f, 255f * 0.7f));
            }

            AkSoundEngine.PostEvent("experimental_hexplate_passive_triggered_SFX", player.gameObject);
            AkSoundEngine.PostEvent("experimental_hexplate_passive_effect_SFX", player.gameObject);

            yield return new WaitForSeconds(OverdriveDuration);

            // removes all stat buffs for both base item and overdrive effect
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.MovementSpeed);

            // reapplies original base item stat buff
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            if (player.HasSynergy(Synergy.SPEED_BLITZ))
            {
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.MovementSpeed, SPEEDBLITZMovementSpeedStat);
            }

            FILLERUPActivated = false;
            
            //player.stats.RecalculateStats(player, false, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }
            isOverdriveActive = false;

            // waits time to simulate cooldown
            yield return new WaitForSeconds(OverdriveCooldown - OverdriveDuration);
            isOnCooldown = false;

            //tk2dBaseSprite s = this.sprite;
            //GameUIRoot.Instance.RegisterDefaultLabel(s.transform, new Vector3(s.GetBounds().max.x + 0f, s.GetBounds().min.y + 0f, 0f), $"{this.EncounterNameOrDisplayName} ready");
            //yield return new WaitForSeconds(1.5f);
            //GameUIRoot.Instance.DeregisterDefaultLabel(s.transform);
        }
    }
}
