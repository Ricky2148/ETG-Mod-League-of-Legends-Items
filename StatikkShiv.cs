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

namespace LOLItems
{
    internal class StatikkShiv : PassiveItem
    {
        // stats pool for item
        private static float DamageStat = 1.2f;
        private static float RateOfFireStat = 1.1f;

        private int ElectroSparkShotCount = 3;
        private static float ElectroSparkDamage = 5f;
        private static float ElectroSparkChainCount = 5f;
        private static float ElectroSparkChainRange = 5f;

        public static void Init()
        {
            string itemName = "Statikk Shiv";
            string resourceName = "LOLItems/Resources/passive_item_sprites/statikk_shiv_item_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<StatikkShiv>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*shocks you*";
            string longDesc = "Supposed to be a replica of Zeus's Lightning Bolt." +
                "\njust a shiv with a taser\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            item.quality = PickupObject.ItemQuality.A;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Statikk Shiv");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.OnReloadedGun += ResetElectroSpark;
            ElectroSparkShotCount = 3;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Statikk Shiv");

            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.OnReloadedGun -= ResetElectroSpark;
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            if (proj.Owner is not PlayerController player) return;
            if (player.CurrentGun is not Gun gun) return;
            if (ElectroSparkShotCount > 0)
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
                chain.vfxPath = "statikk_shiv_lightning_SFX";
                ElectroSparkShotCount--;
            }
        }

        private void ResetElectroSpark(PlayerController player, Gun gun)
        {
            ElectroSparkShotCount = 3;
        }
    }
}
