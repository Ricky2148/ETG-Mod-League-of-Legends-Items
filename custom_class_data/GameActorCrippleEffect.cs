using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameActorCrippleEffect : GameActorEffect
{
    // float value equal to ratio of cripple: 0.8f for 20% decrease in fire rate
    public float CrippleAmount;

    public float CrippleDuration;

    public bool ShouldVanishOnDeath(GameActor actor)
    {
        if ((bool)actor.healthHaver && actor.healthHaver.IsBoss)
        {
            return false;
        }
        if (actor is AIActor && (actor as AIActor).IsSignatureEnemy)
        {
            return false;
        }
        return true;
    }

    public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
    {
        actor.behaviorSpeculator.CooldownScale = CrippleAmount;
        actor.aiShooter.AimTimeScale = CrippleAmount;
        actor.bulletBank.TimeScale = CrippleAmount;
    }

    public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
    {
        if ((bool)actor.aiShooter)
        {
            actor.aiShooter.AimTimeScale = 1f;
        }
        if ((bool)actor.behaviorSpeculator)
        {
            actor.behaviorSpeculator.CooldownScale = 1f;
        }
        if ((bool)actor.bulletBank)
        {
            actor.bulletBank.TimeScale = 1f;
        }
        tk2dSpriteAnimator spriteAnimator = actor.spriteAnimator;
        if ((bool)spriteAnimator && (bool)actor.aiAnimator && spriteAnimator.CurrentClip != null && !spriteAnimator.IsPlaying(spriteAnimator.CurrentClip))
        {
            actor.aiAnimator.PlayUntilFinished(actor.spriteAnimator.CurrentClip.name, suppressHitStates: false, null, -1f, skipChildAnimators: true);
        }
    }
}
