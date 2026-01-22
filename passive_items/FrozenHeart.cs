using Alexandria;
using Alexandria.ItemAPI;
using Dungeonator;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// range around player that decreases enemy fire rate
// not complete

namespace LOLItems.passive_items
{
    internal class FrozenHeart : AuraItem
    {
        public static string ItemName = "Frozen Heart LOLItems";

        // stats pool for item
        private static int ArmorStat = 1;

        private static float WintersCaressCrippleRatio = 0.7f;
        private static float WintersCaressRange = 8f;

        public bool ICETOTHECOREActivated = false;
        public bool FROZENBULLETSActivated = false;

        public static int ID;

        private static GameActorCrippleEffect WintersCaressCrippleEffect = new GameActorCrippleEffect
        {
            duration = 1f,
            effectIdentifier = "frozen_heart_cripple_effect",
            resistanceType = EffectResistanceType.Freeze,
            //AppliesOutlineTint = true,
            //OutlineTintColor = ExtendedColours.skyblue,
            AppliesTint = true,
            TintColor = ExtendedColours.skyblue,
            CrippleAmount = WintersCaressCrippleRatio
        };

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/frozen_heart_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<FrozenHeart>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Warning: Do not thaw!";
            string longDesc = "+1 Armor\nDecreases nearby enemies' fire rate.\n\n" +
                "Emits a chilling air that causes those nearby to have cold hands. It might just be " +
                "a disguised AC unit. They keep complaining that they would've killed you if it weren't for their " +
                "cold hands.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            item.ArmorToGainOnInitialPickup = ArmorStat;
            item.AuraRadius = WintersCaressRange;
            item.DamagePerSecond = 0f;

            item.quality = PickupObject.ItemQuality.B;

            item.SetName("Frozen Heart");
            ID = item.PickupObjectId;

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:frozen_heart_lolitems"
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "frost_bullets",
                "snowballets",
                "heart_of_ice"
            };
            CustomSynergies.Add("Ice to the core", mandatoryConsoleIDs, optionalConsoleIDs, true);

            List<string> mandatoryConsoleIDs2 = new List<string>
            {
                "LOLItems:frozen_heart_lolitems"
            };
            List<string> optionalConsoleIDs2 = new List<string>
            {
                "cold_45",
                "frost_giant",
                "freeze_ray",
                "glacier",
                "snowballer"
            };
            CustomSynergies.Add("Frozen Bullets", mandatoryConsoleIDs2, optionalConsoleIDs2, true);*/
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            /*if (player != null)
            {
                if (player.PlayerHasActiveSynergy("bullets apply cripple"))
                {
                    player.PostProcessProjectile += OnPostProcessProjectile;
                    secondSynergyActivated = true;
                }
            }*/

            //firstSynergyActivated = false;
            //secondSynergyActivated = false;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            /*if (player != null)
            {
                if (secondSynergyActivated)
                {
                    player.PostProcessProjectile -= OnPostProcessProjectile;
                }
            }*/

            //firstSynergyActivated = false;
            //secondSynergyActivated = false;
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.ICE_TO_THE_CORE) && !ICETOTHECOREActivated)
                {
                    WintersCaressCrippleEffect.CrippleAmount = 1f - ((1f - WintersCaressCrippleRatio) * 1.5f);
                    //Plugin.Log($"{WintersCaressCrippleEffect.CrippleAmount}");

                    ICETOTHECOREActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.ICE_TO_THE_CORE) && ICETOTHECOREActivated)
                {
                    WintersCaressCrippleEffect.CrippleAmount = WintersCaressCrippleRatio;
                    //Plugin.Log($"{WintersCaressCrippleEffect.CrippleAmount}");

                    ICETOTHECOREActivated = false;
                }

                if (Owner.HasSynergy(Synergy.FROZEN_BULLETS) && !FROZENBULLETSActivated)
                {
                    Owner.PostProcessProjectile += OnPostProcessProjectile;
                    //Plugin.Log($"postprocessproj on");

                    FROZENBULLETSActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.FROZEN_BULLETS) && FROZENBULLETSActivated)
                {
                    Owner.PostProcessProjectile -= OnPostProcessProjectile;
                    //Plugin.Log($"postprocessproj off");

                    FROZENBULLETSActivated = false;
                }
            }

            base.Update();
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
                    //WintersCaressCrippleEffect.CrippleAmount = WintersCaressCrippleRatio;
                    //WintersCaressCrippleEffect.CrippleDuration = 0.5f;
                    /*if (Owner.PlayerHasActiveSynergy("cripple amount increased") && !firstSynergyActivated)
                    {
                        WintersCaressCrippleEffect.CrippleAmount = 1f - ((1f - WintersCaressCrippleRatio) * 2f);
                        Plugin.Log($"{WintersCaressCrippleEffect.CrippleAmount}");

                        firstSynergyActivated = true;
                    }
                    else if (!Owner.PlayerHasActiveSynergy("cripple amount increased") && firstSynergyActivated)
                    {
                        WintersCaressCrippleEffect.CrippleAmount = WintersCaressCrippleRatio;
                        Plugin.Log($"{WintersCaressCrippleEffect.CrippleAmount}");

                        firstSynergyActivated = false;
                    }*/
                    actor.ApplyEffect(WintersCaressCrippleEffect);
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

        private void OnPostProcessProjectile (Projectile proj, float f)
        {
            //if (!Owner.PlayerHasActiveSynergy("bullets apply cripple")) return;
            if (proj.Shooter == proj.Owner.specRigidbody)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy == null) return;
                    if (enemy.aiActor != null)
                    {
                        enemy.aiActor.ApplyEffect(WintersCaressCrippleEffect);
                    }
                    else if (enemy.GetComponentInParent<AIActor>() != null)
                    {
                        enemy.GetComponentInParent<AIActor>().ApplyEffect(WintersCaressCrippleEffect);
                    }
                };
            }
        }
    }
}