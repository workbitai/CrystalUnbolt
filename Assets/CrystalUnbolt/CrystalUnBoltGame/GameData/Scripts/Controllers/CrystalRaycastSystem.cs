using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalRaycastSystem : MonoBehaviour
    {
        private static bool isActive;

        public static event GameCallback OnInputActivated;

        public void Init()
        {
            isActive = true;
        }

        private void Update()
        {
            if (!isActive || !CrystalLevelController.IsRaycastEnabled || !CrystalLevelController.IsLevelLoaded || ScreenManager.IsPopupOpened) return;

            if (CrystalInputHandler.ClickAction.WasPressedThisFrame())
            {
                Ray ray = Camera.main.ScreenPointToRay(CrystalInputHandler.MousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    IClickableObject clickableObject = hit.transform.GetComponent<IClickableObject>();
                    if (clickableObject != null)
                    {
                        ApplyPUOrClick(clickableObject, hit.point);
                    }
                }
                else
                {
                    // Collecting all 2D colliders
                    Collider2D[] colliders2D = Physics2D.OverlapPointAll(ray.origin);
                    List<IClickableObject> clickableObjects2D = ProcessRaycastColliders2D(colliders2D);

                    if (!clickableObjects2D.IsNullOrEmpty())
                    {
                        // Only CrystalBaseController is clicked
                        if (clickableObjects2D.Count == 1)
                        {
                            ApplyPUOrClick(clickableObjects2D[0], ray.origin);
                            return;
                        }

                        // Bottom collider isn't base hole
                        if (!(clickableObjects2D[^2] is CrystalBaseHole baseHole))
                        {
                            ApplyPUOrClick(clickableObjects2D[0], ray.origin);
                            return;
                        }

                        // recalculate Physics for the base hole exact position
                        colliders2D = Physics2D.OverlapCircleAll(baseHole.Position, baseHole.PhysicsRadius);
                        clickableObjects2D = ProcessRaycastColliders2D(colliders2D);

                        // if the top collider is the hole - checking if we can insert the screw
                        if (CrystalScrewController.SelectedScrew != null)
                        {
                            if (clickableObjects2D[0] is CrystalHoleController)
                            {
                                CrystalLevelController.ProcessClick(clickableObjects2D);
                            }
                        }
                        else
                        {
                            ApplyPUOrClick(clickableObjects2D[0], ray.origin);
                        }
                    }
                }
            }
        }

        private static List<IClickableObject> ProcessRaycastColliders2D(Collider2D[] colliders)
        {
            List<IClickableObject> clickableObjects2D = new List<IClickableObject>();

            // Sorting only for clickables
            for (int i = 0; i < colliders.Length; i++)
            {
                IClickableObject clickableObject = colliders[i].transform.GetComponent<IClickableObject>();
                if (clickableObject != null)
                {
                    clickableObjects2D.Add(clickableObject);
                }
            }

            clickableObjects2D.Sort((first, second) => (int)((first.Position.z - second.Position.z) * 100));

            return clickableObjects2D;
        }

        private void ApplyPUOrClick(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if (CrystalPUController.SelectedPU != null)
            {
                CrystalPUController.ApplyToElement(clickableObject, clickPosition);
            }
            else
            {
                clickableObject.OnObjectClicked(clickPosition);
            }
        }

        public static List<IClickableObject> HasOverlapCircle2D(Vector2 point, float radius, int mask)
        {
            Collider2D[] colliders2D = Physics2D.OverlapCircleAll(point, radius, mask);
            List<IClickableObject> clickableObjects2D = ProcessRaycastColliders2D(colliders2D);

            return clickableObjects2D;
        }

        public static void Disable()
        {
            isActive = false;
        }

        public static void Enable()
        {
            isActive = true;

            OnInputActivated?.Invoke();
        }

        public void ResetControl()
        {

        }
    }
}
