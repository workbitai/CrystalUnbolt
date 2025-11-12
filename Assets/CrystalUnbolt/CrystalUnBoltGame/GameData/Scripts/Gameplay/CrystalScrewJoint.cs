using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalScrewJoint : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Joint2D joint;

        public void Init()
        {
            if(rb == null) rb = GetComponent<Rigidbody2D>();
            if (joint == null) joint = GetComponent<Joint2D>();
        }

        public void SetConnectedBody(Rigidbody2D rb)
        {
            joint.connectedBody = rb;
        }

        public void DisableSimulation()
        {
            rb.simulated = false;
            joint.enabled = false;
        }

        public void EnableSimulation()
        {
            rb.simulated = true;
            joint.enabled = true;
        }
    }
}