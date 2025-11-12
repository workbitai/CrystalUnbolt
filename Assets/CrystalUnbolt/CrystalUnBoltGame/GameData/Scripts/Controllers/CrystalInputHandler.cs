using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalUnbolt
{
    [DefaultExecutionOrder(-1)]
    public class CrystalInputHandler : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;

        private InputAction mousePositionAction;

        public static InputActionAsset InputActionsAsset { get; private set; }

        public static Vector2 MousePosition { get; private set; }
        public static InputAction ClickAction { get; private set; }

        private void Awake()
        {
            InputActionsAsset = inputActions;

            mousePositionAction = inputActions.FindAction("Point");
            mousePositionAction.Enable();

            ClickAction = inputActions.FindAction("Click");
            ClickAction.Enable();

            MousePosition = mousePositionAction.ReadValue<Vector2>();
        }

        private void Update()
        {
            MousePosition = mousePositionAction.ReadValue<Vector2>();
        }
    }
}
