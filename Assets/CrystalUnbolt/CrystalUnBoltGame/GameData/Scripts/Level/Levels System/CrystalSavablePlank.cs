using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalSavablePlank : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] CrystalPlankType plankType;
        [SerializeField] [OnValueChanged("HandleManualLayerChange")] int plankLayer;
        [SerializeField] [HideInInspector] int actualPlankLayer;
        [SerializeField] List<Vector3> screwsPositions;

        public CrystalPlankType PlankType { get => plankType; set => plankType = value; }
        public int PlankLayer
        {
            get => actualPlankLayer; 
            set
            {
                actualPlankLayer = value;
                plankLayer = value + 1;

                if (actualPlankLayer > CrystalEditorSceneController.Instance.CurrentLayerIndex)
                {
                    CrystalEditorSceneController.Instance.CurrentLayerIndex = actualPlankLayer;

                    if (CrystalEditorSceneController.Instance.CurrentLayerIndex > CrystalEditorSceneController.Instance.MaxLayerIndex)
                    {
                        CrystalEditorSceneController.Instance.MaxLayerIndex = CrystalEditorSceneController.Instance.CurrentLayerIndex;
                    }
                }

                LayerUpdate();
            }
        }
        public List<Vector3> ScrewsPositions { get => screwsPositions; set => screwsPositions = value; }

        public void LayerUpdate()
        {
            Color newColor = CrystalEditorSceneController.Instance.LayerColors[Mathf.Clamp(actualPlankLayer, 0, CrystalEditorSceneController.Instance.LayerColors.Length - 1)];
            SpriteRenderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            bool isVisible = (actualPlankLayer <= CrystalEditorSceneController.Instance.CurrentLayerIndex);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (isVisible)
                {
                    renderers[i].color = newColor;
                    
                }
                else //make it barely visible
                {
                    renderers[i].color = newColor.SetAlpha(0.2f);
                }

                renderers[i].sortingOrder = plankLayer;
            }

            if (isVisible)
            {
                UnityEditor.SceneVisibilityManager.instance.EnablePicking(gameObject, true);
            }
            else
            {
                UnityEditor.SceneVisibilityManager.instance.DisablePicking(gameObject, true);
            }
        }

        private void HandleManualLayerChange()
        {
            PlankLayer = plankLayer - 1;
        }
#endif
    }
}
