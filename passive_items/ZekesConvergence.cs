using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

// look into making the aura tint not apply to corpse

namespace LOLItems.passive_items
{
    internal class ZekesConvergence : AuraItem
    {
        public static string ItemName = "Zeke's Convergence";

        // stats pool for item
        private static float HealthStat = 1f;
        //private static int ArmorStat = 1;

        private static float FrostFireTempestDamage = 15f;
        private static float FrostFireTempestRadius = 6f;
        private bool isFrostFireTempestActive = false;

        private static float FrostFireTempestDuration = 10f;
        private static float FrostFireTempestCooldown = 45f; //45f
        private static float FrostFireTempestSlowPercent = 0.5f;

        /*
        private static GameObject EffectVFX = ((Gun)PickupObjectDatabase.GetById(596))
            .DefaultModule.projectiles[0]
            .hitEffects.tileMapHorizontal.effects[0]
            .effects[0].effect;
        */

        private static List<string> VFXSpritePath = new List<string>
            {
                "LOLItems/Resources/vfxs/convergence/convergenceaura_001",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_002",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_003",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_004",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_005",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_006",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_007",
                "LOLItems/Resources/vfxs/convergence/convergenceaura_008"
            };

        private static GameObject EffectVFX;

        private GameObject activeVFXObject;

        private Coroutine FrostFireTempestCoroutine;

        /*
        private static Color SlowEffectColor = new Color
        (
            Mathf.Lerp(0f, ExtendedColours.skyblue.r, 0.1f),
            Mathf.Lerp(0f, ExtendedColours.skyblue.g, 0.1f),
            Mathf.Lerp(0f, ExtendedColours.skyblue.b, 0.1f)
        );
        */

        private static GameActorSpeedEffect FrostFireTempestSlowEffect = new GameActorSpeedEffect
        {
            duration = 1f,
            SpeedMultiplier = FrostFireTempestSlowPercent,
            effectIdentifier = "frostfire_tempest_slow_effect",
            resistanceType = EffectResistanceType.Freeze,
            AppliesTint = true,
            TintColor = ExtendedColours.dodgerBlue,
            //AppliesDeathTint = true,
            //DeathTintColor = new Color(0, 0, 0)
        };

        public bool ABSOLUTECONVERGENCEActivated = false;
        public bool FLAMEOVERICEActivated = false;
        private static float FLAMEOVERICEFrostFireTempestDamageMultiplier = 2f;
        public bool ICEOVERFLAMEActivated = false;
        private static float ICEOVERFLAMEFrostFireTempestSlowPercent = 0.2f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/zekes_convergence_pixelart_sprite_outline";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ZekesConvergence>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "ICY HOT";
            string longDesc = "+1 Heart\nActivating an item creates an aura around you that deals damage and slows. Goes on a cooldown.\n\n" +
                "Another strange piece of armor that helps protect the user. There appears to be a mechanism to emit an aura of fire and ice, " +
                "but the trigger is nowhere to be found. It makes you feel both hot and cold at the same time and it's really annoying.\n";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, HealthStat, StatModifier.ModifyMethod.ADDITIVE);
            //item.ArmorToGainOnInitialPickup = ArmorStat;

            // sets damage aura stats
            item.AuraRadius = 0f;
            item.DamagePerSecond = 0f;
            //item.AuraVFX = EffectVFX;

            EffectVFX = VFXBuilder.CreateVFX
            (
                "convergenceaura",
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

            player.OnUsedPlayerItem += ActivateFrostFireTempest;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            if (player != null)
            {
                player.OnUsedPlayerItem -= ActivateFrostFireTempest;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.ABSOLUTE_CONVERGENCE) && !ABSOLUTECONVERGENCEActivated)
                {
                    Owner.OnNewFloorLoaded += OnLoadedNewFloor;
                    this.OnPickedUp += OnLoadedNewFloor;
                    Owner.OnUsedPlayerItem -= ActivateFrostFireTempest;

                    if (FrostFireTempestCoroutine != null) StopCoroutine(FrostFireTempestCoroutine);

                    isFrostFireTempestActive = true;

                    this.AuraRadius = FrostFireTempestRadius;
                    this.DamagePerSecond = FrostFireTempestDamage;

                    if (activeVFXObject != null)
                    {
                        Destroy(activeVFXObject);
                    }

                    //vector3(width/2 + 1, length/2 - 10, ????) 
                    activeVFXObject = Owner.PlayEffectOnActor(EffectVFX, new Vector3(51 / 16f, 40 / 16f, -2f), true, false, false);
                    //AkSoundEngine.PostEvent("zekes_convergence_sfx_01", Owner.gameObject);

                    var sprite = activeVFXObject.GetComponent<tk2dSprite>();

                    if (sprite != null)
                    {
                        sprite.HeightOffGround = -50f;

                        sprite.scale = new Vector3(2.5f, 2.5f, 0f);

                        sprite.UpdateZDepth();

                        sprite.usesOverrideMaterial = true;

                        sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                        sprite.renderer.material.SetFloat("_Fade", 0.5f);
                    }

                    ABSOLUTECONVERGENCEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ABSOLUTE_CONVERGENCE) && ABSOLUTECONVERGENCEActivated)
                {
                    Owner.OnNewFloorLoaded -= OnLoadedNewFloor;
                    this.OnPickedUp -= OnLoadedNewFloor;
                    Owner.OnUsedPlayerItem += ActivateFrostFireTempest;

                    this.AuraRadius = 0f;
                    this.DamagePerSecond = 0f;

                    if (activeVFXObject != null)
                    {
                        Destroy(activeVFXObject);
                    }

                    isFrostFireTempestActive = false;

                    ABSOLUTECONVERGENCEActivated = false;
                }

                if (Owner.HasSynergy(Synergy.FLAME_OVER_ICE) && !FLAMEOVERICEActivated)
                {
                    FrostFireTempestDamage *= FLAMEOVERICEFrostFireTempestDamageMultiplier;
                    if (isFrostFireTempestActive)
                    {
                        this.DamagePerSecond = FrostFireTempestDamage;
                    }

                    FLAMEOVERICEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.FLAME_OVER_ICE) && FLAMEOVERICEActivated)
                {
                    FrostFireTempestDamage /= FLAMEOVERICEFrostFireTempestDamageMultiplier;
                    if (isFrostFireTempestActive)
                    {
                        this.DamagePerSecond = FrostFireTempestDamage;
                    }

                    FLAMEOVERICEActivated = false;
                }

                if (Owner.HasSynergy(Synergy.ICE_OVER_FLAME) && !ICEOVERFLAMEActivated)
                {
                    FrostFireTempestSlowEffect.SpeedMultiplier = ICEOVERFLAMEFrostFireTempestSlowPercent;

                    ICEOVERFLAMEActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ICE_OVER_FLAME) && ICEOVERFLAMEActivated)
                {
                    FrostFireTempestSlowEffect.SpeedMultiplier = FrostFireTempestSlowPercent;

                    ICEOVERFLAMEActivated = false;
                }
            }

            base.Update();
        }

        // updates the immolate stats based on the player's current health
        private void ActivateFrostFireTempest(PlayerController player, PlayerItem item)
        {
            if (!isFrostFireTempestActive)
            {
                FrostFireTempestCoroutine = StartCoroutine(DoFrostFireTempestEffect(player));
            }
        }

        private System.Collections.IEnumerator DoFrostFireTempestEffect(PlayerController player)
        {
            isFrostFireTempestActive = true;

            this.AuraRadius = FrostFireTempestRadius;
            this.DamagePerSecond = FrostFireTempestDamage;

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            //vector3(width/2 + 1, length/2 - 10, ????) 
            activeVFXObject = player.PlayEffectOnActor(EffectVFX, new Vector3(51 / 16f, 40 / 16f, -2f), true, false, false);
            AkSoundEngine.PostEvent("zekes_convergence_sfx_01", player.gameObject);
            
            var sprite = activeVFXObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = -50f;

                sprite.scale = new Vector3(2.5f, 2.5f, 0f);

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.3f);
            }

            /*if (EffectVFX != null && player != null)
            {
                this.AuraVFX = EffectVFX;
            }*/

            yield return new WaitForSeconds(FrostFireTempestDuration);

            this.AuraRadius = 0f;
            this.DamagePerSecond = 0f;

            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            // something here to disable the vfx

            yield return new WaitForSeconds(FrostFireTempestCooldown - FrostFireTempestDuration);

            isFrostFireTempestActive = false;

            //tk2dBaseSprite s = this.sprite;
            //GameUIRoot.Instance.RegisterDefaultLabel(s.transform, new Vector3(s.GetBounds().max.x + 0f, s.GetBounds().min.y + 0f, 0f), $"{this.EncounterNameOrDisplayName} ready");
            //yield return new WaitForSeconds(1.5f);
            //GameUIRoot.Instance.DeregisterDefaultLabel(s.transform);
        }

        public override void DoAura()
        {
            if (m_extantAuraVFX == null)
            {
            }
            didDamageEnemies = false;
            if (AuraAction == null)
            {
                AuraAction = delegate (AIActor actor, float dist)
                {
                    //Plugin.Log($"dps: {ModifiedDamagePerSecond}");
                    float num = ModifiedDamagePerSecond * BraveTime.DeltaTime;
                    if (DamageFallsOffInRadius)
                    {
                        float t = dist / ModifiedAuraRadius;
                        num = Mathf.Lerp(num, 0f, t);
                    }
                    if (num > 0f)
                    {
                        didDamageEnemies = true;
                    }
                    actor.healthHaver.ApplyDamage(num, Vector2.zero, "Aura", damageTypes);
                    actor.ApplyEffect(FrostFireTempestSlowEffect);
                };
            }
            if (m_owner != null && m_owner.CurrentRoom != null)
            {
                m_owner.CurrentRoom.ApplyActionToNearbyEnemies(m_owner.CenterPosition, ModifiedAuraRadius, AuraAction);
            }
            if (didDamageEnemies)
            {
                m_owner.DidUnstealthyAction();
            }
        }

        private void OnLoadedNewFloor(PlayerController player)
        {
            if (activeVFXObject != null)
            {
                Destroy(activeVFXObject);
            }

            activeVFXObject = player.PlayEffectOnActor(EffectVFX, new Vector3(51 / 16f, 40 / 16f, -2f), true, false, false);
            //AkSoundEngine.PostEvent("zekes_convergence_sfx_01", player.gameObject);

            var sprite = activeVFXObject.GetComponent<tk2dSprite>();

            if (sprite != null)
            {
                sprite.HeightOffGround = -50f;

                sprite.scale = new Vector3(2.5f, 2.5f, 0f);

                sprite.UpdateZDepth();

                sprite.usesOverrideMaterial = true;

                sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
                sprite.renderer.material.SetFloat("_Fade", 0.5f);
            }
        }
    }
}
