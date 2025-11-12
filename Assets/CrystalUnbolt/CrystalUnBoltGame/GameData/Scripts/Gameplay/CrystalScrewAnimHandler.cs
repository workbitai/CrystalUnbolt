using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalScrewAnimHandler : MonoBehaviour
    {
        [SerializeField] CrystalScrewController behavior;

        public void OnExtractCrystalPUAnimEnded()
        {
            behavior.OnExtractCrystalPUAnimEnded();
        }
    }
}
