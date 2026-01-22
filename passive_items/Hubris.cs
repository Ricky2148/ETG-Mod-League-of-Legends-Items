using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria;
using Alexandria.Misc;
using LOLItems.custom_class_data;
using System.Collections;
using HutongGames.PlayMaker.Actions;

//dmg increase, dmg increase increases per kill, scaling infinitely, but very slowly
//maybe make the damage increase additive instead of multiplicative

namespace LOLItems.passive_items
{
    internal class Hubris : PassiveItem
    {
        public static string ItemName = "Hubris";

        // stats pool for item
        private int eminenceCount = 0;
        private int eminenceCountPerStack = 1;
        private float eminenceDamageIncrease = 0.004f;

        public bool QUADRATICSCALINGActivated = false;
        private static int QUADRATICSCALINGEminenceCountPerStack = 1;
        private static int GLADITORIALCHALLENGEEminenceCountPerStackMultiplier = 3;
        private static float GLADITORIALCHALLENGEDuration = 10f;
        public bool PEACEANDWARActivated = false;
        private static int PEACEANDWARStackCount = 0;
        private static int PEACEANDWARStackReq = 50;
        private static float PEACEANDWARHealAmount = 0.5f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/hubris_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Hubris>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "A symbol of victory";
            string longDesc = "Every kill permanently increases your damage a little bit.\n\n" +
                "A congratulatory laurel wreath gifted to the victor. With each triumph, one's strength increases. " +
                "Legends speak of a statue that once bore this headpiece.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            //player.OnKilledEnemy += KillEnemyCount;
            player.OnAnyEnemyReceivedDamage += KillEnemyCount;
            player.OnUsedPlayerItem += ConfigurumSynergyInteraction;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                //player.OnKilledEnemy -= KillEnemyCount;
                player.OnAnyEnemyReceivedDamage -= KillEnemyCount;
                player.OnUsedPlayerItem -= ConfigurumSynergyInteraction;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.QUADRATIC_SCALING) && !QUADRATICSCALINGActivated)
                {
                    eminenceCountPerStack += QUADRATICSCALINGEminenceCountPerStack;
                    //Plugin.Log($"{eminenceCountPerStack}");

                    QUADRATICSCALINGActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.QUADRATIC_SCALING) && QUADRATICSCALINGActivated)
                {
                    eminenceCountPerStack = 1;
                    //Plugin.Log($"{eminenceCountPerStack}");

                    QUADRATICSCALINGActivated = false;
                }
                /*if (Owner.HasSynergy(Synergy.PEACE_AND_WAR) && !PEACEANDWARActivated)
                {
                    eminenceCountPerStack += QUADRATICSCALINGEminenceCountPerStack;
                    Plugin.Log($"{eminenceCountPerStack}");

                    PEACEANDWARActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.PEACE_AND_WAR) && PEACEANDWARActivated)
                {
                    eminenceCountPerStack = 1;
                    Plugin.Log($"{eminenceCountPerStack}");

                    PEACEANDWARActivated = false;
                }*/
            }

            base.Update();
        }

        // removes current damage modifier, increments damage increase count, and adds new damage modifier
        private void KillEnemyCount(float damage, bool fatal, HealthHaver enemyHealth)
        {
            /*
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            eminenceCount++;
            float damageIncrease = eminenceCount * eminenceDamageIncrease;
            ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1.0f + damageIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
            player.stats.RecalculateStats(player, false, false);
            */

            //float damageIncrease;

            if (enemyHealth.aiActor != null && enemyHealth && fatal)
            {
                if (enemyHealth.aiActor.IsNormalEnemy && (enemyHealth.IsBoss || enemyHealth.IsSubboss))
                {
                    eminenceCount += (eminenceCountPerStack * 5);
                    //Plugin.Log($"is boss: {enemyHealth.IsBoss}, is sub boss: {enemyHealth.IsSubboss}");
                }
                else
                {
                    eminenceCount += eminenceCountPerStack;
                    //Plugin.Log($"is normal enemy");
                }

                if (Owner.HasSynergy(Synergy.PEACE_AND_WAR))
                {
                    PEACEANDWARStackCount++;
                    if (PEACEANDWARStackCount >= PEACEANDWARStackReq)
                    {
                        PEACEANDWARStackCount = 0;
                        Owner.healthHaver.ApplyHealing(PEACEANDWARHealAmount);
                    }
                }

                //Plugin.Log($"is normal enemy: {enemyHealth.aiActor.IsNormalEnemy}");

                ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
                float damageIncrease = eminenceCount * eminenceDamageIncrease;
                //Plugin.Log($"{eminenceCount}, {damageIncrease}");
                ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, 1.0f + damageIncrease, StatModifier.ModifyMethod.MULTIPLICATIVE);
                //Owner.stats.RecalculateStats(Owner, false, false);
                Owner.stats.RecalculateStatsWithoutRebuildingGunVolleys(Owner);
            }
        }

        private void ConfigurumSynergyInteraction(PlayerController player, PlayerItem item)
        {
            if (player != null && item != null)
            {
                if (Owner.HasSynergy(Synergy.GLADITORIAL_CHALLENGE))
                {
                    if (item.PickupObjectId == (int)Items.LamentConfigurum)
                    {
                        StartCoroutine(StartConfigurumDuration(player));
                    }
                }
            }
        }

        private System.Collections.IEnumerator StartConfigurumDuration(PlayerController player)
        {
            eminenceCountPerStack *= GLADITORIALCHALLENGEEminenceCountPerStackMultiplier;
            //Plugin.Log($"start configurum effect {eminenceCountPerStack}");

            yield return new WaitForSeconds(GLADITORIALCHALLENGEDuration);
            eminenceCountPerStack = 1;
            QUADRATICSCALINGActivated = false;
            //Plugin.Log($"end configurum effect {eminenceCountPerStack}");
        }
    }
}
