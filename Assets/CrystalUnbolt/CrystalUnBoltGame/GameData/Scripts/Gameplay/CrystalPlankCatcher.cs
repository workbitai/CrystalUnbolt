using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPlankCatcher : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var plank = collision.attachedRigidbody.GetComponent<CrystalPlankController>();

            if(plank != null && plank.IsFlying() && !plank.IsBeingDisabled) plank.Disable();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var plank = collision.attachedRigidbody.GetComponent<CrystalPlankController>();

            if (plank != null && plank.IsFlying() && !plank.IsBeingDisabled) plank.Disable();
        }
    }
}
