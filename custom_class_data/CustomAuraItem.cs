// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// AuraItem
using System;
using UnityEngine;

// trying to set up custom aura item class that allows applying effects to enemies in radius around player
// probably not useful anymore LMAO
public class CustomAuraItem : PassiveItem
{
    public float AuraRadius = 5f;

    public CoreDamageTypes damageTypes;

    public float DamagePerSecond = 5f;

    public bool DealsDamage = true;

    public bool DealsCripple = true;

    public float CrippleAmount = 0.8f;

    public GameObject AuraVFX;

    public NumericSynergyMultiplier[] damageMultiplierSynergies;

    public NumericSynergyMultiplier[] rangeMultiplierSynergies;

    private GameObject m_extantAuraVFX;

    private Action<AIActor, float> AuraAction;

    private bool didDamageEnemies;

    private float ModifiedAuraRadius => AuraRadius * GetRangeMultiplier();

    private float ModifiedDamagePerSecond => DamagePerSecond * GetDamageMultiplier();

    public override void Update()
    {
        base.Update();
        if (m_pickedUp && (bool)m_owner && !m_owner.IsStealthed && !GameManager.Instance.IsLoadingLevel)
        {
            DoAura();
        }
    }

    public override DebrisObject Drop(PlayerController player)
    {
        if (m_extantAuraVFX != null)
        {
            UnityEngine.Object.Destroy(m_extantAuraVFX);
            m_extantAuraVFX = null;
        }
        return base.Drop(player);
    }

    protected float GetDamageMultiplier()
    {
        float num = 1f;
        if (m_owner != null)
        {
            for (int i = 0; i < damageMultiplierSynergies.Length; i++)
            {
                if (m_owner.HasActiveBonusSynergy(damageMultiplierSynergies[i].RequiredSynergy))
                {
                    num *= damageMultiplierSynergies[i].SynergyMultiplier;
                }
            }
        }
        return num;
    }

    protected float GetRangeMultiplier()
    {
        float num = 1f;
        if (m_owner != null)
        {
            for (int i = 0; i < rangeMultiplierSynergies.Length; i++)
            {
                if (m_owner.HasActiveBonusSynergy(rangeMultiplierSynergies[i].RequiredSynergy))
                {
                    num *= rangeMultiplierSynergies[i].SynergyMultiplier;
                }
            }
        }
        return num;
    }

    protected virtual void DoAura()
    {
        if (m_extantAuraVFX == null)
        {
        }
        didDamageEnemies = false;
        if (AuraAction == null)
        {
            AuraAction = delegate (AIActor actor, float dist)
            {
                float num = ModifiedDamagePerSecond * BraveTime.DeltaTime;
                if (num > 0f)
                {
                    didDamageEnemies = true;
                }
                if(DealsDamage) actor.healthHaver.ApplyDamage(num, Vector2.zero, "Aura", damageTypes);
                // custom code to handle cripple effect
                if (DealsCripple)
                {
                    // slows enemy attack speed correctly, but doesn't reset when enemies leave range
                    actor.behaviorSpeculator.CooldownScale = CrippleAmount;
                    actor.aiShooter.AimTimeScale = CrippleAmount;
                    actor.bulletBank.TimeScale = CrippleAmount;
                }
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

    public override void OnDestroy()
    {
        if (m_extantAuraVFX != null)
        {
            UnityEngine.Object.Destroy(m_extantAuraVFX);
            m_extantAuraVFX = null;
        }
        base.OnDestroy();
    }
}
