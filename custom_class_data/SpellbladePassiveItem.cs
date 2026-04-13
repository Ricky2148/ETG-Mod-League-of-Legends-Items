using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.custom_class_data
{
    public class SpellbladePassiveItem : PassiveItem
    {
        public string damageIdentifier = "spellblade_template_damage";

        private bool shouldApplySpellblade = false;
        public float activationDmgValue = 10f;
        public float activationCooldownValue = 3f;

        public bool baseDamageScalesWithPlayerStats = false;
        public float damageStatScaleRatio = 1f;

        public bool activationDealsPercentDamage = false;
        public float percentDamageRatio = 0f;

        private bool isOnCooldown = false;
        private float CooldownTimer = 999999999f;

        public Action<PlayerController> OnSpellbladeProc;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnReloadedGun += OnGunReloaded;
            player.PostProcessProjectile += OnPostProcessProjectile;
            shouldApplySpellblade = true;
        }

        public override void DisableEffect(PlayerController player)
        {
            base.DisableEffect(player);
            if (player != null)
            {
                player.OnReloadedGun -= OnGunReloaded;
                player.PostProcessProjectile -= OnPostProcessProjectile;
            }

            shouldApplySpellblade = false;
        }

        public override void Update()
        {
            if (Owner != null)
            {
                if (isOnCooldown)
                {
                    float now = BraveTime.ScaledTimeSinceStartup;
                    //Plugin.Log($"curTime: {now}");
                    if (now < CooldownTimer)
                    {
                        base.Update();
                        return;
                    }
                    Plugin.Log($"cooldown ended: curTime: {BraveTime.ScaledTimeSinceStartup}");
                    isOnCooldown = false;
                }
            }

            base.Update();
        }

        private void OnGunReloaded(PlayerController player, Gun gun)
        {
            if (isOnCooldown) return;
            shouldApplySpellblade = true;
            Plugin.Log($"sheen activated: {shouldApplySpellblade}");
        }

        private void OnPostProcessProjectile(Projectile proj, float f)
        {
            //Plugin.Log($"spellblade proc: {shouldApplySpellblade}");
            if (proj.Shooter == proj.Owner.specRigidbody && shouldApplySpellblade)
            {
                proj.OnHitEnemy += (projHit, enemy, fatal) =>
                {
                    if (!shouldApplySpellblade) return;
                    if (enemy == null) return;
                    if (enemy.aiActor == null && enemy.GetComponentInParent<AIActor>() == null) return;
                    if (enemy.healthHaver != null)
                    {
                        float dmgToDeal = activationDmgValue;

                        if (baseDamageScalesWithPlayerStats)
                        {
                            dmgToDeal *= (Owner.stats.GetStatValue(PlayerStats.StatType.Damage) * damageStatScaleRatio);
                        }

                        if (activationDealsPercentDamage)
                        {
                            dmgToDeal += enemy.healthHaver.GetMaxHealth() * percentDamageRatio;
                        }

                        enemy.healthHaver.ApplyDamage(
                            dmgToDeal,
                            Vector2.zero,
                            damageIdentifier,
                            CoreDamageTypes.None,
                            DamageCategory.Normal,
                            false
                        );
                    }
                    Plugin.Log($"cooldown started, shouldApplySpellblade: {shouldApplySpellblade}, curTime: {BraveTime.ScaledTimeSinceStartup}, expected cooldown end time: {BraveTime.ScaledTimeSinceStartup + activationCooldownValue}");
                    shouldApplySpellblade = false;
                    isOnCooldown = true;
                    CooldownTimer = BraveTime.ScaledTimeSinceStartup + activationCooldownValue;
                    if (OnSpellbladeProc != null)
                    {
                        OnSpellbladeProc(m_owner);
                    }
                };
            }
        }
    }
}
