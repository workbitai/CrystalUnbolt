using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CrystalUnbolt
{
    public static class WorldSpaceRaycaster
    {
        private static List<CrystalWorldSpaceButton> buttons;

        public static void AddWorldSpaceButton(CrystalWorldSpaceButton button)
        {
            if (buttons == null) buttons = new List<CrystalWorldSpaceButton>();

            if (!buttons.Contains(button)) buttons.Add(button);
        }

        public static void RemoveWorldSpaceButton(CrystalWorldSpaceButton button)
        {
            if (buttons == null) buttons = new List<CrystalWorldSpaceButton>();

            buttons.Remove(button);
        }

        private static CrystalWorldSpaceButton pressedButton;

        public static bool Raycast(Vector2 pointerPosition)
        {
            if (buttons.IsNullOrEmpty()) return false;

            var ray = Camera.main.ScreenPointToRay(pointerPosition);

            var closestDistance = float.MaxValue;
            pressedButton = null;

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];

                if (button.Raycast(ray))
                {
                    var distance = Vector3.Distance(button.transform.position, Camera.main.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        pressedButton = button;
                    }
                }
            }

            if (pressedButton != null)
            {
                pressedButton.Press();

                return true;
            }

            return false;
        }

        public static bool Raycast(PointerEventData eventData)
        {
            if (buttons.IsNullOrEmpty()) return false;

            var ray = Camera.main.ScreenPointToRay(eventData.position);

            var closestDistance = float.MaxValue;
            pressedButton = null;

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];

                if (button.Raycast(ray))
                {
                    var distance = Vector3.Distance(button.transform.position, Camera.main.transform.position);

                    if(distance < closestDistance)
                    {
                        closestDistance = distance;
                        pressedButton = button;
                    }                    
                }
            }

            if(pressedButton != null)
            {
                pressedButton.Press(eventData);

                return true;
            }

            return false;
        }

        public static void OnPointerUp(PointerEventData eventData)
        {
            if(pressedButton != null)
            {
                pressedButton.Release(eventData);

                pressedButton = null;
            }
        }
    }
}