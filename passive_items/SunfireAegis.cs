using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

//health, armor, burn aura around player that deals set dmg per second to enemies in radius, scales with max health of player

namespace LOLItems.passive_items
{
    internal class SunfireAegis : AuraItem
    {
        public static string ItemName = "Sunfire Aegis";

        // stats pool for item
        private static float HealthStat = 1f;
        private static int ArmorStat = 1;

        private static float ImmolateBaseDamage = 0f;
        private static float ImmolateDamagePerHeart = 1.5f;
        private static float ImmolateBaseRadius = 3f;
        private static float ImmolateRadiusPerHeart = 0.5f;

        private static List<string> VFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_001",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_002",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_003",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_004",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_005",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_006",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_007",
                "LOLItems/Resources/vfxs/sunfireaura/sunfireaura_008"
            };

        private static GameObject EffectVFX;

        private GameObject activeVFXObject;

        public bool TRUESUNGODActivated = false;
        private static float TRUESUNGODImmolateDamagePerHeartInc = 0.5f;
        private static float TRUESUNGODImmolateRadiusPerHeartInc = 0.25f;

        public bool VOLTAGENOVAActivated = false;
        private static float VOLTAGENOVAImmolateDamagePerHeartInc = 1.5f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/sunfire_aegis_pixelart_sprite_small";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<SunfireAegis>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Radiates Heat";
            string longDesc = "+1 Heart and Armor\nGain a damaging aura around you. Size and damage increases based on your max hearts.\n\n" +
                "The golden armor glows with a warmth not unlike the sun. Appears to have been blessed " +
                "by the gods to burn the wicked around it.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);
            item.ArmorToGainOnInitialPickup = ArmorStat;

            // sets damage aura stats
            item.AuraRadius = ImmolateBaseRadius;
            item.DamagePerSecond = ImmolateBaseDamage;

            EffectVFX = VFXBuilder.CreateVFX
            (
                "sunfireaura",
                VFXSpritePath,
                8,
                new IntVector2(0, 0),
                tk2dBaseSprite.Anchor.MiddleCenter,
                false,
                0,
                -1,
                Color.cyan,
                tk2dSpriteAnimationClip.WrapMode.Loop,
                true
            );

            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.OnNewFloorLoaded += OnLoadedNewFloor;

            //Plugin.Log($"health: {player.healthHaver.GetMaxHealth()}");
            //presets the dmg to default health values
            base.AuraRadius = ImmolateBaseRadius + 3 * ImmolateRadiusPerHeart;
            base.DamagePerSecond = ImmolateBaseDamage + 3 * ImmolateDamagePerHeart;

            // called when player has health, skips if player has no health (uses only armor like robot)
            if (!player.ForceZeroHealthState)
            {
                player.healthHaver.OnHealthChanged += UpdateImmolateStats;
                UpdateImmolateStats(0f, player.healthHaver.GetMaxHealth());
            }

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            activeVFXObject = player.PlayEffectOnActor(EffectVFX, new Vector3(21 / 16f, 10 / 16f, -2f), true, false, false);
            var sprite = activeVFXObject.GetComponent<tk2dSprite>();
            
            if (sprite != null)
            {
                sprite.HeightOffGround = -50f;

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                //Material mat = sprite.renderer.material;

                //mat.shader = ShaderCache.Acquire("Brave/Internal/SimpleSpriteMask");

                //mat.SetFloat("_EmissivePower", 10f);

                //sprite.ForceUpdateMaterial();
                //sprite.UpdateMaterial();

                //Shader vfxShader = sprite.renderer.material.shader;
                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.25f);
            }
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.OnNewFloorLoaded -= OnLoadedNewFloor;
                player.healthHaver.OnHealthChanged -= UpdateImmolateStats;
            }

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.TRUE_SUN_GOD) && !TRUESUNGODActivated)
                {
                    ImmolateDamagePerHeart += TRUESUNGODImmolateDamagePerHeartInc;
                    ImmolateRadiusPerHeart += TRUESUNGODImmolateRadiusPerHeartInc;

                    TRUESUNGODActivated = true;
                    if (Owner.ForceZeroHealthState)
                    {
                        UpdateImmolateStats(0, 3f);
                    }
                    else
                    {
                        UpdateImmolateStats(0, Owner.healthHaver.GetMaxHealth());
                    }
                }
                else if (!Owner.HasSynergy(Synergy.TRUE_SUN_GOD) && TRUESUNGODActivated)
                {
                    ImmolateDamagePerHeart -= TRUESUNGODImmolateDamagePerHeartInc;
                    ImmolateRadiusPerHeart -= TRUESUNGODImmolateRadiusPerHeartInc;

                    TRUESUNGODActivated = false;
                    if (Owner.ForceZeroHealthState)
                    {
                        UpdateImmolateStats(0, 3f);
                    }
                    else
                    {
                        UpdateImmolateStats(0, Owner.healthHaver.GetMaxHealth());
                    }
                }

                if (Owner.HasSynergy(Synergy.VOLTAGE_NOVA) && !VOLTAGENOVAActivated)
                {
                    ImmolateDamagePerHeart += VOLTAGENOVAImmolateDamagePerHeartInc;

                    VOLTAGENOVAActivated = true;
                    if (Owner.ForceZeroHealthState)
                    {
                        UpdateImmolateStats(0, 3f);
                    }
                    else
                    {
                        UpdateImmolateStats(0, Owner.healthHaver.GetMaxHealth());
                    }
                }
                else if (!Owner.HasSynergy(Synergy.VOLTAGE_NOVA) && VOLTAGENOVAActivated)
                {
                    ImmolateDamagePerHeart -= VOLTAGENOVAImmolateDamagePerHeartInc;

                    VOLTAGENOVAActivated = false;
                    if (Owner.ForceZeroHealthState)
                    {
                        UpdateImmolateStats(0, 3f);
                    }
                    else
                    {
                        UpdateImmolateStats(0, Owner.healthHaver.GetMaxHealth());
                    }
                }
            }

            base.Update();
        }

        // updates the immolate stats based on the player's current health
        private void UpdateImmolateStats(float oldHealth, float newHealth)
        {
            //Plugin.Log($"updated immolate stats: {newHealth}");
            this.DamagePerSecond = (newHealth) * ImmolateDamagePerHeart;
            this.AuraRadius = ImmolateBaseRadius + (newHealth) * ImmolateRadiusPerHeart;
            this.Update();
        }

        private void OnLoadedNewFloor(PlayerController player)
        {
            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            activeVFXObject = player.PlayEffectOnActor(EffectVFX, new Vector3(21 / 16f, 10 / 16f, -2f), true, false, false);
            var sprite = activeVFXObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = -50f;

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                //Material mat = sprite.renderer.material;

                //mat.shader = ShaderCache.Acquire("Brave/Internal/SimpleSpriteMask");

                //mat.SetFloat("_EmissivePower", 10f);

                //sprite.ForceUpdateMaterial();
                //sprite.UpdateMaterial();

                //Shader vfxShader = sprite.renderer.material.shader;
                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.25f);
            }
        }
    }
}
