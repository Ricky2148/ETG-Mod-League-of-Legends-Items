using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.custom_class_data
{
    public class VFXAnchorModule : MonoBehaviour
    {
        public AIActor anchorAIActor;

        public Vector3 offset;

        private void LateUpdate()
        {
            if (anchorAIActor != null)
            {
                base.gameObject.transform.position = anchorAIActor.specRigidbody.UnitBottomCenter.ToVector3ZUp() + offset;
                base.gameObject.GetComponent<tk2dSprite>().UpdateZDepth();

                if (!anchorAIActor.healthHaver.IsAlive)
                {
                    Destroy(base.gameObject);
                }
            }
        }
    }
}
