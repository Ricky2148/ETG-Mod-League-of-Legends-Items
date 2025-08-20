using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using JetBrains.Annotations;

// add a way for phantomhit extra projectiles to be modified by other items like scatter shot or helix bullets
// i don't think the % offset for the delay is accounting for additional
// rate of fire increases from other items

namespace LOLItems
{
    public class GuinsoosRageblade : PassiveItem
    {
        // stats pool for item
        private int phantomHitCount = 0; // Counter for the number of phantom hits

        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.2f;
        public static void Init()
        {
            string itemName = "Guinsoo's Rageblade";
            string resourceName = "LOLItems/Resources/passive_item_sprites/guinsoos_rageblade_item_sprite";
            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<GuinsoosRageblade>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "*not affiliated with Kratos*";
            string longDesc = "Forged in the foulest depths of the Void. These blades increase one's capacity for " +
                "rage and destruction. Perhaps you should not wield them.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);

            /*
            implement double hit passive = every 3rd shot fires 2 bullets
            count with each bullet fired, 3rd bullet fired will fire 2 bullets
            maybe implement atkspd stacking passive
            */

            item.quality = PickupObject.ItemQuality.S;
        }

        // subscribe to the player events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Guinsoo's Rageblade");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.OnReloadedGun += OnGunReloaded;
        }

        public override void DisableEffect(PlayerController player)
        {
            Plugin.Log($"Player dropped or got rid of Guinsoo's Rageblade");

            player.PostProcessProjectile -= OnPostProcessProjectile;
            phantomHitCount = 0; // Reset the count when the item is dropped
            player.OnReloadedGun -= OnGunReloaded;
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            phantomHitCount++;
            if (phantomHitCount >= 3)
            {
                PlayerController player = proj.Owner as PlayerController;
                phantomHitCount = 0;
                player.StartCoroutine(FirePhantomProjectileDelayed(proj));
            }
        }

        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            phantomHitCount = 0; // Reset the count when the gun is reloaded
        }

        private System.Collections.IEnumerator FirePhantomProjectileDelayed(Projectile proj)
        {
            if (proj == null || proj.Owner == null) yield break;
            PlayerController player = proj.Owner as PlayerController;

            // Calculate the delay based on the gun's rate of fire
            float baseDelayRatio = 0.3f;
            float gunRateOfFire = player.CurrentGun.DefaultModule.cooldownTime;
            float delay = Mathf.Max(gunRateOfFire * baseDelayRatio, 0.01f);
            delay = Mathf.Ceil(delay * 100f) / 100f;
            
            yield return new WaitForSeconds(delay);

            // create a phantom projectile based on the exact stats of the original projectile
            Projectile phantomHit = UnityEngine.Object.Instantiate(proj);
            ProjectileData newData = new ProjectileData();
            newData.damage = proj.baseData.damage;
            newData.speed = proj.baseData.speed;
            newData.range = proj.baseData.range;
            newData.force = proj.baseData.force;
            phantomHit.baseData = newData;
            phantomHit.Owner = proj.Owner;
            phantomHit.Shooter = proj.Shooter;
            phantomHit.sprite.color = Color.Lerp(proj.sprite.color, Color.red, 0.7f);

            // Set the position and rotation of the phantom projectile
            phantomHit.transform.position = player.CurrentGun.barrelOffset.position;
            Vector2 direction = proj.LastVelocity.normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set the projectile's rotation to match the direction
            phantomHit.transform.rotation = Quaternion.Euler(0, 0, angle);

            phantomHit.SendInDirection(direction, true, true);
        }
    }
}
