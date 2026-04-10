using Alexandria;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using JetBrains.Annotations;
using LOLItems.custom_class_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

// missles wont have missing health scaling (too fucking annoying)

namespace LOLItems.active_items
{
    internal class Galeforce : PlayerItem
    {
        public static string ItemName = "Galeforce";

        // stats pool for item
        private static float DamageStat = 1.25f;
        private static float RateOfFireStat = 1.2f;
        private static float CloudburstBaseDamage = 25f;
        private static float CloudburstCooldown = 30f;

        //private bool playerHasFlight = false;
        private bool BOWMASTERYActivated = false;

        public Projectile CloudburstProjectile = (PickupObjectDatabase.GetById((int)Items.YariLauncher) as Gun)
            .DefaultModule.projectiles[0].InstantiateAndFakeprefab();
        public int NumToSpawn = 3;

        public static int ID;

        public static void Init()
        {
            string itemName = ItemName;
            string resourceName = "LOLItems/Resources/active_item_sprites/galeforce_pixelart_sprite";
            
            GameObject obj = new GameObject(itemName);
            
            var item = obj.AddComponent<Galeforce>();
            
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            
            string shortDesc = "\"Hasagi!\"";
            string longDesc = "Dash in a direction.\n\n" +
                "A strangely crafted bow that seems to make the feet below you lighter. " +
                "You can't help but feel that there's something hidden with this bow. Maybe there's something " +
                "hidden in the bow?\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "LOLItems");
            
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, DamageStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.RateOfFire, RateOfFireStat, StatModifier.ModifyMethod.MULTIPLICATIVE);
            
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, CloudburstCooldown);
            
            item.consumable = false;
            
            item.usableDuringDodgeRoll = true;
            
            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;

            /*List<string> mandatoryConsoleIDs = new List<string>
            {
                "LOLItems:galeforce",
                "LOLItems:whisper"
            };
            CustomSynergies.Add("FOUR!", mandatoryConsoleIDs, null, true);

            List<string> mandatoryConsoleIDs2 = new List<string>
            {
                "LOLItems:galeforce"
            };
            List<string> optionalConsoleIDs2 = new List<string>
            {
                "bow",
                "charmed_bow",
                "gunbow"
            };
            CustomSynergies.Add("Bow Mastery", mandatoryConsoleIDs2, optionalConsoleIDs2, true);*/
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            Plugin.Log($"Player picked up {this.EncounterNameOrDisplayName}");
            //Plugin.Log($"isFlying: {player.IsFlying}");
        }

        public DebrisObject Drop(PlayerController player)
        { 
            Plugin.Log($"Player dropped or got rid of {this.EncounterNameOrDisplayName}");

            //ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.Damage);
            //ItemBuilder.RemovePassiveStatModifier(this, PlayerStats.StatType.RateOfFire);
            //player.stats.RecalculateStats(player, false, false);
            player.stats.RecalculateStatsWithoutRebuildingGunVolleys(player);

            return base.Drop(player);
        }

        public override void Update()
        {
            if (LastOwner != null)
            {
                if (LastOwner.HasSynergy(Synergy.BOW_MASTERY) && !BOWMASTERYActivated)
                {
                    this.timeCooldown = CloudburstCooldown / 2f;

                    BOWMASTERYActivated = true;
                }
                else if (!LastOwner.HasSynergy(Synergy.BOW_MASTERY) && BOWMASTERYActivated)
                {
                    this.timeCooldown = CloudburstCooldown;
                    //Plugin.Log($"{WintersCaressCrippleEffect.CrippleAmount}");

                    BOWMASTERYActivated = false;
                }
            }

            base.Update();
        }

        public override void DoEffect(PlayerController player)
        {
            player.StartCoroutine(HandleCloudburst(player));
        }

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

        private void HandleSFX(Projectile proj, SpeculativeRigidbody body, bool fatal)
        {
            if (body == null || body.aiActor == null) return;
            AkSoundEngine.PostEvent("galeforce_active_missile_hit_SFX", proj.gameObject);
        }

        public System.Collections.IEnumerator HandleCloudburst(PlayerController player)
        {
            AkSoundEngine.PostEvent("galeforce_active_SFX", player.gameObject);
            // parameters for dash
            float duration = 0.2f;
            float adjSpeed = 30f;
            float elapsed = -BraveTime.DeltaTime;

            // set up projectile stats properly
            // GetFloorPriceMod works instead of explicitly stating it like GetFloorDamageScale
            float currentCloudburstDamage = CloudburstBaseDamage * HelpfulMethods.GetFloorPriceMod();
            CloudburstProjectile.baseData.damage = currentCloudburstDamage * player.stats.GetStatValue(PlayerStats.StatType.Damage);

            if (player.HasSynergy(Synergy.GALEFORCE_FOUR))
            {
                CloudburstProjectile.baseData.damage *= 2.5f;
            }

            CloudburstProjectile.baseData.force = 5f;
            CloudburstProjectile.baseData.speed = 20f;

            CloudburstProjectile.GetComponent<RobotechProjectile>().searchRadius = 30f;
            CloudburstProjectile.GetComponent<RobotechProjectile>().angularAcceleration = 360f;
            CloudburstProjectile.GetComponent<RobotechProjectile>().PenetratesInternalWalls = true;
            CloudburstProjectile.GetComponent<RobotechProjectile>().pierceMinorBreakables = true;
            CloudburstProjectile.GetComponent<RobotechProjectile>().reacquiresTargets = true;

            // doesnt work
            /*CloudburstProjectile.OnHitEnemy += (proj, enemy, fatal) =>
            {
                if (enemy != null && enemy.aiActor != null)
                {
                    float percentDamageIncrease = 0.5f * (1.0f - enemy.healthHaver.GetCurrentHealthPercentage());
                    float damageToDeal = currentCloudburstDamage * percentDamageIncrease;
                    enemy.healthHaver.ApplyDamage(
                            damageToDeal,
                            Vector2.zero,
                            "cloudburst_missing_health_bonus_damage",
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                    );
                    Plugin.Log($"Cloudburst hit {enemy.aiActor.EnemyGuid}, dealing {damageToDeal} bonus damage based on missing health." +
                        $"\nBase damage: {CloudburstProjectile.baseData.damage}");
                }
            };*/

            // set up explosion data
            ExplosionData explosion = CloudburstProjectile.GetComponent<ExplosiveModifier>().explosionData = new ExplosionData();
            explosion.doDamage = true;
            explosion.damage = 10f;
            explosion.doForce = false;

            // dash in last input player direction
            Vector2 angle = player.m_lastNonzeroCommandedDirection.normalized;
            //float angle = player.CurrentGun.CurrentAngle;

            player.healthHaver.TriggerInvulnerabilityPeriod(duration);

            Material mat = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
            player.FallingProhibited = true;
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(102f * 0.3f, 255f * 0.3f, 255f * 0.3f));
            }
            
            //for duration of dash, set player velocity to dash speed and angle to last input angle
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                player.specRigidbody.Velocity = angle * adjSpeed;
                //this.LastOwner.specRigidbody.Velocity = BraveMathCollege.DegreesToVector(angle).normalized * adjSpeed;
                yield return null;
            }
                
            player.FallingProhibited = false;
            if (mat)
            {
                mat.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }

            // make the projectiles spawn in a spread pattern
            float[] CloudburstProjectileAngles =
            {
                player.CurrentGun.CurrentAngle,
                player.CurrentGun.CurrentAngle + 45f,
                player.CurrentGun.CurrentAngle - 45f
            };

            // create projectiles and send out projectiles
            Projectile projectile = CloudburstProjectile;
            for (int i = 0; i < NumToSpawn; i++)
            {
                GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, player.specRigidbody.UnitCenter, Quaternion.Euler(0f, 0f, CloudburstProjectileAngles[i]));
                Projectile component = gameObject.GetComponent<Projectile>();
                component.Owner = player;
                component.Shooter = player.specRigidbody;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
