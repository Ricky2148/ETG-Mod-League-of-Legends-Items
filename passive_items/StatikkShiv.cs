using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using UnityEngine;

// first 3 shots apply the chain lightning, dealing set dmg to certain number of enemies, bounce range set range
// try to use ComplexProjectileModifier?????
// Almost functional

namespace LOLItems.passive_items
{
    internal class StatikkShiv : PassiveItem
    {
        public static string ItemName = "Statikk Shiv";

        // stats pool for item
        //private static float DamageStat = 1.2f;
        //private static float RateOfFireStat = 1.1f;

        //public int BaseElectroSparkShotCount = 3;
        private static float ElectroSparkDamage = 10f;
        private static float ElectroSparkChainCount = 8f;
        private static float ElectroSparkChainRange = 8f;

        private int ElectroSparkShotCount = 5;
        private int ElectroSparkShotCountTracker = 0;

        public bool STATIKKELECTRICITYActivated = false;
        private static int STATIKKELECTRICITYElectroSparkShotCountInc = 4;
        public bool MOLIGHTNINGActivated = false;
        private static float MOLIGHTNINGElectroSparkDamageInc = 3f;
        private static float MOLIGHTNINGElectroSparkChainCountInc = 3f;
        private static float MOLIGHTNINGElectroSparkChainRangeInc = 2f;
        public bool EMPEROROFLIGHTNINGActivated = false;
        private static int EMPEROROFLIGHTNINGElectroSparkShotCountInc = 5;
        private static float EMPEROROFLIGHTNINGElectroSparkDamageInc = 5f;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/passive_item_sprites/statikk_shiv_pixelart_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<StatikkShiv>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*shocks you*";
            string longDesc = "After reloading, first 5 bullets of clip applies a chain lightning to enemies hit.\n\n" +
                "Supposed to be a replica of Zeus's Lightning Bolt." +
                "\njust a shiv with a taser\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.OnReloadedGun += ResetElectroSpark;
            //ElectroSparkShotCount = BaseElectroSparkShotCount;
            ElectroSparkShotCountTracker = ElectroSparkShotCount;
            //Plugin.Log($"pickup electrosparkshotcount: {ElectroSparkShotCount}");
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            if (player != null)
            {
                player.PostProcessProjectile -= OnPostProcessProjectile;
                player.OnReloadedGun -= ResetElectroSpark;
            }
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (Owner.HasSynergy(Synergy.STATIKK_ELECTRICITY) && !STATIKKELECTRICITYActivated)
                {
                    ElectroSparkShotCount += STATIKKELECTRICITYElectroSparkShotCountInc;
                    //Plugin.Log($"statikk electricity on electrosparkshotcount: {ElectroSparkShotCount}");
                    ResetElectroSpark(null, null);

                    STATIKKELECTRICITYActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.STATIKK_ELECTRICITY) && STATIKKELECTRICITYActivated)
                {
                    ElectroSparkShotCount -= STATIKKELECTRICITYElectroSparkShotCountInc;
                    //Plugin.Log($"statikk electricity off electrosparkshotcount: {ElectroSparkShotCount}");
                    ResetElectroSpark(null, null);

                    STATIKKELECTRICITYActivated = false;
                }

                if (Owner.HasSynergy(Synergy.MO_LIGHTNING) && !MOLIGHTNINGActivated)
                {
                    ElectroSparkDamage += MOLIGHTNINGElectroSparkDamageInc;
                    ElectroSparkChainCount += MOLIGHTNINGElectroSparkChainCountInc;
                    ElectroSparkChainRange += MOLIGHTNINGElectroSparkChainRangeInc;

                    MOLIGHTNINGActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.MO_LIGHTNING) && MOLIGHTNINGActivated)
                {
                    ElectroSparkDamage -= MOLIGHTNINGElectroSparkDamageInc;
                    ElectroSparkChainCount -= MOLIGHTNINGElectroSparkChainCountInc;
                    ElectroSparkChainRange -= MOLIGHTNINGElectroSparkChainRangeInc;

                    MOLIGHTNINGActivated = false;
                }

                if (Owner.HasSynergy(Synergy.EMPEROR_OF_LIGHTNING) && !EMPEROROFLIGHTNINGActivated)
                {
                    ElectroSparkShotCount += EMPEROROFLIGHTNINGElectroSparkShotCountInc;
                    ElectroSparkDamage += EMPEROROFLIGHTNINGElectroSparkDamageInc;
                    //Plugin.Log($"emperor on electrosparkshotcount: {ElectroSparkShotCount}");
                    ResetElectroSpark(null, null);

                    EMPEROROFLIGHTNINGActivated = true;
                }
                else if (!Owner.HasSynergy(Synergy.EMPEROR_OF_LIGHTNING) && EMPEROROFLIGHTNINGActivated)
                {
                    ElectroSparkShotCount -= EMPEROROFLIGHTNINGElectroSparkShotCountInc;
                    ElectroSparkDamage -= EMPEROROFLIGHTNINGElectroSparkDamageInc;
                    //Plugin.Log($"emperor off electrosparkshotcount: {ElectroSparkShotCount}");
                    ResetElectroSpark(null, null);

                    EMPEROROFLIGHTNINGActivated = false;
                }
            }

            base.Update();
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
            if (proj.Shooter == proj.Owner.specRigidbody && ElectroSparkShotCountTracker > 0)
            {
                //applies chain lightning effect to the projectile
                ComplexProjectileModifier shockRounds = PickupObjectDatabase.GetById(298)
                    as ComplexProjectileModifier;
                CustomLightningChainEnemiesModifier chain = proj.gameObject.GetOrAddComponent<CustomLightningChainEnemiesModifier>();
                chain.LinkVFXPrefab = shockRounds.ChainLightningVFX;
                chain.damageTypes = CoreDamageTypes.Electric;
                chain.maximumLinkDistance = ElectroSparkChainRange;
                chain.damagePerHit = ElectroSparkDamage;
                chain.maxLinkCount = ElectroSparkChainCount;
                chain.DispersalDensity = 5f;
                chain.DispersalMaxCoherency = 0.7f;
                chain.DispersalMinCoherency = 0.3f;
                chain.UsesDispersalParticles = false;
                string[] sfxList = {
                    "statikk_shiv_lightning_SFX_1",
                    "statikk_shiv_lightning_SFX_2",
                    "statikk_shiv_lightning_SFX_3",
                    "statikk_shiv_lightning_SFX_4",
                    "statikk_shiv_lightning_SFX_5"
                };
                chain.updateSFXList(sfxList);
                ElectroSparkShotCountTracker--;
            }
        }

        private void ResetElectroSpark(PlayerController player, Gun gun)
        {
            ElectroSparkShotCountTracker = ElectroSparkShotCount;
        }
    }
}
