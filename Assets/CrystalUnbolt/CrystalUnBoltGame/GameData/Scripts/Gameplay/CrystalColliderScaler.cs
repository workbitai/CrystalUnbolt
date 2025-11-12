using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalColliderScaler : MonoBehaviour
    {
        private PolygonCollider2D polygonCollider;
        [SerializeField, Range(0.01f, 2f)] float scaleFactor = 1;

        [SerializeField, HideInInspector] List<Vector2[]> paths;

        [Button]
        public void SaveState()
        {
            if (polygonCollider == null) polygonCollider = GetComponent<PolygonCollider2D>();
            if (polygonCollider == null) return;

            paths = new List<Vector2[]>();

            for(int i = 0; i < polygonCollider.pathCount; i++)
            {
                Vector2[] polygonPath = polygonCollider.GetPath(i);

                paths.Add(polygonPath);
            }

            scaleFactor = 1;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnValidate()
        {
            if (polygonCollider == null) polygonCollider = GetComponent<PolygonCollider2D>();
            if (polygonCollider == null) return;

            if (paths == null) return;

            for(int i = 0; i < paths.Count; i++)
            {
                Vector2[] path = paths[i];
                Vector2[] scaledPath = new Vector2[path.Length];

                for (int j = 0; j < path.Length; j++)
                {
                    scaledPath[j] = path[j] * scaleFactor;
                }

                polygonCollider.SetPath(i, scaledPath);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(polygonCollider);
#endif
        }
    }
}
