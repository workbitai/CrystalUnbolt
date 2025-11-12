using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalMessageBox
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] TextMeshProUGUI messageText;
        [SerializeField] SimpleBounce bounceEffect;
        
        public void Init()
        {
            bounceEffect.Init(rectTransform);

            rectTransform.gameObject.SetActive(false);
        }

        public void Activate(string text, Vector3 position, int fontSize = 52)
        {
            messageText.text = text;
            messageText.fontSize = fontSize;

            rectTransform.position = rectTransform.position.SetY(position.y);
            rectTransform.gameObject.SetActive(true);

            bounceEffect.Bounce();
        }

        public void Disable()
        {
            rectTransform.gameObject.SetActive(false);
        }

        public void ActivateTutorial()
        {
            CrystalTutorialCanvasController.ActivateTutorialCanvas(rectTransform, false, false);
        }
    }
}