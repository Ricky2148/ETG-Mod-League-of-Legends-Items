using LOLItems.passive_items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.custom_class_data
{
    public class HealthHaverOnPreDeathActionModule : HealthHaver
    {
        public AIActor targetAIActor;

        public GameObject idleVFX;

        public GameObject explosionVFX;

        public Vector3 vfxOffset;

        public EnemyTheBombTracker theBombTracker;

        public new void Start()
        {
            //Plugin.Log($"started HealthHaverOnPreDeathActionModule: {targetAIActor.healthHaver}");
            targetAIActor.healthHaver.OnPreDeath += OnPreDeathActivation;
        }

        public void OnPreDeathActivation(Vector2 vector2)
        {
            //Plugin.Log($"activated HealthHaverOnPreDeathActionModule");

            if (theBombTracker.timerCoroutine != null)
            {
                //Plugin.Log($"killed coroutine");
                targetAIActor.StopCoroutine(theBombTracker.timerCoroutine);
            }

            AkSoundEngine.PostEvent("detOrb_SFX_loop_002" + "_stop", targetAIActor.gameObject);

            UnityEngine.Object.Instantiate(explosionVFX, targetAIActor.specRigidbody.UnitBottomCenter.ToVector3ZUp() + vfxOffset + new Vector3(0, targetAIActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y), Quaternion.identity);

            AkSoundEngine.PostEvent("detOrb_SFX_explosion_001", targetAIActor.gameObject);

            if (theBombTracker.activeVFXObject != null)
            {
                //Plugin.Log($"destroyed vfx");
                Destroy(theBombTracker.activeVFXObject);
            }
        }
    }
}
