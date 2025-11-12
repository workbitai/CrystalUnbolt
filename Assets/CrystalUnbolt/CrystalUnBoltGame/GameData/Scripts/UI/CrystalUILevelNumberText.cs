using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CrystalUILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static CrystalUILevelNumberText instance;

        private UIScaleAnimation uIScalableObject;

        private static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        private static TextMeshProUGUI levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<TextMeshProUGUI>();

            uIScalableObject = new UIScaleAnimation(transform);
        }

        public static void Show(bool immediately = false)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = false)
        {
            if (!IsDisplayed)
                return;

            if (immediately)
                IsDisplayed = false;

            UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
            {
                IsDisplayed = false;
                levelNumberText.enabled = false;
            });
        }

        public void UpdateLevelNumber(int number)
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, number);
        }

    }
}
