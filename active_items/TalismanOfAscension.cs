using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using HutongGames.Utility;
using LOLItems.custom_class_data;
using LOLItems.guon_stones;
using LOLItems.passive_items;
using LOLItems.weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LOLItems.active_items
{
    internal class TalismanOfAscension : PlayerItem
    {
        public static string ItemName = "Talisman of Ascension";

        public static int ID;
        public float duration = 5f;

        public List<PlayerStats.StatType> StatTypeList = new List<PlayerStats.StatType>(
            (PlayerStats.StatType[])Enum.GetValues(typeof(PlayerStats.StatType))
            );

        public int timesUsed = 0;
        private static int baseNumBuffs = 2;
        private static float numBuffsIncPerStack = 0.25f;
        private static float baseBuffValueMin = 0.2f;
        private static float buffValueMinIncPerStack = 0.1f;
        private static float baseBuffValueMax = 1.0f;
        private static float buffValueMaxIncPerStack = 0.25f;

        public Dictionary<PlayerStats.StatType, float> StatTypeWeights = new Dictionary<PlayerStats.StatType, float>();

        private static List<string> effectSFXList = new List<string>
        {
            "chrTrg_gainHp_SFX",
            "chrTrg_gainMp_SFX",
        };

        //public System.Array StatTypeList = Enum.GetValues(typeof(PlayerStats.StatType));

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/talisman_of_ascension_pixelart_sprite_sparks";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<TalismanOfAscension>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*aw dang it*";
            string longDesc = "Each use rerolls this item's stat buffs, randomizing which stats are buffed and how much they're buffed. Strength and num of buffs increase with each use.\n\n" +
                "This ancient talisman is said to have been bathed in the intense heat of the shuriman desert atop its highest peak. " +
                "The gods answered this showing of reverence by imbuing it with a spirit of fortune. It then became presitigous as every emperor who bore this talisman would " +
                "exhibit unreasonable feats of luck. However, after millennia of constant use, the talisman's effects have dwindled and have become unreliable.\n\n" +
                "https://www.youtube.com/watch\n?v=IPFiKEm-oNI\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            //ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1f);
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.PerRoom, 5);
            item.consumable = false;

            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.A;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula);
            ID = item.PickupObjectId;

            item.timesUsed = 0;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1, StatModifier.ModifyMethod.ADDITIVE);

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.StatTypeList.Remove(PlayerStats.StatType.Health);
            item.StatTypeList.Remove(PlayerStats.StatType.Coolness);
            item.StatTypeList.Remove(PlayerStats.StatType.AdditionalGunCapacity);
            item.StatTypeList.Remove(PlayerStats.StatType.AdditionalItemCapacity);
            item.StatTypeList.Remove(PlayerStats.StatType.GlobalPriceMultiplier);
            item.StatTypeList.Remove(PlayerStats.StatType.Curse);
            item.StatTypeList.Remove(PlayerStats.StatType.AdditionalBlanksPerFloor);
            item.StatTypeList.Remove(PlayerStats.StatType.ShadowBulletChance);
            item.StatTypeList.Remove(PlayerStats.StatType.ThrownGunDamage);
            item.StatTypeList.Remove(PlayerStats.StatType.DodgeRollDamage);
            item.StatTypeList.Remove(PlayerStats.StatType.EnemyProjectileSpeedMultiplier);
            item.StatTypeList.Remove(PlayerStats.StatType.ExtremeShadowBulletChance);
            //item.StatTypeList.Remove(PlayerStats.StatType.RangeMultiplier);
            item.StatTypeList.Remove(PlayerStats.StatType.DodgeRollDistanceMultiplier);
            item.StatTypeList.Remove(PlayerStats.StatType.DodgeRollSpeedMultiplier);
            item.StatTypeList.Remove(PlayerStats.StatType.TarnisherClipCapacityMultiplier);
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            /*foreach (PlayerStats.StatType a in StatTypeList)
            {
                Plugin.Log($"{a}");
            }*/

            /*
             * 0. MovementSpeed
             * 1. RateOfFire
             * 2. Accuracy
             * 3. Damage
             * 4. ProjectileSpeed
             * 5. AmmoCapacityMultiplier
             * 6. ReloadSpeed
             * 7. AdditionalShotPiercing
             * 8. KnockbackMultiplier
             * 9. PlayerBulletScale
             * 10. AdditionalClipCapacityMultiplier
             * 11. AdditionalShotBounces
             * 12. DamageToBosses
             * 13. ChargeAmountMultiplier
             * 14. RangeMultiplier
             * 15. MoneyMultiplierFromEnemies
             */

            //assuming placeholder stat increase value spread of 1.5 - 5.0
            //formula: ([stat increase value] * [stat increase weight]) + (1.0f [to ensure a baseline stat increase and no decreases])
            StatTypeWeights.Clear();
            StatTypeWeights.Add(StatTypeList[0], 1.0f);  //MovementSpeed (special cased to be additive NOT multiplicative)
            StatTypeWeights.Add(StatTypeList[1], 0.3f);  //RateOfFire
            StatTypeWeights.Add(StatTypeList[2], 0.2f);  //Accuracy (special cased to be inverse)
            StatTypeWeights.Add(StatTypeList[3], 0.3f);  //Damage
            StatTypeWeights.Add(StatTypeList[4], 0.4f);  //ProjectileSpeed
            StatTypeWeights.Add(StatTypeList[5], 0.2f);  //AmmoCapacityMultiplier
            StatTypeWeights.Add(StatTypeList[6], 1.0f);  //ReloadSpeed (special cased to be inverse?)
            StatTypeWeights.Add(StatTypeList[7], 1.0f);  //AdditionalShotPiercing (special cased to be additive NOT multiplicative)
            StatTypeWeights.Add(StatTypeList[8], 0.5f);  //KnockbackMultiplier
            StatTypeWeights.Add(StatTypeList[9], 0.4f);  //PlayerBulletScale
            StatTypeWeights.Add(StatTypeList[10], 0.4f); //AdditionalClipCapacityMultiplier
            StatTypeWeights.Add(StatTypeList[11], 1.0f); //AddtionalShotBounces (special cased to be additive NOT multiplicative)
            StatTypeWeights.Add(StatTypeList[12], 0.6f); //DamageToBosses
            StatTypeWeights.Add(StatTypeList[13], 1.0f); //ChargeAmountMultiplier
            StatTypeWeights.Add(StatTypeList[14], 0.3f); //RangeMultiplier
            StatTypeWeights.Add(StatTypeList[15], 0.2f); //MoneyMultiplierFromEnemies

            /*foreach (KeyValuePair<PlayerStats.StatType, float> b in StatTypeWeights)
            {
                Plugin.Log($"{b.Key} | {b.Value}");
            }*/
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void DoEffect(PlayerController user)
        {
            //AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);

            HelpfulMethods.PlayRandomSFX(base.gameObject, effectSFXList);

            /*Vector2 vector = user.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
            Vector2 vector2 = user.specRigidbody.HitboxPixelCollider.UnitTopRight;
            PixelCollider pixelCollider = user.specRigidbody.GetPixelCollider(ColliderType.Ground);
            if (pixelCollider != null && pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Manual)
            {
                vector = Vector2.Min(vector, pixelCollider.UnitBottomLeft);
                vector2 = Vector2.Max(vector2, pixelCollider.UnitTopRight);
            }
            Vector2 playerDimensions = (user.specRigidbody.HitboxPixelCollider.UnitDimensions) / 2f;

            vector += Vector2.Min(playerDimensions * 0.15f, new Vector2(0.25f, 0.25f));
            vector2 -= Vector2.Min(playerDimensions * 0.15f, new Vector2(0.25f, 0.25f));
            vector2.y -= Mathf.Min(playerDimensions.y * 0.1f, 0.1f);

            GlobalSparksDoer.DoRadialParticleBurst((timesUsed + 5), vector, vector2, 15f, 5f, 5f, 0.3f, 1f, ExtendedColours.vibrantOrange, GlobalSparksDoer.SparksType.SOLID_SPARKLES);*/

            //int numOfBuffs = (int)Math.Ceiling(UnityEngine.Random.value * 5);
            int numOfBuffs = baseNumBuffs + Mathf.FloorToInt(numBuffsIncPerStack * timesUsed);
            //Plugin.Log($"{numOfBuffs} = {baseNumBuffs} + ({numBuffsIncPerStack} * {timesUsed})");
            //List<PlayerStats.StatType> listOfBuffs = new List<PlayerStats.StatType>();

            List<int> list1 = new List<int>();

            for (int a = 0; a < numOfBuffs; a++)
            {
                //Enum.GetValues(typeof(PlayerStats.StatType))

                list1.Add(UnityEngine.Random.Range(0, StatTypeList.Count));
                //list1.Add(a);
                //Plugin.Log($"{list1[a]}");
            }

            //int i = 0,j = 0;
            foreach (PlayerStats.StatType statType in StatTypeList)
            {
                ItemBuilder.RemovePassiveStatModifier(this, statType);

                //Plugin.Log($"i: {i}, j: {j}");
                
                /*if (list1[j].Equals(i))
                {
                    //ItemBuilder.AddPassiveStatModifier(this, statType, 10, StatModifier.ModifyMethod.MULTIPLICATIVE);

                    Plugin.Log($"{statType}");

                    j++;
                }*/

                //i++;
            }

            user.stats.RecalculateStatsWithoutRebuildingGunVolleys(user);

            List<string> itemDescriptionAppendix = new List<string>();

            foreach (int a in list1)
            {
                //float statIncValue = 0.5f * timesUsed;
                float buffValueMin = baseBuffValueMin + (buffValueMinIncPerStack * timesUsed);
                float buffValueMax = baseBuffValueMax + (buffValueMaxIncPerStack * timesUsed);
                //float statIncValue = UnityEngine.Random.Range((baseBuffValueMin + (buffValueMinIncPerStack * timesUsed)), (baseBuffValueMax + (buffValueMaxIncPerStack * timesUsed)));
                float statIncValue = UnityEngine.Random.Range(buffValueMin, buffValueMax);

                //Plugin.Log($"{buffValueMin}, {buffValueMax} | {statIncValue}");
                //special cased stattypes to be additive and not include the +1f
                if (StatTypeList[a] == PlayerStats.StatType.MovementSpeed | StatTypeList[a] == PlayerStats.StatType.AdditionalShotPiercing | StatTypeList[a] == PlayerStats.StatType.AdditionalShotBounces)
                {
                    //Plugin.Log("additive");

                    if (StatTypeList[a] == PlayerStats.StatType.AdditionalShotPiercing | StatTypeList[a] == PlayerStats.StatType.AdditionalShotBounces)
                    {
                        statIncValue = Math.Max(1.0f, (statIncValue * StatTypeWeights[StatTypeList[a]]));
                    }
                    else
                    {
                        statIncValue = statIncValue * StatTypeWeights[StatTypeList[a]];
                    }
                    //Plugin.Log($"statToMod: {StatTypeList[a]}, statIncValue: {statIncValue}, statWeightValue: {StatTypeWeights[StatTypeList[a]]}");
                    
                    //itemDescriptionAppendix += $"{StatTypeList[a]}: +{statIncValue}\n";
                    itemDescriptionAppendix.Add($"{StatTypeList[a]}: +{(Mathf.Round(statIncValue * 100f)) / 100f}\n");
                    ItemBuilder.AddPassiveStatModifier(this, StatTypeList[a], statIncValue, StatModifier.ModifyMethod.ADDITIVE);
                }
                //special cased stattypes to be inversely scaling
                else if (StatTypeList[a] == PlayerStats.StatType.ReloadSpeed | StatTypeList[a] == PlayerStats.StatType.Accuracy)
                {
                    //Plugin.Log("inversely multiplicative");
                    statIncValue = 1.0f / (1.0f + (statIncValue * StatTypeWeights[StatTypeList[a]]));
                    //Plugin.Log($"statToMod: {StatTypeList[a]}, statIncValue: {statIncValue}, statWeightValue: {StatTypeWeights[StatTypeList[a]]}");
                    
                    //itemDescriptionAppendix += $"{StatTypeList[a]}: *{statIncValue}\n";
                    itemDescriptionAppendix.Add($"{StatTypeList[a]}: x{(Mathf.Round(statIncValue * 100f)) / 100f}\n");
                    ItemBuilder.AddPassiveStatModifier(this, StatTypeList[a], statIncValue, StatModifier.ModifyMethod.MULTIPLICATIVE);
                }
                //assumed normal interaction with mulitplicative stat increase with a +1f to ensure net positive effect
                else
                {
                    //Plugin.Log("regular multiplicative");
                    statIncValue = 1.0f + (statIncValue * StatTypeWeights[StatTypeList[a]]);
                    //Plugin.Log($"statToMod: {StatTypeList[a]}, statIncValue: {statIncValue}, statWeightValue: {StatTypeWeights[StatTypeList[a]]}");
                    
                    //itemDescriptionAppendix += $"{StatTypeList[a]}: *{statIncValue}\n";
                    itemDescriptionAppendix.Add($"{StatTypeList[a]}: x{(Mathf.Round(statIncValue * 100f)) / 100f}\n");
                    ItemBuilder.AddPassiveStatModifier(this, StatTypeList[a], statIncValue, StatModifier.ModifyMethod.MULTIPLICATIVE);
                }

                //Plugin.Log($"{StatTypeList[a]}");
            }

            user.stats.RecalculateStatsWithoutRebuildingGunVolleys(user);

            timesUsed++;

            StartCoroutine(DisplayStats(user, itemDescriptionAppendix));
            //this.encounterTrackable.journalData.m_cachedNotificationPanelDescription = "some bullshit";
            //this.encounterTrackable.m_journalData.m_cachedNotificationPanelDescription = "some other bullshit";
            //this.encounterTrackable.journalData.ClearCache();
            //this.encounterTrackable.m_journalData.ClearCache();
            //this.SetName(timesUsed.ToStringSafe());
            //this.SetShortDescription(timesUsed.ToStringSafe());
            //this.SetLongDescription(itemDescriptionAppendix);
            //Plugin.Log(itemDescriptionAppendix);
        }

        private IEnumerator DisplayStats(PlayerController player, List<string> displayText)
        {
            foreach (string s in displayText)
            {
                GameUIRoot.Instance.RegisterDefaultLabel(player.sprite.transform, new Vector3(player.sprite.GetBounds().max.x + 0f, player.sprite.GetBounds().min.y + 0f, 0f), s);
                yield return new WaitForSeconds(5f);
                GameUIRoot.Instance.DeregisterDefaultLabel(player.sprite.transform);
            }
        }
    }
}
