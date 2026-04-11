using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using LOLItems.custom_class_data;
using LOLItems.guon_stones;
using LOLItems.passive_items;
using LOLItems.weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.active_items
{
    internal class RefillablePotion : EstusFlaskItem
    {
        public static string ItemName = "Refillable Potion";

        private static float POGFDamageStat = 1.15f;
        private static float POGFRateOfFireStat = 1.5f;
        private static float POGFReloadStat = 0.65f;
        private static float POGFKnockbackStat = 5f;

        protected SpeculativeRigidbody userSRB;
        private bool m_usedOverrideMaterial;

        public bool BUNCHOBALLOONSActivated = false;
        private static int BUNCHOBALLOONSUseIncreases = 1;

        public static int ID;

        public float duration = 12f;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/refillable_potion_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<RefillablePotion>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "\"this healed me\"";
            string longDesc = "A potion that heals you.\n\n" +
                "Used to be a commercial product sold with the slogan \"It's so refreshing, you'll want to drink it twice! Thankfully, you can never finish it in one go!\"\n" +
                "The product also sold with a lifetime warranty of infinite refills anytime anywhere, which completely bankrupted the company.\n\n" +
                "Somehow, the refill service is still active but only between floors.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.None, 25);

            item.numDrinksPerFloor = 2;
            item.healingAmount = 0.5f;
            item.drinkDuration = 0f;

            item.healVFX = (PickupObjectDatabase.GetById((int)Items.OldKnightsFlask) as EstusFlaskItem).healVFX;

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1);
            
            item.usableDuringDodgeRoll = true;
            item.quality = PickupObject.ItemQuality.D;
            item.AddToSubShop(ItemBuilder.ShopType.Goopton);
            ID = item.PickupObjectId;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
        }

        public DebrisObject Drop(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            return base.Drop(player);
        }

        public override void Update()
        {
            if (LastOwner != null)
            {
                if (LastOwner.HasSynergy(Synergy.BUNCH_O_POTIONS) && !BUNCHOBALLOONSActivated)
                {
                    numDrinksPerFloor = 2 + BUNCHOBALLOONSUseIncreases;

                    BUNCHOBALLOONSActivated = true;
                }
                else if (!LastOwner.HasSynergy(Synergy.BUNCH_O_POTIONS) && BUNCHOBALLOONSActivated)
                {
                    numDrinksPerFloor = 2;

                    BUNCHOBALLOONSActivated = false;
                }
            }

            base.Update();
        }

        public override bool CanBeUsed(PlayerController user)
        {
            if (user.healthHaver.GetCurrentHealthPercentage() >= 1.0f)
            {
                return false;
            }
            return base.m_remainingDrinksThisFloor > 0;
        }

        public override void DoEffect(PlayerController user)
        {
            if (m_remainingDrinksThisFloor > 0)
            {
                if (healVFX != null)
                {
                    user.PlayEffectOnActor(healVFX, Vector3.zero);
                }

                userSRB = user.specRigidbody;
                m_remainingDrinksThisFloor--;
                user.healthHaver.ApplyHealing(healingAmount); 
                //AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
                AkSoundEngine.PostEvent("Health_Potion_active_SFX", base.gameObject);

                StartCoroutine(HandleDuration(user));
            }
            if (m_remainingDrinksThisFloor <= 0)
            {
                base.sprite.SetSprite(NoDrinkSprite);
            }
        }

        private IEnumerator HandleDuration(PlayerController user)
        {
            if (base.IsCurrentlyActive)
            {
                Debug.LogError("Using a ActiveBasicStatItem while it is already active!");
                yield break;
            }
            base.IsCurrentlyActive = true;

            if (user.HasSynergy(Synergy.COCKTAIL_POTION))
            {
                foreach (PlayerItem item in user.activeItems)
                {
                    if (item.PickupObjectId == (int)Items.PotionOfGunFriendship)
                    {
                        AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.Damage, POGFDamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.RateOfFire, POGFRateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.ReloadSpeed, POGFReloadStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        ItemBuilder.AddPassiveStatModifier(this, PlayerStats.StatType.KnockbackMultiplier, POGFKnockbackStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        user.stats.RecalculateStatsWithoutRebuildingGunVolleys(user);
                    }

                    if (item.PickupObjectId == (int)Items.PotionOfLeadSkin)
                    {
                        user.StartCoroutine(HandleShield(user));
                        AkSoundEngine.PostEvent("Play_OBJ_metalskin_activate_01", base.gameObject);
                    }
                }
            }

            m_activeElapsed = 0f;
            m_activeDuration = duration;
            while (m_activeElapsed < m_activeDuration && base.IsCurrentlyActive)
            {
                yield return null;
            }
            base.IsCurrentlyActive = false;

            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.ReloadSpeed);
            ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.KnockbackMultiplier);
            user.stats.RecalculateStatsWithoutRebuildingGunVolleys(user);
        }

        private IEnumerator HandleShield(PlayerController user)
        {
            m_activeElapsed = 0f;
            m_activeDuration = duration / 2f;
            m_usedOverrideMaterial = user.sprite.usesOverrideMaterial;
            user.sprite.usesOverrideMaterial = true;
            user.SetOverrideShader(ShaderCache.Acquire("Brave/ItemSpecific/MetalSkinShader"));
            SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
            speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
            user.healthHaver.IsVulnerable = false;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                user.healthHaver.IsVulnerable = false;
                yield return null;
            }
            if ((bool)user)
            {
                user.healthHaver.IsVulnerable = true;
                user.ClearOverrideShader();
                user.sprite.usesOverrideMaterial = m_usedOverrideMaterial;
                SpeculativeRigidbody speculativeRigidbody2 = user.specRigidbody;
                speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
            }
            if ((bool)this)
            {
                AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
            }
        }

        private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
        {
            Projectile component = otherRigidbody.GetComponent<Projectile>();
            if (component != null && !(component.Owner is PlayerController))
            {
                PassiveReflectItem.ReflectBullet(component, retargetReflectedBullet: true, userSRB.gameActor, 10f);
                PhysicsEngine.SkipCollision = true;
            }
        }

        public override void OnPreDrop(PlayerController user)
        {
            if (base.IsCurrentlyActive)
            {
                StopAllCoroutines();
                if ((bool)user)
                {
                    user.healthHaver.IsVulnerable = true;
                    user.ClearOverrideShader();
                    user.sprite.usesOverrideMaterial = m_usedOverrideMaterial;
                    SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
                    speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
                    base.IsCurrentlyActive = false;
                }
                if ((bool)this)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
                }
            }
        }
    }
}
