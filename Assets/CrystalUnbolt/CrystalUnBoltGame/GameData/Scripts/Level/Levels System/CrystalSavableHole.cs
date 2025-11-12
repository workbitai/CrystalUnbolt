using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalSavableHole : MonoBehaviour
    {
        [SerializeField] [OnValueChanged("UpdateScrew")] bool hasScrew;
        [SerializeField] [HideInInspector] GameObject screwRef;

        public bool HasScrew
        {
            get => hasScrew; set
            {
                hasScrew = value;
                UpdateScrew();
            }
        }

        public void UpdateScrew()
        {
#if UNITY_EDITOR
            if (hasScrew)
            {
                if(screwRef != null)
                {
                    return;
                }

                screwRef = PrefabUtility.InstantiatePrefab(CrystalEditorSceneController.Instance.ScrewPrefab) as GameObject;
                screwRef.transform.SetParent(transform);
                screwRef.transform.ResetLocal();
            }
            else
            {
                if(screwRef != null)
                {
                    DestroyImmediate(screwRef);
                }
            }
#endif
        }
    }
}
