#pragma warning disable 649

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalEditorSceneController : MonoBehaviour
    {
        private const string PREFS_WIDTH_KEY = "EditorCameraOverrideWidth";

#if UNITY_EDITOR
        private static CrystalEditorSceneController instance;
        public static CrystalEditorSceneController Instance { get => instance; }

        [SerializeField] GameObject container;
        [SerializeField] private GameObject backgroundContainer;
        [SerializeField] private List<GameObject> unpickableObjects;
        [SerializeField] float gizmoRadius;
        [SerializeField] float gizmoThickness;
        [SerializeField] Color gizmoColor;
        [SerializeField] GameObject screwPrefab;
        [SerializeField] List<CrystalPlankData> plankData;
        [SerializeField] Color[] layerColors;
        private CrystalSavableHole holeRef1;
        private CrystalSavableHole holeRef2;
        private bool isQuickModeEnabled;
        private int currentLayerIndex;
        private int maxLayerIndex;

        private bool stageChenged;
        private CrystalHoleData[] holeDataCached;
        private CrystalPlankLevelData[] plankDataCached;

        public GameObject Container { get => container; set => container = value; }
        public GameObject ScrewPrefab { get => screwPrefab; set => screwPrefab = value; }
        public List<CrystalPlankData> PlankData { get => plankData; set => plankData = value; }
        public bool IsQuickModeEnabled { get => isQuickModeEnabled; set => isQuickModeEnabled = value; }
        public Color[] LayerColors { get => layerColors; set => layerColors = value; }
        public int CurrentLayerIndex
        {
            get => currentLayerIndex; set
            {
                currentLayerIndex = value;
                UpdatePlanks();
            }
        }
        public int MaxLayerIndex { get => maxLayerIndex; set => maxLayerIndex = value; }

        public CrystalEditorSceneController()
        {
            instance = this;
            SceneView.duringSceneGui += DuringSceneGui;
            Selection.selectionChanged += SelectionChanged;
            EditorApplication.hierarchyChanged += UpdateHoleSelection;
        }



        public void SpawnHole(GameObject prefab, Vector3 position, bool hasScrew = false)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.hideFlags = HideFlags.DontSave;
            gameObject.transform.SetParent(container.transform);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.name = prefab.name + " ( Child # " + container.transform.childCount + ")";

            SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 30000;
            }

            CrystalSavableHole savableItem = gameObject.AddComponent<CrystalSavableHole>();
            savableItem.hideFlags = HideFlags.DontSave;
            savableItem.HasScrew = hasScrew;

            SelectGameObject(gameObject);
        }

        //used when level loads in level editor
        public void SpawnPlank(CrystalPlankLevelData data, GameObject prefab)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.hideFlags = HideFlags.DontSave;
            gameObject.transform.SetParent(container.transform);
            gameObject.transform.position = data.Position;
            gameObject.transform.rotation = Quaternion.Euler(data.Rotation);
            gameObject.name = prefab.name + "(el # " + container.transform.childCount + ")";
            gameObject.transform.localScale = data.Scale;

            CrystalSavablePlank savableItem = gameObject.AddComponent<CrystalSavablePlank>();
            savableItem.PlankType = data.PlankType;
            savableItem.PlankLayer = data.PlankLayer;
            savableItem.ScrewsPositions = data.ScrewsPositions;
            savableItem.hideFlags = HideFlags.DontSave;

            SelectGameObject(gameObject);

            if (data.PlankLayer > maxLayerIndex)
            {
                maxLayerIndex = data.PlankLayer;
            }
        }

        public void SpawnPlankWithUndo(CrystalPlankLevelData data, GameObject prefab)
        {
            Undo.IncrementCurrentGroup();

            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            Undo.RegisterCreatedObjectUndo(gameObject, "SpawnPlankWithUndo");
            Undo.SetTransformParent(gameObject.transform, container.transform, "SpawnPlankWithUndo");
            Undo.RegisterFullObjectHierarchyUndo(gameObject.transform, "SpawnPlankWithUndo");

            gameObject.hideFlags = HideFlags.DontSave;
            gameObject.transform.position = data.Position;
            gameObject.transform.rotation = Quaternion.Euler(data.Rotation);
            gameObject.transform.localScale = data.Scale;
            gameObject.name = prefab.name + "(el # " + container.transform.childCount + ")";

            CrystalSavablePlank savableItem = gameObject.AddComponent<CrystalSavablePlank>();

            Undo.RegisterCreatedObjectUndo(savableItem, "SpawnPlankWithUndo");
            Undo.RecordObject(savableItem, "SpawnPlankWithUndo");

            savableItem.hideFlags = HideFlags.DontSave;
            savableItem.PlankType = data.PlankType;
            savableItem.PlankLayer = data.PlankLayer;
            savableItem.ScrewsPositions = data.ScrewsPositions;

            SelectGameObject(gameObject);

            Undo.SetCurrentGroupName("SpawnPlankWithUndo");

            if (data.PlankLayer > maxLayerIndex)
            {
                maxLayerIndex = data.PlankLayer;
            }
        }

        public void SpawnBackground(GameObject prefab)
        {
            for (int i = backgroundContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(backgroundContainer.transform.GetChild(i).gameObject);
            }

            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.hideFlags = HideFlags.DontSave;
            gameObject.transform.SetParent(backgroundContainer.transform);
            gameObject.transform.position = Vector3.forward;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            SceneVisibilityManager.instance.DisablePicking(gameObject, true);
        }

        public void SelectGameObject(GameObject selectedGameObject)
        {
            Selection.activeGameObject = selectedGameObject;
        }

        public void Clear()
        {
            if(container == null)
            {
                return;
            }

            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < unpickableObjects.Count; i++)
            {
                SceneVisibilityManager.instance.DisablePicking(unpickableObjects[i], true);
            }

            currentLayerIndex = 0;
            maxLayerIndex = 0;
        }

        public void UpdateHoleSelection()
        {
            try
            {
                CrystalSavableHole[] savableItems = container.GetComponentsInChildren<CrystalSavableHole>();

                for (int i = 0; i < savableItems.Length; i++)
                {
                    SceneVisibilityManager.instance.DisablePicking(savableItems[i].gameObject, true);
                    SceneVisibilityManager.instance.EnablePicking(savableItems[i].gameObject, false);
                    SceneVisibilityManager.instance.EnablePicking(savableItems[i].transform.GetChild(0).gameObject, false);
                }
            }
            catch
            {

            }
        }

        public CrystalHoleData[] GetHoleData()
        {
            CrystalSavableHole[] savableItems = container.GetComponentsInChildren<CrystalSavableHole>();
            CrystalHoleData[] result = new CrystalHoleData[savableItems.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CrystalHoleData(savableItems[i].transform.position, savableItems[i].HasScrew);
            }

            return result;
        }

        public void UpdatePlanks()
        {
            CrystalSavablePlank[] savableItems = container.GetComponentsInChildren<CrystalSavablePlank>();

            for (int i = 0; i < savableItems.Length; i++)
            {
                savableItems[i].LayerUpdate();
            }
        }

        public CrystalPlankLevelData[] GetPlankData()
        {
            CrystalSavablePlank[] savableItems = container.GetComponentsInChildren<CrystalSavablePlank>();
            UpdateScrewPositions(savableItems);
            CrystalPlankLevelData[] result = new CrystalPlankLevelData[savableItems.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CrystalPlankLevelData(savableItems[i].PlankType, savableItems[i].PlankLayer, savableItems[i].transform.position, savableItems[i].transform.rotation.eulerAngles, savableItems[i].transform.localScale, savableItems[i].ScrewsPositions);
            }

            return result;
        }

        public void UpdateScrewPositions(CrystalSavablePlank[] planks)
        {
            CrystalSavableHole[] holes = container.GetComponentsInChildren<CrystalSavableHole>();
            CrystalPlankController plankBehavior;

            for (int i = 0; i < planks.Length; i++)
            {
                planks[i].ScrewsPositions.Clear();
                plankBehavior = planks[i].GetComponent<CrystalPlankController>();

                for (int j = 0; j < holes.Length; j++)
                {
                    if (holes[j].HasScrew && (plankBehavior.DoesPointOverlapsCollider(holes[j].transform.position)))
                    {
                        Vector3 screwPosition = holes[j].transform.position - planks[i].transform.position;

                        planks[i].ScrewsPositions.Add(RotateVectorAroundPivot(screwPosition, planks[i].transform.position, -Vector3.forward * planks[i].transform.eulerAngles.z));
                    }
                }
            }
        }

        private Vector3 RotateVectorAroundPivot(Vector3 vector, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * vector; // rotate it
        }



        public void OnDrawGizmos()
        {
            if (holeRef1 != null)
            {
                Color backupColor = Handles.color;
                Handles.color = gizmoColor;
                Handles.DrawWireDisc(holeRef1.transform.position, Vector3.forward, gizmoRadius, gizmoThickness);

                CrystalSavableHole hole = Selection.activeGameObject.GetComponent<CrystalSavableHole>();

                if ((hole != null) && (hole != holeRef1))
                {
                    Handles.DrawLine(holeRef1.transform.position, hole.transform.position, gizmoThickness);
                }

                Handles.color = backupColor;

            }
        }

        [Button]
        public void SaveCameraPosition()
        {
            PlayerPrefs.SetFloat(PREFS_WIDTH_KEY, SceneView.lastActiveSceneView.size);
        }

        public void SetUpCamera()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.AlignViewToObject(transform);

            if (PlayerPrefs.HasKey(PREFS_WIDTH_KEY))
            {
                sceneView.size = PlayerPrefs.GetFloat(PREFS_WIDTH_KEY, 6f);
            }
            else
            {
                sceneView.size = PlayerPrefs.GetFloat("CameraScalerWidth", 6f);
            }

            
            SceneView.RepaintAll();
        }

        public void ShowEditorOverlay()
        {
            SceneView.lastActiveSceneView.TryGetOverlay("Level editor overlay", out var overlay);

            if (overlay != null)
            {
                overlay.displayed = true;
                overlay.collapsed = false;
            }
        }

        public void HideEditorOverlay()
        {
            SceneView.lastActiveSceneView.TryGetOverlay("Level editor overlay", out var overlay);

            if (overlay != null)
            {
                overlay.displayed = false;
            }
        }

        public void SetHoleRef(CrystalSavableHole holeRef)
        {
            if (holeRef1 == null)
            {
                holeRef1 = holeRef;
                SceneView.RepaintAll();// updating scene to get gizmo
            }
            else
            {
                if (holeRef == holeRef1)
                {
                    return;
                }

                holeRef2 = holeRef;
                CreatePlank();
            }
        }

        // quick mode
        private void CreatePlank()
        {
            SceneView.RepaintAll();// getting rid of the gizmos
            Debug.Log($"Create plank called from hole1: {holeRef1.transform.position} to hole2: {holeRef2.transform.position}");

            float holesDifference = (holeRef1.transform.position - holeRef2.transform.position).magnitude;

            bool suitablePlankFound = false;
            CrystalPlankType plankToSpawn = CrystalPlankType.Normal_size_1;
            GameObject prefab;

            for (int i = 0; i < PlankData.Count; i++)
            {
                if (PlankData[i].UseInQuickMode && PlankData[i].QuickModeSize > 0)
                {
                    if (PlankData[i].QuickModeSize > holesDifference)
                    {
                        suitablePlankFound = true;
                        plankToSpawn = PlankData[i].Type;
                        prefab = PlankData[i].Prefab;
                        Vector3 spawnPosition = holeRef1.transform.position + (holeRef2.transform.position - holeRef1.transform.position) * 0.5f;

                        Quaternion rightRotation = Quaternion.LookRotation(Vector3.right);
                        Quaternion secondHoleRotation = Quaternion.LookRotation((holeRef2.transform.position - holeRef1.transform.position).normalized);

                        float angle = Vector2.Angle(Vector3.right, holeRef2.transform.position - holeRef1.transform.position);
                        // float angle = Quaternion.Angle(rightRotation, secondHoleRotation);

                        if (holeRef1.transform.position.x < holeRef2.transform.position.x && holeRef1.transform.position.y > holeRef2.transform.position.y)
                        {
                            angle = -angle;
                        }

                        if (holeRef1.transform.position.x > holeRef2.transform.position.x && holeRef1.transform.position.y > holeRef2.transform.position.y)
                        {
                            angle = -angle;
                        }

                        if (!holeRef1.HasScrew)
                        {
                            holeRef1.HasScrew = true;
                        }

                        if (!holeRef2.HasScrew)
                        {
                            holeRef2.HasScrew = true;
                        }

                        SpawnPlankWithUndo(new CrystalPlankLevelData(plankToSpawn, currentLayerIndex, spawnPosition, Vector3.forward * angle, Vector3.one, new List<Vector3>()), prefab);

                        break;
                    }
                }
            }

            if (!suitablePlankFound)
            {
                Debug.Log("Suitable plank wasn't found");
            }

            holeRef1 = null;
            holeRef2 = null;
        }

        private static void DuringSceneGui(SceneView sceneView)
        {
            if (Event.current.keyCode == KeyCode.A)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    Instance.IsQuickModeEnabled = true;
                }
                else if (Event.current.type == EventType.KeyUp)
                {
                    Instance.IsQuickModeEnabled = false;
                    Instance.holeRef1 = null;
                    Instance.holeRef2 = null;
                }
            }

            if ((Event.current.keyCode == KeyCode.D) && (Event.current.type == EventType.KeyDown))
            {
                if (Selection.activeGameObject != null)
                {
                    CrystalSavableHole hole = Selection.activeGameObject.GetComponent<CrystalSavableHole>();

                    if (hole != null)
                    {
                        hole.HasScrew = !hole.HasScrew;
                    }
                }
            }
        }

        private void SelectionChanged()
        {
            if (!IsQuickModeEnabled)
            {
                return;
            }

            if (Selection.activeGameObject != null)
            {
                CrystalSavableHole hole = Selection.activeGameObject.GetComponent<CrystalSavableHole>();

                if (hole != null)
                {
                    SetHoleRef(hole);
                }

            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void Unsubscribe()
        {
            HideEditorOverlay();
            SceneView.duringSceneGui -= DuringSceneGui;
            Selection.selectionChanged -= SelectionChanged;
            EditorApplication.hierarchyChanged -= UpdateHoleSelection;
        }

        public bool IsStageChanged()
        {
            if (stageChenged)
            {
                return stageChenged;
            }

            if(container == null)
            {
                return false;
            }

            if (!holeDataCached.SequenceEqual(GetHoleData()))
            {
                stageChenged = true;
                return stageChenged;
            }

            if (!plankDataCached.SequenceEqual(GetPlankData()))
            {
                stageChenged = true;
                return stageChenged;
            }

            return stageChenged;
        }

        public void RegisterStageState()
        {
            stageChenged = false;
            holeDataCached = GetHoleData();
            plankDataCached = GetPlankData();
        }




#endif
    }
}
