using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalCameraScaler : MonoBehaviour
    {
        [SerializeField] float minWidth = 6;

        private void Awake()
        {
            Camera camera = GetComponent<Camera>();

            if (!camera.orthographic) return;

            float width = camera.aspect * camera.orthographicSize;
            PlayerPrefs.SetFloat("CameraScalerWidth", width);

            if (width < minWidth)
            {
                camera.orthographicSize *= minWidth / width;
            }
        }

    }
}
