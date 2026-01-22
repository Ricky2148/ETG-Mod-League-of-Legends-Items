using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using LOLItems.custom_class_data;
using Steamworks;
using UnityEngine;

// maybe adjust sound volume

namespace LOLItems.passive_items
{
    internal class KrakenSlayer : PassiveItem
    {
        public static string ItemName = "Kraken Slayer";

        // stats pool for item
        public int bringItDownCount = 0;
        public float bringItDownDamage = 20f;
        private static float bringItDownMissingHealthScale = 0.75f;
        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.25f;
        public string[] sfxList = new string[]
        {
            "kraken_slayer_passive_SFX_1",
            "kraken_slayer_passive_SFX_2",
            "kraken_slayer_passive_SFX_3"
        };

        public bool TOPTIERFISHINGTOOLActivated = false;
        private static float TOPTIERFISHINGTOOLbringItDownDamageInc = 10f;
        public bool ENTANGLEMENTActivated = false;

        private static GameActorSpeedEffect ENTANGLEMENTSlowEffect = new GameActorSpeedEffect
        {
            duration = 1.5f,
            effectIdentifier = "kraken_slayer_entanglement_slow_effect",
            resistanceType = EffectResistanceType.None,
            AppliesOutlineTint = true,
            OutlineTintColor = ExtendedColours.purple,
            SpeedMultiplier = 0.6f,
        };
        public bool MEGALODONSLAYERActivated = false;
        private static float MEGALODONSLAYERbringItDownMissingHealthScaleInc = 1.25f;
        private static float ASAILORSBESTFRIENDbringItDownDamageMultiplier = 2f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/kraken_slayer_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<KrakenSlayer>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "The Ultimate fishing tool";
            string longDesc = "Increase damage and fire rate\nEvery 3rd bullet deals additional damage. This damage increases based on the floor and the target's missing health.\n\n" +
                "A fishing tool passed down through generations. It's said that the original user " +
                "felled a kraken with it and freed the seas from its terror.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE); ;
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE); ;

            item.quality = ItemQuality.S;
            ID = item.PickupObjectId;
        }

        // subscribe to events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.PostProcessBeamTick += OnPostProcessProjectile;
            //player.OnReloadedGun += OnGunReloaded;
            //player.OnNewFloorLoaded += CheckDamageScale;
            //CheckDamageScale();
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.PostProcessBeamTick -= OnPostProcessProjectile;
                //player.OnReloadedGun -= OnGunReloaded;
                bringItDownCount = 0; // Reset the count when the item is dropped
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.TOP_TIER_FISHING_TOOL) && !TOPTIERFISHINGTOOLActivated)
                {
                    bringItDownDamage += TOPTIERFISHINGTOOLbringItDownDamageInc;

                    TOPTIERFISHINGTOOLActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.TOP_TIER_FISHING_TOOL) && TOPTIERFISHINGTOOLActivated)
                {
                    bringItDownDamage = 20f;

                    TOPTIERFISHINGTOOLActivated = false;
                }
                if (Owner.HasSynergy(Synergy.ENTANGLEMENT) && !ENTANGLEMENTActivated)
                {
                    ENTANGLEMENTActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ENTANGLEMENT) && ENTANGLEMENTActivated)
                {
                    ENTANGLEMENTActivated = false;
                }
                if (Owner.HasSynergy(Synergy.MEGALODON_SLAYER) && !MEGALODONSLAYERActivated)
                {
                    bringItDownMissingHealthScale += MEGALODONSLAYERbringItDownMissingHealthScaleInc;

                    MEGALODONSLAYERActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.MEGALODON_SLAYER) && MEGALODONSLAYERActivated)
                {
                    bringItDownMissingHealthScale = 0.75f;

                    MEGALODONSLAYERActivated = false;
                }
            }

            base.Update();
        }

        // returns a float value representing the damage scale for the current floor
        /*public float GetFloorDamageScale()
        {
            string currentFloor = GameManager.Instance.GetLastLoadedLevelDefinition().dungeonSceneName;

            switch (currentFloor)
            {
                case "tt_castle": return 1.0f;
                case "tt_sewer": return 1.25f;
                case "tt5": return 1.25f;
                case "tt_cathedral": return 1.5f;
                case "tt_mines": return 1.5f;
                case "ss_resourcefulrat": return 1.75f;
                case "tt_catacombs": return 1.75f;
                case "tt_nakatomi": return 2.0f;
                case "tt_forge": return 2.0f;
                case "tt_bullethell": return 2.25f;
                default: return 0f; // safety fallback
            }
        }*/

        private void OnPostProcessProjectile(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickrate)
        {
            float randomVal = UnityEngine.Random.value;
            if (randomVal <= 0.04f)
            {
                bringItDownCount++;
                //Plugin.Log($"randomVal: {randomVal}, bringitdowncount: {bringItDownCount}");
            }

            if (bringItDownCount >= 3)
            {
                /*if (beam.sprite != null)
                {
                    beam.sprite.color = Color.Lerp(beam.sprite.color, Color.cyan, 0.7f);
                }*/
                //HelpfulMethods.PlayRandomSFX(beam.gameObject, sfxList);
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
                    // scales damage based on enemy's missing health percentage
                    float percentDamageIncrease = bringItDownMissingHealthScale * (1.0f - firstEnemy.healthHaver.GetCurrentHealthPercentage());
                    // scale damage down by tickrate
                    float damageToDeal = bringItDownDamage * (1.0f + percentDamageIncrease) * HelpfulMethods.GetFloorDamageScale();
                    // damage is 1/4 against bosses and sub-bosses
                    /*if (firstEnemy.healthHaver.IsBoss || firstEnemy.healthHaver.IsSubboss)
                    {
                        damageToDeal *= 0.25f;
                    }*/
                    if (Owner.HasSynergy(Synergy.A_SAILORS_BEST_FRIEND))
                    {
                        foreach (PlayerItem item in Owner.activeItems)
                        {
                            if (item.PickupObjectId == (int)Items.DoubleVision && item != null && item.IsCurrentlyActive)
                            {
                                damageToDeal *= ASAILORSBESTFRIENDbringItDownDamageMultiplier;
                            }
                        }
                    }
                    if (ENTANGLEMENTActivated)
                    {
                        firstEnemy.ApplyEffect(ENTANGLEMENTSlowEffect);
                    }
                    // calculates additional extra damage to apply to enemy
                    firstEnemy.healthHaver.ApplyDamage(
                        damageToDeal,
                        Vector2.zero,
                        "kraken_slayer_bring_it_down_damage",
                        CoreDamageTypes.None,
                        DamageCategory.Normal,
                        false
                    );
                    //Plugin.Log($"damage dealt: {damageToDeal}");
                }
                bringItDownCount = 0;
            }
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                bringItDownCount++;
                if (bringItDownCount >= 3)
                {
                    if (proj.sprite != null)
                    {
                        proj.sprite.color = Color.Lerp(proj.sprite.color, Color.cyan, 0.7f);
                    }
                    HelpfulMethods.PlayRandomSFX(proj.gameObject, sfxList);
                    proj.OnHitEnemy += (projHit, enemy, fatal) =>
                    {
                        if (enemy == null) return;
                        //if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                        AIActor firstEnemy = null;
                        if (enemy.aiActor != null)
                        {
                            firstEnemy = enemy.aiActor;
                        }
                        else if (enemy.GetComponentInParent<AIActor>() != null)
                        {
                            firstEnemy = enemy.GetComponentInParent<AIActor>();
                        }
                        else
                        {
                            return;
                        }
                        if (enemy.healthHaver != null)
                        {
                            // scales damage based on enemy's missing health percentage
                            float percentDamageIncrease = bringItDownMissingHealthScale * (1.0f - enemy.healthHaver.GetCurrentHealthPercentage());
                            // GetFloorPriceMod works instead of explicitly stating it like GetFloorDamageScale
                            float damageToDeal = bringItDownDamage * (1.0f + percentDamageIncrease) * HelpfulMethods.GetFloorPriceMod();
                            // damage is 1/4 against bosses and sub-bosses
                            /*if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                            {
                                damageToDeal *= 0.25f;
                            }*/
                            if (Owner.HasSynergy(Synergy.A_SAILORS_BEST_FRIEND))
                            {
                                foreach (PlayerItem item in Owner.activeItems)
                                {
                                    if (item.PickupObjectId == (int)Items.DoubleVision && item != null && item.IsCurrentlyActive)
                                    {
                                        damageToDeal *= ASAILORSBESTFRIENDbringItDownDamageMultiplier;
                                    }
                                }
                            }
                            if (ENTANGLEMENTActivated)
                            {
                                firstEnemy.ApplyEffect(ENTANGLEMENTSlowEffect);
                            }

                            // calculates additional extra damage to apply to enemy
                            enemy.healthHaver.ApplyDamage(
                                damageToDeal,
                                Vector2.zero,
                                "kraken_slayer_bring_it_down_damage",
                                CoreDamageTypes.None,
                                DamageCategory.Normal,
                                false
                            );
                        }
                    };
                    bringItDownCount = 0;
                }
            }
        }

        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            bringItDownCount = 0; // Reset the count when the gun is reloaded
        }

        /*private void PlayRandomSFX(Projectile proj, string[] sfxList)
        {
            var rand = new System.Random();
            int sfxIndex = rand.Next(sfxList.Length);
            string sfxName = sfxList[sfxIndex];
            AkSoundEngine.PostEvent(sfxName, proj.gameObject);
        }*/
    }
}
