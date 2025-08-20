using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria;
using Alexandria.ItemAPI;
using Steamworks;
using UnityEngine;

// maybe adjust sound volume

namespace LOLItems
{
    internal class KrakenSlayer : PassiveItem
    {
        // stats pool for item
        private int bringItDownCount = 0;
        private float bringItDownDamage = 20f;
        private static float bringItDownDamageScale = 0.25f;
        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.25f;
        public static void Init()
        {
            string itemName = "Kraken Slayer";
            string resourceName = "LOLItems/Resources/passive_item_sprites/kraken_slayer_item_sprite";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<KrakenSlayer>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "The Ultimate fishing tool";
            string longDesc = "A fishing tool passed down through generations. It's said that the original user " +
                "felled a kraken with it and freed the seas from its terror.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE); ;
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE); ;

            item.quality = ItemQuality.S;
        }

        // subscribe to events
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up Kraken Slayer");

            player.PostProcessProjectile += OnPostProcessProjectile;
            player.OnReloadedGun += OnGunReloaded;
            //player.OnNewFloorLoaded += CheckDamageScale;
            //CheckDamageScale();
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            Plugin.Log($"Player dropped or got rid of Kraken Slayer");

            player.PostProcessProjectile -= OnPostProcessProjectile;
            player.OnReloadedGun -= OnGunReloaded;
            bringItDownCount = 0; // Reset the count when the item is dropped
        }

        // returns a float value representing the damage scale for the current floor
        public float GetFloorDamageScale()
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
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            bringItDownCount++;
            if (bringItDownCount >= 3)
            {
                proj.sprite.color = Color.Lerp(proj.sprite.color, Color.cyan, 0.7f);
                AkSoundEngine.PostEvent("kraken_slayer_passive_SFX", proj.gameObject);
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (enemy != null && enemy.aiActor != null)
                    {
                        // scales damage based on enemy's missing health percentage
                        float percentDamageIncrease = 0.75f * (1.0f - enemy.healthHaver.GetCurrentHealthPercentage());
                        float damageToDeal = bringItDownDamage * (1.0f + percentDamageIncrease) * GetFloorDamageScale();
                        // damage is 1/4 against bosses and sub-bosses
                        if (enemy.healthHaver.IsBoss || enemy.healthHaver.IsSubboss)
                        {
                            damageToDeal *= 0.25f; 
                        }

                        Plugin.Log($"Bring it down damage dealt: {damageToDeal}");

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

        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            bringItDownCount = 0; // Reset the count when the gun is reloaded
        }
    }
}
