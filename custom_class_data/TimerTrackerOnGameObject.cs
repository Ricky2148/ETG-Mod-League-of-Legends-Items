using LOLItems.passive_items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.custom_class_data
{
    internal class TimerTrackerOnGameObject : MonoBehaviour
    {
        public float limit = 5f;

        public AIActor targetAIActor;

        public GameObject idleVFX;

        public GameObject explosionVFX;

        public Vector3 vfxOffset = Vector3.zero;

        public EnemyTheBombTracker theBombTracker;

        private void Update()
        {
            float now = BraveTime.ScaledTimeSinceStartup;
            if (now < limit)
            {
                return;
            }

            DetonateBombAfterTimer();
        }

        public void DetonateBombAfterTimer()
        {
            AkSoundEngine.PostEvent("detOrb_SFX_loop_002" + "_stop", targetAIActor.gameObject);

            UnityEngine.Object.Instantiate(explosionVFX, targetAIActor.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset + new Vector3(0, targetAIActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y), Quaternion.identity);

            AkSoundEngine.PostEvent("detOrb_SFX_explosion_001", targetAIActor.gameObject);

            targetAIActor.healthHaver.ApplyDamage(
                theBombTracker.storedDamage,
                Vector2.zero,
                "the_bomb_detonation_damage",
                CoreDamageTypes.None,
                DamageCategory.Normal,
                ignoreInvulnerabilityFrames: true,
                hitPixelCollider: null,
                ignoreDamageCaps: true
            );

            if (theBombTracker.activeVFXObject != null)
            {
                //Plugin.Log($"destroyed vfx");
                Destroy(theBombTracker.activeVFXObject);
            }

            Destroy(this);
        }
    }
}
