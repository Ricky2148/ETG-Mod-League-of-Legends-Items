using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LOLItems.custom_class_data
{
    public class VFXAnchorOnHandModule : MonoBehaviour
    {
        public PlayerHandController hand;

        public Vector3 offset;

        private void LateUpdate()
        {
            if (hand != null)
            {
                base.gameObject.transform.position = hand.sprite.WorldCenter.ToVector3ZUp() + offset;
                base.gameObject.GetComponent<tk2dSprite>().UpdateZDepth();
            }
        }
    }
}
